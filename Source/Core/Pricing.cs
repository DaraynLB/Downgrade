using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>
    /// Base values (FR-014, research R5). Always an abstract StatRequest with no Thing, so quality and
    /// rarity markups that other mods add to real items stay out of budgets.
    /// </summary>
    public static class Pricing
    {
        public static float Price(ThingDef def, ThingDef stuff, QualityCategory quality = QualityCategory.Normal)
        {
            if (def == null)
                return 0f;
            if (def.MadeFromStuff && stuff == null)
                stuff = GenStuff.DefaultStuffFor(def);
            return StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(def, stuff, quality));
        }

        /// <summary>The item's base value at Normal quality, times its stack size.</summary>
        public static float BaseValue(Thing t) => Price(t.def, t.Stuff) * t.stackCount;

        public static bool HasQuality(ThingDef def) => def.HasComp(typeof(CompQuality));

        /// <summary>Cost of raising one quality step from <paramref name="from"/>; infinite without quality.</summary>
        public static float QualityStepCost(ThingDef def, ThingDef stuff, QualityCategory from)
        {
            if (!HasQuality(def) || from >= QualityCategory.Legendary)
                return float.PositiveInfinity;
            return Price(def, stuff, from + 1) - Price(def, stuff, from);
        }
    }
}
