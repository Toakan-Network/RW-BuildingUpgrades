using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public enum AreaRestrictionMode { MapWide, Home, Custom }

    public class RW_UpgraderSettings : ModSettings
    {
        public bool autoEnableByDefault = false;
        public bool autoEnableMaterialByDefault = false;
        public AreaRestrictionMode areaRestrictionMode = AreaRestrictionMode.Home;
        public string customAreaLabel = "";
        public int maxQualityTier = (int)QualityCategory.Legendary;
        public int maxMaterialTier = 1;
        public float baseResourceCostPercent = 15f;
        public bool increaseCostPerTier = false;
        public Dictionary<string, string> allowedStuffsPerCategory = new Dictionary<string, string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref autoEnableByDefault, "autoEnableByDefault", false);
            Scribe_Values.Look(ref autoEnableMaterialByDefault, "autoEnableMaterialByDefault", false);
            Scribe_Values.Look(ref areaRestrictionMode, "areaRestrictionMode", AreaRestrictionMode.Home);
            Scribe_Values.Look(ref customAreaLabel, "customAreaLabel", "");
            Scribe_Values.Look(ref maxQualityTier, "maxQualityTier", (int)QualityCategory.Legendary);
            Scribe_Values.Look(ref maxMaterialTier, "maxMaterialTier", 1);
            Scribe_Values.Look(ref baseResourceCostPercent, "baseResourceCostPercent", 15f);
            Scribe_Values.Look(ref increaseCostPerTier, "increaseCostPerTier", false);
            Scribe_Collections.Look(ref allowedStuffsPerCategory, "allowedStuffsPerCategory", LookMode.Value, LookMode.Value);
            allowedStuffsPerCategory ??= new Dictionary<string, string>();
        }

        public bool IsStuffAllowed(string categoryDefName, string stuffDefName)
        {
            if (!allowedStuffsPerCategory.TryGetValue(categoryDefName, out string csv))
            {
                return true;
            }
            return csv.Split(',').Contains(stuffDefName);
        }

        public void SetStuffAllowed(string categoryDefName, string stuffDefName, bool allowed, IEnumerable<string> allMembersOfCategory)
        {
            HashSet<string> set = allowedStuffsPerCategory.TryGetValue(categoryDefName, out string csv)
                ? new HashSet<string>(csv.Split(',').Where(s => s.Length > 0))
                : new HashSet<string>(allMembersOfCategory);

            if (allowed)
            {
                set.Add(stuffDefName);
            }
            else
            {
                set.Remove(stuffDefName);
            }

            allowedStuffsPerCategory[categoryDefName] = string.Join(",", set);
        }
    }
}
