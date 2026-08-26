using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API.Custom.Abilities.Enums;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.API.Custom.Abilities;

/// <summary>
/// Represents a base class for creating custom abilities.
/// </summary>
/// <remarks>
/// CustomAbility provides the foundational behavior for extensions to implement
/// in-game abilities, including configurations for cooldowns, durations, uses, and more.
/// It works in conjunction with the <see cref="ExPlayer"/> and is designed to be
/// extended by custom implementations.
/// </remarks>
public abstract class CustomAbility : CustomObject<CustomAbility>
{
    /// <summary>
    /// The active duration of the ability (in seconds).
    /// Determines how long the ability remains effective after activation.
    /// </summary>
    public virtual float Duration { get; set; } = 0f;

    /// <summary>
    /// The cooldown period of the ability (in seconds).
    /// Represents the time required after using the ability before it can be activated again.
    /// </summary>
    public virtual float Cooldown { get; set; } = 0f;

    /// <summary>
    /// Specifies the maximum number of times the ability can be used.
    /// A value of 0 or less indicates that the ability has no usage limit.
    /// </summary>
    public virtual int MaxUses { get; set; } = 0;
    
    /// <summary>
    /// Gets the player that owns the ability.
    /// </summary>
    [YamlIgnore]
    public ExPlayer Player { get; internal set; }

    /// <summary>
    /// Gets the amount of uses.
    /// </summary>
    [YamlIgnore]
    public int Uses { get; private set; } = 0;
    
    /// <summary>
    /// Gets the amount of remaining uses.
    /// </summary>
    [YamlIgnore]
    public int UsesRemaining => MaxUses > 0 ? MaxUses - Uses : int.MaxValue;
    
    /// <summary>
    /// Gets the timestamp at which the ability was last used.
    /// </summary>
    [YamlIgnore]
    public float LastUse { get; internal set; }

    /// <summary>
    /// Gets the timestamp at which the ability can next be used. This value is calculated
    /// based on the cooldown duration and any remaining active duration of the current use,
    /// if applicable. The value is represented as the time since the server started, in seconds.
    /// </summary>
    [YamlIgnore]
    public float NextUse
    {
        get
        {
            var add = 0f;

            if (IsBeingUsed && !IsInstant)
                add += RemainingDuration + Cooldown;
            else
                add += Cooldown;
            
            return Time.realtimeSinceStartup + add;
        }
    }

    /// <summary>
    /// Gets the amount of time, in seconds, that has elapsed since the ability was last used.
    /// Returns 0 if the ability has not been used yet.
    /// </summary>
    [YamlIgnore]
    public float TimeSinceLastUse
    {
        get
        {
            if (LastUse == 0f)
                return 0f;
            
            return Time.realtimeSinceStartup - LastUse;
        }
    }

    /// <summary>
    /// Gets the time (in seconds) until the ability can next be used.
    /// </summary>
    [YamlIgnore]
    public float TimeTillNextUse
    {
        get
        {
            var add = 0f;

            if (IsBeingUsed && !IsInstant)
                add += RemainingDuration + Cooldown;
            else
                add += Cooldown;

            return add;
        }
    }
    
    /// <summary>
    /// Gets the remaining duration (in seconds).
    /// </summary>
    [YamlIgnore]
    public float RemainingDuration { get; internal set; }
    
    /// <summary>
    /// Gets the remaining cooldown (in seconds).
    /// </summary>
    [YamlIgnore]
    public float RemainingCooldown { get; internal set; }
    
    /// <summary>
    /// Whether or not the ability is being used.
    /// </summary>
    [YamlIgnore]
    public bool IsBeingUsed => RemainingDuration > 0f;
    
    /// <summary>
    /// Whether or not the ability is instant use.
    /// </summary>
    [YamlIgnore]
    public bool IsInstant => Duration <= 0f;
    
    /// <summary>
    /// Whether or not the ability has a limited use count.
    /// </summary>
    [YamlIgnore]
    public bool IsLimited => MaxUses > 0;
    
    /// <summary>
    /// Whether or not the ability is in cooldown.
    /// </summary>
    [YamlIgnore]
    public bool IsInCooldown => RemainingCooldown > 0f;
    
    /// <summary>
    /// Whether or not the ability has run out of uses.
    /// </summary>
    [YamlIgnore]
    public bool IsOutOfUses => IsLimited && Uses >= MaxUses;

    /// <summary>
    /// Gets the status of the ability.
    /// </summary>
    [YamlIgnore]
    public AbilityStatus Status
    {
        get
        {
            if (IsBeingUsed)
                return AbilityStatus.InUse;

            if (IsOutOfUses)
                return AbilityStatus.OutOfUses;

            if (IsInCooldown)
                return AbilityStatus.InCooldown;

            return AbilityStatus.ReadyToUse;
        }
    }

    /// <summary>
    /// Called when the ability is added to the player for the first time.
    /// Override this method to define custom logic or initialization steps
    /// that should occur when the ability is assigned.
    /// </summary>
    public virtual void OnAdded()
    {
        
    }

    /// <summary>
    /// Executes logic when the custom ability is removed from the associated player.
    /// </summary>
    /// <remarks>
    /// This method is intended to be overridden in derived classes to define specific behavior
    /// that should occur when the ability is no longer assigned to a player. It can be used for
    /// cleanup, state reset, or other custom removal processes.
    /// </remarks>
    public virtual void OnRemoved()
    {
        
    }

    /// <summary>
    /// Removes the current ability from the associated player and resets its state.
    /// </summary>
    /// <remarks>
    /// This method detaches the ability from the player, clears its state, and sets all internal values such as
    /// cooldown, duration, and usage count to their defaults. It also invokes the <c>OnRemoved</c> callback to perform custom removal logic.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the ability is not associated with a valid player or reference hub.</exception>
    public void Remove()
    {
        if (Player?.ReferenceHub != null)
        {
            Player.customAbilities.Remove(GetType());
            
            OnRemoved();

            Player = null!;

            RemainingDuration = 0f;
            RemainingCooldown = 0f;

            Uses = 0;
        }
    }

    /// <summary>
    /// Modifies the number of uses for the current ability by the specified value.
    /// </summary>
    /// <param name="value">The value by which to modify the number of uses. It can be positive or negative.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the ability does not have a usage limit (i.e., <see cref="MaxUses"/> is less than 1).
    /// </exception>
    public void ModifyUses(int value)
    {
        if (MaxUses < 1)
            throw new InvalidOperationException("This ability does not have a usage limit.");
        
        Uses += value;
    }

    /// <summary>
    /// Modifies the remaining cooldown of the ability by the specified value.
    /// </summary>
    /// <param name="value">The value by which to modify the cooldown. Can be positive to increase the cooldown or negative to decrease it.</param>
    /// <exception cref="InvalidOperationException">Thrown if the ability does not have a cooldown configured.</exception>
    public void ModifyCooldown(float value)
    {
        if (Cooldown > 0f)
        {
            RemainingCooldown += value;
        }
        else
        {
            throw new InvalidOperationException("This ability does not have a cooldown.");
        }
    }

    /// <summary>
    /// Modifies the remaining duration of the ability if it has a valid duration.
    /// </summary>
    /// <param name="value">The value to add to the remaining duration. Can be negative to reduce the duration.</param>
    /// <exception cref="InvalidOperationException">Thrown when the ability does not have a duration defined.</exception>
    public void ModifyDuration(float value)
    {
        if (Duration > 0f)
        {
            RemainingDuration += value;
        }
        else
        {
            throw new InvalidOperationException("This ability does not have a duration.");
        }   
    }

    /// <summary>
    /// Resets the ability's usage count to 0.
    /// </summary>
    public void ResetUses()
        => Uses = 0;
    
    /// <summary>
    /// Resets the ability's remaining cooldown to 0.
    /// </summary>
    public void ResetCooldown()
        => RemainingCooldown = 0f;

    /// <summary>
    /// Marks the completion of the current usage of the ability by ending its active duration.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to complete usage of an ability that is instant.
    /// </exception>
    public void CompleteUsage()
    {
        if (IsInstant)
            throw new InvalidOperationException("This ability is instant.");

        RemainingDuration = 0f;
    }
    
    /// <summary>
    /// Attempts to use the ability, considering its current state and restrictions such as cooldown and usage limits.
    /// </summary>
    /// <param name="force">Indicates whether the ability should be forcibly used, bypassing certain restrictions like cooldown and active usage.</param>
    /// <returns>An <see cref="AbilityError"/> value representing the result of the attempt, such as success or a specific error condition.</returns>
    public AbilityError TryUse(bool force)
    {
        if (!force)
        {
            if (IsBeingUsed)
                return AbilityError.BeingUsed;

            if (IsOutOfUses)
                return AbilityError.NoMoreUses;
            
            if (IsInCooldown)
                return AbilityError.InCooldown;
        }

        if (IsBeingUsed)
            Cancel(false, false);
        
        Uses++;
        
        LastUse = Time.realtimeSinceStartup;

        RemainingCooldown = 0f;
        RemainingDuration = 0f;
        
        if (IsInstant)
        {
            OnUsed();

            if (Cooldown > 0f)
            {
                RemainingCooldown = Cooldown;
                
                OnEnteredCooldown();
            }

            if (Uses == MaxUses)
            {
                OnRanOutOfUses();
            }
        }
        else
        {
            RemainingDuration = Duration;
            
            OnStartedUsing();
        }
        
        return AbilityError.None;
    }

    /// <summary>
    /// Cancels the ongoing use of the ability, optionally applying cooldown and counting the usage.
    /// </summary>
    /// <param name="applyCooldown">Determines whether the ability's cooldown should be applied.</param>
    /// <param name="countUse">Specifies whether the ability's use should be counted towards its maximum uses.</param>
    /// <returns>True if the ability was successfully canceled; otherwise, false.</returns>
    public bool Cancel(bool applyCooldown, bool countUse)
    {
        if (!IsBeingUsed)
            return false;

        RemainingDuration = 0f;
        RemainingCooldown = 0f;
        
        OnCancelled();

        if (applyCooldown && Cooldown > 0f)
        {
            RemainingCooldown = Cooldown;
            
            OnEnteredCooldown();
        }

        if (countUse)
        {
            Uses++;
            
            if (Uses == MaxUses)
                OnRanOutOfUses();
        }

        return true;
    }

    /// <summary>
    /// Invoked when the ability enters the cooldown period.
    /// </summary>
    public virtual void OnEnteredCooldown()
    {
        
    }

    /// <summary>
    /// Invoked when the ability exits the cooldown period.
    /// </summary>
    public virtual void OnExitedCooldown()
    {
        
    }

    /// <summary>
    /// Invoked when the ability runs out of uses.
    /// </summary>
    public virtual void OnRanOutOfUses()
    {
        
    }

    /// <summary>
    /// Invoked when the ability is cancelled.
    /// </summary>
    public virtual void OnCancelled()
    {
        
    }

    /// <summary>
    /// Invoked when the ability starts being used. This method is triggered immediately after the ability is successfully initialized and begins its active duration.
    /// </summary>
    public virtual void OnStartedUsing()
    {
        
    }

    /// <summary>
    /// Executed when the ability finishes being used. This method is called automatically after the ability's duration ends.
    /// </summary>
    public virtual void OnFinishedUsing()
    {
        
    }

    /// <summary>
    /// Executed during each update while the ability is being used.
    /// </summary>
    public virtual void OnUsingUpdate()
    {
        
    }

    /// <summary>
    /// Called periodically during the cooldown phase of the ability.
    /// Enables behaviors or updates specific to cooldown progression.
    /// </summary>
    public virtual void OnCooldownUpdate()
    {
        
    }

    /// <summary>
    /// Gets called when the ability is used.
    /// </summary>
    public virtual void OnUsed()
    {
        
    }

    /// <summary>
    /// Updates the state of the ability. Handles the progression of duration while the ability is being used,
    /// the cooldown period after the ability is finished, and the transitions between these states.
    /// </summary>
    /// <remarks>
    /// If the ability is currently being used, the method decreases the remaining duration, invokes
    /// <c>OnUsingUpdate</c> during usage, and manages the transition to cooldown or out-of-uses states
    /// after usage ends.
    /// 
    /// If the ability is not being used but is in cooldown, the method decreases the remaining cooldown
    /// and invokes <c>OnExitedCooldown</c> when the cooldown finishes.
    /// </remarks>
    public virtual void OnUpdate()
    {
        if (IsBeingUsed)
        {
            RemainingDuration -= Time.deltaTime;
            
            OnUsingUpdate();

            if (RemainingDuration <= 0f)
            {
                OnFinishedUsing();

                if (Cooldown > 0f)
                {
                    RemainingCooldown = Cooldown;
                    
                    OnEnteredCooldown();
                }

                if (IsOutOfUses)
                {
                    OnRanOutOfUses();
                }
            }
        }
        else
        {
            if (RemainingCooldown > 0f)
            {
                RemainingCooldown -= Time.deltaTime;

                OnCooldownUpdate();
                
                if (RemainingCooldown <= 0f)
                {
                    OnExitedCooldown();
                }
            }
        }
    }

    /// <summary>
    /// Invoked when the player's role changes. Allows implementing custom logic to handle role transitions for the player.
    /// </summary>
    /// <param name="args">The event arguments containing information about the role change, such as the player instance and previous role details.</param>
    public virtual void OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        
    }

    private static void _OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;

        var list = ListPool<KeyValuePair<Type, CustomAbility>>.Shared.Rent();
        var count = player.customAbilities.Count;
        
        list.AddRange(player.customAbilities);
        
        foreach (var kvp in list)
            kvp.Value.OnChangedRole(args);

        if (player.customAbilities.Count != count)
        {
            player.customAbilities.Clear();
            player.customAbilities.AddRange(list);
        }
        
        ListPool<KeyValuePair<Type, CustomAbility>>.Shared.Return(list);

        foreach (var kvp in RegisteredObjects)
            kvp.Value.OnChangedRole(args);
    }

    internal static void Initialize()
    {
        PlayerEvents.ChangedRole += _OnChangedRole;   
    }
}