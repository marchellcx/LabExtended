namespace LabExtended.API.Custom.Abilities.Enums;

/// <summary>
/// Represents an error that occurred while using a custom ability.
/// </summary>
public enum AbilityError
{
    /// <summary>
    /// No error.
    /// </summary>
    None,
    
    /// <summary>
    /// No more uses.
    /// </summary>
    NoMoreUses,
    
    /// <summary>
    /// Ability is on cooldown.
    /// </summary>
    InCooldown,
    
    /// <summary>
    /// Ability is being used.
    /// </summary>
    BeingUsed,
    
    /// <summary>
    /// Other error.
    /// </summary>
    Other,
}