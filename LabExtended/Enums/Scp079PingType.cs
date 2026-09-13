namespace LabExtended.Enums;

/// <summary>
/// An enum for specifying the type of ping for SCP-079.
/// </summary>
public enum Scp079PingType : byte
{
    /// <summary>
    /// Pinging a generator.
    /// </summary>
    Generator,

    /// <summary>
    /// Pinging a projectile.
    /// </summary>
    Projectile,

    /// <summary>
    /// Pinging a MicroHID device.
    /// </summary>
    MicroHid,

    /// <summary>
    /// Pinging a human player.
    /// </summary>
    Human,

    /// <summary>
    /// Pinging an elevator.
    /// </summary>
    Elevator,

    /// <summary>
    /// Pinging a door.
    /// </summary>
    Door,

    /// <summary>
    /// Pinged nothing, or the ping type is unknown.
    /// </summary>
    Default
}