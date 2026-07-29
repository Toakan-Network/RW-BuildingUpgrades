using RimWorld;
using Verse;

namespace RW_Upgrader
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef Upgrade;

        public static JobDef UpgradeMaterial;

        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}
