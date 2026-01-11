using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Commands.Utilities
{
    /// <summary>
    /// Provides utility methods for executing actions on collections of players within a command context and reporting
    /// the results.
    /// </summary>
    /// <remarks>This static class contains helper methods intended to simplify common command-related
    /// operations involving multiple players. Methods in this class are designed to ensure consistent result reporting
    /// and error handling when working with player collections in command implementations.</remarks>
    public static class CommandUtils
    {
        /// <summary>
        /// Executes a specified action for each valid player in the provided collection and reports the results using
        /// the command's output mechanism.
        /// </summary>
        /// <remarks>If the players collection is empty or contains no valid players, the command fails
        /// with an appropriate message and no actions are executed. Each player's result is reported individually.
        /// Players with a null ReferenceHub are skipped and noted in the output.</remarks>
        /// <param name="command">The command context used to report execution results. Cannot be null.</param>
        /// <param name="players">The collection of players to execute the action on. Must contain at least one valid player. Cannot be null.</param>
        /// <param name="action">A function to execute for each valid player, which returns a result string to be included in the output.
        /// Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if command, players, or action is null.</exception>
        public static void ForEachExecute(this CommandBase command, IEnumerable<ExPlayer> players, Func<ExPlayer, string> action)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (players == null)
                throw new ArgumentNullException(nameof(players));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (players.GetCountFast() < 1)
            {
                command.Fail("&1No valid players were specified.&r");
                return;
            }

            command.Ok(x =>
            {
                x.AppendLine($"&6Executing command &3{command.CommandData.Name}&r on &3{players.GetCountFast()}&r player(s):");

                foreach (var player in players)
                {
                    if (player?.ReferenceHub == null)
                    {
                        x.AppendLine("- &1Skipped invalid player&r.");
                        continue;
                    }

                    var result = action(player);

                    if (!string.IsNullOrEmpty(result))
                    {
                        x.AppendLine($"- &6{player.ToCommandString()}&r: {result}.");
                    }
                    else
                    {
                        x.AppendLine($"- &6{player.ToCommandString()}&r: empty result");
                    }
                }
            });
        }
    }
}