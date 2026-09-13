using CommandSystem;

using HarmonyLib;

using NiveraAPI.Commands.API;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using RemoteAdmin;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Used to insert commands from <see cref="CommandManager.Commands"/> into the Remote Admin panel.
/// </summary>
public static class RemoteAdminCommandListPatch
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ParseCommandsToStruct))]
    private static bool Prefix(List<ICommand> list, ref QueryProcessor.CommandData[] __result)
    {
        var commands = ListPool<QueryProcessor.CommandData>.Shared.Rent();

        foreach (var command in list)
        {
            var description = command.Description;

            if (string.IsNullOrWhiteSpace(description))
                description = null;
            else if (description.Length > QueryProcessor.CommandDescriptionSyncMaxLength)
                description = description.Substring(0, QueryProcessor.CommandDescriptionSyncMaxLength) + "...";

            var data = new QueryProcessor.CommandData();

            data.Command = command.Command;
            data.Usage = command is IUsageProvider usageProvider ? usageProvider.Usage : null;
            data.Description = description;
            data.AliasOf = null;
            data.Hidden = command is IHiddenCommand;
            
            commands.Add(data);

            if (command.Aliases?.Length > 0)
            {
                for (var i = 0; i < command.Aliases.Length; i++)
                {
                    var alias = command.Aliases[i];
                    var aliasData = new QueryProcessor.CommandData();

                    aliasData.Command = alias;
                    aliasData.Usage = data.Usage;
                    aliasData.Description = data.Description;
                    aliasData.AliasOf = command.Command;
                    aliasData.Hidden = data.Hidden;
                    
                    commands.Add(aliasData);
                }
            }
        }
        
        foreach (var command in CommandManager.Commands)
        {
            Parse(commands, command, command.Path[0]);

            if (command.Aliases.Count > 0)
            {
                foreach (var alias in command.Aliases)
                {
                    Parse(commands, command, alias);
                }
            }
        }

        __result = ListPool<QueryProcessor.CommandData>.Shared.ToArrayReturn(commands);
        return false;
    }

    private static void Parse(List<QueryProcessor.CommandData> commands, CommandInfo<ExPlayer> command, string commandRoot)
    {
        commandRoot = string.Join("_", command.Name);
        
        foreach (var overload in command.Overloads)
            commands.Add(Parse(command, overload, commandRoot));
    }

    private static QueryProcessor.CommandData Parse(CommandInfo<ExPlayer> command, CommandOverload<ExPlayer> overload, string commandRoot)
    {
        var data = new QueryProcessor.CommandData();

        data.Hidden = command.Flags.Contains("IsHidden");
        
        if (overload.Name.Length < 1)
        {
            data.Command = commandRoot;
            data.Description = overload.Description;
        }
        else
        {
            data.Command = string.Concat(commandRoot, "_", string.Join("_", overload.Name));
            data.Description = overload.Description;
        }

        var usage = new string[overload.Parameters.Count];

        for (var i = 0; i < overload.Parameters.Count; i++)
        {
            var parameter = overload.Parameters[i];

            var prefix = "[";
            var postfix = "]";
            var name = parameter.Name;

            if (parameter.IsOptional)
            {
                prefix = "(";
                postfix = ")";
            }

            usage[i] = string.Concat(prefix, name, postfix);
        }

        data.Usage = usage;
        return data;
    }
}