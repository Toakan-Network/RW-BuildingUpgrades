using RimWorld;
using Verse;

namespace RW_Upgrader
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef Upgrade;

        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}
