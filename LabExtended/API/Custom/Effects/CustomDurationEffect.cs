using LabExtended.Attributes;

using UnityEngine;
using YamlDotNet.Serialization;

namespace LabExtended.API.Custom.Effects;

/// <summary>
/// A subtype of UpdatingCustomEffect which adds a duration property.
/// </summary>
[LoaderIgnore]
public abstract class CustomDurationEffect : CustomTickingEffect
{
    /// <summary>
    /// Gets or sets the effect's duration.
    /// </summary>
    public float Duration { get; set; } = 0f;
    
    /// <summary>
    /// Gets or sets the remaining duration (in seconds).
    /// </summary>
    [YamlIgnore]
    public float RemainingDuration { get; set; } = 0f;

    /// <inheritdoc cref="CustomTickingEffect.Tick"/>
    public override void Tick()
    {
        base.Tick();

        if (!IsActive)
            return;

        RemainingDuration -= Time.deltaTime;

        if (RemainingDuration <= 0f)
        {
            RemainingDuration = 0f;
            
            IsActive = false;
            
            RemoveEffects();
        }
    }
    
    internal override void OnApplyEffects()
    {
        RemainingDuration = Duration;
        IsActive = RemainingDuration > 0f;
        
        if (IsActive)
            base.OnApplyEffects();
    }

    internal override void OnRemoveEffects()
    {
        RemainingDuration = 0f;
        base.OnRemoveEffects();
    }
}