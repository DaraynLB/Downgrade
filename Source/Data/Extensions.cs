using Verse;

namespace Downgrade
{
    /// <summary>
    /// Marks a named boss or hero. Allowed on PawnKindDef and HediffDef: a pawn is exempt if its kind
    /// carries it, or any hediff it has does. Class name is a public contract; don't rename.
    /// </summary>
    public class RerollExemptExtension : DefModExtension
    {
    }

    /// <summary>
    /// Marks an apparel ThingDef as jewelry or a charm: left alone unless "Era-check jewelry and
    /// charms" is on. Class name is a public contract; don't rename.
    /// </summary>
    public class JewelryExtension : DefModExtension
    {
    }
}
