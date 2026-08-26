namespace LabExtended.API.Custom.Abilities.Enums;

/// <summary>
/// Represents the status of a custom ability.
/// </summary>
public enum AbilityStatus
{
    /// <summary>
    /// The ability is currently being used.
    /// </summary>
    InUse,
    
    /// <summary>
    /// The ability is currently on cooldown.
    /// </summary>
    InCooldown,
    
    /// <summary>
    /// The ability is ready to be used.
    /// </summary>
    ReadyToUse,
    
    /// <summary>
    /// The ability has no more uses.
    /// </summary>
    OutOfUses,
}