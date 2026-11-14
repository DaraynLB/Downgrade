using HarmonyLib;
using UnityEngine;
using Verse;

namespace Downgrade
{
    public class DowngradeMod : Mod
    {
        public const string HarmonyId = "daraynlb.downgrade";

        public static DowngradeMod Instance { get; private set; }

        public static DowngradeSettings Settings { get; private set; }

        public DowngradeMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<DowngradeSettings>();
            // No patches in this feature; Harmony stays a declared dependency so later
            // compatibility patches work (plan.md Complexity Tracking).
            new Harmony(HarmonyId).PatchAll();
        }

        public override string SettingsCategory() => "Downgrade";
    }
}
