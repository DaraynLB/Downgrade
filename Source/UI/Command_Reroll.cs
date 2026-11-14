using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Downgrade
{
    /// <summary>The Reroll gizmo (contracts/ui-contract.md "Reroll command").</summary>
    [StaticConstructorOnStartup]
    public class Command_Reroll : Command
    {
        private static readonly Texture2D IconTex = ContentFinder<Texture2D>.Get("UI/Commands/Downgrade_Reroll");

        public readonly Pawn pawn;

        private static DowngradeSettings Settings => DowngradeMod.Settings;

        public Command_Reroll(Pawn pawn)
        {
            this.pawn = pawn;
            defaultLabel = "Downgrade.Command.Label".Translate();
            defaultDesc = "Downgrade.Command.Desc".Translate(Eras.Label(Settings.lastPickedEra));
            icon = IconTex;
        }

        // Research R2: every selected pawn's button groups into one. The game calls ProcessInput on the
        // clicked button and then ProcessGroupInput with the whole group (a group of one for a single
        // pawn), so the era menu opens there, once. Siblings must not run ProcessInput themselves.
        public override bool GroupsWith(Gizmo other) => other is Command_Reroll;

        public override bool InheritInteractionsFrom(Gizmo other) => false;

        public override void ProcessGroupInput(Event ev, List<Gizmo> group)
        {
            base.ProcessGroupInput(ev, group);
            var pawns = new List<Pawn> { pawn };
            if (group != null)
            {
                foreach (var g in group)
                {
                    if (g is Command_Reroll c && c.pawn != null && !pawns.Contains(c.pawn))
                        pawns.Add(c.pawn);
                }
            }
            OpenEraMenu(pawns);
        }

        public static void OpenEraMenu(List<Pawn> pawns)
        {
            var options = new List<FloatMenuOption>();
            foreach (var era in Eras.OfferedEras)
            {
                var label = Eras.Label(era);
                if (era == Settings.lastPickedEra)
                    label += " " + "Downgrade.LastUsed".Translate();
                var chosen = era;
                options.Add(new FloatMenuOption(label, () => Run(pawns, chosen)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public static void Run(List<Pawn> pawns, TechLevel era)
        {
            // FR-002: remembered across saves, reloads and new games.
            Settings.lastPickedEra = era;
            DowngradeMod.Instance.WriteSettings();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            int rerolled = 0, skipped = 0;
            var lastOutcome = RerollOutcome.Failed;
            foreach (var p in pawns)
            {
                // FR-019: one pawn's failure never stops the others. Execute already catches its own
                // failures; this is the backstop for anything it doesn't.
                try
                {
                    lastOutcome = RerollExecutor.Execute(p, era);
                }
                catch (Exception e)
                {
                    DLog.RerollFailed(p, e.Message);
                    lastOutcome = RerollOutcome.Failed;
                }
                if (lastOutcome == RerollOutcome.Rerolled)
                    rerolled++;
                else
                    skipped++;
            }

            // US2 scenario 2: selected pawns with no Reroll button (animals, mechanoids, disabled
            // categories) are counted as skipped too.
            var batch = new HashSet<Pawn>(pawns);
            int noButton = Find.Selector.SelectedPawns.Count(p => !batch.Contains(p));
            skipped += noButton;
            DLog.Batch(era, rerolled, skipped, noButton, watch.ElapsedMilliseconds);

            string text;
            if (pawns.Count == 1 && noButton == 0 && lastOutcome == RerollOutcome.NothingAbove)
            {
                text = "Downgrade.Result.NothingAbove".Translate(pawns[0].LabelShort, Eras.Label(era));
            }
            else
            {
                text = "Downgrade.Result.Rerolled".Translate(rerolled, Eras.Label(era));
                if (skipped > 0)
                    text += " " + "Downgrade.Result.Skipped".Translate(skipped).Resolve();
            }
            Messages.Message(text, MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }
}
