using System.Collections.Generic;
using Verse;

namespace Downgrade
{
    /// <summary>
    /// Materials allowed per era (PRD §4.4). A stuff is allowed in an era if it's listed at or below
    /// that era. Field names are a public contract (contracts/mod-extension-contract.md).
    /// </summary>
    public class MaterialEraDef : Def
    {
        public List<string> neolithic = new List<string>();
        public List<string> medieval = new List<string>();
        public List<string> industrial = new List<string>();
        public List<string> spacer = new List<string>();
        public List<string> ultra = new List<string>();

        /// <summary>Per-unit market value upper bounds used to guess an unknown material's era.</summary>
        public PriceThresholds priceThresholds = new PriceThresholds();
    }

    public class PriceThresholds
    {
        public float neolithic = 2.1f;
        public float medieval = 4.0f;
        public float industrial = 6.0f;
        public float spacer = 15.0f;
    }
}
