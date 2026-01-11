using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Formats command information.
/// </summary>
public static class CommandFormatter
{
    /// <summary>
    /// Appends custom commands to the selected string.
    /// </summary>
    /// <param name="str">The string to append the commands to.</param>
    public static void AppendCommands(ref string str, bool trueColor)
    {
        if (str is null)
            throw new ArgumentNullException(nameof(str));

        if (CommandManager.Commands.Count < 1)
            return;
        
        str += StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine();

            if (trueColor)
            {
                foreach (var command in CommandManager.Commands)
                {
                    if (command.DefaultOverload != null)
                    {
                        x.Append($"&3{command.Name}&r &6({command.Description})&r");

                        if (command.Aliases.Count > 0)
                            x.Append($" &5(aliases: {string.Join(", ", command.Aliases)})&r");

                        x.AppendLine();
                    }

                    foreach (var overload in command.Overloads)
                        x.AppendLine($"&3{command.Name} {overload.Name}&r &6({overload.Description})&r");
                }
            }
            else
            {
                foreach (var command in CommandManager.Commands)
                {
                    if (command.DefaultOverload != null)
                    {
                        x.Append($"{command.Name} ({command.Description})");

                        if (command.Aliases.Count > 0)
                            x.Append($" (aliases: {string.Join(", ", command.Aliases)})");

                        x.AppendLine();
                    }

                    foreach (var overload in command.Overloads)
                        x.AppendLine($"{command.Name} {overload.Name} ({overload.Description})");
                }
            }
        });
    }
    
    /// <summary>
    /// Converts the command into an info-help string.
    /// </summary>
    /// <param name="commandData">The command instance.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetString(this CommandData commandData, bool allDetails, bool trueColor)
    {
        if (commandData is null)
            throw new ArgumentNullException(nameof(commandData));
        
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (allDetails)
            {
                if (trueColor)
                {
                    x.AppendLine($"&7[&r&7COMMAND&r &3{commandData.Name}&r&3]&r");
                    x.AppendLine($"- &7Description&r: &3{commandData.Description}&r");
                    x.AppendLine($"- &7Type&r: &3{commandData.Type.FullName}&r");

                    if (commandData.Permission != null)
                        x.AppendLine($"- &7Permission&r: &1{commandData.Permission}&r");

                    if (commandData.Aliases.Count > 0)
                        x.AppendLine($"- &7Aliases&r: {string.Join("&r, &3", commandData.Aliases)}");

                    x.AppendLine($"- &7Supports Remote Admin&r: {commandData.SupportsRemoteAdmin.TrueColorFormatBool()}");
                    x.AppendLine($"- &7Supports Server Console&r: {commandData.SupportsServer.TrueColorFormatBool()}");
                    x.AppendLine($"- &7Supports Player Console&r: {commandData.SupportsPlayer.TrueColorFormatBool()}");

                    x.AppendLine($"- &7Can Be Continued&r: {commandData.IsContinuable.TrueColorFormatBool()}");

                    if (commandData.TimeOut.HasValue)
                        x.AppendLine($"- &7Time Out&r: &3{commandData.TimeOut.Value}&rs");

                    void AppendOverload(string overloadHeader, CommandOverload overload)
                    {
                        x.AppendLine();
                        x.AppendLine($"-> &7{overloadHeader}&r");

                        if (string.IsNullOrWhiteSpace(overload.Name) ||  (commandData.DefaultOverload != null && overload == commandData.DefaultOverload))
                            x.Append($" -< &7Usage&r: &3{commandData.Name}&r");
                        else
                            x.Append($" -< &7Usage&r: &3{commandData.Name} {overload.Name}&r");

                        if (overload.Parameters.Count > 0)
                        {
                            foreach (var parameter in overload.Parameters)
                            {
                                if (parameter.HasDefault)
                                {
                                    x.Append($" &2({parameter.Name})&r");
                                }
                                else
                                {
                                    x.Append($" &1[{parameter.Name}]&r");
                                }
                            }
                        }

                        x.AppendLine();

                        if (overload.ParameterCount > 0)
                        {
                            x.AppendLine($" -< &7Parameters&r &3({overload.ParameterCount})&r:");

                            for (var y = 0; y < overload.Parameters.Count; y++)
                            {
                                var parameter = overload.Parameters[y];

                                x.Append($"  -< &3[{y}]&r &7{parameter.Name}&r &3({parameter.Description})&r");

                                if (parameter.HasDefault)
                                    x.Append($" &1(default: {parameter.DefaultValue?.ToString() ?? "null"})&r");

                                foreach (var restriction in parameter.Restrictions)
                                    x.Append($"\n   --> &1Restriction&r: &3{restriction}&r");

                                x.AppendLine();
                            }
                        }
                    }

                    if (commandData.DefaultOverload != null)
                        AppendOverload("default", commandData.DefaultOverload);

                    foreach (var overload in commandData.Overloads)
                        AppendOverload($"{overload.Name}", overload);
                }
                else 
                {
                    x.AppendLine($"[COMMAND \"{commandData.Name}\"]");
                    x.AppendLine($"- Description: {commandData.Description}");
                    x.AppendLine($"- Type: {commandData.Type.FullName}");

                    if (commandData.Permission != null)
                        x.AppendLine($"- Permission: {commandData.Permission}");

                    if (commandData.Aliases.Count > 0)
                        x.AppendLine($"- Aliases: {string.Join(", ", commandData.Aliases)}");

                    x.AppendLine($"- Supports Remote Admin: {commandData.SupportsRemoteAdmin}");
                    x.AppendLine($"- Supports Server Console: {commandData.SupportsServer}");
                    x.AppendLine($"- Supports Player Console: {commandData.SupportsPlayer}");

                    x.AppendLine($"- Can Be Continued: {commandData.IsContinuable}");

                    if (commandData.TimeOut.HasValue)
                        x.AppendLine($"- Time Out: {commandData.TimeOut.Value}s");

                    void AppendOverload(string overloadHeader, CommandOverload overload)
                    {
                        x.AppendLine();
                        x.AppendLine($"-> {overloadHeader}");

                        if (string.IsNullOrWhiteSpace(overload.Name) ||
                            (commandData.DefaultOverload != null && overload == commandData.DefaultOverload))
                            x.Append($" -< Usage: \"{commandData.Name}");
                        else
                            x.Append($" -< Usage: \"{commandData.Name} {overload.Name}");

                        if (overload.Parameters.Count > 0)
                        {
                            foreach (var parameter in overload.Parameters)
                            {
                                if (parameter.HasDefault)
                                {
                                    x.Append($" ({parameter.Name})");
                                }
                                else
                                {
                                    x.Append($" [{parameter.Name}]");
                                }
                            }
                        }

                        x.AppendLine("\"");

                        if (overload.ParameterCount > 0)
                        {
                            x.AppendLine($" -< Parameters ({overload.ParameterCount}):");

                            for (var y = 0; y < overload.Parameters.Count; y++)
                            {
                                var parameter = overload.Parameters[y];

                                x.Append($"  -< [{y}] {parameter.Name} ({parameter.Description})");

                                if (parameter.HasDefault)
                                    x.Append($" (default: {parameter.DefaultValue?.ToString() ?? "null"})");

                                foreach (var restriction in parameter.Restrictions)
                                    x.Append($"\n   --> Restriction: {restriction}");

                                x.AppendLine();
                            }
                        }
                    }

                    if (commandData.DefaultOverload != null)
                        AppendOverload("default", commandData.DefaultOverload);

                    foreach (var overload in commandData.Overloads)
                        AppendOverload($"\"{overload.Name}\"", overload);
                }
            }
            else
            {
                if (trueColor)
                {
                    foreach (var overload in commandData.Overloads)
                    {
                        if (string.IsNullOrWhiteSpace(overload.Name) || (commandData.DefaultOverload != null && overload == commandData.DefaultOverload))
                            x.Append($"&3{commandData.Name}&r");
                        else
                            x.Append($"&3{commandData.Name} {overload.Name}&r");

                        if (overload.Parameters.Count > 0)
                        {
                            foreach (var parameter in overload.Parameters)
                            {
                                if (parameter.HasDefault)
                                {
                                    x.Append($" &2({parameter.Name})&r");
                                }
                                else
                                {
                                    x.Append($" &1[{parameter.Name}]&r");
                                }
                            }
                        }

                        x.AppendLine($" &3({(commandData.DefaultOverload != null && commandData.DefaultOverload == overload
                            ? commandData.Description
                            : overload.Description)})&r");
                    }
                }
                else
                {
                    foreach (var overload in commandData.Overloads)
                    {
                        if (string.IsNullOrWhiteSpace(overload.Name) ||
                            (commandData.DefaultOverload != null && overload == commandData.DefaultOverload))
                            x.Append($"\"{commandData.Name}");
                        else
                            x.Append($"\"{commandData.Name} {overload.Name}");

                        if (overload.Parameters.Count > 0)
                        {
                            foreach (var parameter in overload.Parameters)
                            {
                                if (parameter.HasDefault)
                                {
                                    x.Append($" ({parameter.Name})");
                                }
                                else
                                {
                                    x.Append($" [{parameter.Name}]");
                                }
                            }
                        }

                        x.AppendLine($"\" ({(commandData.DefaultOverload != null && commandData.DefaultOverload == overload
                            ? commandData.Description
                            : overload.Description)})");
                    }
                }
            }
        });
    }
}