namespace LabExtended.API.Custom.Teams.Enums;

/// <summary>
/// Describes reasons for team spawn failures.
/// </summary>
public enum CustomTeamSpawnFail : byte
{
    /// <summary>
    /// There were not enough players to match the minPlayerCount argument.
    /// </summary>
    NotEnoughPlayers,
}