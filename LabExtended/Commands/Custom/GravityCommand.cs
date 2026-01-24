using LabExtended.API;

using LabExtended.Commands.Utilities;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using UnityEngine;

using LabExtended.API.Containers;

namespace LabExtended.Commands.Custom
{
    /// <summary>
    /// Provides server-side commands for modifying or resetting the gravity of players within the game environment.
    /// </summary>
    /// <remarks>This command class enables administrators to set a custom gravity vector for one or more
    /// players, or to restore their gravity to the default value. Gravity is represented as a three-dimensional vector,
    /// allowing for advanced control over player movement and physics. These commands are intended for use in
    /// server-side scenarios and require appropriate permissions.</remarks>
    [Command("pitch", "Modifies a player's gravity.")]
    public class GravityCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("set", "Sets the gravity of a player.", "gravity.set")]
        private void Set(
            [CommandParameter("Players", "The list of players to modify the gravity of.")] List<ExPlayer> players,
            [CommandParameter("Gravity", "The value of the gravity")] Vector3 gravity)
        {
            this.ForEachExecute(players, player =>
            {
                player.Gravity = gravity;
                return $"Set gravity to &3{gravity.ToPreciseString()}&r";
            });
        }

        [CommandOverload("reset", "Resets the gravity of a player to the default value.", "gravity.reset")]
        private void Reset(
            [CommandParameter("Players", "The list of players to reset the gravity of.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.Gravity = PositionContainer.DefaultGravity;
                return "Reset gravity to default.";
            });
        }
    }
}