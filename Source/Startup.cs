using System.Linq;
using Verse;

namespace Downgrade
{
    /// <summary>Runs once after all defs are loaded: detect other mods, inject the comp, log.</summary>
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            CompatCE.Detect();
            CompatIO.Detect();
            CompatERP.Detect();
            CompatIsekai.Detect();

            // research R1: every humanlike race, so modded races get the button too.
            int races = 0;
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Generated corpse defs carry their pawn's race too; only real pawns get the comp (#1).
                if (def.race == null || !def.race.Humanlike || !typeof(Pawn).IsAssignableFrom(def.thingClass))
                    continue;
                def.comps ??= new System.Collections.Generic.List<CompProperties>();
                if (!def.comps.Any(c => c is CompProperties_Downgrade))
                {
                    var props = new CompProperties_Downgrade();
                    props.ResolveReferences(def);
                    def.comps.Add(props);
                }
                races++;
            }

            DowngradeMod.Settings.SeedTraitCostsIfEmpty();

            var version = typeof(Startup).Assembly.GetName().Version;
            DLog.Loaded($"{version.Major}.{version.Minor}.{version.Build}", races,
                CompatCE.Active, CompatIO.Active, CompatERP.Active, CompatIsekai.Active);
            GearPools.LogCounts();
        }
    }
}
