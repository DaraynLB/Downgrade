using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Downgrade
{
    /// <summary>Starter trait whitelist with silver costs (PRD §4.5). Seeds the settings on first run.</summary>
    public class TraitWhitelistDef : Def
    {
        public List<StarterTrait> traits = new List<StarterTrait>();
    }

    public class StarterTrait
    {
        public TraitDef trait;
        public int degree;
        public int cost;
    }
}
