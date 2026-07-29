using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public static class UpgradeUtility
    {
        public static CompUpgradable GetUpgradable(Thing t)
        {
            if (!(t is Building) || !(t is ThingWithComps))
            {
                return null;
            }
            return t.TryGetComp<CompUpgradable>();
        }

        public static bool PawnCanUpgradeNow(Pawn pawn, Thing t)
        {
            CompUpgradable upgradable = GetUpgradable(t);
            if (upgradable == null || !upgradable.CanUpgrade)
            {
                return false;
            }
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }
            if (pawn.skills == null || pawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled)
            {
                return false;
            }
            if (!pawn.CanReserve(t))
            {
                return false;
            }
            return true;
        }

        public static List<ThingDefCountClass> RequiredResources(Thing t)
        {
            CompUpgradable upgradable = GetUpgradable(t);
            int qualityOrdinal = (int)(t.TryGetComp<CompQuality>()?.Quality ?? QualityCategory.Normal);
            float tierMultiplier = RW_UpgraderMod.Settings.increaseCostPerTier ? (qualityOrdinal + 1) : 1;
            float fraction = (RW_UpgraderMod.Settings.baseResourceCostPercent / 100f) * tierMultiplier * (upgradable?.Props.resourceCostFractionPerTier ?? 1f);
            List<ThingDefCountClass> result = new List<ThingDefCountClass>();

            if (t.def.costList != null)
            {
                foreach (ThingDefCountClass cost in t.def.costList)
                {
                    int count = ScaledCount(cost.count, fraction);
                    if (count > 0)
                    {
                        result.Add(new ThingDefCountClass(cost.thingDef, count));
                    }
                }
            }

            if (t.def.MadeFromStuff && t.Stuff != null && t.def.costStuffCount > 0)
            {
                int count = ScaledCount(t.def.costStuffCount, fraction);
                if (count > 0)
                {
                    result.Add(new ThingDefCountClass(t.Stuff, count));
                }
            }

            return result;
        }

        public static float WorkAmount(Thing t)
        {
            CompUpgradable upgradable = GetUpgradable(t);
            float fraction = upgradable?.Props.workFractionPerTier ?? 0.5f;
            float baseWork = t.def.GetStatValueAbstract(StatDefOf.WorkToBuild, t.Stuff);
            return baseWork * fraction;
        }

        private static int ScaledCount(int baseCount, float fraction)
        {
            return System.Math.Max(1, UnityEngine.Mathf.CeilToInt(baseCount * fraction));
        }

        public static bool TryFindIngredients(Pawn pawn, List<ThingDefCountClass> required, List<LocalTargetInfo> foundThings, List<int> foundCounts)
        {
            HashSet<Thing> claimed = new HashSet<Thing>();

            foreach (ThingDefCountClass need in required)
            {
                int remaining = need.count;
                while (remaining > 0)
                {
                    Thing found = GenClosest.ClosestThingReachable(
                        pawn.Position,
                        pawn.Map,
                        ThingRequest.ForDef(need.thingDef),
                        PathEndMode.ClosestTouch,
                        TraverseParms.For(pawn),
                        9999f,
                        r => !claimed.Contains(r) && !r.IsForbidden(pawn) && pawn.CanReserve(r));

                    if (found == null)
                    {
                        return false;
                    }

                    int take = System.Math.Min(remaining, found.stackCount);
                    foundThings.Add(found);
                    foundCounts.Add(take);
                    claimed.Add(found);
                    remaining -= take;
                }
            }

            return true;
        }

        public static void ConsumeNearbyResources(Thing t, List<ThingDefCountClass> required)
        {
            if (t.Map == null)
            {
                return;
            }
            foreach (ThingDefCountClass need in required)
            {
                int remaining = need.count;
                foreach (IntVec3 cell in t.OccupiedRect().ExpandedBy(1).Cells)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }
                    if (!cell.InBounds(t.Map))
                    {
                        continue;
                    }
                    foreach (Thing thing in cell.GetThingList(t.Map).ToArray())
                    {
                        if (remaining <= 0)
                        {
                            break;
                        }
                        if (thing.def != need.thingDef)
                        {
                            continue;
                        }
                        int take = System.Math.Min(remaining, thing.stackCount);
                        thing.SplitOff(take).Destroy();
                        remaining -= take;
                    }
                }
            }
        }
    }
}
