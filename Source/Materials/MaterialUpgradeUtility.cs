using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public static class MaterialUpgradeUtility
    {
        public static int? GetTier(ThingDef stuffDef)
        {
            if (stuffDef?.stuffProps == null)
            {
                return null;
            }

            int? best = null;
            foreach (StuffCategoryDef category in stuffDef.stuffProps.categories)
            {
                MaterialTierExtension ext = category.GetModExtension<MaterialTierExtension>();
                if (ext != null && (best == null || ext.tier > best))
                {
                    best = ext.tier;
                }
            }
            return best;
        }

        public static StuffCategoryDef GetNextTierCategory(int currentTier)
        {
            return DefDatabase<StuffCategoryDef>.AllDefsListForReading
                .FirstOrDefault(c => c.GetModExtension<MaterialTierExtension>()?.tier == currentTier + 1);
        }

        public static bool CanUpgradeMaterial(Thing t)
        {
            if (!(t is Building) || !t.def.MadeFromStuff || t.Stuff == null)
            {
                return false;
            }

            int? currentTier = GetTier(t.Stuff);
            if (currentTier == null)
            {
                return false;
            }

            return GetNextTierCategory(currentTier.Value) != null;
        }

        public static List<ThingDefCountClass> RequiredResourcesForSwap(Thing t, ThingDef newStuff)
        {
            List<ThingDefCountClass> result = new List<ThingDefCountClass>();

            if (t.def.costList != null)
            {
                foreach (ThingDefCountClass cost in t.def.costList)
                {
                    result.Add(new ThingDefCountClass(cost.thingDef, cost.count));
                }
            }

            if (t.def.costStuffCount > 0)
            {
                result.Add(new ThingDefCountClass(newStuff, t.def.costStuffCount));
            }

            return result;
        }

        public static float WorkAmount(Thing t, ThingDef newStuff)
        {
            return t.def.GetStatValueAbstract(StatDefOf.WorkToBuild, newStuff);
        }

        public static ThingDef PickBestStuff(Thing t, Pawn pawn)
        {
            int? currentTier = GetTier(t.Stuff);
            if (currentTier == null)
            {
                return null;
            }

            StuffCategoryDef nextCategory = GetNextTierCategory(currentTier.Value);
            if (nextCategory == null)
            {
                return null;
            }

            IEnumerable<ThingDef> candidates = GenStuff.AllowedStuffsFor(t.def)
                .Where(s => s.stuffProps.categories.Contains(nextCategory))
                .Where(s => RW_UpgraderMod.Settings.IsStuffAllowed(nextCategory.defName, s.defName))
                .OrderByDescending(s => s.GetStatValueAbstract(StatDefOf.MarketValue));

            foreach (ThingDef candidate in candidates)
            {
                List<ThingDefCountClass> required = RequiredResourcesForSwap(t, candidate);
                List<LocalTargetInfo> foundThings = new List<LocalTargetInfo>();
                List<int> foundCounts = new List<int>();
                if (UpgradeUtility.TryFindIngredients(pawn, required, foundThings, foundCounts))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
