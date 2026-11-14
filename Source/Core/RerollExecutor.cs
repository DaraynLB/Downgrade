using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Downgrade
{
    public enum RerollOutcome
    {
        Rerolled,
        NothingAbove,
        Failed,
    }

    /// <summary>
    /// Applies a plan to one pawn: plan → make new items → apply through an undo log → commit and record.
    /// Any failure leaves the pawn and its record as they were (FR-019).
    /// </summary>
    public static class RerollExecutor
    {
        /// <summary>Dev-only: throw after the first item is added, to check that rollback restores the pawn.</summary>
        public static bool debugFailAfterFirstPut;

        public static RerollOutcome Execute(Pawn pawn, TechLevel era)
        {
            RerollPlan plan;
            try
            {
                plan = RerollPlanner.Plan(pawn, era);
            }
            catch (Exception e)
            {
                DLog.RerollFailed(pawn, "planning: " + e.Message);
                Log.Message(e.ToString());
                return RerollOutcome.Failed;
            }

            if (plan.NothingToDo)
            {
                DLog.Skip(pawn, era);
                return RerollOutcome.NothingAbove;
            }

            // Create phase: nothing on the pawn changes yet.
            var made = new List<Thing>();
            var ammo = new List<GivenAmmo>();
            try
            {
                foreach (var p in plan.addThings)
                {
                    p.made = Make(p);
                    made.Add(p.made);
                }
                foreach (var p in plan.addThings)
                {
                    var a = MakeAmmo(p);
                    if (a == null)
                        continue;
                    ammo.Add(a);
                    made.AddRange(a.stacks);
                }
            }
            catch (Exception e)
            {
                DestroyAll(made);
                DLog.RerollFailed(pawn, "making items: " + e.Message);
                return RerollOutcome.Failed;
            }

            // Apply phase, through the undo log.
            var tx = new RerollTransaction(pawn);
            try
            {
                foreach (var t in plan.removeThings)
                    tx.TakeOff(t);
                foreach (var p in plan.addThings)
                {
                    tx.Put(p.made, p.slot);
                    if (debugFailAfterFirstPut)
                        throw new InvalidOperationException("forced failure (dev check)");
                }
                foreach (var a in ammo)
                {
                    foreach (var stack in a.stacks)
                        tx.Put(stack, GearSlot.Inventory);
                }
                // Traits after gear (FR-018): last reroll's traits out, this reroll's in.
                foreach (var t in plan.removeTraits)
                    tx.RemoveTrait(t);
                foreach (var t in plan.addTraits)
                    tx.GainTrait(new Trait(t.def, t.degree));
            }
            catch (Exception e)
            {
                tx.Rollback();
                DestroyAll(made); // anything not yet placed; placed items were destroyed by the rollback
                DLog.RerollFailed(pawn, e.Message);
                return RerollOutcome.Failed;
            }

            tx.Commit();
            WriteRecord(pawn, plan, ammo);
            DLog.Reroll(plan);
            foreach (var a in ammo)
                DLog.CeAmmo(a.ammo, a.count, a.weapon);
            return RerollOutcome.Rerolled;
        }

        private static Thing Make(PlannedThing p)
        {
            var thing = ThingMaker.MakeThing(p.def, p.stuff);
            thing.stackCount = p.stackCount;
            // FR-017: quality bought from the leftover. With Itemization Overhaul, its own hooks roll
            // the tier's bonuses from this (research R7).
            if (p.quality != QualityCategory.Normal)
                thing.TryGetComp<CompQuality>()?.SetQuality(p.quality, ArtGenerationContext.Outsider);
            return thing;
        }

        /// <summary>Spare Combat Extended ammo made for one new ranged weapon (FR-022, research R10).</summary>
        private class GivenAmmo
        {
            public ThingDef weapon;
            public ThingDef ammo;
            public int count;
            public List<Thing> stacks = new List<Thing>();
        }

        /// <summary>Loads a new ranged weapon and makes its spare ammo, split by stack limit; null if none.</summary>
        private static GivenAmmo MakeAmmo(PlannedThing p)
        {
            if (p.category != GearCategory.Weapon || p.made == null || !p.def.IsRangedWeapon)
                return null;
            if (!CompatCE.TryFill(p.made, out var ammoDef, out int count))
                return null;

            var a = new GivenAmmo { weapon = p.def, ammo = ammoDef, count = count };
            int limit = Math.Max(1, ammoDef.stackLimit);
            for (int left = count; left > 0;)
            {
                var stack = ThingMaker.MakeThing(ammoDef);
                stack.stackCount = Math.Min(limit, left);
                left -= stack.stackCount;
                a.stacks.Add(stack);
            }
            return a;
        }

        private static void DestroyAll(IEnumerable<Thing> things)
        {
            foreach (var t in things)
            {
                if (t != null && !t.Destroyed && t.ParentHolder == null)
                    t.Destroy(DestroyMode.Vanish);
            }
        }

        private static void WriteRecord(Pawn pawn, RerollPlan plan, List<GivenAmmo> ammo)
        {
            var comp = pawn.TryGetComp<CompDowngrade>();
            if (comp == null)
                return;
            var r = comp.record;
            r.hasRecord = true;
            r.weaponBudget = plan.Budget(GearCategory.Weapon).budget;
            r.apparelBudget = plan.Budget(GearCategory.Apparel).budget;
            r.implantBudget = plan.Budget(GearCategory.Implant).budget;
            // Ammo is recorded too, so the next reroll takes it away with the weapon it came with.
            r.givenThings = plan.addThings.Select(p => p.made)
                .Concat(ammo.SelectMany(a => a.stacks))
                .ToList();
            r.addedTraits = plan.addTraits.Select(t => new AddedTrait { def = t.def, degree = t.degree }).ToList();
            r.lastEra = plan.era;
        }
    }
}
