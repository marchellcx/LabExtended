using HarmonyLib;
using InventorySystem.Items.Pickups;

using LabExtended.API;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Provides a Harmony patch for synchronizing the kinematic state of pickups, ensuring that frozen pickups are
    /// correctly reported as kinematic during server freeze checks.
    /// </summary>
    public static class PickupIsKinematicSyncPatch
    {
        [HarmonyPatch(typeof(PickupStandardPhysics), nameof(PickupStandardPhysics.ServerSendFreeze), MethodType.Getter)]
        private static bool Prefix(PickupStandardPhysics __instance, ref bool __result)
        {
            if (__instance != null
                && __instance.Pickup != null
                && __instance.Rb != null 
                && __instance.Rb.isKinematic 
                && ExMap.FrozenPickups.Contains(__instance.Pickup))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
