using Verse;

namespace RW_Upgrader
{
    public class CompProperties_Upgradable : CompProperties
    {
        public float resourceCostFractionPerTier = 0.3f;

        public float workFractionPerTier = 0.5f;

        public CompProperties_Upgradable()
        {
            compClass = typeof(CompUpgradable);
        }
    }
}
