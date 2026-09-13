using HarmonyLib;

using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Handlers;
using LabExtended.Extensions;

using MapGeneration;

using Mirror;

using NiveraAPI.Extensions;

using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Functions.Scp079;

public static class Scp079TeslaAbilityPatch
{
    /// <summary>
    /// Patch for Scp079TeslaAbility.ServerProcessCmd to add event handling and custom logic for using Tesla gates.
    /// </summary>
    /// <param name="__instance">The instance of the Scp079TeslaAbility.</param>
    /// <param name="reader">The network reader containing the command data.</param>
    /// <returns>Returns false to prevent the original method from executing.</returns>
    [HarmonyPatch(typeof(Scp079TeslaAbility), nameof(Scp079TeslaAbility.ServerProcessCmd))]
    public static bool Prefix(Scp079TeslaAbility __instance, NetworkReader reader)
    {
        if (!__instance.IsReady)
            return false;

        var camera = __instance.CurrentCamSync.CurrentCamera;

        if (camera is null)
            return false;

        if (!ExTeslaGate.Lookup.TryGetFirst(
                x => RoomUtils.CompareCoords(x.Value.Transform.position, camera.Position), out var gate))
            return false;

        if (gate.Value.IsDisabled)
            return false;

        var usingTeslaArgs = new Scp079UsingTeslaEventArgs(__instance.Owner, gate.Value.Base);
        
        Scp079Events.OnUsingTesla(usingTeslaArgs);

        if (!usingTeslaArgs.IsAllowed)
            return false;
        
        __instance.RewardManager.MarkRoom(camera.Room);
        __instance.AuxManager.CurrentAux -= __instance._cost;
        
        gate.Value.Base.RpcInstantBurst();

        __instance._nextUseTime = NetworkTime.time + __instance._cooldown;
        __instance.ServerSendRpc(false);
        
        Scp079Events.OnUsedTesla(new(__instance.Owner, gate.Value.Base));
        return false;
    }
}