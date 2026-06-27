using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom;

/// <summary>
/// Used to get / set player switches.
/// </summary>
[Command("toggles", "Gets / sets the value of player toggles.")]
public class TogglesCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("effect", "Toggles ignoring a specific effect on a player.", null)]
    public void Effect(
        [CommandParameter("Target", "The targeted player")] ExPlayer target,
        [CommandParameter("Name", "Name of the effect.")] string name)
    {
        if (!target.Effects.TryGetEffect(name, true, out var effect))
        {
            Fail($"Unknown effect: &1{name}&r");
            return;
        }

        if (target.Toggles.IgnoredEffects.Add(effect.GetType()))
        {
            Ok($"Effect &3{name}&r is now ignored for player {target.ToLogString()}");
            
            effect.ServerDisable();
        }
        else
        {
            target.Toggles.IgnoredEffects.Remove(effect.GetType());
            
            Ok($"Effect &3{name}&r is no longer ignored for player {target.ToLogString()}");
        }
    }

    [CommandOverload("list", "Lists all toggles and their current values on a specific player.", null)]
    public void List(
        [CommandParameter("Target", "The player to view toggles of.")] ExPlayer? target = null)
    {
        target ??= Sender;

        var type = typeof(SwitchContainer);
        var props = type.GetAllProperties()
            .Where(p => p.PropertyType == typeof(bool))
            .OrderBy(p => p.Name);

        Ok(x =>
        {
            x.AppendLine($"Showing toggles of: {target.ToLogString()}");
            x.AppendLine();

            foreach (var prop in props)
                x.AppendLine($"{prop.Name}: {(prop.GetValue(target.Toggles) is true ? "&2TRUE&r" : "&1FALSE&r")}");
        });
    }

    /// <summary>
    /// Sets a player's switch.
    /// </summary>
    [CommandOverload("set", "Sets the value of player toggles.", "toggles.set")]
    public void SetToggle(
        [CommandParameter("Target", "The targeted player")] ExPlayer target,
        [CommandParameter("Property", "The property to set")] string property,
        [CommandParameter("Value", "The value of the player toggle.")] bool value)
    {
        var prop = typeof(SwitchContainer).FindProperty(p => p.SetMethod != null
                                                             && string.Equals(property, p.Name,
                                                                 StringComparison.InvariantCultureIgnoreCase));

        if (prop is null)
        {
            Fail($"Property \"{property}\" could not be found.");
            return;
        }
        
        if (prop.PropertyType != typeof(bool))
        {
            Fail($"Only boolean properties can be changed via commands.");
            return;
        }

        if (!Sender.RegexPermission("toggles.set." + prop.Name.ToLowerInvariant()))
        {
            Fail($"You do not have permission to set the \"{prop.Name}\" property.");
            return;
        }

        if ((bool)prop.GetValue(target.Toggles) == value)
        {
            Fail($"Property \"{prop.Name}\" is already set to {value} (for player \"{target.Nickname} ({target.UserId})\").");
            return;
        }
        
        prop.SetValue(target.Toggles, value);
        
        Ok($"Property \"{prop.Name}\" was set to \"{value}\" for player \"{target.Nickname} ({target.UserId})\"");
    }
    
    /// <summary>
    /// Sets a player's switch.
    /// </summary>
    [CommandOverload("get", "Gets the value of player toggles.", "toggles.get")]
    public void GetToggle(
        [CommandParameter("Target", "The targeted player")] ExPlayer target,
        [CommandParameter("Property", "The property to set")] string property)
    {
        var prop = typeof(SwitchContainer).FindProperty(p => p.SetMethod != null
                                                             && string.Equals(property, p.Name,
                                                                 StringComparison.InvariantCultureIgnoreCase));

        if (prop is null)
        {
            Fail($"Property \"{property}\" could not be found.");
            return;
        }
        
        if (prop.PropertyType != typeof(bool))
        {
            Fail($"Only boolean properties can be retrieved via commands.");
            return;
        }
        
        var value = (bool)prop.GetValue(target.Toggles);
        
        Ok($"Property \"{prop.Name}\" is {(value ? "ENABLED" : "DISABLED")} for player \"{target.Nickname} ({target.UserId})\"");
    }
}