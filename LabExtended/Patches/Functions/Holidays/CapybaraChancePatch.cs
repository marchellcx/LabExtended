using HarmonyLib;

using LabExtended.Utilities;

using NiveraAPI.IO.Configs;

using UnityEngine;

namespace LabExtended.Patches.Functions.Holidays;

/// <summary>
/// Implements the Scp956CapybaraChance config.
/// </summary>
public static class CapybaraChancePatch
{
    /// <summary>
    /// Gets or sets the probability of SCP-956 utilizing the Capybara model.
    /// </summary>
    [Config("misc", "scp-956-capybara-chance", "Sets the chance of SCP-956 using the Capybara model.")]
    public static float? CapybaraChance { get; set; } = null;

    /// <summary>
    /// The number which forces a Capybara model ...........
    /// </summary>
    public const byte CapybaraNumber = 67;
    
    [HarmonyPatch(typeof(Scp956Pinata), nameof(Scp956Pinata.Network_carpincho), MethodType.Setter)]
    private static bool Prefix(Scp956Pinata __instance, ref byte value)
    {
        if (CapybaraChance is null)
            return true;

        var chance = Mathf.Clamp(CapybaraChance.Value, 0f, 100f);

        if (!WeightUtils.GetBool(chance))
        {
            if (value == CapybaraNumber)
            {
                value = 1;
            }
        }
        else
        {
            value = CapybaraNumber;
        }

        return true;
    }
}