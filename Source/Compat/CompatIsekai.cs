namespace Downgrade
{
    /// <summary>
    /// Isekai RPG Leveling. Detected only for the startup log: Downgrade never touches levels, stats,
    /// ranks or rank traits (FR-025), because it only removes traits it added itself and only
    /// rerolls hediffs that come from an installable item.
    /// </summary>
    public static class CompatIsekai
    {
        public static bool Active { get; private set; }

        public static void Detect()
        {
            Active = ModDetect.FindByName("Isekai") != null;
        }
    }
}
