using HarmonyLib;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Rewards;

using PlayerStatsSystem;

namespace LabExtended.Patches.Functions.Scp079Rewards;

/// <summary>
/// Patches the TerminationRewards.GainReward method to prevent SCP-079 from gaining experience when killing a player that has the "CanCountAs079ExpTarget" toggle disabled.
/// </summary>
public static class Scp079TerminationPatch
{
    /// <summary>
    /// Prevents SCP-079 from gaining experience when killing a player that has the "CanCountAs079ExpTarget" toggle disabled.
    /// </summary>
    /// <param name="scp079">The instance of the SCP-079 role.</param>
    /// <param name="deadPly">The reference hub of the dead player.</param>
    /// <param name="damageHandler">The damage handler responsible for the kill.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(TerminationRewards), nameof(TerminationRewards.GainReward))]
    public static bool Prefix(Scp079Role scp079, ReferenceHub deadPly, DamageHandlerBase damageHandler)
    {
        if (ExPlayer.TryGet(deadPly, out var player) && !player.Toggles.CanCountAs079ExpTarget)
            return false;

        return true;
    }
}