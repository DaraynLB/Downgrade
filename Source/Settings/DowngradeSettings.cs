using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Global options (data-model.md "Settings"). Defaults mirror PRD §4.6.</summary>
    public class DowngradeSettings : ModSettings
    {
        // Who can be rerolled
        public bool allowHostile = true;
        public bool allowVisitors;
        public bool allowTraders;
        public bool allowPrisoners;
        public bool allowColonists;
        public bool allowNamedBosses;

        // Leftover budget. qualityCapAuto = "Automatic (Masterwork, or Excellent with Itemization Overhaul)".
        public bool qualityCapAuto = true;
        public QualityCategory qualityCap = QualityCategory.Masterwork;
        public int maxTraitsAdded = 2;

        /// <summary>Key "defName:degree". Seeded from Downgrade_StarterTraits.</summary>
        public Dictionary<string, TraitCostEntry> traitCosts = new Dictionary<string, TraitCostEntry>();

        // Compatibility
        public bool rerollEliteRaidBionics = true;
        public bool eraCheckJewelry;

        // Era assignments. Unknown defNames (a mod was removed) are kept but ignored.
        public Dictionary<string, TechLevel> itemEraOverrides = new Dictionary<string, TechLevel>();
        public Dictionary<string, TechLevel> stuffEraOverrides = new Dictionary<string, TechLevel>();
        public Dictionary<string, bool> combatDrugOverrides = new Dictionary<string, bool>();

        public TechLevel lastPickedEra = TechLevel.Medieval;

        public QualityCategory EffectiveQualityCap =>
            qualityCapAuto ? (CompatIO.Active ? QualityCategory.Excellent : QualityCategory.Masterwork) : qualityCap;

        public bool AllowsCategory(PawnCategory category)
        {
            switch (category)
            {
                case PawnCategory.Colonist: return allowColonists;
                case PawnCategory.Prisoner: return allowPrisoners;
                case PawnCategory.Hostile: return allowHostile;
                case PawnCategory.Trader: return allowTraders;
                default: return allowVisitors;
            }
        }

        public static string TraitKey(TraitDef def, int degree) => def.defName + ":" + degree;

        /// <summary>
        /// Resolves a <see cref="TraitKey"/>. False when the trait no longer exists (its mod was removed)
        /// or has no such degree; the entry is kept in settings but ignored (data-model Validation).
        /// </summary>
        public static bool TryParseTraitKey(string key, out TraitDef def, out int degree)
        {
            def = null;
            degree = 0;
            int colon = key?.LastIndexOf(':') ?? -1;
            if (colon <= 0 || !int.TryParse(key.Substring(colon + 1), out degree))
                return false;
            def = DefDatabase<TraitDef>.GetNamedSilentFail(key.Substring(0, colon));
            int d = degree;
            // Not DataAtDegree: it logs an error and falls back to the first degree when missing.
            return def?.degreeDatas != null && def.degreeDatas.Any(x => x.degree == d);
        }

        public void SeedTraitCostsIfEmpty()
        {
            if (traitCosts.Count > 0)
                return;
            foreach (var t in DowngradeDefOf.Downgrade_StarterTraits.traits)
            {
                if (t.trait == null)
                    continue;
                traitCosts[TraitKey(t.trait, t.degree)] = new TraitCostEntry { enabled = true, cost = t.cost };
            }
        }

        public void ResetTraitCosts()
        {
            traitCosts.Clear();
            SeedTraitCostsIfEmpty();
        }

        public void ResetToDefaults()
        {
            allowHostile = true;
            allowVisitors = allowTraders = allowPrisoners = allowColonists = false;
            allowNamedBosses = false;
            qualityCapAuto = true;
            qualityCap = QualityCategory.Masterwork;
            maxTraitsAdded = 2;
            rerollEliteRaidBionics = true;
            eraCheckJewelry = false;
            itemEraOverrides.Clear();
            stuffEraOverrides.Clear();
            combatDrugOverrides.Clear();
            ResetTraitCosts();
            // lastPickedEra is a remembered choice, not a setting: left as is.
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowHostile, "allowHostile", true);
            Scribe_Values.Look(ref allowVisitors, "allowVisitors", false);
            Scribe_Values.Look(ref allowTraders, "allowTraders", false);
            Scribe_Values.Look(ref allowPrisoners, "allowPrisoners", false);
            Scribe_Values.Look(ref allowColonists, "allowColonists", false);
            Scribe_Values.Look(ref allowNamedBosses, "allowNamedBosses", false);
            Scribe_Values.Look(ref qualityCapAuto, "qualityCapAuto", true);
            Scribe_Values.Look(ref qualityCap, "qualityCap", QualityCategory.Masterwork);
            Scribe_Values.Look(ref maxTraitsAdded, "maxTraitsAdded", 2);
            Scribe_Collections.Look(ref traitCosts, "traitCosts", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref rerollEliteRaidBionics, "rerollEliteRaidBionics", true);
            Scribe_Values.Look(ref eraCheckJewelry, "eraCheckJewelry", false);
            Scribe_Collections.Look(ref itemEraOverrides, "itemEraOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref stuffEraOverrides, "stuffEraOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref combatDrugOverrides, "combatDrugOverrides", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref lastPickedEra, "lastPickedEra", TechLevel.Medieval);

            if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                traitCosts ??= new Dictionary<string, TraitCostEntry>();
                itemEraOverrides ??= new Dictionary<string, TechLevel>();
                stuffEraOverrides ??= new Dictionary<string, TechLevel>();
                combatDrugOverrides ??= new Dictionary<string, bool>();
            }
        }
    }

    public class TraitCostEntry : IExposable
    {
        public bool enabled = true;
        public int cost;

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref cost, "cost", 0);
        }
    }
}
