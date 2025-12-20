using Christmas.Scp2536;

using HarmonyLib;

using LabExtended.Core;

namespace LabExtended.Patches.Functions.Holidays
{
    /// <summary>
    /// Prevents the SCP-2536 tree from spawning (if the Scp2536Disabled config is enabled) by failing the target locator.
    /// </summary>
    public static class Scp2536SpawnPreventionPatch
    {
        [HarmonyPatch(typeof(Scp2536Controller), nameof(Scp2536Controller.ServerFindTarget))]
        private static bool Prefix(ref bool __result)
        {
            if (ApiLoader.ApiConfig != null && ApiLoader.ApiConfig.Scp2536Disabled)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}