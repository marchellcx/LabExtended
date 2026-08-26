using LabExtended.API;
using LabExtended.API.Custom.Abilities;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom;

/// <summary>
/// Represents a command handler for managing custom abilities.
/// </summary>
[Command("customabilities", "Commands for custom abilities.", "customa", "ca", "cabilities", "customability", "cability")]
public class CustomAbilitiesCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("all", "Lists all registered abilities.", "customabilities.all")]
    private void All()
    {
        if (CustomAbility.RegisteredObjects.Count < 1)
        {
            Fail("No abilities are registered.");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine();
            x.AppendLine("Registered abilities:");

            foreach (var kvp in CustomAbility.RegisteredObjects)
                x.AppendLine($"[{kvp.Key}] {kvp.Value.GetType().Name}");
        });
    }
    
    [CommandOverload("list", "Lists all abilities of a player.", "customabilities.list")]
    private void List(
        [CommandParameter("Target", "The player whose abilities should be listed.")] ExPlayer player,
        [CommandParameter("Enabled", "Whether to list only enabled abilities.")] bool enabled)
    {
        if (player.Abilities.Count < 1)
        {
            Fail($"Player {player.ToCommandString()} has no abilities.");
        }
        else
        {
            Ok(x =>
            {
                x.AppendLine();
                x.AppendLine($"Showing abilities of player {player.ToCommandString()}:");

                foreach (var kvp in player.Abilities)
                {
                    if (!kvp.Value.IsEnabled && enabled)
                        continue;

                    x.AppendLine($"[&3{kvp.Value.Id}&r] &1{kvp.Key.Name}&r");
                    x.AppendLine($"  - Instant: {kvp.Value.IsInstant.TrueColorFormatBool()}");
                    x.AppendLine($"  - Enabled: {kvp.Value.IsEnabled.TrueColorFormatBool()}");
                    x.AppendLine($"  - Using: {kvp.Value.IsBeingUsed.TrueColorFormatBool()}");
                    x.AppendLine($"  - Cooldown: {kvp.Value.Cooldown}");
                    x.AppendLine($"  - Duration: {kvp.Value.Duration}");
                    x.AppendLine($"  - Max Uses: {kvp.Value.MaxUses}");
                    x.AppendLine($"  - Uses: {kvp.Value.Uses}");
                    x.AppendLine($"  - Remaining Uses: {kvp.Value.UsesRemaining}");
                    x.AppendLine($"  - Remaining Cooldown: {kvp.Value.RemainingCooldown}s");
                    x.AppendLine($"  - Remaining Duration: {kvp.Value.RemainingDuration}s");
                    x.AppendLine($"  - Time Since Last Use: {kvp.Value.TimeSinceLastUse}s");
                    x.AppendLine($"  - Time Till Next Use: {kvp.Value.TimeTillNextUse}s");
                    x.AppendLine($"  - Status: {kvp.Value.Status}");
                }
            });
        }
    }

    [CommandOverload("detail", "Lists details of a specific ability of a player.", "customabilities.detail")]
    private void Detail(
        [CommandParameter("Target", "The player whose ability should be detailed.")] ExPlayer player,
        [CommandParameter("ID", "ID of the ability.")] string id)
    {
        if (player.Abilities.Count < 1)
        {
            Fail($"Player {player.ToCommandString()} has no abilities.");
            return;
        }

        if (!player.Abilities.TryGetFirst(x => x.Value.Id == id, out var kvp))
        {
            Fail($"Player {player.ToCommandString()} has no ability with ID {id}.");
            return;
        }

        Ok(x =>
        {
            x.AppendLine();
            x.AppendLine($"[&3{kvp.Value.Id}&r] &1{kvp.Key.Name}&r ({player.ToCommandString()})");
            x.AppendLine($"  - Instant: {kvp.Value.IsInstant.TrueColorFormatBool()}");
            x.AppendLine($"  - Enabled: {kvp.Value.IsEnabled.TrueColorFormatBool()}");
            x.AppendLine($"  - Using: {kvp.Value.IsBeingUsed.TrueColorFormatBool()}");
            x.AppendLine($"  - Cooldown: {kvp.Value.Cooldown}");
            x.AppendLine($"  - Duration: {kvp.Value.Duration}");
            x.AppendLine($"  - Max Uses: {kvp.Value.MaxUses}");
            x.AppendLine($"  - Uses: {kvp.Value.Uses}");
            x.AppendLine($"  - Remaining Uses: {kvp.Value.UsesRemaining}");
            x.AppendLine($"  - Remaining Cooldown: {kvp.Value.RemainingCooldown}s");
            x.AppendLine($"  - Remaining Duration: {kvp.Value.RemainingDuration}s");
            x.AppendLine($"  - Time Since Last Use: {kvp.Value.TimeSinceLastUse}s");
            x.AppendLine($"  - Time Till Next Use: {kvp.Value.TimeTillNextUse}s");
            x.AppendLine($"  - Status: {kvp.Value.Status}");
        });
    }

    [CommandOverload("enable", "Enables a specific ability of a player.", "customabilities.enable")]
    private void Enable(
        [CommandParameter("Target", "The player for which the ability should be enabled.")] ExPlayer player,
        [CommandParameter("ID", "ID of the ability.")] string id)
    {
        if (!CustomAbility.RegisteredObjects.TryGetValue(id, out var ability))
        {
            Fail($"No ability with ID {id} is registered.");
            return;
        }

        if (player.Abilities.TryGetValue(ability.GetType(), out var customAbility))
        {
            if (customAbility.IsEnabled)
            {
                Fail($"Ability {ability.Id} is already enabled.");
                return;
            }
            
            customAbility.Enable();
            
            Ok($"Enabled ability {ability.Id} for player {player.ToCommandString()}.");
        }
        else
        {
            if (player.AddAbility(ability.GetType(), out customAbility))
            {
                customAbility.Enable();
                
                Ok($"Enabled ability {ability.Id} for player {player.ToCommandString()}.");
            }
            else
            {
                Fail($"Could not enable ability {ability.Id} for player {player.ToCommandString()}.");
            }
        }
    }

    [CommandOverload("disable", "Disables a specific ability of a player.", "customabilities.disable")]
    private void Disable(
        [CommandParameter("Target", "The player for which the ability should be disabůed.")] ExPlayer player,
        [CommandParameter("ID", "ID of the ability.")] string id)
    {
        if (!CustomAbility.RegisteredObjects.TryGetValue(id, out var ability))
        {
            Fail($"No ability with ID {id} is registered.");
            return;
        }

        if (player.Abilities.TryGetValue(ability.GetType(), out var customAbility))
        {
            if (!customAbility.IsEnabled)
            {
                Fail($"Ability {ability.Id} is not enabled.");
                return;
            }
            
            customAbility.Disable();
            
            Ok($"Disabled ability {ability.Id} for player {player.ToCommandString()}.");
        }
        else
        {
            Fail($"Ability {ability.Id} is not added for player {player.ToCommandString()}.");
        }
    }
    
    [CommandOverload("print", "Prints information about a specific ability of a player.", "customabilities.print")]
    private void Print(
        [CommandParameter("Target", "The player for which the ability should be printed.")] ExPlayer player,
        [CommandParameter("ID", "ID of the ability.")] string id)
    {
        if (!CustomAbility.RegisteredObjects.TryGetValue(id, out var ability))
        {
            Fail($"No ability with ID {id} is registered.");
            return;
        }

        if (player.Abilities.TryGetValue(ability.GetType(), out var customAbility))
        {
            if (!customAbility.IsEnabled)
            {
                Fail($"Ability {ability.Id} is not enabled.");
                return;
            }

            Ok(x =>
            {
                customAbility.Print(x);

                if (x.Length < 1)
                    x.AppendLine($"No information available for ability {ability.Id}.");
            });
        }
        else
        {
            Fail($"Ability {ability.Id} is not added for player {player.ToCommandString()}.");
        }
    }
}