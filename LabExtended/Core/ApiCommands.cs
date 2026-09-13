using LabApi.Events.Handlers;
using LabApi.Events.Arguments.ServerEvents;

using LabApi.Features.Enums;
using LabExtended.Attributes;

using NiveraAPI.Commands;
using NiveraAPI.Commands.API;
using NiveraAPI.Commands.Results;
using NiveraAPI.Commands.Attributes;
using NiveraAPI.Commands.Interfaces;

using NiveraAPI.Extensions;
using NiveraAPI.IO.Configs;

using System.Reflection;

namespace LabExtended.Core;

public static class ApiCommands
{
    /// <summary>
    /// Gets or sets a value indicating whether the server player should bypass all permission checks.
    /// </summary>
    [Config("commands", "host-permissions", "Whether or not the server player should bypass all permission checks.")]
    public static bool HostPermissions { get; set; } = true;

    /// <summary>
    /// Gets the command manager for handling commands in the LabExtended API.
    /// </summary>
    public static CommandManager<ExPlayer> Manager { get; private set; }

    private static void SubAttributeProcessor(MethodInfo method, CommandParameter parameter, CommandParameter.ParameterAttributes attributes, ParameterSubAttribute parameterSubAttribute)
    {

    }

    private static bool CheckPermissions(CommandContext<ExPlayer> context)
    {
        if (context.Overload.Permissions.Length < 1)
            return true;

        if (context.Sender.IsHost && HostPermissions)
            return true;

        if (context.Overload.RequiresAllPermissions)
            return context.Overload.Permissions.All(perm => context.Sender.CheckPermission(perm));

        return context.Overload.Permissions.Any(perm => context.Sender.CheckPermission(perm));
    }

    private static void OnExecuting(CommandExecutingEventArgs args)
    {
        if (!ExPlayer.TryGet(args.Sender, out var player))
            return;

        var query = string.Concat(args.Command, " ", string.Join(" ", args.Arguments));
        var search = Manager.SearchCommand(query);

        if (!search.WasFound)
        {
            if (args.CommandFound)
                return;

            args.IsAllowed = false;

            player.SendRemoteAdminMessage($"Command &3{args.CommandName}&r not found.", false, tag: args.CommandName.ToUpper());

            if (search.PossibleOverloads?.Length > 0)
            {

            }
        }
        else
        {
            Manager.ExecuteSearch(ref search, player);
        }
    }

    private static void OnFailed(CommandContext<ExPlayer> context, CommandSearchResult<ExPlayer> search)
    {
        OnAwaited(context, search, null!);
    }

    private static void OnAwaited(CommandContext<ExPlayer> context, CommandSearchResult<ExPlayer> search, IAwaiter<ExPlayer> awaiter)
    {
        var split = search.SourceQuery.Split(' ');
        var args = split.ToSegment(0, split.Length);

        var text = string.Empty;
        var success = false;

        if (context.Result is TextResult textResult)
        {
            text = textResult.Text;
            success = textResult.Success;
        }
        else if (context.Result is ErrorResult errorResult)
        {
            text = errorResult.Message!;
            success = errorResult.Success;

            if (errorResult.Exception != null)
            {
                if (string.IsNullOrEmpty(text))
                {
                    text = errorResult.Exception.ToString();
                }
                else
                {
                    text += $"\n{errorResult.Exception}";
                }
            }
            else if (string.IsNullOrEmpty(text))
            {
                text = "An unknown error occurred.";
            }
        }
        else if (context.Result is MissingPermissionsResult missingPermissionsResult)
        {
            text = $"Missing permissions: {string.Join("&r, &3", missingPermissionsResult.Permissions)}";
            success = missingPermissionsResult.Success;
        }
        else
        {
            text = "Unknown result type.";
            success = false;
        }

        context.Sender.SendRemoteAdminMessage(text, success, tag: string.Join("_", context.Overload.Name).ToUpper());

        ServerEvents.OnCommandExecuted(new(context.Sender.ReferenceHub.queryProcessor._sender, CommandType.RemoteAdmin, null!, args, success, text));
    }

    [LoaderInitialize]
    private static void Initialize()
    {
        Manager = new();

        Manager.CheckPermissions = CheckPermissions;
        Manager.SubAttributeProcessor = SubAttributeProcessor;

        foreach (var asm in ApiLoader.GetPluginAssemblies())
        {
            var commands = Manager.RegisterCommands(asm);

            if (commands.Count > 0)
            {
                foreach (var kvp in commands)
                {
                    var overloads = string.Join(", ", kvp.Value.Overloads.Select(o => o.Name));

                    ApiLog.Info("CommandManager", $"Registered command &3{kvp.Value.FullName}&r (&6{kvp.Key.Name}&r) with overloads: {overloads}");
                }
            }
        }

        if (Manager.Commands.Count > 0)
        {
            ApiLog.Info("CommandManager", $"Registered a total of &6{Manager.Commands.Count}&r commands.");

            Manager.Failed += OnFailed;
            Manager.Awaited += OnAwaited;

            ServerEvents.CommandExecuting += OnExecuting;
        }
        else
        {
            ApiLog.Warn("CommandManager", "No commands were registered.");
        }
    }
}
