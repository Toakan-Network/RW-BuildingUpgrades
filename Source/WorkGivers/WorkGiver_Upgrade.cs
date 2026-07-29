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

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompUpgradable upgradable = UpgradeUtility.GetUpgradable(t);
            if (upgradable == null || !upgradable.AutoUpgradeEnabled)
            {
                return false;
            }
            return UpgradeUtility.PawnCanUpgradeNow(pawn, t);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!HasJobOnThing(pawn, t, forced))
            {
                return null;
            }

            List<ThingDefCountClass> required = UpgradeUtility.RequiredResources(t);
            List<LocalTargetInfo> foundThings = new List<LocalTargetInfo>();
            List<int> foundCounts = new List<int>();

            if (!UpgradeUtility.TryFindIngredients(pawn, required, foundThings, foundCounts))
            {
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
