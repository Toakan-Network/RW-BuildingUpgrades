using RimWorld;
using Verse;

namespace RW_Upgrader
{
    [DefOf]
    public static class RW_UpgraderKeyBindingDefOf
    {
        public static KeyBindingDef RW_Upgrader_AutoUpgradeMaterial;

        static RW_UpgraderKeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RW_UpgraderKeyBindingDefOf));
        }
    }
}
