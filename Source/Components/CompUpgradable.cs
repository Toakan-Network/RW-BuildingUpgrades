using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public class CompUpgradable : ThingComp
    {
        public const int FailCooldownTicks = GenDate.TicksPerHour;

        public const int SuccessCooldownTicks = GenDate.TicksPerDay;

        private bool autoUpgradeEnabled;

        private bool autoUpgradeMaterialEnabled;

        private int nextQualityAttemptTick;

        private int nextMaterialAttemptTick;

        public CompProperties_Upgradable Props => (CompProperties_Upgradable)props;

        public bool AutoUpgradeEnabled => autoUpgradeEnabled;

        public bool AutoUpgradeMaterialEnabled => autoUpgradeMaterialEnabled;

        public bool QualityOnCooldown => Find.TickManager.TicksGame < nextQualityAttemptTick;

        public bool MaterialOnCooldown => Find.TickManager.TicksGame < nextMaterialAttemptTick;

        public void SetQualityCooldown(int ticks)
        {
            nextQualityAttemptTick = Find.TickManager.TicksGame + ticks;
        }

        public void SetMaterialCooldown(int ticks)
        {
            nextMaterialAttemptTick = Find.TickManager.TicksGame + ticks;
        }

        public bool CanUpgrade
        {
            get
            {
                CompQuality compQuality = parent.TryGetComp<CompQuality>();
                if (compQuality == null)
                {
                    return false;
                }
                return (int)compQuality.Quality < ZoneUpgradeUtility.EffectiveMaxQualityTier(parent);
            }
        }

        public void SetAutoUpgradeStates(bool quality, bool material)
        {
            autoUpgradeEnabled = quality;
            autoUpgradeMaterialEnabled = material;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref autoUpgradeEnabled, "autoUpgradeEnabled", false);
            Scribe_Values.Look(ref autoUpgradeMaterialEnabled, "autoUpgradeMaterialEnabled", false);
            Scribe_Values.Look(ref nextQualityAttemptTick, "nextQualityAttemptTick", 0);
            Scribe_Values.Look(ref nextMaterialAttemptTick, "nextMaterialAttemptTick", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                autoUpgradeEnabled = RW_UpgraderMod.Settings.autoEnableByDefault;
                autoUpgradeMaterialEnabled = RW_UpgraderMod.Settings.autoEnableMaterialByDefault;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (CanUpgrade)
            {
                yield return new Command_Toggle
                {
                    icon = TexCommand.RearmTrap,
                    defaultLabel = "RW_Upgrader_AutoUpgrade".Translate(),
                    defaultDesc = "RW_Upgrader_AutoUpgradeDesc".Translate(),
                    isActive = () => autoUpgradeEnabled,
                    toggleAction = delegate
                    {
                        autoUpgradeEnabled = !autoUpgradeEnabled;
                    }
                };
            }

            if (MaterialUpgradeUtility.CanUpgradeMaterial(parent))
            {
                yield return new Command_Toggle
                {
                    icon = TexCommand.RearmTrap,
                    defaultLabel = "RW_Upgrader_AutoUpgradeMaterial".Translate(),
                    defaultDesc = "RW_Upgrader_AutoUpgradeMaterialDesc".Translate(),
                    isActive = () => autoUpgradeMaterialEnabled,
                    toggleAction = delegate
                    {
                        autoUpgradeMaterialEnabled = !autoUpgradeMaterialEnabled;
                    }
                };
            }
        }
    }
}
