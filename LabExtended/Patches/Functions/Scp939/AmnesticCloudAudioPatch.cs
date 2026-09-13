using HarmonyLib;

using LabExtended.Core;
using LabExtended.API;

using Mirror;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Patches.Functions.Scp939;

/// <summary>
/// This patch is applied to the Scp939AmnesticCloudInstance class to modify the behavior of the RpcPlayCreateSound method. 
/// It prevents the Amnestic Cloud sound from being played for players who have the "CanHearAmnesticCloudSpawn" toggle disabled.
/// </summary>
public static class AmnesticCloudAudioPatch
{
    /// <summary>
    /// The hash of the RPC function.
    /// </summary>
    public const int FunctionHash = -193115792;

    /// <summary>
    /// This patch is used to prevent the Amnestic Cloud sound from being played for players who have the "CanHearAmnesticCloudSpawn" toggle disabled.
    /// </summary>
    /// <param name="__instance">The instance of the Scp939AmnesticCloudInstance.</param>
    /// <returns>Returns false to prevent the original method from being executed if the patch succeeds, true otherwise.</returns>
    [HarmonyPatch(typeof(Scp939AmnesticCloudInstance), nameof(Scp939AmnesticCloudInstance.RpcPlayCreateSound))]
    public static bool Prefix(Scp939AmnesticCloudInstance __instance)
    {
        try
        {
            MirrorMethods.WriteToWhere(p => p.Toggles.CanHearAmnesticCloudSpawn,
                w =>
                {
                    w.WriteMessageId<RpcMessage>();
                    w.WriteUInt(__instance.netId);
                    w.WriteByte(__instance.ComponentIndex);
                    w.WriteUShort(unchecked((ushort)FunctionHash));
                    w.WriteUInt(0);
                });
            return false;
        }
        catch (Exception ex)
        {
            ApiLog.Error("AmnesticCloudAudioPatch", ex);
            return true;
        }
    }
}