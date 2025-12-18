using AdminToys;

using HarmonyLib;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Fixes a null reference exception caused by interactable toys being destroyed while being interacted with.
    /// </summary>
    public static class InteractableToySearchNreFix
    {
        [HarmonyPatch(typeof(InvisibleInteractableToy.InteractableToySearchCompletor), nameof(InvisibleInteractableToy.InteractableToySearchCompletor.ValidateDistance))]
        private static bool Prefix(InvisibleInteractableToy.InteractableToySearchCompletor __instance, ref bool __result)
        {
            __result = false;

            if (__instance == null)
                return false;

            if (__instance.Hub == null)
                return false;

            if (__instance._target == null || __instance._target.gameObject == null)
                return false;

            __result = (__instance._target.transform.position - __instance.Hub.transform.position).sqrMagnitude <= __instance._maxDistanceSqr;
            return false;
        }
    }
}