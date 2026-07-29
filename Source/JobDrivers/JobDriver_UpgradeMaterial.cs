using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RW_Upgrader
{
    public class JobDriver_UpgradeMaterial : JobDriver
    {
        private float workDone;

        private Thing Target => job.GetTarget(TargetIndex.A).Thing;

        private ThingDef NewStuff => job.thingDefToCarry;

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
            this.FailOn(() => !UpgradeUtility.PawnCanUpgradeNow(pawn, Target) || NewStuff == null);

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
                if (workDone >= MaterialUpgradeUtility.WorkAmount(target, NewStuff))
                {
                    FinishMaterialUpgrade(actor, target, NewStuff);
                    ReadyForNextToil();
                }
            };
            work.WithEffect(() => Target.def.constructEffect ?? Target.def.repairEffect, TargetIndex.A);
            work.WithProgressBar(TargetIndex.A, () => workDone / MaterialUpgradeUtility.WorkAmount(Target, NewStuff));
            work.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.activeSkill = () => SkillDefOf.Construction;
            work.handlingFacing = true;
            yield return work;
        }

        private void FinishMaterialUpgrade(Pawn actor, Thing oldThing, ThingDef newStuff)
        {
            UpgradeUtility.ConsumeNearbyResources(oldThing, MaterialUpgradeUtility.RequiredResourcesForSwap(oldThing, newStuff));

            Map map = oldThing.Map;
            IntVec3 position = oldThing.Position;
            Rot4 rotation = oldThing.Rotation;
            Faction faction = oldThing.Faction;
            ThingDef def = oldThing.def;
            ThingDef oldStuff = oldThing.Stuff;
            ThingStyleDef styleDef = oldThing.StyleDef;

            CompQuality oldQualityComp = oldThing.TryGetComp<CompQuality>();
            QualityCategory before = oldQualityComp?.Quality ?? QualityCategory.Normal;
            int oldHitPoints = oldThing.HitPoints;
            int oldMaxHitPoints = oldThing.MaxHitPoints;

            CompUpgradable oldUpgradable = oldThing.TryGetComp<CompUpgradable>();
            bool autoQuality = oldUpgradable?.AutoUpgradeEnabled ?? false;
            bool autoMaterial = oldUpgradable?.AutoUpgradeMaterialEnabled ?? false;

            oldThing.Destroy();

            Thing newThing = ThingMaker.MakeThing(def, newStuff);
            newThing.SetFactionDirect(faction);
            newThing.HitPoints = Mathf.Max(1, Mathf.CeilToInt((float)oldHitPoints / oldMaxHitPoints * newThing.MaxHitPoints));
            if (styleDef != null)
            {
                newThing.StyleDef = styleDef;
            }

            CompQuality newQualityComp = newThing.TryGetComp<CompQuality>();
            if (newQualityComp != null)
            {
                int skillLevel = actor.skills.GetSkill(SkillDefOf.Construction).Level;
                bool inspired = actor.InspirationDef == InspirationDefOf.Inspired_Creativity;
                QualityCategory rolled = QualityUtility.GenerateQualityCreatedByPawn(skillLevel, inspired);
                QualityCategory after = (QualityCategory)Mathf.Clamp(
                    Mathf.Max((int)before, (int)rolled),
                    0,
                    RW_UpgraderMod.Settings.maxQualityTier);
                newQualityComp.SetQuality(after, ArtGenerationContext.Colony);
            }

            GenSpawn.Spawn(newThing, position, map, rotation, WipeMode.Vanish);

            CompUpgradable newUpgradable = newThing.TryGetComp<CompUpgradable>();
            if (newUpgradable != null)
            {
                newUpgradable.SetAutoUpgradeStates(autoQuality, autoMaterial);
            }

            Messages.Message("RW_Upgrader_MaterialUpgradeSucceeded".Translate(newThing.LabelShort, oldStuff.LabelAsStuff, newStuff.LabelAsStuff), newThing, MessageTypeDefOf.PositiveEvent);
        }
    }
}
