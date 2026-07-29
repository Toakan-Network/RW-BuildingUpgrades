using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public class WorkGiver_UpgradeMaterial : WorkGiver_Scanner
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
            Zone_UpgradeStandard zone = ZoneUpgradeUtility.GetGoverningZone(t.Thing);
            return zone != null && ZoneUpgradeUtility.BelowZoneMaterialMinimum(t.Thing, zone) ? 100f : 0f;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompUpgradable upgradable = UpgradeUtility.GetUpgradable(t);
            if (upgradable == null || !upgradable.AutoUpgradeMaterialEnabled)
            {
                return null;
            }
            if (!MaterialUpgradeUtility.PawnCanUpgradeMaterialNow(pawn, t))
            {
                return null;
            }
            if (!UpgradeUtility.PassesAreaRestriction(t))
            {
                return null;
            }

            ThingDef newStuff = MaterialUpgradeUtility.PickBestStuff(t, pawn);
            if (newStuff == null)
            {
                JobFailReason.Is("RW_Upgrader_MissingMaterials".Translate());
                return null;
            }

            List<ThingDefCountClass> required = MaterialUpgradeUtility.RequiredResourcesForSwap(t, newStuff);
            List<LocalTargetInfo> foundThings = new List<LocalTargetInfo>();
            List<int> foundCounts = new List<int>();

            if (!UpgradeUtility.TryFindIngredients(pawn, required, foundThings, foundCounts))
            {
                JobFailReason.Is("RW_Upgrader_MissingMaterials".Translate());
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.UpgradeMaterial, t);
            job.thingDefToCarry = newStuff;
            if (foundThings.Count > 0)
            {
                job.targetQueueB = foundThings;
                job.countQueue = foundCounts;
            }
            return job;
        }
    }
}
