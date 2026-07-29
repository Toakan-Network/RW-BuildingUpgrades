using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public class JobDriver_Upgrade : JobDriver
    {
        private float workDone;

        private Thing Target => job.GetTarget(TargetIndex.A).Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workDone, "workDone", 0f);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !UpgradeUtility.PawnCanUpgradeNow(pawn, Target));

            Toil gotoBuilding = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return Toils_Jump.JumpIf(gotoBuilding, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            foreach (Toil haulToil in JobDriver_DoBill.CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C, false, true, false))
            {
                yield return haulToil;
            }

            yield return gotoBuilding;

            Toil work = ToilMaker.MakeToil("MakeNewToils");
            work.tickIntervalAction = delegate(int delta)
            {
                Pawn actor = work.actor;
                Thing target = Target;
                if (actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Construction, 0.25f * delta);
                }
                actor.rotationTracker.FaceTarget(target);
                float amount = actor.GetStatValue(StatDefOf.ConstructionSpeed) * delta;
                workDone += amount;
                if (workDone >= UpgradeUtility.WorkAmount(target))
                {
                    FinishUpgrade(actor, target);
                    ReadyForNextToil();
                }
            };
            work.WithEffect(() => Target.def.constructEffect ?? Target.def.repairEffect, TargetIndex.A);
            work.WithProgressBar(TargetIndex.A, () => workDone / UpgradeUtility.WorkAmount(Target));
            work.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.activeSkill = () => SkillDefOf.Construction;
            work.handlingFacing = true;
            yield return work;
        }

        private void FinishUpgrade(Pawn actor, Thing target)
        {
            UpgradeUtility.ConsumeNearbyResources(target, UpgradeUtility.RequiredResources(target));
            target.TryGetComp<CompUpgradable>()?.SetQualityCooldown(CompUpgradable.SuccessCooldownTicks);

            CompQuality compQuality = target.TryGetComp<CompQuality>();
            if (compQuality == null)
            {
                return;
            }

            QualityCategory before = compQuality.Quality;
            int skillLevel = actor.skills.GetSkill(SkillDefOf.Construction).Level;
            bool inspired = actor.InspirationDef == InspirationDefOf.Inspired_Creativity;
            QualityCategory rolled = QualityUtility.GenerateQualityCreatedByPawn(skillLevel, inspired);
            QualityCategory after = (QualityCategory)UnityEngine.Mathf.Max((int)before, (int)rolled);

            compQuality.SetQuality(after, ArtGenerationContext.Colony);

            if (after > before)
            {
                Messages.Message("RW_Upgrader_UpgradeSucceeded".Translate(target.LabelShort, before.GetLabel(), after.GetLabel()), target, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("RW_Upgrader_UpgradeNoChange".Translate(target.LabelShort), target, MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
