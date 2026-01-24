using LabExtended.API;

using LabExtended.Commands.Utilities;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom
{
    /// <summary>
    /// Represents a command that modifies the pitch of a player's voice chat using a specified multiplier.
    /// </summary>
    [Command("pitch", "Modifies the pitch of a player's voice chat.")]
    public class PitchCommand : CommandBase
    {
        [CommandOverload("set", "Sets the pitch of a player's voice chat using a specified multiplier.", "pitch.set")]
        private void Set(
            [CommandParameter("Players", "The list of players to modify the pitch of.")] List<ExPlayer> players,
            [CommandParameter("Pitch", "The value of the pitch multiplier.")] float value)
        {
            this.ForEachExecute(players, player =>
            {
                player.VoicePitch = value;
                return $"Set voice pitch to &3{value}&r";
            });
        }

        [CommandOverload("reset", "Resets the pitch of a player's voice chat to the default value.", "pitch.reset")]
        private void Reset(
            [CommandParameter("Players", "The list of players to reset the pitch of.")] List<ExPlayer> players)
        {
            this.ForEachExecute(players, player =>
            {
                player.VoicePitch = 1f;
                return "Reset voice pitch.";
            });
        }
    }
}