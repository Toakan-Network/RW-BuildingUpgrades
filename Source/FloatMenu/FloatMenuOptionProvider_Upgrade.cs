using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public class FloatMenuOptionProvider_Upgrade : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            if (context.FirstSelectedPawn.skills != null)
            {
                return !context.FirstSelectedPawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled;
            }
            return false;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;
            CompUpgradable upgradable = UpgradeUtility.GetUpgradable(clickedThing);
            if (upgradable == null || !upgradable.CanUpgrade)
            {
                return null;
            }
            if (clickedThing.Faction != pawn.Faction)
            {
                return null;
            }
            if (!pawn.CanReach(clickedThing, PathEndMode.Touch, Danger.Deadly))
            {
                return new FloatMenuOption("CannotUpgradeThing".Translate(clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }

            List<ThingDefCountClass> required = UpgradeUtility.RequiredResources(clickedThing);
            List<LocalTargetInfo> foundThings = new List<LocalTargetInfo>();
            List<int> foundCounts = new List<int>();
            if (!UpgradeUtility.TryFindIngredients(pawn, required, foundThings, foundCounts))
            {
                return new FloatMenuOption("CannotUpgradeThing".Translate(clickedThing) + ": " + "RW_Upgrader_MissingMaterials".Translate().CapitalizeFirst(), null);
            }

            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RW_Upgrader_UpgradeThing".Translate(clickedThing), delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf.Upgrade, clickedThing);
                if (foundThings.Count > 0)
                {
                    job.targetQueueB = foundThings;
                    job.countQueue = foundCounts;
                }
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), pawn, clickedThing);
        }
    }
}
