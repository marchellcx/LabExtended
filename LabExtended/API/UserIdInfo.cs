using CentralAuth;

using LabExtended.Enums;
using LabExtended.Extensions;

namespace LabExtended.API;

/// <summary>
/// Represents collected information about a user ID.
/// </summary>
public struct UserIdInfo
{
    /// <summary>
    /// The minimum length of a Steam ID.
    /// </summary>
    public const byte SteamIdLength = 17;

    /// <summary>
    /// The minimum length of a Discord ID.
    /// </summary>
    public const byte DiscordIdLength = 18;

    /// <summary>
    /// Gets the full string of the user ID, including its type.
    /// </summary>
    public string FullId;

    /// <summary>
    /// Gets the string of the user ID, without it's type.
    /// </summary>
    public string ClearId;

    /// <summary>
    /// Gets the type of the user ID (steam, discord, etc.).
    /// </summary>
    public UserIdType Type;

    /// <summary>
    /// The parsed value of the user ID.
    /// </summary>
    public ulong ParsedId;

    /// <summary>
    /// Whether or not the ID belongs to a dummy player.
    /// </summary>
    public bool IsDummy;

    /// <summary>
    /// Whether or not the ID could be parsed (<see cref="ParsedId"/> property).
    /// </summary>
    public bool IsParsable;

    /// <summary>
    /// Whether or not the ID belongs to a player using the Steam authentification.
    /// </summary>
    public bool IsSteam => Type is UserIdType.Steam;

    /// <summary>
    /// Whether or not the ID belongs to the server player.
    /// </summary>
    public bool IsServer => Type is UserIdType.Server;

    /// <summary>
    /// Whether or not the ID belongs to a player using the Discord authentification.
    /// </summary>
    public bool IsDiscord => Type is UserIdType.Discord;

    /// <summary>
    /// Whether or not the ID belongs to a member of the Northwood Staff.
    /// </summary>
    public bool IsNorthwood => Type is UserIdType.Northwood;

    /// <summary>
    /// Whether or not the ID belongs to an online player.
    /// </summary>
    public bool IsPlayer => !IsServer;

    /// <summary>
    /// Gets the string representation of the ID type.
    /// </summary>
    public string TypeString => Type
        .ToString()
        .ToLowerInvariant();

    /// <summary>
    /// Whether or not the ID matches another query.
    /// </summary>
    /// <param name="otherQuery">The other query.</param>
    /// <returns>true if the ID is a match</returns>
    public bool IsMatch(string otherQuery)
        => !string.IsNullOrWhiteSpace(FullId) && GetInfo(otherQuery).FullId == FullId;

    /// <summary>
    /// Whether or not the ID matches another query.
    /// </summary>
    /// <param name="otherInfo">The other query.</param>
    /// <returns>true if the ID is a match</returns>
    public bool IsMatch(UserIdInfo otherInfo)
        => !string.IsNullOrWhiteSpace(FullId) && !string.IsNullOrWhiteSpace(otherInfo.FullId) && otherInfo.FullId == FullId;

    /// <summary>
    /// Gets the clear ID string.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The clear ID string.</returns>
    public static string GetClearId(string query)
        => GetInfo(query).ClearId;

    /// <summary>
    /// Gets the full ID string.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The full ID string.</returns>
    public static string GetFullId(string query)
        => GetInfo(query).FullId;

    /// <summary>
    /// Gets the parsed ID.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The parsed ID.</returns>
    public static ulong GetParsedId(string query)
        => GetInfo(query).ParsedId;

    /// <summary>
    /// Gets the ID type.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The ID type.</returns>
    public static UserIdType GetIdType(string query)
        => GetInfo(query).Type;

    /// <summary>
    /// Whether or not a query is a server player ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a server player's ID</returns>
    public static bool IsServerId(string query)
        => GetInfo(query).IsServer;

    /// <summary>
    /// Whether or not a query is a player ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a player's ID</returns>
    public static bool IsPlayerId(string query)
        => GetInfo(query).IsPlayer;

    /// <summary>
    /// Whether or not a query is a Northwood Staff ID.
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>true if the query is a Northwood Staff's ID.</returns>
    public static bool IsNorthwoodId(string query)
        => GetInfo(query).IsNorthwood;

    /// <summary>
    /// Whether or not a query is a Steam ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a Steam ID</returns>
    public static bool IsSteamId(string query)
        => GetInfo(query).IsSteam;

    /// <summary>
    /// Whether or not a query is a Discord ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a Discord ID</returns>
    public static bool IsDiscordId(string query)
        => GetInfo(query).IsDiscord;

    /// <summary>
    /// Parses a string of a user ID to a <see cref="UserIdInfo"/> struct.
    /// </summary>
    /// <param name="query">The query to parse.</param>
    /// <returns>The parsed struct instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static UserIdInfo GetInfo(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentNullException(nameof(query));

        query = query.Trim();

        var info = new UserIdInfo();

        var idPart = string.Empty;
        var typePart = string.Empty;

        if (query.TrySplit('@', true, 2, out var parts))
        {
            idPart = parts[0];
            typePart = parts[1];
        }
        else
        {
            idPart = query;
            typePart = query;
        }

        if (string.Equals(typePart, PlayerAuthenticationManager.DedicatedId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(typePart, PlayerAuthenticationManager.HostId, StringComparison.OrdinalIgnoreCase))
            info.Type = UserIdType.Server;
        else if (string.Equals(typePart, PlayerAuthenticationManager.DummyId, StringComparison.OrdinalIgnoreCase))
            info.Type = UserIdType.Dummy;
        else if (Enum.TryParse<UserIdType>(typePart, true, out var type))
            info.Type = type;
        else
            info.Type = UserIdType.Unknown;

        info.FullId = query;
        info.ClearId = idPart;

        if (ulong.TryParse(idPart, out var parsedId))
        {
            info.ParsedId = parsedId;
            info.IsParsable = true;

            if (info.Type is UserIdType.Unknown)
            {
                if (query.Length >= SteamIdLength)
                {
                    if (query.StartsWith("765", StringComparison.Ordinal))
                        info.Type = UserIdType.Steam;
                    else if (query.Length >= DiscordIdLength)
                        info.Type = UserIdType.Discord;
                }
            }
        }
        else
        {
            info.ParsedId = 0;
            info.IsParsable = false;
        }

        return info;
    }
}