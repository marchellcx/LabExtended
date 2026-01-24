using InventorySystem.Items.ThrowableProjectiles;

using LabExtended.API;
using LabExtended.Extensions;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn
{
    public partial class SpawnCommand
    {
        [CommandOverload("projectile", "Spawns a projectile at a given position with a given velocity.")]
        private void Projectile(
            [CommandParameter("Type", "The type of the item.")] ItemType type,
            [CommandParameter("Amount", "The amount of projectiles to spawn.")] int amount,
            [CommandParameter("FuseTime", "The amount of seconds before the projectile explodes.")] float fuseTime,

            [CommandParameter(ParserType = typeof(ExPlayer), ParserProperty = "Position.Position")]
            [CommandParameter("Position", "The position to spawn the projectile at.")] Vector3 position,

            [CommandParameter(ParserType = typeof(ExPlayer), ParserProperty = "Velocity")]
            [CommandParameter("Velocity", "The velocity of the projectile.")] Vector3 velocity,

            [CommandParameter("Scale", "The scale of the projectile.")] Vector3 scale)
        {
            if (!type.TryGetItemPrefab<ThrowableItem>(out _))
            {
                Fail($"Item &1{type}&r is not a projectile.");
                return;
            }

            for (var x = 0; x < amount; x++)
                ExMap.SpawnProjectile(type, position, scale, velocity, Quaternion.identity, velocity.magnitude, fuseTime);

            Ok($"Spawned &1{amount}&r &3{type}&r projectiles at &3{position.ToPreciseString()}&r at velocity &1{velocity.ToPreciseString()}&r!");
        }
    }
}