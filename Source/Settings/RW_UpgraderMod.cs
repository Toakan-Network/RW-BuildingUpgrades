using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_Upgrader
{
    public class RW_UpgraderMod : Mod
    {
        public static RW_UpgraderSettings Settings;

        private Vector2 scrollPosition;

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
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 1600f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            List<StuffCategoryDef> tieredCategories = DefDatabase<StuffCategoryDef>.AllDefsListForReading
                .Where(c => c.GetModExtension<MaterialTierExtension>() != null)
                .OrderBy(c => c.GetModExtension<MaterialTierExtension>().tier)
                .ToList();
            int highestTier = tieredCategories.Count > 0 ? tieredCategories.Max(c => c.GetModExtension<MaterialTierExtension>().tier) : 0;

            DrawSectionHeader(listing, "RW_Upgrader_Setting_Section_General".Translate());

            listing.CheckboxLabeled("RW_Upgrader_Setting_AutoEnable".Translate(), ref Settings.autoEnableByDefault, "RW_Upgrader_Setting_AutoEnableDesc".Translate());
            listing.CheckboxLabeled("RW_Upgrader_Setting_AutoEnableMaterial".Translate(), ref Settings.autoEnableMaterialByDefault);

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
            listing.Label("RW_Upgrader_Setting_BaseCost".Translate(Mathf.RoundToInt(Settings.baseResourceCostPercent)));
            Settings.baseResourceCostPercent = listing.Slider(Settings.baseResourceCostPercent, 1f, 100f);

            listing.CheckboxLabeled("RW_Upgrader_Setting_IncreasePerTier".Translate(), ref Settings.increaseCostPerTier, "RW_Upgrader_Setting_IncreasePerTierDesc".Translate());

            listing.GapLine();
            DrawSectionHeader(listing, "RW_Upgrader_Setting_Section_FurnitureQuality".Translate());

            QualityCategory maxQuality = (QualityCategory)Settings.maxQualityTier;
            listing.Label("RW_Upgrader_Setting_MaxTier".Translate(maxQuality.GetLabel().CapitalizeFirst()));
            Settings.maxQualityTier = (int)listing.Slider(Settings.maxQualityTier, 0f, (float)QualityCategory.Legendary);

            listing.GapLine();
            DrawSectionHeader(listing, "RW_Upgrader_Setting_Section_BuildingMaterials".Translate());
            DrawMaterialSection(listing, tieredCategories, highestTier, isFurniture: false, ref Settings.maxBuildingMaterialTier);

            listing.GapLine();
            DrawSectionHeader(listing, "RW_Upgrader_Setting_Section_FurnitureMaterials".Translate());
            DrawMaterialSection(listing, tieredCategories, highestTier, isFurniture: true, ref Settings.maxFurnitureMaterialTier);

            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawSectionHeader(Listing_Standard listing, string text)
        {
            GameFont previousFont = Text.Font;
            Text.Font = GameFont.Medium;
            listing.Label(text);
            Text.Font = previousFont;
            listing.Gap();
        }

        private static void DrawMaterialSection(Listing_Standard listing, List<StuffCategoryDef> tieredCategories, int highestTier, bool isFurniture, ref int maxTier)
        {
            if (tieredCategories.Count > 0)
            {
                int currentMaxTier = maxTier;
                StuffCategoryDef maxCategory = tieredCategories.FirstOrDefault(c => c.GetModExtension<MaterialTierExtension>().tier == currentMaxTier);
                string labelKey = isFurniture ? "RW_Upgrader_Setting_MaxFurnitureMaterialTier" : "RW_Upgrader_Setting_MaxBuildingMaterialTier";
                listing.Label(labelKey.Translate(maxCategory?.LabelCap ?? "RW_Upgrader_NoRequirement".Translate()));
                maxTier = (int)listing.Slider(maxTier, 0f, highestTier);
                listing.Gap();
            }

            foreach (StuffCategoryDef category in tieredCategories)
            {
                List<ThingDef> members = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(td => td.IsStuff && td.stuffProps.categories.Contains(category))
                    .ToList();

                listing.Gap();
                listing.Label("RW_Upgrader_Setting_AllowUpgradeTo".Translate(category.LabelCap));
                foreach (ThingDef stuffDef in members)
                {
                    bool allowed = Settings.IsStuffAllowed(isFurniture, category.defName, stuffDef.defName);
                    bool newVal = allowed;
                    listing.CheckboxLabeled("  " + stuffDef.LabelCap, ref newVal);
                    if (newVal != allowed)
                    {
                        Settings.SetStuffAllowed(isFurniture, category.defName, stuffDef.defName, newVal, members.Select(m => m.defName));
                    }
                }
            }
        }
    }
}
