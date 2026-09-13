using HarmonyLib;

using LabExtended.Core;
using LabExtended.Settings;
using NiveraAPI.Utilities;

using System.Reflection;

using UserSettings.ServerSpecific;

using static LabExtended.Settings.SettingsManager;

namespace LabExtended.Patches.Functions.Settings;

/// <summary>
/// Provides Harmony patches to customize the behavior of server-specific settings synchronization for different
/// assemblies.
/// </summary>
public static class VanillaSettingsAdapterPatch
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
    private static bool DefinedSettingsGetterPrefix(ref ServerSpecificSettingBase[] __result)
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        if (!GlobalSettingsByAssembly.TryGetValue(assembly, out __result) || __result == null)
            __result = [];

        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
    private static bool DefinedSettingsSetterPrefix(ref ServerSpecificSettingBase[] value) 
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        if (value == null || value.Length == 0)
            GlobalSettingsByAssembly.Remove(assembly);
        else
            GlobalSettingsByAssembly[assembly] = value;

        return false;
    }


    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Getter)]
    private static bool SendOnJoinFilterGetterPrefix(ref Predicate<ReferenceHub> __result) 
    {
        // todo getter
        return true;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Setter)]
    private static bool SendOnJoinFilterSetterPrefix(ref Predicate<ReferenceHub> value) 
    {
        // todo setter + Implement SendOnJoinFilter check for each assembly (after some sleep)
        return true;
    }


    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
    private static bool SendToAllPrefix()
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        for (var i = 0; i < ExPlayer.Players.Count; i++)
        {
            var player = ExPlayer.Players[i];

            player.SyncSettingsByAssembly(assembly, []);
            player.SyncEntries();
        }

        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
    private static bool SendToSpecificHubPrefix(ReferenceHub hub)
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        if (!ExPlayer.TryGet(hub, out var player))
            return false;

        player.SyncSettingsByAssembly(assembly, []);
        player.SyncEntries();

        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
    private static bool SendToSpecificHubWithSettingsPrefix(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        if (!ExPlayer.TryGet(hub, out var player))
            return false;

        player.SyncSettingsByAssembly(assembly, collection);
        player.SyncEntries();

        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
    private static bool SendToConditionallyPrefix(Func<ReferenceHub, bool> filter)
    {
        var assembly = ReflectionHelper.GetCallerAssembly(2, false, IsIgnoredAssembly);

        if (IsIgnoredAssembly(assembly))
            assembly = ApiLoader.GameAssembly;

        if (GlobalSettingsByAssembly.TryGetValue(assembly, out var settings))
        {
            for (var i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (filter(player.ReferenceHub))
                {
                    player.SyncSettingsByAssembly(assembly, []);
                    player.SyncEntries();
                }
            }
        }

        return false;
    }

    private static bool IsIgnoredAssembly(Assembly? assembly) =>
        assembly == null ||
        assembly.Equals(ApiLoader.GameAssembly) ||
        assembly.Equals(ApiLoader.HarmonyAssembly) ||
        assembly.Equals(ApiLoader.MirrorAssembly) ||
        assembly.Equals(ApiLoader.LabApiAssembly);
}
