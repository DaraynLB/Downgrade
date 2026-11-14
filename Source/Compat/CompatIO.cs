using Verse;

namespace Downgrade
{
    /// <summary>Itemization Overhaul – Core. Runtime-detected only; no compile-time reference (Principle II).</summary>
    public static class CompatIO
    {
        public const string PackageId = "astryl.ItemizationOverhaulCore";

        public static bool Active { get; private set; }

        public static void Detect()
        {
            Active = ModsConfig.IsActive(PackageId);
        }

        /// <summary>True when an item must be left alone even if it's above the era. Filled in by US6.</summary>
        public static bool IsSkipped(Thing t) => false;
    }
}
