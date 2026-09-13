using Christmas.Scp2536;

using HarmonyLib;

using NiveraAPI.IO.Configs;

namespace LabExtended.Patches.Functions.Holidays;

/// <summary>
/// Prevents the SCP-2536 tree from spawning (if the Scp2536Disabled config is enabled) by failing the target locator.
/// </summary>
public static class Scp2536SpawnPreventionPatch
{
    /// <summary>
    /// Gets or sets a value indicating whether SCP-2536 spawning should be disabled.
    /// </summary>
    [Config("misc", "scp-2536-disabled", "Whether or not the SCP-2536 should be prevented from spawning.")]
    public static bool IsDisabled { get; set; }

    [HarmonyPatch(typeof(Scp2536Controller), nameof(Scp2536Controller.ServerFindTarget))]
    private static bool Prefix(ref bool __result)
    {
        if (IsDisabled)
        {
            __result = false;
            return false;
        }

        return true;
    }
}