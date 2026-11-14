using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>One (def, stuff) pair that a reroll can give.</summary>
    public class GearCandidate
    {
        public ThingDef def;
        public ThingDef stuff;
        public float price;
        public float commonality;
    }

    /// <summary>
    /// Candidate replacement gear per era (research R4), built on first use and cached. Pawn-kind
    /// weapon and apparel tags are deliberately not applied: Downgrade's purpose is to give gear
    /// outside those lists.
    /// </summary>
    public static class GearPools
    {
        private class EraPool
        {
            public List<GearCandidate> ranged = new List<GearCandidate>();
            public List<GearCandidate> melee = new List<GearCandidate>();
            public List<GearCandidate> apparel = new List<GearCandidate>();
        }

        private static readonly Dictionary<TechLevel, EraPool> pools = new Dictionary<TechLevel, EraPool>();

        /// <summary>One unspawned instance per def, only for EquipmentUtility.CanEquip checks. Never given.</summary>
        private static readonly Dictionary<ThingDef, Thing> probes = new Dictionary<ThingDef, Thing>();

        /// <summary>Clear cached pools. Called when the player changes an era override.</summary>
        public static void Invalidate()
        {
            pools.Clear();
        }

        public static List<GearCandidate> Ranged(TechLevel era) => Get(era).ranged;
        public static List<GearCandidate> Melee(TechLevel era) => Get(era).melee;
        public static List<GearCandidate> Apparel(TechLevel era) => Get(era).apparel;

        public static bool IsWeaponDef(ThingDef def) =>
            def.equipmentType == EquipmentType.Primary && !def.weaponTags.NullOrEmpty();

        /// <summary>
        /// Grenades and one-shot launchers count as ranged weapons in the game's data, but are never
        /// handed out as a replacement for a real weapon.
        /// </summary>
        private static bool IsConsumableWeapon(ThingDef def)
        {
            var verbs = def.Verbs;
            if (verbs == null)
                return false;
            foreach (var v in verbs)
            {
                if (v.ai_IsBuildingDestroyer)
                    return true;
                if (v.verbClass != null && typeof(Verb_ShootOneUse).IsAssignableFrom(v.verbClass))
                    return true;
            }
            return false;
        }

        private static EraPool Get(TechLevel era)
        {
            if (pools.TryGetValue(era, out var pool))
                return pool;
            pool = Build(era);
            pools[era] = pool;
            return pool;
        }

        private static EraPool Build(TechLevel era)
        {
            var pool = new EraPool();

            foreach (var pair in ThingStuffPair.AllWith(IsWeaponDef))
            {
                var def = pair.thing;
                if (!Allowed(def, pair.stuff, era) || IsConsumableWeapon(def)
                    || def.HasComp(typeof(CompBladelinkWeapon)))
                    continue;
                var c = Candidate(pair);
                if (def.IsRangedWeapon)
                    pool.ranged.Add(c);
                else if (def.IsMeleeWeapon)
                    pool.melee.Add(c);
            }

            foreach (var pair in ThingStuffPair.AllWith(td => td.IsApparel))
            {
                if (Allowed(pair.thing, pair.stuff, era))
                    pool.apparel.Add(Candidate(pair));
            }

            return pool;
        }

        private static bool Allowed(ThingDef def, ThingDef stuff, TechLevel era) =>
            !def.destroyOnDrop && !Eras.IsAbove(def, era) && Eras.StuffAllowed(stuff, era);

        private static GearCandidate Candidate(ThingStuffPair pair) => new GearCandidate
        {
            def = pair.thing,
            stuff = pair.stuff,
            price = Pricing.Price(pair.thing, pair.stuff),
            commonality = pair.Commonality,
        };

        /// <summary>EquipmentUtility.CanEquip is the check race-restriction frameworks patch, and it needs a Thing.</summary>
        public static bool CanEquip(ThingDef def, Pawn pawn)
        {
            if (!probes.TryGetValue(def, out var probe))
            {
                probe = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                probes[def] = probe;
            }
            return EquipmentUtility.CanEquip(probe, pawn);
        }

        public static void LogCounts()
        {
            var ultra = Get(TechLevel.Ultra);
            DLog.Pools(ultra.ranged.Count + ultra.melee.Count, ultra.apparel.Count, ImplantCount(),
                Eras.UnlabeledItems().Count, Eras.UnknownStuffs().Count);
        }

        /// <summary>Era body parts available; filled in by US4.</summary>
        private static int ImplantCount() => 0;
    }
}
