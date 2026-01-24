using LabExtended.API;

using LabExtended.Commands.Utilities;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom
{
    /// <summary>
    /// Provides server-side commands for modifying and resetting the stamina usage multiplier for specified players.
    /// </summary>
    /// <remarks>Use this command to adjust how quickly players consume stamina during gameplay. The 'usage'
    /// overload allows setting a custom multiplier, while the 'reset' overload restores the default value. These
    /// commands are intended for server administrators to fine-tune player stamina behavior for gameplay balance or
    /// testing purposes.</remarks>
    [Command("stamina", "Modifies player stamina usage multiplier.")]
    public class StaminaCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("usage", "Modifies the stamina usage multiplier.", "stamina.usage")]
        private void Set(
            [CommandParameter("Players", "The list of players to modify the stamina usage of.")] List<ExPlayer> players,
            [CommandParameter("Usage", "The value of the stamina usage multiplier.")] float value)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeStaminaUsageMultiplier = value;
                return $"Set stamina usage multiplier to &3{value}&r";
            });
        }

        [CommandOverload("reset", "Resets the stamina usage multiplier to the default value.", "stamina.reset")]
        private void Reset(
            [CommandParameter("Players", "The list of players to reset the pitch of.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.FakeStaminaUsageMultiplier = null;
                return "Reset stamina usage multiplier.";
            });
        }
    }
}