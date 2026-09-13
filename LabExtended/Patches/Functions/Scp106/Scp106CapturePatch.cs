using HarmonyLib;

using PlayerRoles.PlayableScps.Scp106;

namespace LabExtended.Patches.Functions.Scp106;

/// <summary>
/// Patches the <see cref="Scp106Attack.ServerShoot"/> method to add custom logic for capturing players with SCP-106.
/// </summary>
public static class Scp106CapturePatch
{
    /// <summary>
    /// Prevents SCP-106 from capturing players that have the "CanBeCapturedBy106" toggle disabled or if the SCP-106 player has the "CanCaptureAs106" toggle disabled.
    /// </summary>
    /// <param name="__instance">The instance of <see cref="Scp106Attack"/> being patched.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
    public static bool Prefix(Scp106Attack __instance)
    {
        if (!ExPlayer.TryGet(__instance._targetHub, out var player) || !ExPlayer.TryGet(__instance.Owner, out var scp))
            return true;

        if (!player.Toggles.CanBeCapturedBy106 || !scp.Toggles.CanCaptureAs106)
        {
            __instance.SendCooldown(__instance._missCooldown);
            return false;
        }

        return true;
    }
}