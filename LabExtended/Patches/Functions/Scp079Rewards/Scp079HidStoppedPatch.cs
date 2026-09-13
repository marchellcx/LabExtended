using HarmonyLib;

using MapGeneration;

using PlayerRoles.FirstPersonControl;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Rewards;

namespace LabExtended.Patches.Functions.Scp079Rewards;

/// <summary>
/// Patches the <see cref="HidStoppedReward.TryGrant"/> method to check if the player is a valid target for SCP-079's experience gain when they stop moving.
/// </summary>
public static class Scp079HidStoppedPatch
{
    /// <summary>
    /// Patches the <see cref="HidStoppedReward.TryGrant"/> method to check if the player is a valid target for SCP-079's experience gain when they stop moving.
    /// </summary>
    /// <param name="ply">The player being checked.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(HidStoppedReward), nameof(HidStoppedReward.TryGrant))]
    public static bool Prefix(ReferenceHub ply)
    {
        if (!ExPlayer.TryGet(ply, out var player))
            return true;

        if (!player.Toggles.CanCountAs079ExpTarget)
            return false;

        if (!player.Role.Is<IFpcRole>(out var fpcRole))
            return false;

        var playerPos = fpcRole.FpcModule.Position;
        var playerRoom = playerPos.TryGetRoom(out var room) ? room : null;

        if (playerRoom is null)
            return false;

        if (!ExPlayer.Players.Any(x => x.Toggles.CanCountAs079ExpTarget && HidStoppedReward.IsNearbyTeammate(playerPos, x.ReferenceHub)))
            return false;

        foreach (var role in Scp079Role.ActiveInstances)
        {
            if (Scp079RewardManager.CheckForRoomInteractions(role, playerRoom))
            {
                HidStoppedReward._available = false;
                Scp079RewardManager.GrantExp(role, 50, Scp079HudTranslation.ExpGainHidStopped);
            }
        }

        return false;
    }
}