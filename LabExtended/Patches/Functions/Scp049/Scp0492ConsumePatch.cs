using HarmonyLib;

using LabApi.Events.Arguments.Scp0492Events;
using LabApi.Events.Handlers;

using PlayerRoles;
using PlayerRoles.Ragdolls;
using PlayerRoles.PlayableScps.Scp049.Zombies;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp049;

/// <summary>
/// Patch for <see cref="ZombieConsumeAbility.ServerValidateBegin(BasicRagdoll)"/> to add a custom event <see cref="Scp0492Events.OnStartingConsumingCorpse(Scp0492StartingConsumingCorpseEventArgs)"/>.
/// </summary>
public static class Scp0492ConsumePatch
{
    /// <summary>
    /// Patch for <see cref="ZombieConsumeAbility.ServerValidateBegin(BasicRagdoll)"/> to add a custom event <see cref="Scp0492Events.OnStartingConsumingCorpse(Scp0492StartingConsumingCorpseEventArgs)"/>.
    /// </summary>
    /// <param name="__instance">The instance of the ZombieConsumeAbility.</param>
    /// <param name="ragdoll">The ragdoll being consumed.</param>
    /// <param name="__result">The result of the validation.</param>
    /// <returns>Returns false to prevent the original method from executing.</returns>
    [HarmonyPatch(typeof(ZombieConsumeAbility), nameof(ZombieConsumeAbility.ServerValidateBegin))]
    public static bool Prefix(ZombieConsumeAbility __instance, BasicRagdoll ragdoll, ref byte __result)
    {
        if (!ExPlayer.TryGet(__instance.Owner, out var scp))
            return true;

        var owner = ExPlayer.Get(ragdoll.Info.OwnerHub);
        var error = ZombieConsumeAbility.ConsumeError.None;

        if (!scp.Toggles.CanConsumeRagdollsAsZombie)
        {
            error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
        }
        else if (owner is { Toggles.CanBeConsumedByZombies: false })
        {
            error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
        }
        else if (ZombieConsumeAbility.ConsumedRagdolls.Contains(ragdoll))
        {
            error = ZombieConsumeAbility.ConsumeError.AlreadyConsumed;
        }
        else if (!ragdoll.Info.RoleType.IsHuman() || !__instance.ServerValidateAny())
        {
            error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
        }
        else if (Mathf.Approximately(scp.Stats.Health.NormalizedValue, 1f))
        {
            error = ZombieConsumeAbility.ConsumeError.FullHealth;
        }
        else if (ZombieConsumeAbility.AllAbilities.Any(a => a.IsInProgress 
                                                            && a.CurRagdoll == ragdoll))
        {
            error = ZombieConsumeAbility.ConsumeError.BeingConsumed;
        }

        var consumingEventArgs = new Scp0492StartingConsumingCorpseEventArgs(scp.ReferenceHub, ragdoll, error);
        
        Scp0492Events.OnStartingConsumingCorpse(consumingEventArgs);

        __result = (byte)consumingEventArgs.Error;
        return false;
    }
}