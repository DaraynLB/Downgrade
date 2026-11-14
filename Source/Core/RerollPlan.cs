using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Downgrade
{
    public enum GearCategory
    {
        Weapon,
        Apparel,
        Implant,
    }

    public enum GearSlot
    {
        Equipment,
        Apparel,
        Inventory,
    }

    /// <summary>A replacement item to make and give.</summary>
    public class PlannedThing
    {
        public ThingDef def;
        public ThingDef stuff;
        public QualityCategory quality = QualityCategory.Normal;
        public GearCategory category;
        public GearSlot slot;
        public Thing replaces;
        public int stackCount = 1;

        /// <summary>Set by the executor's create phase.</summary>
        public Thing made;

        public override string ToString()
        {
            var s = def.defName;
            if (stuff != null)
                s += "/" + stuff.defName;
            if (Pricing.HasQuality(def))
                s += "/" + quality;
            if (stackCount > 1)
                s += " x" + stackCount;
            return s;
        }
    }

    public class PlannedHediff
    {
        public HediffDef def;
        public BodyPartRecord part;
        public float price;
    }

    public class PlannedTrait
    {
        public TraitDef def;
        public int degree;
        public int cost;

        public string Label => def.DataAtDegree(degree)?.label ?? def.defName;
    }

    /// <summary>One category's budget, spent on a working copy (the record is never lowered).</summary>
    public class CategoryBudget
    {
        public float budget;
        public float gearSpent;
        public float qualitySpent;

        /// <summary>FR-016: cheapest item given without being affordable; the category has no leftover.</summary>
        public bool usedFallback;

        public float Remaining => Mathf.Max(0f, budget - gearSpent - qualitySpent);
        public float Leftover => usedFallback ? 0f : Remaining;
    }

    /// <summary>Everything one reroll of one pawn will do (data-model.md "RerollPlan"). Built by the planner.</summary>
    public class RerollPlan
    {
        public Pawn pawn;
        public TechLevel era;
        public bool firstReroll;

        public List<Thing> removeThings = new List<Thing>();
        public List<Hediff> removeHediffs = new List<Hediff>();

        /// <summary>Parts that must end natural; RestorePart only if still missing after removal (research R8).</summary>
        public List<BodyPartRecord> restoreParts = new List<BodyPartRecord>();

        public List<Trait> removeTraits = new List<Trait>();
        public List<PlannedThing> addThings = new List<PlannedThing>();
        public List<PlannedHediff> addHediffs = new List<PlannedHediff>();
        public List<PlannedTrait> addTraits = new List<PlannedTrait>();

        public readonly Dictionary<GearCategory, CategoryBudget> budgets = new Dictionary<GearCategory, CategoryBudget>
        {
            { GearCategory.Weapon, new CategoryBudget() },
            { GearCategory.Apparel, new CategoryBudget() },
            { GearCategory.Implant, new CategoryBudget() },
        };

        public float traitPool;
        public float traitSpent;
        public float discarded;

        /// <summary>Nothing above the era and nothing Downgrade gave last time: the pawn is skipped.</summary>
        public bool NothingToDo;

        public CategoryBudget Budget(GearCategory c) => budgets[c];

        /// <summary>The bracketed part of the "Each pawn rerolled" log line (contracts/log-contract.md).</summary>
        public string Describe()
        {
            string Cat(string key, GearCategory c)
            {
                var b = budgets[c];
                return $"{key}={b.budget:0} (gear {b.gearSpent:0}, quality {b.qualitySpent:0})";
            }

            var removed = removeThings.Select(t => t.def.defName)
                .Concat(removeHediffs.Select(h => h.def.defName))
                .Concat(removeTraits.Select(t => "trait:" + t.def.defName));
            var added = addThings.Select(p => p.ToString())
                .Concat(addHediffs.Select(h => h.def.defName + (h.part != null ? "@" + h.part.Label : "")))
                .Concat(restoreParts.Select(p => "natural@" + p.Label));
            var traits = addTraits.Select(t => $"{t.Label} {t.cost}");

            return $"budget {Cat("W", GearCategory.Weapon)} {Cat("A", GearCategory.Apparel)} {Cat("I", GearCategory.Implant)}; " +
                   $"removed [{string.Join(", ", removed)}]; added [{string.Join(", ", added)}]; " +
                   $"traits [{string.Join(", ", traits)}] from pool {traitPool:0}; discarded {discarded:0}";
        }
    }
}
