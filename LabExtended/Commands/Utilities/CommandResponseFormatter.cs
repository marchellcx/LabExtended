using LabApi.Features.Enums;
using LabApi.Events.Arguments.ServerEvents;

using NorthwoodLib.Pools;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Commands.Parameters;

using RemoteAdmin;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Used to format command responses.
/// </summary>
internal static class CommandResponseFormatter
{
    internal static bool TrueColorResponses => ApiLoader.ApiConfig?.CommandSection != null && ApiLoader.ApiConfig.CommandSection.TrueColorResponses;

    internal static void WriteError(this CommandExecutingEventArgs args, string response, string? color = null)
    {
        if (args.CommandType is CommandType.Client)
        {
            if (args.Sender is PlayerCommandSender sender)
            {
                sender.ReferenceHub.gameConsoleTransmission.SendToClient(response.SanitizeTrueColorString(), color ?? "magenta");
            }
        }
        else
        {
            if (args.CommandType is CommandType.Console)
            {
                if (TrueColorResponses)
                {
                    args.Sender.Respond(response.FormatTrueColorString(null, false, false), false);
                }
                else
                {
                    args.Sender.Respond(response.SanitizeTrueColorString(), false);
                }
            }
            else if (TrueColorResponses)
            {
                args.Sender.Respond(response.FormatTrueColorString(null, true, false), false);
            }
            else
            {
                args.Sender.Respond(response.SanitizeTrueColorString(), false);
            }
        }
    }
    
    internal static bool WriteResponse(this CommandContext ctx, out ContinuableCommandBase continuableCommand)
    {
        if (ctx.Response != null)
        {
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(ctx.FormatCommandResponse(TrueColorResponses), ctx.Response is { IsSuccess: true }, true, ctx.Command.Name.ToUpperInvariant());
            }
            else
            {
                ctx.Sender.SendConsoleMessage(ctx.FormatCommandResponse(false), ctx.Response is { IsSuccess: true } ? "green" : "red");
            }

            if (ctx.Response.IsContinued)
            {
                continuableCommand = ctx.Instance as ContinuableCommandBase;
                return continuableCommand != null;
            }
        }

        continuableCommand = null!;
        return false;
    }

    internal static string FormatLikelyCommands(List<CommandData> likelyCommands, string query)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"&1Unable to find a command matching your query (&6{query.Trim()}&r), did you perhaps mean one of these?&r");
            x.AppendLine();

            for (var i = 0; i < likelyCommands.Count; i++)
                x.Append(likelyCommands[i].GetString(false, TrueColorResponses));
        });
    }
    
    internal static string FormatCommandResponse(this CommandContext ctx, bool trueColor)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (trueColor)
            {
                if (ctx.Type is CommandType.Console)
                {
                    x.Append(ctx.Response.Content.FormatTrueColorString("6", false, false));
                }
                else
                {
                    if (ctx.Type is CommandType.Client)
                    {
                        x.Append("<color=black>[");
                        x.Append(ctx.Command.Name.ToUpperInvariant());
                        x.Append("]</color> ");
                    }

                    x.Append(ctx.Response.Content.FormatTrueColorString(null, true, false));
                }
            }
            else
            {
                if (ctx.Type is CommandType.Console)
                {
                    x.Append(ctx.Response.Content.SanitizeTrueColorString());
                }
                else
                {
                    if (ctx.Type is CommandType.Client)
                    {
                        x.Append("[");
                        x.Append(ctx.Command.Name.ToUpperInvariant());
                        x.Append("] ");
                    }

                    x.Append(ctx.Response.Content.SanitizeTrueColorString());
                }
            }
        });
    }

    internal static string FormatExceptionResponse(this CommandContext ctx, Exception ex)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("&0[");
                x.Append(ctx.Command.Name.ToUpperInvariant());
                x.Append("]&r ");
            }

            x.AppendLine("&3Failed while invoking command:&r &1");
            x.Append(ex.Message);
            x.AppendLine("&r");
        });
    }

    internal static string FormatMissingPermissionsFailure(string requiredPermission, string commandName, CommandType type)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (type is CommandType.Console)
            {
                x.Append("&0[");
                x.Append(commandName.ToUpperInvariant());
                x.Append("]&r ");
            }

            x.Append("&1You are missing the required&r &3");
            x.Append(requiredPermission);
            x.AppendLine("&r &1permission to execute this command.&r");
        });
    }

    internal static string FormatUnknownOverloadFailure(CommandData commandData)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine("&1Unknown overload, try using one of these:&r");
            x.AppendLine(commandData.GetString(false, TrueColorResponses));
        });
    }

    internal static string FormatMissingArgumentsFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("&0[");
                x.Append(ctx.Command.Name.ToUpperInvariant());
                x.Append("]&r ");
            }

            x.Append("&1Missing required command arguments!&r");

            for (var i = 0; i < ctx.Overload.ParameterCount; i++)
            {
                var parameter = ctx.Overload.Parameters[i];

                x.AppendLine();

                x.Append("&3[");
                x.Append(i);
                x.Append("]&r &7");
                x.Append(parameter.Name);
                x.Append("&r &3(");
                x.Append(parameter.Description);
                x.Append(")&r");
            }
        });
    }

    internal static string FormatInvalidArgumentsFailure(this CommandContext ctx, List<CommandParameterParserResult> results)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("&0[");
                x.Append(ctx.Command.Name.ToUpperInvariant());
                x.Append("]&r ");
            }

            x.Append("&1Failed while parsing command arguments!&r");

            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (result.Parameter != null)
                {
                    x.AppendLine();

                    x.Append("&3[");
                    x.Append(i);
                    x.Append("]&r &7");
                    x.Append(result.Parameter.Name);
                    x.Append("&r:");

                    if (result.Success)
                    {
                        x.Append(" &2OK&r");
                    }
                    else
                    {
                        x.Append(" &1");
                        x.Append(result.Error);
                        x.Append("&r");

                        if (!string.IsNullOrEmpty(result.Parameter.Description))
                        {
                            x.Append(" &3(");
                            x.Append(result.Parameter.Description);
                            x.Append(")&r");
                        }
                    }
                }
            }
        });
    }

    internal static string FormatTokenParserFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("&0[");
                x.Append(ctx.Command.Name.ToUpperInvariant());
                x.Append("]&r ");
            }

            x.Append("&1Failed while parsing command line tokens!&r");
        });
    }
}