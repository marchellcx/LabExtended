using LabExtended.API;

using LabExtended.Commands.Utilities;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom
{
    /// <summary>
    /// Provides server-side commands for modifying player speed settings, including setting and resetting speed
    /// limiters and multipliers for specified players.
    /// </summary>
    [Command("speed", "Modifies player speed values.")]
    public class SpeedCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("limit", "Sets the player's speed limiter.", "speed.limit")]
        private void Limit(
            [CommandParameter("Players", "The list of players to modify the speed limiter of.")] List<ExPlayer> players,
            [CommandParameter("Limit", "The value of the speed limiter.")] float value)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeSpeedLimiter = value;
                return $"Set speed limiter to &3{value}&r";
            });
        }

        [CommandOverload("multiplier", "Sets the player's speed multiplier.", "speed.multiplier")]
        private void Multiplier(
            [CommandParameter("Players", "The list of players to modify the speed multiplier of.")] List<ExPlayer> players,
            [CommandParameter("Multiplier", "The value of the speed multiplier.")] float value)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeSpeedMultiplier = value;
                return $"Set speed multiplier to &3{value}&r";
            });
        }

        [CommandOverload("resetlimit", "Resets the speed limiter", "speed.resetlimit")]
        private void ResetLimit(
            [CommandParameter("Players", "The list of players to reset the speed limiter of.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeSpeedLimiter = null;
                return "Reset speed limiter.";
            });
        }

        [CommandOverload("resetmultiplier", "Resets the speed multiplier", "speed.resetmultiplier")]
        private void ResetMultiplier(
            [CommandParameter("Players", "The list of players to reset the speed multiplier of.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeSpeedMultiplier = null;
                return "Reset speed multiplier.";
            });
        }
    }
}