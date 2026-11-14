using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Downgrade
{
    /// <summary>
    /// Decides everything a reroll will do, without touching the pawn or its record (plan.md
    /// "Structure Decision"). The executor applies the plan; the dry-run debug action only logs it.
    /// </summary>
    public static class RerollPlanner
    {
        private static DowngradeSettings Settings => DowngradeMod.Settings;
        private static float AffordableBand => DowngradeDefOf.Downgrade_Tuning.affordableBand;

        /// <summary>A worn, held or carried item that this reroll replaces.</summary>
        private class Replaced
        {
            public Thing thing;
            public GearSlot slot;
            public float weight; // base value; splits the category budget across slots
        }

        public static RerollPlan Plan(Pawn pawn, TechLevel era)
        {
            var comp = pawn.TryGetComp<CompDowngrade>();
            var record = comp?.record ?? new RerollRecord();
            var plan = new RerollPlan { pawn = pawn, era = era, firstReroll = !record.hasRecord };

            // Items Downgrade gave last time that the pawn still carries (working copy; record untouched).
            var given = new HashSet<Thing>(record.givenThings.Where(t =>
                t != null && !t.Destroyed && RerollRecord.IsCarriedBy(t, pawn)));

            var weapons = GatherWeapons(pawn, era, given);
            var apparel = GatherApparel(pawn, era, given);
            var ammo = GatherAmmo(pawn, weapons, apparel, given);

            plan.NothingToDo = weapons.Count == 0 && apparel.Count == 0 && ammo.Count == 0
                               && record.givenHediffs.Count == 0 && record.addedTraits.Count == 0;
            if (plan.NothingToDo)
                return plan;

            SetBudget(plan, record, GearCategory.Weapon, weapons, given);
            SetBudget(plan, record, GearCategory.Apparel, apparel, given);
            // Implants are planned by US4; until then the recorded implant budget is carried unchanged.
            SetBudget(plan, record, GearCategory.Implant, new List<Replaced>(), given);

            foreach (var r in weapons.Concat(apparel))
                plan.removeThings.Add(r.thing);
            plan.removeThings.AddRange(ammo);

            PlanWeapons(plan, weapons);
            PlanApparel(plan, apparel);

            plan.discarded = plan.budgets.Values.Sum(b => b.Leftover);
            return plan;
        }

        // ---------- Gather ----------

        // FR-005: an item's era is the later of its def's and its material's (PRD decision 15).
        private static bool Replaces(Thing t, TechLevel era, HashSet<Thing> given) =>
            given.Contains(t) ||
            ((Eras.IsAbove(t.def, era) || !Eras.StuffAllowed(t.Stuff, era)) && !CompatIO.IsSkipped(t));

        private static List<Replaced> GatherWeapons(Pawn pawn, TechLevel era, HashSet<Thing> given)
        {
            var list = new List<Replaced>();
            foreach (var eq in pawn.equipment.AllEquipmentListForReading)
            {
                if (GearPools.IsWeaponDef(eq.def) && Replaces(eq, era, given))
                    list.Add(new Replaced { thing = eq, slot = GearSlot.Equipment });
            }
            if (pawn.inventory != null)
            {
                foreach (var t in pawn.inventory.innerContainer)
                {
                    if (GearPools.IsWeaponDef(t.def) && Replaces(t, era, given))
                        list.Add(new Replaced { thing = t, slot = GearSlot.Inventory });
                }
            }
            return WithWeights(list);
        }

        private static List<Replaced> GatherApparel(Pawn pawn, TechLevel era, HashSet<Thing> given)
        {
            var list = new List<Replaced>();
            foreach (var ap in pawn.apparel.WornApparel)
            {
                if (Replaces(ap, era, given))
                    list.Add(new Replaced { thing = ap, slot = GearSlot.Apparel });
            }
            return WithWeights(list);
        }

        /// <summary>
        /// Inventory items that go with the replaced gear (FR-012, FR-022, research R10): Combat Extended
        /// ammo that a replaced weapon fires, and anything else Downgrade gave last time (its ammo) that
        /// isn't already being replaced as a weapon or apparel. No budget: ammo isn't priced.
        /// </summary>
        private static List<Thing> GatherAmmo(Pawn pawn, List<Replaced> weapons, List<Replaced> apparel,
            HashSet<Thing> given)
        {
            var list = new List<Thing>();
            var replaced = new HashSet<Thing>(weapons.Concat(apparel).Select(r => r.thing));

            var ammoDefs = new HashSet<ThingDef>();
            foreach (var r in weapons)
            {
                if (r.thing.def.IsRangedWeapon)
                    ammoDefs.UnionWith(CompatCE.AmmoDefsOf(r.thing));
            }

            if (pawn.inventory != null)
            {
                foreach (var t in pawn.inventory.innerContainer)
                {
                    if (!replaced.Contains(t) && (ammoDefs.Contains(t.def) || given.Contains(t)))
                        list.Add(t);
                }
            }
            // Given items outside the inventory are weapons or apparel, gathered above.
            return list;
        }

        private static List<Replaced> WithWeights(List<Replaced> list)
        {
            foreach (var r in list)
                r.weight = Pricing.BaseValue(r.thing);
            return list.OrderByDescending(r => r.weight).ToList();
        }

        // ---------- Budget (FR-014, FR-015, Edge Cases "Second reroll") ----------

        private static void SetBudget(RerollPlan plan, RerollRecord record, GearCategory cat,
            List<Replaced> replaced, HashSet<Thing> given)
        {
            // Only newly found above-era items add value; items Downgrade gave are bought back from the record.
            float newlyFound = replaced.Where(r => !given.Contains(r.thing)).Sum(r => r.weight);
            plan.Budget(cat).budget = (record.hasRecord ? record.Budget(cat) : 0f) + newlyFound;
        }

        // ---------- Choosing (research R4) ----------

        /// <summary>
        /// Best it can afford, with variety: weighted random (by commonality) among affordable candidates
        /// priced at least <see cref="AffordableBand"/> of the most expensive affordable one. If nothing
        /// fits the allowance, the cheapest candidate; if even that exceeds what's left of the category,
        /// it's given anyway and the category has no leftover (FR-016).
        /// </summary>
        private static GearCandidate Choose(List<GearCandidate> candidates, float allowance, CategoryBudget budget)
        {
            if (candidates.Count == 0)
                return null;

            var affordable = candidates.Where(c => c.price <= allowance).ToList();
            if (affordable.Count == 0)
            {
                var cheapest = candidates.OrderBy(c => c.price).First();
                if (cheapest.price > budget.Remaining)
                    budget.usedFallback = true;
                return cheapest;
            }

            float top = affordable.Max(c => c.price);
            var band = affordable.Where(c => c.price >= top * AffordableBand).ToList();
            return band.Sum(c => c.commonality) > 0f
                ? band.RandomElementByWeight(c => c.commonality)
                : band.RandomElement();
        }

        /// <summary>This slot's share of what's left: unspent shares carry forward to later slots.</summary>
        private static float Allowance(CategoryBudget budget, float weight, float remainingWeight) =>
            remainingWeight <= 0f ? budget.Remaining : budget.Remaining * (weight / remainingWeight);

        private static void Spend(RerollPlan plan, GearCategory cat, Replaced r, GearCandidate c)
        {
            plan.Budget(cat).gearSpent += c.price;
            plan.addThings.Add(new PlannedThing
            {
                def = c.def,
                stuff = c.stuff,
                category = cat,
                slot = r.slot,
                replaces = r.thing,
            });
        }

        // ---------- Weapons (FR-007) ----------

        private static void PlanWeapons(RerollPlan plan, List<Replaced> weapons)
        {
            var budget = plan.Budget(GearCategory.Weapon);
            float remainingWeight = weapons.Sum(r => r.weight);
            var pawn = plan.pawn;

            foreach (var r in weapons)
            {
                bool ranged = r.thing.def.IsRangedWeapon;
                var sameRole = Usable(ranged ? GearPools.Ranged(plan.era) : GearPools.Melee(plan.era), pawn);
                // Edge Cases: no weapon of the role in the era → the other role; none at all → unarmed.
                var candidates = sameRole.Count > 0
                    ? sameRole
                    : Usable(ranged ? GearPools.Melee(plan.era) : GearPools.Ranged(plan.era), pawn);

                var choice = Choose(candidates, Allowance(budget, r.weight, remainingWeight), budget);
                remainingWeight -= r.weight;
                if (choice != null)
                    Spend(plan, GearCategory.Weapon, r, choice);
            }
        }

        private static List<GearCandidate> Usable(List<GearCandidate> pool, Pawn pawn)
        {
            var canEquip = new Dictionary<ThingDef, bool>();
            return pool.Where(c =>
            {
                if (!canEquip.TryGetValue(c.def, out var ok))
                    canEquip[c.def] = ok = GearPools.CanEquip(c.def, pawn);
                return ok;
            }).ToList();
        }

        // ---------- Apparel (FR-008) ----------

        private static void PlanApparel(RerollPlan plan, List<Replaced> apparel)
        {
            if (apparel.Count == 0)
                return;

            var pawn = plan.pawn;
            var body = pawn.RaceProps.body;
            var budget = plan.Budget(GearCategory.Apparel);
            float remainingWeight = apparel.Sum(r => r.weight);

            var replacedSet = new HashSet<Thing>(apparel.Select(r => r.thing));
            var worn = pawn.apparel.WornApparel.Where(a => !replacedSet.Contains(a)).Select(a => a.def).ToList();
            var wearable = new Dictionary<ThingDef, bool>();
            var pool = GearPools.Apparel(plan.era);

            foreach (var r in apparel)
            {
                var old = r.thing.def.apparel;
                var candidates = pool.Where(c =>
                {
                    var props = c.def.apparel;
                    if (!props.bodyPartGroups.Any(old.bodyPartGroups.Contains) || !props.layers.Any(old.layers.Contains))
                        return false;
                    if (!wearable.TryGetValue(c.def, out var ok))
                    {
                        ok = props.PawnCanWear(pawn) && ApparelUtility.HasPartsToWear(pawn, c.def)
                             && GearPools.CanEquip(c.def, pawn);
                        wearable[c.def] = ok;
                    }
                    return ok && worn.All(w => ApparelUtility.CanWearTogether(c.def, w, body));
                }).ToList();

                var choice = Choose(candidates, Allowance(budget, r.weight, remainingWeight), budget);
                remainingWeight -= r.weight;
                if (choice == null)
                    continue; // FR-008: the slot is left empty
                Spend(plan, GearCategory.Apparel, r, choice);
                worn.Add(choice.def);
            }
        }
    }
}
