using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

namespace LabExtended.Commands.Custom
{
    /// <summary>
    /// Provides server-side commands for managing player invisibility, allowing administrators to make players
    /// invisible to others, target specific visibility relationships, and reset invisibility states.
    /// </summary>
    public class GhostCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("global", "Makes players invisible to others")]
        private void Global(
            [CommandParameter("Players", "List of players to make globally invisible.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.IsInvisible = true;
                return "Set invisible to: Everyone";
            });
        }

        [CommandOverload("target", "Makes specified players invisible to specified targets", "ghost.target")]
        private void Target(
            [CommandParameter("Players", "List of players who will not be seen by players in \"Targets\"")] List<ExPlayer> players,
            [CommandParameter("Targets", "List of players who will not see players in \"Players\"")] List<ExPlayer> targets)
        {
            var targetsString = string.Join(", ", targets.Select(t => t.Nickname));

            this.ForEachExecute(players, player =>
            {
                targets.ForEach(target =>
                {
                    player.MakeInvisibleFor(target);
                });

                return $"Set invisible to: {targetsString}";
            });
        }

        [CommandOverload("resetglobal", "Resets global invisibility for specified players", "ghost.resetglobal")]
        private void ResetGlobal(
            [CommandParameter("Players", "List of players to reset global invisibility for.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.IsInvisible = false;
                return "Set visible to: Everyone";
            });
        }

        [CommandOverload("resettarget", "Resets targeted invisibility for specified players", "ghost.resettarget")]
        private void ResetTarget(
            [CommandParameter("Players", "List of players to reset targeted invisibility for.")] List<ExPlayer> players,
            [CommandParameter("Targets", "List of players to reset invisibility from.")] List<ExPlayer> targets)
        {
            var targetsString = string.Join(", ", targets.Select(t => t.Nickname));

            this.ForEachExecute(players, player =>
            {
                targets.ForEach(target =>
                {
                    player.MakeVisibleFor(target);
                });

                return $"Set visible to: {targetsString}";
            });
        }
    }
}
