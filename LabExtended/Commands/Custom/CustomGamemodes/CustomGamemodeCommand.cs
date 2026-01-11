using LabExtended.API;
using LabExtended.API.Custom.Gamemodes;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomGamemodes
{
    /// <summary>
    /// Represents a command handler for managing the Custom Gamemode API.
    /// Provides functionality for executing management commands related to custom gamemodes.
    /// </summary>
    [Command("customgamemode", "Management commands for the Custom Gamemode API.", "cg")]
    public class CustomGamemodeCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("list", "Lists all registered gamemodes.", "customgamemode.list")]
        private void List()
        {
            if (CustomGamemode.RegisteredObjects.Count < 1)
            {
                Fail("No gamemodes have been registered.");
                return;
            }
            
            Ok(x =>
            {
                x.AppendLine();

                foreach (var pair in CustomGamemode.RegisteredObjects)
                {
                    x.AppendLine($"[{pair.Key}] {pair.Value.GetType().Name}");
                }
            });
        }

        [CommandOverload("detail", "Shows detailed state information about a specific active gamemode.", "customgamemode.detail")]
        private void Detail(
            [CommandParameter("ID", "The ID of the mod to show the state of.")] string id)
        {
            if (!CustomGamemode.TryGet(id, out var gamemode))
            {
                Fail($"Unknown gamemode ID: {id}");
                return;
            }

            if (!gamemode.IsActive)
            {
                Fail($"Gamemode '{gamemode.Id}' is not active.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();
                x.AppendLine($"- ID: {gamemode.Id}");
                x.AppendLine($"- RunTime: {gamemode.RunTime}");

                x.AppendLine();
                x.AppendLine($"Additional Data:");

                gamemode.PrintState(x);
            });
        }

        [CommandOverload("active", "Lists all currently active gamemodes (and the queue).", "customgamemode.active")]
        private void Active()
        {
            Ok(x =>
            {
                var active = CustomGamemode.GetActive();

                if (active.Count < 1)
                {
                    x.AppendLine("No gamemode is currently active.");
                }
                else
                {
                    foreach (var mode in active)
                    {
                        x.AppendLine();
                        x.AppendLine($"- Gamemode 'ID: {mode.Id}' has been active for '{mode.RunTime}'");
                    }
                }
            });
        }

        [CommandOverload("enable", "Enables a new gamemode.", "customgamemode.enable")]
        private void Enable(
            [CommandParameter("ID", "The ID of the gamemode to enable.")] string id)
        {
            if (!CustomGamemode.TryGet(id, out var gamemode))
            {
                Fail($"Unknown gamemode ID: {id}");
                return;
            }

            if (!gamemode.Enable())
            {
                if (gamemode.IsActive)
                {
                    Fail($"Gamemode '{gamemode.Id}' is already active.");
                    return;
                }

                if (!gamemode.CanActivateMidRound && !ExRound.IsWaitingForPlayers)
                {
                    Fail($"Gamemode '{gamemode.Id}' cannot be activated mid-round.");
                    return;
                }

                var active = CustomGamemode.GetActive();

                if (gamemode.IncompatibleGamemodes?.Length > 0 && active.Any(x => gamemode.IncompatibleGamemodes.Contains(x.Id)))
                {
                    var incompatible = string.Join(", ", gamemode.IncompatibleGamemodes.Where(x => active.Any(y => y.Id == x)));

                    Fail($"Gamemode '{gamemode.Id}' is incompatible with the following active gamemodes: {incompatible}");
                    return;
                }

                Fail($"Gamemode '{gamemode.Id}' could not be activated.");
                return;
            }

            Ok($"Started gamemode '{gamemode.Id}'");
        }

        [CommandOverload("disable", "Disables a specific or all active gamemode(s).", "customgamemode.disable")]
        private void Disable(
            [CommandParameter("ID", "The ID of the gamemode to disable (specify * or all for all).")] string id)
        {
            if (id != "*" && id != "all")
            {
                if (!CustomGamemode.TryGet(id, out var gamemode))
                {
                    Fail($"Unknown gamemode ID: {id}");
                    return;
                }

                if (!gamemode.IsActive)
                {
                    Fail($"Gamemode '{gamemode.Id}' is not active.");
                    return;
                }

                if (!gamemode.Disable())
                {
                    Fail($"Gamemode '{gamemode.Id}' could not be disabled.");
                    return;
                }

                Ok($"Disabled gamemode '{gamemode.Id}'");
            }
            else
            {
                var active = CustomGamemode.GetActive();

                if (active.Count < 1)
                {
                    Fail("No gamemode is currently active.");
                    return;
                }

                foreach (var mode in active)
                    mode.Disable();

                Ok("Disabled all active gamemodes.");
            }
        }
    }
}