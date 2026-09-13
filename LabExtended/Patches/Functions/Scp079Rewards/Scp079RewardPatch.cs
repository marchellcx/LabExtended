using HarmonyLib;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Functions.Scp079Rewards;

/// <summary>
/// This class contains a Harmony patch for the Scp079TierManager.ServerGrantExperience method. It checks if the player has disabled experience gain for SCP-079 and prevents the experience gain if so.
/// </summary>
public static class Scp079RewardPatch
{
    /// <summary>
    /// This patch is used to prevent SCP-079 from gaining experience if the player has disabled it in their toggles.
    /// </summary>
    /// <param name="__instance">The instance of the Scp079TierManager.</param>
    /// <param name="amount">The amount of experience to grant.</param>
    /// <param name="reason">The reason for the experience gain.</param>
    /// <param name="subject">The subject of the experience gain.</param>
    /// <returns>Returns false to prevent the original method from executing if the conditions are not met.</returns>
    [HarmonyPatch(typeof(Scp079TierManager), nameof(Scp079TierManager.ServerGrantExperience))]
    public static bool Prefix(Scp079TierManager __instance, int amount, Scp079HudTranslation reason, RoleTypeId subject)
    {
        if (amount <= 0)
            return false;

        if (!ExPlayer.TryGet(__instance.Owner, out var scp))
            return true;

        if (!scp.Toggles.CanGainExpAs079)
            return false;

        __instance._expGainQueue.Enqueue(new Scp079TierManager.ExpQueuedNotification(amount, reason, subject));
        __instance.TotalExp += amount;

        return false;
    }
}