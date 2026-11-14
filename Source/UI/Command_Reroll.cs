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

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            OpenEraMenu(new List<Pawn> { pawn });
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
            int rerolled = 0, skipped = 0;
            foreach (var p in pawns)
            {
                var outcome = RerollExecutor.Execute(p, era);
                if (outcome == RerollOutcome.Rerolled)
                    rerolled++;
                else
                    skipped++;
            }

            string text = pawns.Count == 1 && rerolled == 0
                ? "Downgrade.Result.NothingAbove".Translate(pawns[0].LabelShort, Eras.Label(era)).ToString()
                : "Downgrade.Result.Rerolled".Translate(rerolled, Eras.Label(era)).ToString();
            Messages.Message(text, MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }
}
