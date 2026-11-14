using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Era rules for items and materials (data-model.md "Era", research R3/R6).</summary>
    public static class Eras
    {
        public static readonly TechLevel[] OfferedEras =
        {
            TechLevel.Neolithic, TechLevel.Medieval, TechLevel.Industrial, TechLevel.Spacer, TechLevel.Ultra
        };

        /// <summary>An item with no era label counts as this (FR-006).</summary>
        public const TechLevel UnlabeledEra = TechLevel.Spacer;

        private static DowngradeSettings Settings => DowngradeMod.Settings;

        private static Dictionary<string, TechLevel> stuffTable;
        private static List<ThingDef> unlabeledItems;
        private static List<ThingDef> unknownStuffs;

        public static string Label(TechLevel era) => era.ToStringHuman().CapitalizeFirst();

        // ---------- Items ----------

        /// <summary>The def's own era with no player override: techLevel, with Undefined as Spacer.</summary>
        public static TechLevel DefaultEra(ThingDef def)
        {
            var tl = def.techLevel;
            if (tl == TechLevel.Undefined)
                return UnlabeledEra;
            if (tl == TechLevel.Animal)
                return TechLevel.Neolithic;
            return tl; // Archotech stays Archotech: above every offered era.
        }

        public static TechLevel EffectiveEra(ThingDef def)
        {
            if (Settings.itemEraOverrides.TryGetValue(def.defName, out var era))
                return era;
            return DefaultEra(def);
        }

        public static bool IsAbove(ThingDef def, TechLevel era) => EffectiveEra(def) > era;

        public static bool IsGearCandidateDef(ThingDef def) =>
            def.IsApparel || (def.equipmentType == EquipmentType.Primary && !def.weaponTags.NullOrEmpty());

        /// <summary>Weapons and apparel that declare no tech level, for the settings window.</summary>
        public static List<ThingDef> UnlabeledItems()
        {
            unlabeledItems ??= DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.techLevel == TechLevel.Undefined && IsGearCandidateDef(d))
                .OrderBy(d => d.label ?? d.defName)
                .ToList();
            return unlabeledItems;
        }

        // ---------- Materials ----------

        private static Dictionary<string, TechLevel> StuffTable
        {
            get
            {
                if (stuffTable != null)
                    return stuffTable;
                stuffTable = new Dictionary<string, TechLevel>();
                var def = DowngradeDefOf.Downgrade_MaterialEras;
                // Lowest era wins, so add from Ultra down.
                Add(def.ultra, TechLevel.Ultra);
                Add(def.spacer, TechLevel.Spacer);
                Add(def.industrial, TechLevel.Industrial);
                Add(def.medieval, TechLevel.Medieval);
                Add(def.neolithic, TechLevel.Neolithic);
                return stuffTable;
            }
        }

        private static void Add(List<string> names, TechLevel era)
        {
            if (names == null)
                return;
            foreach (var n in names)
                stuffTable[n] = era;
        }

        public static bool InMaterialTable(ThingDef stuff) => StuffTable.ContainsKey(stuff.defName);

        public static TechLevel GuessStuffEra(ThingDef stuff)
        {
            var t = DowngradeDefOf.Downgrade_MaterialEras.priceThresholds;
            float v = stuff.BaseMarketValue;
            if (v <= t.neolithic) return TechLevel.Neolithic;
            if (v <= t.medieval) return TechLevel.Medieval;
            if (v <= t.industrial) return TechLevel.Industrial;
            if (v <= t.spacer) return TechLevel.Spacer;
            return TechLevel.Ultra;
        }

        /// <summary>The era without the player's override: the material table, else the price guess.</summary>
        public static TechLevel DefaultStuffEra(ThingDef stuff) =>
            StuffTable.TryGetValue(stuff.defName, out var era) ? era : GuessStuffEra(stuff);

        public static TechLevel StuffEra(ThingDef stuff)
        {
            if (Settings.stuffEraOverrides.TryGetValue(stuff.defName, out var era))
                return era;
            return DefaultStuffEra(stuff);
        }

        /// <summary>FR-011. Items not made from stuff are always allowed here.</summary>
        public static bool StuffAllowed(ThingDef stuff, TechLevel era) => stuff == null || StuffEra(stuff) <= era;

        /// <summary>Stuff in no material-table list, for the settings window.</summary>
        public static List<ThingDef> UnknownStuffs()
        {
            unknownStuffs ??= DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.IsStuff && !InMaterialTable(d))
                .OrderBy(d => d.label ?? d.defName)
                .ToList();
            return unknownStuffs;
        }
    }
}
