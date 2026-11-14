using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using Verse;

namespace Downgrade
{
    /// <summary>Dev Mode tools for checking budget math from Player.log (plan.md "Testing").</summary>
    public static class DebugActions
    {
        [DebugAction("Downgrade", "Dry-run reroll", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DryRun(Pawn p)
        {
            if (p?.RaceProps?.Humanlike != true)
                return;
            var options = Eras.OfferedEras
                .Select(era => new FloatMenuOption(Eras.Label(era), () => DLog.DryRun(RerollPlanner.Plan(p, era))))
                .ToList();
            Find.WindowStack.Add(new FloatMenu(options));
        }

        [DebugAction("Downgrade", "Toggle forced reroll failure", allowedGameStates = AllowedGameStates.Playing)]
        private static void ToggleForcedFailure()
        {
            RerollExecutor.debugFailAfterFirstPut = !RerollExecutor.debugFailAfterFirstPut;
            Messages.Message("Downgrade forced failure: " + (RerollExecutor.debugFailAfterFirstPut ? "ON" : "off"),
                RimWorld.MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }
}
