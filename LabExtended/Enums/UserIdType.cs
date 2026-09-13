namespace LabExtended.Enums;

/// <summary>
/// Represents the type of user ID.
/// </summary>
public enum UserIdType
{
    /// <summary>
    /// Indicates that the user ID is a Steam ID (ends with @steam).
    /// </summary>
    Steam,

    /// <summary>
    /// Indicates that the user ID is a Discord ID (ends with @discord).
    /// </summary>
    Discord,

    /// <summary>
    /// Indicates that the user ID is a Northwood ID (ends with @northwood).
    /// </summary>
    Northwood,

    /// <summary>
    /// Indicates that the user ID is a server ID (ID_DEDICATED).
    /// </summary>
    Server,

    /// <summary>
    /// Indicates that the user ID is a dummy ID (ID_Dummy).
    /// </summary>
    Dummy,

    /// <summary>
    /// Indicates that the user ID type is unknown or unrecognized.
    /// </summary>
    Unknown
}
