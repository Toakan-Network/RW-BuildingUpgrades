using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public class CompUpgradable : ThingComp
    {
        private bool autoUpgradeEnabled;

        private bool autoUpgradeMaterialEnabled;

        public CompProperties_Upgradable Props => (CompProperties_Upgradable)props;

        public bool AutoUpgradeEnabled => autoUpgradeEnabled;

        public bool AutoUpgradeMaterialEnabled => autoUpgradeMaterialEnabled;

        public bool CanUpgrade
        {
            get
            {
                CompQuality compQuality = parent.TryGetComp<CompQuality>();
                if (compQuality == null)
                {
                    return false;
                }
                return (int)compQuality.Quality < RW_UpgraderMod.Settings.maxQualityTier;
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
