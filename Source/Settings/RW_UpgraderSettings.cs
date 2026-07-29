using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public enum AreaRestrictionMode { MapWide, Home, Custom }

    public class RW_UpgraderSettings : ModSettings
    {
        public bool autoEnableByDefault = false;
        public AreaRestrictionMode areaRestrictionMode = AreaRestrictionMode.Home;
        public string customAreaLabel = "";
        public int maxQualityTier = (int)QualityCategory.Legendary;
        public float baseResourceCostPercent = 15f;
        public bool increaseCostPerTier = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref autoEnableByDefault, "autoEnableByDefault", false);
            Scribe_Values.Look(ref areaRestrictionMode, "areaRestrictionMode", AreaRestrictionMode.Home);
            Scribe_Values.Look(ref customAreaLabel, "customAreaLabel", "");
            Scribe_Values.Look(ref maxQualityTier, "maxQualityTier", (int)QualityCategory.Legendary);
            Scribe_Values.Look(ref baseResourceCostPercent, "baseResourceCostPercent", 15f);
            Scribe_Values.Look(ref increaseCostPerTier, "increaseCostPerTier", false);
        }
    }
}
