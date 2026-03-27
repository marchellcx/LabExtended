using HarmonyLib;

using InventorySystem;

using LabExtended.Core;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Prevents the base-game from refreshing modifiers.
    /// </summary>
    public static class DisableRefreshModifiersPatch
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RefreshModifiers))]
        private static bool Prefix(Inventory __instance)
            => ApiLoader.ApiConfig == null || ApiLoader.ApiConfig.DisableCustomModifiers;
    }
}