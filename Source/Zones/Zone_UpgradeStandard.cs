using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_Upgrader
{
    public class Zone_UpgradeStandard : Zone
    {
        private static readonly Color ZoneColor = new Color(1f, 0.85f, 0.3f, 0.09f);

        protected override Color NextZoneColor => ZoneColor;

        public int minQualityTier = -1;

        public int minMaterialTier = -1;

        public Zone_UpgradeStandard()
        {
        }

        public Zone_UpgradeStandard(ZoneManager zoneManager) : base("RW_Upgrader_ZoneLabel".Translate(), zoneManager)
        {
        }

        private string CurrentQualityLabel => minQualityTier < 0
            ? "RW_Upgrader_NoRequirement".Translate()
            : ((QualityCategory)minQualityTier).GetLabel().CapitalizeFirst();

        private string CurrentMaterialLabel
        {
            get
            {
                if (minMaterialTier < 0)
                {
                    return "RW_Upgrader_NoRequirement".Translate();
                }
                StuffCategoryDef category = DefDatabase<StuffCategoryDef>.AllDefsListForReading
                    .FirstOrDefault(c => c.GetModExtension<MaterialTierExtension>()?.tier == minMaterialTier);
                return category?.LabelCap ?? "RW_Upgrader_NoRequirement".Translate();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref minQualityTier, "minQualityTier", -1);
            Scribe_Values.Look(ref minMaterialTier, "minMaterialTier", -1);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone"),
                defaultLabel = Hidden ? "CommandUnhideLabel".Translate() : "CommandHideLabel".Translate(),
                defaultDesc = "CommandHideZoneDesc".Translate(),
                isActive = () => Hidden,
                toggleAction = delegate { Hidden = !Hidden; }
            };

            yield return new Command_Action
            {
                defaultLabel = "RW_Upgrader_ZoneSetMinQuality".Translate(CurrentQualityLabel),
                icon = TexCommand.RearmTrap,
                action = delegate
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("RW_Upgrader_NoRequirement".Translate(), delegate { minQualityTier = -1; })
                    };
                    foreach (QualityCategory q in QualityUtility.AllQualityCategories)
                    {
                        options.Add(new FloatMenuOption(q.GetLabel().CapitalizeFirst(), delegate { minQualityTier = (int)q; }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "RW_Upgrader_ZoneSetMinMaterial".Translate(CurrentMaterialLabel),
                icon = TexCommand.RearmTrap,
                action = delegate
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("RW_Upgrader_NoRequirement".Translate(), delegate { minMaterialTier = -1; })
                    };
                    foreach (StuffCategoryDef category in DefDatabase<StuffCategoryDef>.AllDefsListForReading
                        .Where(c => c.GetModExtension<MaterialTierExtension>() != null)
                        .OrderBy(c => c.GetModExtension<MaterialTierExtension>().tier))
                    {
                        int tier = category.GetModExtension<MaterialTierExtension>().tier;
                        options.Add(new FloatMenuOption(category.LabelCap, delegate { minMaterialTier = tier; }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }
    }
}
