using HarmonyLib;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

namespace LabExtended.Patches.Functions.Scp939;

/// <summary>
/// Patches the <see cref="EnvironmentalMimicry.ServerProcessCmd"/> method to add a check for the player's toggle to use mimicry as SCP-939.
/// </summary>
public static class Scp939MimicryPatch
{
    /// <summary>
    /// Patch for <see cref="EnvironmentalMimicry.ServerProcessCmd"/> to add a check for the player's toggle to use mimicry as SCP-939.
    /// </summary>
    /// <param name="__instance">The instance of <see cref="EnvironmentalMimicry"/> being patched.</param>
    /// <param name="reader">The <see cref="NetworkReader"/> used to read network data.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(EnvironmentalMimicry), nameof(EnvironmentalMimicry.ServerProcessCmd))]
    public static bool Prefix(EnvironmentalMimicry __instance, NetworkReader reader)
    {
        if (!__instance.Cooldown.IsReady)
            return false;

        if (!ExPlayer.TryGet(__instance.Owner, out var scp))
            return true;

        if (!scp.Toggles.CanUseMimicryAs939)
            return false;

        __instance._syncOption = reader.ReadByte();
        __instance.Cooldown.Trigger(__instance._activationCooldown);
        __instance.ServerSendRpc(true);

        return false;
    }
}