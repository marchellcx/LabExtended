using HarmonyLib;

using LabExtended.Core;
using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.Patches.Functions.Holidays
{
    /// <summary>
    /// Implements the Scp956CapybaraChance config.
    /// </summary>
    public static class CapybaraChancePatch
    {
        /// <summary>
        /// The number which forces a Capybara model ...........
        /// </summary>
        public const byte CapybaraNumber = 67;
        
        [HarmonyPatch(typeof(Scp956Pinata), nameof(Scp956Pinata.Network_carpincho), MethodType.Setter)]
        private static bool Prefix(Scp956Pinata __instance, ref byte value)
        {
            if (ApiLoader.ApiConfig?.Scp956CapybaraChance is null)
                return true;

            var chance = Mathf.Clamp(ApiLoader.ApiConfig.Scp956CapybaraChance.Value, 0f, 100f);

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
}