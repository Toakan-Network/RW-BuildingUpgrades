using System;
using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public static class ZoneUpgradeUtility
    {
        public static Zone_UpgradeStandard GetGoverningZone(Thing t)
        {
            if (t?.Map == null)
            {
                return null;
            }

            foreach (IntVec3 cell in t.OccupiedRect().Cells)
            {
                if (t.Map.zoneManager.ZoneAt(cell) is Zone_UpgradeStandard zone)
                {
                    return zone;
                }
            }

            foreach (IntVec3 cell in t.OccupiedRect().ExpandedBy(1).Cells)
            {
                if (t.Map.zoneManager.ZoneAt(cell) is Zone_UpgradeStandard zone)
                {
                    return zone;
                }
            }

            return null;
        }

        public static bool BelowZoneQualityMinimum(Thing t, Zone_UpgradeStandard zone)
        {
            if (zone.minQualityTier < 0)
            {
                return false;
            }
            CompQuality q = t.TryGetComp<CompQuality>();
            return q != null && (int)q.Quality < zone.minQualityTier;
        }

        public static bool BelowZoneMaterialMinimum(Thing t, Zone_UpgradeStandard zone)
        {
            if (t.Stuff == null)
            {
                return false;
            }
            int minTier = MaterialUpgradeUtility.IsFurniture(t) ? zone.minFurnitureMaterialTier : zone.minBuildingMaterialTier;
            if (minTier < 0)
            {
                return false;
            }
            int? currentTier = MaterialUpgradeUtility.GetTier(t.Stuff);
            return currentTier != null && currentTier < minTier;
        }

        public static int EffectiveMaxQualityTier(Thing t)
        {
            Zone_UpgradeStandard zone = GetGoverningZone(t);
            return Math.Max(RW_UpgraderMod.Settings.maxQualityTier, zone?.minQualityTier ?? -1);
        }

        public static int EffectiveMaxMaterialTier(Thing t)
        {
            Zone_UpgradeStandard zone = GetGoverningZone(t);
            bool isFurniture = MaterialUpgradeUtility.IsFurniture(t);
            int globalMax = isFurniture ? RW_UpgraderMod.Settings.maxFurnitureMaterialTier : RW_UpgraderMod.Settings.maxBuildingMaterialTier;
            int zoneMin = (isFurniture ? zone?.minFurnitureMaterialTier : zone?.minBuildingMaterialTier) ?? -1;
            return Math.Max(globalMax, zoneMin);
        }
    }
}
