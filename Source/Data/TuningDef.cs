using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Balance constants that aren't player settings (Principle IV). Instance: Downgrade_Tuning.</summary>
    public class TuningDef : Def
    {
        /// <summary>Candidates priced at least this fraction of the most expensive affordable one are
        /// "best it can afford" and picked by weighted random (research R4).</summary>
        public float affordableBand = 0.5f;

        /// <summary>Combat Extended: full magazines of spare ammo given per ranged weapon (research R10).</summary>
        public int ceAmmoMagazines = 4;

        /// <summary>Upper end of the "max traits added" slider.</summary>
        public int maxTraitsCeiling = 5;
    }

    [DefOf]
    public static class DowngradeDefOf
    {
        public static TuningDef Downgrade_Tuning;
        public static MaterialEraDef Downgrade_MaterialEras;
        public static TraitWhitelistDef Downgrade_StarterTraits;

        static DowngradeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DowngradeDefOf));
        }
    }
}
