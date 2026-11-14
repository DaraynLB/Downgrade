using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Per-pawn saved reroll state (data-model.md "RerollRecord", research R9).</summary>
    public class RerollRecord : IExposable
    {
        public bool hasRecord;
        public float weaponBudget;
        public float apparelBudget;
        public float implantBudget;

        /// <summary>Items Downgrade gave on the last reroll; taken away and bought again on the next.</summary>
        public List<Thing> givenThings = new List<Thing>();

        /// <summary>Era body parts Downgrade installed on the last reroll.</summary>
        public List<GivenHediff> givenHediffs = new List<GivenHediff>();

        /// <summary>Traits Downgrade added; the only traits it ever removes.</summary>
        public List<AddedTrait> addedTraits = new List<AddedTrait>();

        public TechLevel lastEra = TechLevel.Undefined;

        public float Budget(GearCategory c)
        {
            switch (c)
            {
                case GearCategory.Weapon: return weaponBudget;
                case GearCategory.Apparel: return apparelBudget;
                default: return implantBudget;
            }
        }

        /// <summary>
        /// Drops given items the pawn no longer wears, holds or carries. They aren't destroyed (the
        /// player may have taken them), and dropping them keeps save references from dangling.
        /// </summary>
        public void PruneGivenThings(Pawn pawn)
        {
            givenThings.RemoveAll(t => t == null || t.Destroyed || !IsCarriedBy(t, pawn));
        }

        public static bool IsCarriedBy(Thing t, Pawn pawn)
        {
            if (t is Apparel ap && pawn.apparel != null && pawn.apparel.Contains(ap))
                return true;
            if (t is ThingWithComps eq && pawn.equipment != null && pawn.equipment.Contains(eq))
                return true;
            return pawn.inventory != null && pawn.inventory.innerContainer.Contains(t);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref hasRecord, "hasRecord", false);
            Scribe_Values.Look(ref weaponBudget, "weaponBudget", 0f);
            Scribe_Values.Look(ref apparelBudget, "apparelBudget", 0f);
            Scribe_Values.Look(ref implantBudget, "implantBudget", 0f);
            Scribe_Collections.Look(ref givenThings, "givenThings", LookMode.Reference);
            Scribe_Collections.Look(ref givenHediffs, "givenHediffs", LookMode.Deep);
            Scribe_Collections.Look(ref addedTraits, "addedTraits", LookMode.Deep);
            Scribe_Values.Look(ref lastEra, "lastEra", TechLevel.Undefined);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                givenThings ??= new List<Thing>();
                givenHediffs ??= new List<GivenHediff>();
                addedTraits ??= new List<AddedTrait>();
                givenThings.RemoveAll(t => t == null);
                givenHediffs.RemoveAll(h => h?.def == null);
                addedTraits.RemoveAll(t => t?.def == null);
            }
        }
    }

    /// <summary>An era body part Downgrade installed. The part is stored as its body index.</summary>
    public class GivenHediff : IExposable
    {
        public HediffDef def;
        public int partIndex = -1;

        public BodyPartRecord Part(Pawn pawn) =>
            partIndex < 0 ? null : pawn.RaceProps.body.GetPartAtIndex(partIndex);

        public static GivenHediff From(Pawn pawn, HediffDef def, BodyPartRecord part) => new GivenHediff
        {
            def = def,
            partIndex = part == null ? -1 : pawn.RaceProps.body.GetIndexOfPart(part),
        };

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref partIndex, "partIndex", -1);
        }
    }

    public class AddedTrait : IExposable
    {
        public TraitDef def;
        public int degree;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref degree, "degree", 0);
        }
    }
}
