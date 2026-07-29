using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public class WorkGiver_Upgrade : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn?.Map == null;
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            CompUpgradable upgradable = UpgradeUtility.GetUpgradable(t.Thing);
            if (upgradable != null && upgradable.QualityOnCooldown)
            {
                return 0f;
            }
            Zone_UpgradeStandard zone = ZoneUpgradeUtility.GetGoverningZone(t.Thing);
            return zone != null && ZoneUpgradeUtility.BelowZoneQualityMinimum(t.Thing, zone) ? 100f : 0f;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompUpgradable upgradable = UpgradeUtility.GetUpgradable(t);
            if (upgradable == null || !upgradable.AutoUpgradeEnabled)
            {
                return null;
            }
            if (upgradable.QualityOnCooldown)
            {
                return null;
            }
            if (!UpgradeUtility.PawnCanUpgradeNow(pawn, t))
            {
                return null;
            }

            if (!UpgradeUtility.PassesAreaRestriction(t))
            {
                return null;
            }

            List<ThingDefCountClass> required = UpgradeUtility.RequiredResources(t);
            List<LocalTargetInfo> foundThings = new List<LocalTargetInfo>();
            List<int> foundCounts = new List<int>();

            if (!UpgradeUtility.TryFindIngredients(pawn, required, foundThings, foundCounts))
            {
                upgradable.SetQualityCooldown(CompUpgradable.FailCooldownTicks);
                JobFailReason.Is("RW_Upgrader_MissingMaterials".Translate());
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Upgrade, t);
            if (foundThings.Count > 0)
            {
                job.targetQueueB = foundThings;
                job.countQueue = foundCounts;
            }
            return job;
        }
    }
}
