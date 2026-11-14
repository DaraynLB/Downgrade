using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Every Downgrade log line, in the stable formats of contracts/log-contract.md.</summary>
    public static class DLog
    {
        private const string Prefix = "[Downgrade] ";
        private static readonly HashSet<string> warnedModules = new HashSet<string>();

        private static string OnOff(bool b) => b ? "on" : "off";

        public static void Loaded(string version, int races, bool ce, bool io, bool erp, bool isekai) =>
            Log.Message($"{Prefix}Loaded v{version}. Comp added to {races} humanlike races. " +
                        $"CE={OnOff(ce)} IO={OnOff(io)} ERP={OnOff(erp)} Isekai={OnOff(isekai)}.");

        public static void Pools(int weapons, int apparel, int implants, int unlabeled, int unknownMaterials) =>
            Log.Message($"{Prefix}Pools: {weapons} weapons, {apparel} apparel, {implants} implants. " +
                        $"Unlabeled items: {unlabeled}. Unknown materials: {unknownMaterials}.");

        public static string PawnId(Pawn p) =>
            $"{p.LabelShort} ({p.kindDef?.defName ?? "?"}, {p.Faction?.Name ?? "no faction"})";

        public static void Reroll(RerollPlan plan) =>
            Log.Message($"{Prefix}Reroll {PawnId(plan.pawn)} to {Eras.Label(plan.era)}: {plan.Describe()}.");

        public static void DryRun(RerollPlan plan) =>
            Log.Message($"{Prefix}Dry run {PawnId(plan.pawn)} to {Eras.Label(plan.era)}: " +
                        (plan.NothingToDo ? $"nothing above {Eras.Label(plan.era)}." : plan.Describe() + "."));

        public static void Skip(Pawn p, TechLevel era) =>
            Log.Message($"{Prefix}Skip {p.LabelShort}: nothing above {Eras.Label(era)}.");

        public static void RerollFailed(Pawn p, string reason) =>
            Log.Warning($"{Prefix}Reroll failed for {p.LabelShort}, left unchanged: {reason}");

        public static void CeAmmo(ThingDef ammo, int count, ThingDef weapon) =>
            Log.Message($"{Prefix}CE ammo {ammo.defName} x{count} for {weapon.defName}.");

        /// <summary>Once per module per session.</summary>
        public static void CompatDisabled(string module, string reason)
        {
            if (warnedModules.Add(module))
                Log.Warning($"{Prefix}{module} compat disabled: {reason}");
        }

        /// <summary>Downgrade's own invariant broke (for example, putting a pawn's original gear back failed).</summary>
        public static void InvariantBroken(string what) => Log.Error($"{Prefix}{what}");
    }
}
