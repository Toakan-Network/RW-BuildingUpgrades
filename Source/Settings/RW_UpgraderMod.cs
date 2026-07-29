using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_Upgrader
{
    public class RW_UpgraderMod : Mod
    {
        public static RW_UpgraderSettings Settings;

        public RW_UpgraderMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RW_UpgraderSettings>();
        }

        public override string SettingsCategory()
        {
            return "Building Upgrades";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("RW_Upgrader_Setting_AutoEnable".Translate(), ref Settings.autoEnableByDefault, "RW_Upgrader_Setting_AutoEnableDesc".Translate());

            listing.Gap();
            string areaLabel = Settings.areaRestrictionMode switch
            {
                AreaRestrictionMode.MapWide => "RW_Upgrader_Setting_AreaMapWide".Translate(),
                AreaRestrictionMode.Home => "RW_Upgrader_Setting_AreaHome".Translate(),
                _ => Settings.customAreaLabel,
            };
            if (listing.ButtonTextLabeled("RW_Upgrader_Setting_AreaRestriction".Translate(), areaLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption("RW_Upgrader_Setting_AreaMapWide".Translate(), delegate { Settings.areaRestrictionMode = AreaRestrictionMode.MapWide; }),
                    new FloatMenuOption("RW_Upgrader_Setting_AreaHome".Translate(), delegate { Settings.areaRestrictionMode = AreaRestrictionMode.Home; }),
                };
                if (Find.CurrentMap != null)
                {
                    foreach (Area area in Find.CurrentMap.areaManager.AllAreas)
                    {
                        if (area is Area_Allowed)
                        {
                            string label = area.Label;
                            options.Add(new FloatMenuOption(label, delegate
                            {
                                Settings.areaRestrictionMode = AreaRestrictionMode.Custom;
                                Settings.customAreaLabel = label;
                            }));
                        }
                    }
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            listing.Gap();
            QualityCategory maxQuality = (QualityCategory)Settings.maxQualityTier;
            listing.Label("RW_Upgrader_Setting_MaxTier".Translate(maxQuality.GetLabel().CapitalizeFirst()));
            Settings.maxQualityTier = (int)listing.Slider(Settings.maxQualityTier, 0f, (float)QualityCategory.Legendary);

            listing.Gap();
            listing.Label("RW_Upgrader_Setting_BaseCost".Translate(Mathf.RoundToInt(Settings.baseResourceCostPercent)));
            Settings.baseResourceCostPercent = listing.Slider(Settings.baseResourceCostPercent, 1f, 100f);

            listing.CheckboxLabeled("RW_Upgrader_Setting_IncreasePerTier".Translate(), ref Settings.increaseCostPerTier, "RW_Upgrader_Setting_IncreasePerTierDesc".Translate());

            listing.End();
        }
    }
}
