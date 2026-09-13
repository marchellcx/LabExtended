using HarmonyLib;

using PlayerRoles.PlayableScps.Scp096;

namespace LabExtended.Patches.Functions.Scp096;

/// <summary>
/// This class contains a Harmony patch for the <see cref="Scp096TargetsTracker.IsObservedBy"/> method. It modifies the behavior of the method to check if the player can trigger SCP-096's rage state when observed, based on the toggles set for both the SCP and the observing player.
/// </summary>
public static class Scp096TriggerPatch
{
    /// <summary>
    /// This patch checks if the player can trigger SCP-096's rage state when observed. It verifies the toggles for both the SCP and the observing player to determine if the interaction is allowed.
    /// </summary>
    /// <param name="__instance">The instance of <see cref="Scp096TargetsTracker"/> being patched.</param>
    /// <param name="target">The target <see cref="ReferenceHub"/> being observed.</param>
    /// <param name="__result">The result of the original method.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.IsObservedBy))]
    public static bool Prefix(Scp096TargetsTracker __instance, ReferenceHub target, ref bool __result)
    {
        var scp = ExPlayer.Get(__instance.Owner);
        var player = ExPlayer.Get(target);

        if (scp is null || player is null)
            return true;

        if (!scp.Toggles.CanBeTriggeredAs096)
        {
            __result = false;
            return false;
        }

        if (!player.Toggles.CanTriggerScp096)
        {
            __result = false;
            return false;
        }

        return true;
    }
}