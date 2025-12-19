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
        [CommandOverload("list", "Lists all registered gamemodes.", null)]
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

        [CommandOverload("active", "Lists all currently active gamemodes (and the queue).", null)]
        private void Active()
        {
            Ok(x =>
            {
                if (CustomGamemode.Current is null)
                {
                    x.AppendLine("- No gamemode is currently active.");
                }
                else
                {
                    x.AppendLine(
                        $"- Gamemode 'ID: {CustomGamemode.Current.Id}' is running for '{CustomGamemode.Current.RunTime}'");
                }

                if (CustomGamemode.Queue.Count > 0)
                {
                    x.AppendLine("- Queue:");
                    
                    foreach (var mode in CustomGamemode.Queue)
                    {
                        if (mode != null)
                        {
                            x.AppendLine($"[ID: {mode.Id}] {mode.GetType().Name}");
                        }
                    }
                }
            });
        }

        [CommandOverload("enable", "Enables a new gamemode.", null)]
        private void Enable(
            [CommandParameter("ID", "The ID of the gamemode to enable.")] string id,
            [CommandParameter("Override", "Whether or not the current gamemode should be stopped to start this one.")] bool overrideCurrent = false)
        {
            if (!CustomGamemode.TryGet(id, out var gamemode))
            {
                Fail($"Unknown gamemode ID: {id}");
                return;
            }

            if (!CustomGamemode.Enable(gamemode, overrideCurrent))
            {
                var index = CustomGamemode.Queue.ToList().IndexOf(gamemode);

                if (index != -1)
                {
                    Ok($"Gamemode '{gamemode.Id}' cannot be activated mid-round and has been added to the queue, position: {index + 1}");
                    return;
                }

                if (CustomGamemode.Current != null)
                {
                    Fail("The current gamemode could not be disabled.");
                    return;
                }
                
                Fail($"Gamemode '{gamemode.Id}' could not be activated.");
                return;
            }

            if (CustomGamemode.Current != null && CustomGamemode.Current == gamemode)
            {
                Ok($"Started gamemode '{gamemode.Id}'");
            }
        }

        [CommandOverload("disable", "Disables the currently active gamemode.", null)]
        private void Disable()
        {
            if (!CustomGamemode.Disable())
            {
                Fail("The current gamemode could not be disabled.");
            }
            else
            {
                Ok("Gamemode has been disabled.");
            }
        }
    }
}