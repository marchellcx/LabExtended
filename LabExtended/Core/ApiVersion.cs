using LabExtended.API;
using NiveraAPI.IO.Configs;

namespace LabExtended.Core;

/// <summary>
/// Loader version handler.
/// </summary>
public static class ApiVersion
{
    /// <summary>
    /// The major version part.
    /// </summary>
    public const int Major = 1;

    /// <summary>
    /// The minor version part.
    /// </summary>
    public const int Minor = 4;

    /// <summary>
    /// The build version part.
    /// </summary>
    public const int Build = 0;

    /// <summary>
    /// The patch version part.
    /// </summary>
    public const int Patch = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the loader should ignore version compatibility checks.
    /// </summary>
    [Config("loader", "ignore-version-check", "If true, the loader will ignore version compatibility checks.")]
    public static bool IgnoreVersionCheck { get; set; } = false;

    /// <summary>
    /// Gets the loader's current version.
    /// </summary>
    public static Version Version { get; } = new(Major, Minor, Build, Patch);

    /// <summary>
    /// Gets the game's current version.
    /// </summary>
    public static Version Game { get; } = new(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

    /// <summary>
    /// Gets the loader's game version compatibility.
    /// </summary>
    public static VersionRange? Compatibility { get; } = new VersionRange(new(14, 2, 7));

    /// <summary>
    /// Checks for server version compatibility.
    /// </summary>
    /// <returns>true if this loader version is compatible with this server version.</returns>
    public static bool CheckCompatibility()
    {
        if (IgnoreVersionCheck || !Compatibility.HasValue || Compatibility.Value.InRange(Game)) 
            return true;

        ApiLog.Error("LabExtended", $"Attempted to load for an unsupported game version (&1{Game}&r) - supported: &2{Compatibility.Value}&r");
        return false;
    }
}