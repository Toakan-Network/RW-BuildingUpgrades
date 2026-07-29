using RimWorld;
using Verse;

namespace RW_Upgrader
{
    public class Designator_ZoneAdd_UpgradeStandard : Designator_ZoneAdd
    {
        public Designator_ZoneAdd_UpgradeStandard()
        {
            zoneTypeToPlace = typeof(Zone_UpgradeStandard);
            defaultLabel = "RW_Upgrader_DesignateZone".Translate();
            defaultDesc = "RW_Upgrader_DesignateZoneDesc".Translate();
            icon = TexCommand.RearmTrap;
        }

        protected override string NewZoneLabel => "RW_Upgrader_ZoneLabel".Translate();

        protected override Zone MakeNewZone()
        {
            return new Zone_UpgradeStandard(Map.zoneManager);
        }
    }
}
