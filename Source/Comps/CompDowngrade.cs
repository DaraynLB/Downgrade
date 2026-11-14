using System.Collections.Generic;
using Verse;

namespace Downgrade
{
    public class CompProperties_Downgrade : CompProperties
    {
        public CompProperties_Downgrade()
        {
            compClass = typeof(CompDowngrade);
        }
    }

    /// <summary>
    /// Injected into every humanlike race (research R1). Provides the Reroll gizmo, which RimWorld shows
    /// on any selected pawn regardless of faction, and saves the pawn's reroll record.
    /// </summary>
    public class CompDowngrade : ThingComp
    {
        public RerollRecord record = new RerollRecord();

        /// <summary>Null if the comp ever ends up on something that isn't a pawn (#1).</summary>
        public Pawn Pawn => parent as Pawn;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var pawn = Pawn;
            if (pawn != null && Eligibility.IsEligible(pawn))
                yield return new Command_Reroll(pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (Pawn != null)
                    record.PruneGivenThings(Pawn);
                if (!record.hasRecord)
                    return; // keep saves clean for pawns never rerolled
            }
            Scribe_Deep.Look(ref record, "downgradeRecord");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && record == null)
                record = new RerollRecord();
        }
    }
}
