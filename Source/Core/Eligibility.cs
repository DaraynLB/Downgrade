using RimWorld;
using Verse;

namespace Downgrade
{
    public enum PawnCategory
    {
        Colonist,
        Prisoner,
        Hostile,
        Trader,
        Visitor,
    }

    /// <summary>Who gets the Reroll button (FR-001, contracts/ui-contract.md "Eligible pawn").</summary>
    public static class Eligibility
    {
        public static PawnCategory CategoryOf(Pawn p)
        {
            if (p.IsColonist)
                return PawnCategory.Colonist;
            if (p.IsPrisonerOfColony)
                return PawnCategory.Prisoner;
            if (p.HostileTo(Faction.OfPlayer))
                return PawnCategory.Hostile;
            if (p.GetTraderCaravanRole() != TraderCaravanRole.None)
                return PawnCategory.Trader;
            return PawnCategory.Visitor;
        }

        /// <summary>Named boss or hero: the pawn's kind or any of its hediffs carries the marker (FR-026).</summary>
        public static bool IsExempt(Pawn p)
        {
            if (p.kindDef != null && p.kindDef.HasModExtension<RerollExemptExtension>())
                return true;
            var hediffs = p.health?.hediffSet?.hediffs;
            if (hediffs == null)
                return false;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].def.HasModExtension<RerollExemptExtension>())
                    return true;
            }
            return false;
        }

        /// <summary>Living, on the current map, can carry gear, category enabled, not an exempt boss.</summary>
        public static bool IsEligible(Pawn p)
        {
            if (p == null || p.Dead || !p.Spawned || p.Map != Find.CurrentMap)
                return false;
            if (!p.RaceProps.Humanlike || p.apparel == null || p.equipment == null)
                return false;
            var settings = DowngradeMod.Settings;
            if (!settings.AllowsCategory(CategoryOf(p)))
                return false;
            return settings.allowNamedBosses || !IsExempt(p);
        }
    }
}
