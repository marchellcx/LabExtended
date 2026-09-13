using System.Reflection;

using CommandSystem.Commands.Shared;

using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Loader.Features.Yaml;

using LabExtended.Events;
using LabExtended.Commands;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core.Configs;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using LabExtended.API;

using LabExtended.API.Toys;

using LabExtended.Commands.Utilities;
using LabExtended.Commands.Parameters;

using LabExtended.Patches.Functions;

using LabExtended.Patches.Events.Scp049;
using LabExtended.Patches.Events.Mirror;
using LabExtended.Utilities.Firearms;

using Version = System.Version;

using LabExtended.Patches.Fixes.LabAPI;
using NiveraAPI.IO.Configs;
using Mirror;
using LabApi.Features;
using LabExtended.Custom.Items;
using LabExtended.Custom.Abilities;
using LabExtended.Custom.Teams;
using LabExtended.Audio;
using LabExtended.RemoteAdmin;
using LabExtended.RemoteAdmin.Actions;
using LabExtended.Custom.Effects;
using LabExtended.Custom.Roles;
using LabExtended.Hints;
using LabExtended.Containers;
using LabExtended.Settings;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).

namespace LabExtended.Core;

/// <summary>
/// Responsible for loading LabExtended.
/// </summary>
public class ApiLoader : Plugin
{
    /// <summary>
    /// Initializes a new loader instance.
    /// </summary>
    public ApiLoader()
    {
        Loader = this;
        LoaderPoint();
    }

    /// <summary>
    /// The message that LabAPI prints once it starts enabling plugins.
    /// </summary>
    public const string LoadFinishedMessage = "[LOADER] Enabling all plugins";

    /// <summary>
    /// Whether or not to disable all plugins once the server's process quits.
    /// </summary>
    [Config("misc", "disable-plugins-on-quit", "Whether or not to disable all plugins once the server's process quits.")]
    public static bool DisablePluginsOnQuit = true;

    /// <summary>
    /// Gets the loader's assembly.
    /// </summary>
    public static Assembly Assembly { get; } = typeof(ApiLoader).Assembly;

    /// <summary>
    /// Gets the game assembly.
    /// </summary>
    public static Assembly GameAssembly { get; } = typeof(ServerConsole).Assembly;

    /// <summary>
    /// Gets the Mirror assembly.
    /// </summary>
    public static Assembly MirrorAssembly { get; } = typeof(NetworkBehaviour).Assembly;

    /// <summary>
    /// Gets the LabAPI assembly.
    /// </summary>
    public static Assembly LabApiAssembly { get; } = typeof(LabApiProperties).Assembly;

    /// <summary>
    /// Gets the Harmony assembly.
    /// </summary>
    public static Assembly HarmonyAssembly { get; } = typeof(HarmonyLib.Harmony).Assembly;

    /// <summary>
    /// Gets the loader singleton.
    /// </summary>
    public static ApiLoader Loader { get; private set; }

    /// <summary>
    /// Gets the loader's name.
    /// </summary>
    public override string Name { get; } = "LabExtended";

    /// <summary>
    /// Gets the loader's author.
    /// </summary>
    public override string Author { get; } = "marchellcx";

    /// <summary>
    /// Gets the loader's description.
    /// </summary>
    public override string Description { get; } = "An extended API for LabAPI.";

    /// <inheritdoc cref="Plugin.IsTransparent"/>
    public override bool IsTransparent => true;

    /// <summary>
    /// Gets the loader's current version.
    /// </summary>
    public override Version Version => ApiVersion.Version;

    /// <summary>
    /// Gets the loader's required LabAPI version.
    /// </summary>
    public override Version? RequiredApiVersion { get; } = null;

    /// <summary>
    /// Gets the loader's priority.
    /// </summary>
    public override LoadPriority Priority { get; } = LoadPriority.Highest;

    /// <summary>
    /// Dummy method.
    /// </summary>
    public override void Enable()
    {
        
    }

    /// <summary>
    /// Dummy method.
    /// </summary>
    public override void Disable()
    {
        
    }

    /// <summary>
    /// Gets all plugin assemblies that are currently loaded by LabAPI.
    /// </summary>
    /// <param name="predicate">A predicate to filter the plugins.</param>
    /// <returns>An array of plugin assemblies.</returns>
    public static Assembly[] GetPluginAssemblies(Predicate<Plugin>? predicate = null)
    {
        var hashSet = HashSetPool<Assembly>.Shared.Rent();

        foreach (var kvp in PluginLoader.Plugins)
        {
            if (kvp.Value == null)
                continue;

            if (predicate != null && !predicate(kvp.Key))
                continue;

            hashSet.Add(kvp.Value);
        }

        var array = hashSet.ToArray();

        HashSetPool<Assembly>.Shared.Return(hashSet);
        return array;
    }

    // This method is invoked by the LogPatch when LabAPI logs it's "enabling all plugins" line.
    private static void LogPoint()
    {
        ApiLog.Info("LabExtended", "LabAPI has finished loading, registering plugin hooks.");

        ExServerEvents.Logging -= Internal_Log;

        var loadedAssemblies = ListPool<Assembly>.Shared.Rent();

        foreach (var plugin in PluginLoader.Plugins.Keys)
        {
            try
            {
                if (plugin is null)
                    continue;

                if (Loader != null && plugin == Loader)
                    continue;

                var type = plugin.GetType();
                var assembly = type.Assembly;

                if (!loadedAssemblies.Contains(assembly))
                {
                    loadedAssemblies.Add(assembly);

                    assembly.RegisterUpdates();
                    assembly.RegisterCommands();

                    if (type.HasAttribute<LoaderPatchAttribute>())
                        assembly.ApplyPatches();
                }

                var loadMethod = type.FindMethod("ExtendedLoad");

                loadMethod?.Invoke(loadMethod.IsStatic ? null : plugin, null);

                ApiLog.Info("LabExtended", $"Loaded plugin &3{plugin.Name}&r!");
            }
            catch (Exception ex)
            {
                ApiLog.Error("LabExtended", $"Failed while loading plugin &3{plugin.Name}&r:\n{ex.ToColoredString()}");
            }
        }

        try
        {
            loadedAssemblies.ForEach(x => x.InvokeStaticMethods(
                y => y.HasAttribute<LoaderInitializeAttribute>(out var attribute) && attribute.Priority >= 0,
                y => y.GetCustomAttribute<LoaderInitializeAttribute>().Priority, false));
        }
        catch
        {
            // ignored, logged by the extension
        }

        ListPool<Assembly>.Shared.Return(loadedAssemblies);

        InvokeApi(false);

        Assembly.ApplyPatches();

        ReflectionUtils.Load();

        ApiLog.Info("LabExtended", "Loading finished!");
    }

    // This method is invoked by the loader.
    private static void LoaderPoint()
    {
        ApiLog.Info("LabExtended", $"Loading version &1{ApiVersion.Version}&r ..");

        if (!ApiVersion.CheckCompatibility())
            return;

        if (!string.IsNullOrWhiteSpace(BuildInfoCommand.ModDescription))
            BuildInfoCommand.ModDescription += $"\nLabExtended v{ApiVersion.Version}";
        else
            BuildInfoCommand.ModDescription = $"\nLabExtended v{ApiVersion.Version}";

        ExServerEvents.Logging += Internal_Log;
        ExServerEvents.Quitting += Internal_Quit;

        Assembly.RegisterUpdates();
        Assembly.RegisterCommands();

        InvokeApi(true);

        ApiLog.Info("LabExtended", "Waiting for LabAPI ..");
    }

    private static void InvokeApi(bool isPreload)
    {
        if (isPreload)
        {
            LabApiNullPluginVersionFix.Internal_Init();
            SwitchContainer.Internal_Init();
            LogPatch.Internal_Init();
        }
        else
        {
            UnityLoop.Internal_InitFirst();
            PlayerUpdateHelper.Internal_Init();

            ThreadUtils.Internal_Init();
            TimingDispatcher.Internal_Init();

            CustomRole.Initialize();
            CustomItem.Internal_Init();
            CustomFirearm.Internal_Init();
            CustomProjectile.Internal_Init();
            CustomAbility.Initialize();

            CustomTeamHandler.Internal_Init();
            CustomPlayerEffect.Internal_Init();
            CustomTeamRegistry.Internal_Init();

            Elevator.Internal_Init();

            ExMap.Internal_Init();
            ExRound.Internal_Init();
            ExServer.Internal_Init();
            ExTeslaGate.Internal_Init();
            ExServerEvents.Internal_Init();

            HintController.Internal_Init();

            RemoteAdminActionProvider.Internal_Init();
            RemoteAdminController.Internal_Init();

            SettingsManager.Internal_Init();

            AdminToy.Internal_Init();

            CommandManager.Internal_Init();
            CommandParameterParserUtils.Internal_Init();
            CommandPropertyUtils.Internal_Init();

            InternalEvents.Internal_Init();

            Scp049CancellingResurrectionPatch.Internal_Init();
            MirrorSetSyncVarPatch.Internal_Init();

            FirearmModuleCache.Internal_Init();

            AudioSettings.Initialize();

            UnityLoop.Internal_InitLast(); // has to be last
        }
    }

    private static void Internal_Log(string logMessage)
    {
        if (logMessage is null || !logMessage.EndsWith(LoadFinishedMessage))
            return;

        LogPoint();
    }

    private static void Internal_Quit()
    {
        ExServerEvents.Quitting -= Internal_Quit;

        if (!DisablePluginsOnQuit)
            return;

        foreach (var plugin in PluginLoader.Plugins.Keys)
        {
            if (plugin is null)
                continue;

            if (Loader != null && plugin == Loader)
                continue;

            ApiLog.Debug("LabExtended", $"Unloading plugin &6{plugin.Name}&r ..");

            try
            {
                plugin.UnregisterCommands();
                plugin.Disable();
            }
            catch (Exception ex)
            {
                ApiLog.Error("LabExtended", $"Could not unload plugin &1{plugin.Name}&r:\n{ex.ToColoredString()}");
            }
        }
    }
}