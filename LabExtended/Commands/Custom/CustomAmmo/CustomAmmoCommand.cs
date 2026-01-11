using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

namespace LabExtended.Commands.Custom.CustomAmmo;

/// <summary>
/// Provides server-side commands for managing custom ammunition in player inventories, including retrieving, setting,
/// adding, removing, and clearing custom ammo amounts.
/// </summary>
/// <remarks>This command is intended for administrative use and allows direct manipulation of custom ammo values
/// for any player on the server. All subcommands default to affecting the command sender if no target player is
/// specified.</remarks>
[Command("customammo", "Custom Ammo management.", "cammo")]
public class CustomAmmoCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("get", "Gets the amount of custom ammo in a player's inventory.", "customammo.get")]
    private void GetCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            var amount = player.Ammo.GetCustomAmmo(ammoId);
            return $"&6{ammoId}x&r of &6{ammoId}&r";
        });
    }

    [CommandOverload("set", "Sets a specific amount of custom ammo in a player's inventory.", "customammo.set")]
    private void SetCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to set.")] int amount,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            player.Ammo.SetCustomAmmo(ammoId, amount);
            return $"&3{ammoId}&r set to &3{amount}&r";
        });
    }
    
    [CommandOverload("add", "Adds a specific amount of custom ammo to a player's inventory.", "customammo.add")]
    private void AddCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to add.")] int amount,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            var newAmmo = player.Ammo.AddCustomAmmo(ammoId, amount);
            return $"added &3{amount}&r of &3{ammoId}&r, now: &3{newAmmo}&r";
        });
    }
    
    [CommandOverload("remove", "Removes a specific amount of custom ammo from a player's inventory.", "customammo.remove")]
    private void RemoveCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to remove.")] int amount,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            var newAmmo = player.Ammo.RemoveCustomAmmo(ammoId, amount);
            return $"removed &3{amount}&r of &3{ammoId}&r, now: &3{newAmmo}&r";
        });
    }
    
    [CommandOverload("clear", "Removes all custom ammo from a player's inventory.", "customammo.clear")]
    private void ClearCommand(
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            player.Ammo.ClearCustomAmmo();
            return "&6all custom ammo cleared&r";
        });
    }
}