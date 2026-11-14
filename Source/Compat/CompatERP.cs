using Verse;

namespace Downgrade
{
    /// <summary>Elite Raid Plus. Detected by mod name (open check C3: its packageId isn't confirmed yet).</summary>
    public static class CompatERP
    {
        public static bool Active => Mod != null;

        public static ModContentPack Mod { get; private set; }

        public static void Detect()
        {
            Mod = ModDetect.FindByName("Elite Raid");
        }
    }

    /// <summary>Finds a running mod by a fragment of its name, for mods whose packageId isn't confirmed.</summary>
    internal static class ModDetect
    {
        public static ModContentPack FindByName(string fragment)
        {
            foreach (var mod in LoadedModManager.RunningModsListForReading)
            {
                if (mod.Name != null && mod.Name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return mod;
            }
            return null;
        }
    }
}
