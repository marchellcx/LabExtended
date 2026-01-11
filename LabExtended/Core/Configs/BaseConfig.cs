using System.ComponentModel;

namespace LabExtended.Core.Configs;

/// <summary>
/// The base config of LabExtended
/// </summary>
public class BaseConfig
{
    /// <summary>
    /// Whether or not to show debug messages.
    /// </summary>
    [Description("Toggles logging of debug messages.")]
    public bool DebugEnabled { get; set; }

    /// <summary>
    /// Whether or not to add true color tags to logs.
    /// </summary>
    [Description("Toggles true color log formatting.")]
    public bool TrueColorEnabled { get; set; } = true;

    /// <summary>
    /// Whether or not to show transpiler debug logs (this includes transpilers of other plugins).
    /// </summary>
    [Description("Toggles debug logs of transpilers.")]
    public bool TranspilerDebugEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assembly name is prepended to log messages.
    /// </summary>
    [Description("Whether or not to prepend the assembly name in logs.")]
    public bool PrependAssemblyNameInLogs { get; set; } = true;

    /// <summary>
    /// Whether or not to disable round lock once the player who enabled it leaves the server.
    /// </summary>
    [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
    public bool DisableRoundLockOnLeave { get; set; } = true;

    /// <summary>
    /// Whether or not to disable lobby lock once the player who enabled it leaves the server.
    /// </summary>
    [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
    public bool DisableLobbyLockOnLeave { get; set; } = true;

    /// <summary>
    /// Whether or not to unload all plugins once the server's process quits.
    /// </summary>
    [Description("Whether or not to unload all plugins once the server's process quits.")]
    public bool UnloadPluginsOnQuit { get; set; } = true;
}