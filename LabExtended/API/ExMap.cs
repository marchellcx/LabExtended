using Hazards;

using Interactables;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API.Prefabs;

using LabExtended.Events;
using LabExtended.Events.Mirror;

using LabExtended.Core;
using LabExtended.Extensions;

using MapGeneration;
using MapGeneration.Holidays;
using MapGeneration.Distributors;

using Mirror;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Ragdolls;
using PlayerRoles.PlayableScps.Scp3114;

using PlayerStatsSystem;

using RelativePositioning;

using UnityEngine;

using Utils;
using Utils.Networking;
using Random = UnityEngine.Random;

namespace LabExtended.API;

/// <summary>
/// Map management functions.
/// </summary>
public static class ExMap
{
    private static readonly FacilityZone[] allZones = EnumUtils<FacilityZone>.Values.Except([FacilityZone.None, FacilityZone.Other]).ToArray();
    
    /// <summary>
    /// List of spawned pickups.
    /// </summary>
    public static List<ItemPickupBase> Pickups { get; } = new();

    /// <summary>
    /// List of spawned ragdolls.
    /// </summary>
    public static List<BasicRagdoll> Ragdolls { get; } = new();

    /// <summary>
    /// List of spawned lockers.
    /// </summary>
    public static List<Locker> Lockers { get; } = new();

    /// <summary>
    /// List of locker chambers.
    /// </summary>
    public static List<LockerChamber> Chambers { get; } = new();

    /// <summary>
    /// A dictionary of locker chambers and their parent lockers.
    /// </summary>
    public static Dictionary<LockerChamber, Locker> ChamberToLocker { get; } = new();

    /// <summary>
    /// List of spawned waypoints.
    /// </summary>
    public static IEnumerable<WaypointBase> Waypoints => WaypointBase.AllWaypoints.Where(x => x != null);

    /// <summary>
    /// List of spawned NetID waypoints.
    /// </summary>
    public static IEnumerable<NetIdWaypoint> NetIdWaypoints => NetIdWaypoint.AllNetWaypoints.Where(x => x != null);

    /// <summary>
    /// Amount of ambient clips.
    /// </summary>
    public static int AmbientClipsCount => AmbientSoundPlayer?.clips.Length ?? 0;

    /// <summary>
    /// Gets the AmbientSoundPlayer component.
    /// </summary>
    public static AmbientSoundPlayer? AmbientSoundPlayer { get; private set; }

    /// <summary>
    /// Gets the default color of a room's light.
    /// </summary>
    public static Color DefaultLightColor { get; } = Color.clear;

    /// <summary>
    /// Gets or sets the map's seed.
    /// </summary>
    public static int Seed
    {
        get => SeedSynchronizer.Seed;
        set
        {
            if (SeedSynchronizer.MapGenerated)
                return;

            SeedSynchronizer.Seed = value;
        }
    }

    #region Holidays - Halloween
    /// <summary>
    /// Gets or sets whether or not the Hubert Moon skybox variation is active.
    /// </summary>
    public static bool IsHubertSkyboxActive
    {
        get => SkyboxHubert._singleton != null && SkyboxHubert._singleton.NetworkHubert;
        set
        {
            if (SkyboxHubert._singleton == null)
                return;

            SkyboxHubert._singleton.NetworkHubert = value;
        }
    }

    /// <summary>
    /// Spawns an instance of Hubert Moon if available.
    /// </summary>
    /// <remarks>This method is obsolete. Hubert Moon is no longer available in the game and this method will
    /// always return <see langword="null"/>.</remarks>
    /// <returns>A <see cref="HubertMoon"/> instance if Hubert Moon is available; otherwise, <see langword="null"/>.</returns>
    [Obsolete("Hubert Moon is no longer available in the game.")]
    public static HubertMoon? SpawnHubertMoon()
        => null;
    #endregion

    /// <summary>
    /// Broadcasts a message to all players.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    /// <param name="duration">Duration of the message.</param>
    /// <param name="clearPrevious">Whether or not to clear previous broadcasts.</param>
    /// <param name="flags">Flags of the broadcast.</param>
    public static void Broadcast(object msg, ushort duration, bool clearPrevious = true,
        Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
    {
        if (clearPrevious)
            global::Broadcast.Singleton?.RpcClearElements();

        global::Broadcast.Singleton?.RpcAddElement(msg.ToString(), duration, flags);
    }

    /// <summary>
    /// Shows a hint to all players.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    /// <param name="duration">Duration of the message.</param>
    /// <param name="isPriority">Whether or not to show this message immediately.</param>
    public static void ShowHint(object msg, ushort duration, bool isPriority = false)
        => ExPlayer.Players.ForEach(x => x.SendHint(msg, duration, isPriority));

    /// <summary>
    /// Plays a random ambient clip.
    /// </summary>
    public static void PlayAmbientSound()
        => AmbientSoundPlayer?.GenerateRandom();

    /// <summary>
    /// Plays the specified ambient clip.
    /// </summary>
    /// <param name="id">Index of the clip to play.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void PlayAmbientSound(int id)
    {
        if (id < 0 || id >= AmbientClipsCount)
            throw new ArgumentOutOfRangeException(nameof(id));

        AmbientSoundPlayer?.RpcPlaySound(AmbientSoundPlayer.clips[id].index);
    }

    /// <summary>
    /// Calculates and returns the pocket dimension exit position for a specified player.
    /// </summary>
    /// <param name="player">The player instance for whom the pocket dimension exit position is being determined.</param>
    /// <param name="zones">An optional array of facility zones to consider when determining the exit position.</param>
    /// <returns>A <see cref="Vector3"/> representing the calculated exit position. If the player is invalid or their role lacks the required movement module, returns <see cref="Vector3.zero"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="player"/> is <see langword="null"/> or does not have a valid reference hub.</exception>
    public static Vector3 GetPocketExitPosition(ExPlayer player, params FacilityZone[] zones)
    {
        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));

        if (player.Role.Role == null 
            || player.Role.MovementModule == null
            || player.Role.MovementModule.CharController == null)
            return Vector3.zero;

        var controller = player.Role.MovementModule.CharController;
        return GetPocketExitPosition(controller.center, controller.radius, controller.height, zones);
    }

    /// <summary>
    /// Calculates and retrieves a safe position for an SCP-106 pocket dimension exit.
    /// </summary>
    /// <param name="center">The offset applied to the position during calculation.</param>
    /// <param name="radius">The radius used to determine the bounds for the pocket exit position.</param>
    /// <param name="height">The vertical height to account for during calculations.</param>
    /// <param name="zones">An optional array of facility zones to restrict where the pocket exit may be generated. If not provided, all valid zones will be used by default.</param>
    /// <returns>A <see cref="Vector3"/> representing the calculated position for the pocket exit. Returns <see langword="Vector3.zero"/> if no valid position can be found.</returns>
    /// <exception cref="Exception">Thrown if an invalid zone (Other or None) is specified in the <paramref name="zones"/> array.</exception>
    public static Vector3 GetPocketExitPosition(Vector3 center, float radius, float height, params FacilityZone[] zones)
    {
        if (zones?.Length < 1)
            zones = allZones;

        var zone = zones.RandomItem();

        if (zone is FacilityZone.Other or FacilityZone.None)
            throw new("Invalid zone specified: Other or None cannot be used to get position");

        var poses = Scp106PocketExitFinder.GetPosesForZone(zone);

        if (poses?.Length < 1)
            return Vector3.zero;

        var pose = Scp106PocketExitFinder.GetRandomPose(poses);
        var range = Scp106PocketExitFinder.GetRaycastRange(zone);
        var forward = pose.forward;

        if (Random.value > 0.5f)
            forward = -forward;

        return GetSafePosition(pose.position, forward, center, range, radius, height);
    }

    /// <summary>
    /// Determines a safe position in a specified direction within a given range.
    /// </summary>
    /// <param name="position">The starting position for the calculation.</param>
    /// <param name="direction">The direction in which to search for the safe position.</param>
    /// <param name="center">The offset applied to the position during calculation.</param>
    /// <param name="range">The maximum distance to search for a safe position.</param>
    /// <param name="radius">The radius of the search area to consider for safety.</param>
    /// <param name="height">The vertical height to account for during calculations.</param>
    /// <returns>A <see cref="Vector3"/> representing the closest determined safe position.</returns>
    public static Vector3 GetSafePosition(Vector3 position, Vector3 direction, Vector3 center, float range,
        float radius, float height)
    {
        direction = Quaternion.Euler(0f, Random.Range(-30f, 30f), 0f) * direction;

        var num = Mathf.Lerp(radius, range, Random.value);

        var vector = Vector3.up * height / 2f;
        var vector2 = position + center + SafeLocationFinder.GroundOffset + Vector3.up * radius;
        
        if (!Physics.SphereCast(vector2, radius, direction, out var hitInfo, num + radius, FpcStateProcessor.Mask))
            return vector2 + direction * num + vector;
        
        return hitInfo.point + hitInfo.normal * radius + vector;
    }

    /// <summary>
    /// Spawns a new tantrum.
    /// </summary>
    /// <param name="position">Where to spawn the tantrum.</param>
    /// <param name="setActive">Whether or not to set the tantrum as active.</param>
    /// <returns></returns>
    public static TantrumEnvironmentalHazard PlaceTantrum(Vector3 position, bool setActive = true)
    {
        var instance = PrefabList.Tantrum.Spawn<TantrumEnvironmentalHazard>();

        if (!setActive)
            instance.SynchronizedPosition = new(position);
        else
            instance.SynchronizedPosition = new(position + (Vector3.up * 0.25f));

        instance._destroyed = !setActive;

        NetworkServer.Spawn(instance.gameObject);
        return instance;
    }

    /// <summary>
    /// Spawns a grenade.
    /// </summary>
    /// <param name="position">The position to spawn the grenade at.</param>
    /// <param name="type">The type of grenade.</param>
    /// <param name="attacker">The attacker.</param>
    public static void Explode(Vector3 position, ExplosionType type = ExplosionType.Grenade, ExPlayer? attacker = null)
        => ExplosionUtils.ServerExplode(position, (attacker ?? ExPlayer.Host).Footprint, type);

    /// <summary>
    /// Spawns an explosion effect.
    /// </summary>
    /// <param name="position">Where to spawn the effect.</param>
    /// <param name="type">Type of effect.</param>
    public static void SpawnExplosion(Vector3 position, ItemType type = ItemType.GrenadeHE)
        => ExplosionUtils.ServerSpawnEffect(position, type);

    /// <summary>
    /// Spawns a ragdoll converted by SCP-3114.
    /// </summary>
    /// <param name="position">Where to spawn the ragdoll.</param>
    /// <param name="scale">Scale of the ragdoll.</param>
    /// <param name="rotation">Rotation of the ragdoll.</param>
    /// <returns>The spawned ragdoll.</returns>
    public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Quaternion rotation)
        => SpawnBonesRagdoll(position, scale, Vector3.zero, rotation);

    /// <summary>
    /// Spawns a ragdoll converted by SCP-3114.
    /// </summary>
    /// <param name="position">Where to spawn the ragdoll.</param>
    /// <param name="scale">Scale of the ragdoll.</param>
    /// <param name="velocity">Velocity of the player.</param>
    /// <param name="rotation">Rotation of the ragdoll.</param>
    /// <returns>The spawned ragdoll.</returns>
    public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Vector3 velocity,
        Quaternion rotation)
    {
        var ragdollInstance = SpawnRagdoll(RoleTypeId.Tutorial, position, scale, rotation, true, ExPlayer.Host,
            new UniversalDamageHandler(-1f, DeathTranslations.Warhead));

        if (ragdollInstance is null)
            throw new($"Failed to spawn ragdoll.");

        var bonesRagdoll = SpawnRagdoll(RoleTypeId.Scp3114, position, scale, velocity, rotation, true, ExPlayer.Host,
            new Scp3114DamageHandler(ragdollInstance, false)) as DynamicRagdoll;

        Ragdolls.Remove(ragdollInstance);

        NetworkServer.Destroy(ragdollInstance.gameObject);

        if (bonesRagdoll is null)
            throw new("Failed to spawn bones ragdoll.");

        Scp3114RagdollToBonesConverter.ServerConvertNew(ExPlayer.Host.Role.Scp3114, bonesRagdoll);

        ExPlayer.Host.Role.Set(RoleTypeId.None);
        return bonesRagdoll;
    }

    /// <summary>
    /// Spawns a ragdoll of a specific role.
    /// </summary>
    /// <param name="ragdollRoleType">The role to spawn a ragdoll of.</param>
    /// <param name="position">Where to spawn the ragdoll.</param>
    /// <param name="scale">Ragdoll's scale.</param>
    /// <param name="rotation">Ragdoll's rotation.</param>
    /// <param name="spawn">Whether or not to spawn it for players.</param>
    /// <param name="owner">The ragdoll's owner.</param>
    /// <param name="damageHandler">The ragdoll's cause of death.</param>
    /// <returns>The spawned ragdoll instance.</returns>
    public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale,
        Quaternion rotation, bool spawn = true, ExPlayer? owner = null, DamageHandlerBase? damageHandler = null)
        => SpawnRagdoll(ragdollRoleType, position, scale, Vector3.zero, rotation, spawn, owner, damageHandler);

    /// <summary>
    /// Spawns a ragdoll of a specific role.
    /// </summary>
    /// <param name="ragdollRoleType">The role to spawn a ragdoll of.</param>
    /// <param name="position">Where to spawn the ragdoll.</param>
    /// <param name="scale">Ragdoll's scale.</param>
    /// <param name="velocity">The player velocity.</param>
    /// <param name="rotation">Ragdoll's rotation.</param>
    /// <param name="spawn">Whether or not to spawn it for players.</param>
    /// <param name="owner">The ragdoll's owner.</param>
    /// <param name="damageHandler">The ragdoll's cause of death.</param>
    /// <returns>The spawned ragdoll instance.</returns>
    public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale,
        Vector3 velocity, Quaternion rotation, bool spawn = true, ExPlayer? owner = null,
        DamageHandlerBase? damageHandler = null)
    {
        if (!ragdollRoleType.TryGetPrefab(out var role))
            throw new($"Failed to find role prefab for role {ragdollRoleType}");

        if (role is not IRagdollRole ragdollRole)
            throw new($"Role {ragdollRoleType} does not have a ragdoll.");

        damageHandler ??= new UniversalDamageHandler(-1f, DeathTranslations.Crushed);

        var ragdoll = UnityEngine.Object.Instantiate(ragdollRole.Ragdoll);

        ragdoll.NetworkInfo = new((owner ?? ExPlayer.Host).ReferenceHub, damageHandler,
            ragdoll.transform.localPosition, ragdoll.transform.localRotation);

        ragdoll.transform.position = position;
        ragdoll.transform.rotation = rotation;

        ragdoll.transform.localScale = scale;

        if (ragdoll.TryGetComponent<Rigidbody>(out var rigidbody))
            rigidbody.linearVelocity = velocity;

        if (spawn)
            NetworkServer.Spawn(ragdoll.gameObject);

        Ragdolls.Add(ragdoll);
        return ragdoll;
    }

    /// <summary>
    /// Flickers light across the selected zones.
    /// </summary>
    /// <param name="duration">Flicker duration.</param>
    /// <param name="zones">The zone whitelist.</param>
    public static void FlickerLights(float duration, params FacilityZone[] zones)
    {
        foreach (var light in RoomLightController.Instances)
        {
            var room = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(r =>
                r != null && r.LightControllers.Contains(light));

            if (room is null)
                continue;

            if (zones.Length < 1 || zones.Contains(room.Zone))
                light.ServerFlickerLights(duration);
        }
    }

    /// <summary>
    /// Sets color of all lights.
    /// </summary>
    /// <param name="color">The color to set.</param>
    public static void SetLightColor(Color color)
        => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = color);

    /// <summary>
    /// Resets color of all lights.
    /// </summary>
    public static void ResetLightsColor()
        => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = DefaultLightColor);

    /// <summary>
    /// Spawns a new item.
    /// </summary>
    /// <param name="type">Type of the item.</param>
    /// <param name="position">Where to spawn it.</param>
    /// <param name="scale">Scale of the item.</param>
    /// <param name="rotation">Rotation of the item.</param>
    /// <param name="serial">Optional item serial number.</param>
    /// <param name="spawn">Whether or not to spawn it for players.</param>
    /// <returns>The spawned item.</returns>
    public static ItemPickupBase SpawnItem(ItemType type, Vector3 position, Vector3 scale, Quaternion rotation,
        ushort? serial = null, bool spawn = true)
        => SpawnItem<ItemPickupBase>(type, position, scale, rotation, serial, spawn)!;

    /// <summary>
    /// Spawns a new item.
    /// </summary>
    /// <param name="item">Type of the item.</param>
    /// <param name="position">Where to spawn it.</param>
    /// <param name="scale">Scale of the item.</param>
    /// <param name="rotation">Rotation of the item.</param>
    /// <param name="serial">Optional item serial number.</param>
    /// <param name="spawn">Whether or not to spawn it for players.</param>
    /// <typeparam name="T">Generic type of the item.</typeparam>
    /// <returns>The spawned item.</returns>
    public static T? SpawnItem<T>(ItemType item, Vector3 position, Vector3 scale, Quaternion rotation,
        ushort? serial = null, bool spawn = true) where T : ItemPickupBase
    {
        if (!item.TryGetItemPrefab(out var prefab))
            return null;

        var pickup = UnityEngine.Object.Instantiate((T)prefab.PickupDropModel, position, rotation);

        pickup.transform.position = position;
        pickup.transform.rotation = rotation;

        pickup.transform.localScale = scale;

        pickup.Info = new(item, prefab.Weight, serial ?? ItemSerialGenerator.GenerateNext());

        if (spawn)
        {
            NetworkServer.Spawn(pickup.gameObject);

            LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new(pickup));
        }

        return pickup;
    }

    /// <summary>
    /// Spawns an amount of an item.
    /// </summary>
    /// <param name="type">The type of item to spawn.</param>
    /// <param name="count">The amount to spawn.</param>
    /// <param name="position">Where to spawn.</param>
    /// <param name="scale">Scale of each item.</param>
    /// <param name="rotation">Rotation of each item.</param>
    /// <param name="spawn">Whether or not to spawn.</param>
    /// <typeparam name="T">Generic type of the item.</typeparam>
    /// <returns></returns>
    public static List<T> SpawnItems<T>(ItemType type, int count, Vector3 position, Vector3 scale, Quaternion rotation,
        bool spawn = true) where T : ItemPickupBase
    {
        var list = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            var item = SpawnItem<T>(type, position, scale, rotation, null, spawn);

            if (item == null)
                continue;

            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Spawns a projectile.
    /// </summary>
    /// <param name="item">The projectile type.</param>
    /// <param name="position">Where to spawn it.</param>
    /// <param name="scale">Projectile scale.</param>
    /// <param name="velocity">Projectile velocity.</param>
    /// <param name="rotation">Projectile rotation.</param>
    /// <param name="force">Throwing force.</param>
    /// <param name="fuseTime">How long until detonation (in seconds).</param>
    /// <param name="spawn">Whether or not to spawn.</param>
    /// <param name="activate">Whether or not to activate.</param>
    /// <returns>The spawned projectile.</returns>
    public static ThrownProjectile? SpawnProjectile(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity,
        Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true)
        => SpawnProjectile<ThrownProjectile>(item, position, scale, velocity, rotation, force, fuseTime, spawn,
            activate);

    /// <summary>
    /// Spawns a projectile.
    /// </summary>
    /// <param name="item">The projectile type.</param>
    /// <param name="position">Where to spawn it.</param>
    /// <param name="scale">Projectile scale.</param>
    /// <param name="velocity">Projectile velocity.</param>
    /// <param name="rotation">Projectile rotation.</param>
    /// <param name="force">Throwing force.</param>
    /// <param name="fuseTime">How long until detonation (in seconds).</param>
    /// <param name="spawn">Whether or not to spawn.</param>
    /// <param name="activate">Whether or not to activate.</param>
    /// <typeparam name="T">Generic projectile type.</typeparam>
    /// <returns>The spawned projectile.</returns>
    public static T? SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity,
        Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true)
        where T : ThrownProjectile
        => SpawnProjectile<T>(item, position, scale, Vector3.forward, Vector3.up, rotation, velocity, force, fuseTime,
            spawn, activate);

    /// <summary>
    /// Spawns a projectile.
    /// </summary>
    /// <param name="item">Projectile type.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="scale">Projectile scale.</param>
    /// <param name="forward">Forward vector (can be <see cref="Vector3.forward"/>)</param>
    /// <param name="up">Upwards vector (can be <see cref="Vector3.up"/>)</param>
    /// <param name="rotation">Projectile rotation.</param>
    /// <param name="velocity">Projectile velocity.</param>
    /// <param name="force">Throwing force.</param>
    /// <param name="fuseTime">Time until detonation.</param>
    /// <param name="spawn">Whether or not to spawn.</param>
    /// <param name="activate">Whether or not to activate.</param>
    /// <param name="serial">Optional item serial number.</param>
    /// <typeparam name="T">Generic projectile type.</typeparam>
    /// <returns></returns>
    public static T? SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 forward, Vector3 up,
        Quaternion rotation, Vector3 velocity, float force, float fuseTime = 2f, bool spawn = true,
        bool activate = true, ushort? serial = null) where T : ThrownProjectile
    {
        if (!item.TryGetItemPrefab<ThrowableItem>(out var throwableItem))
            return null;

        var projectile = UnityEngine.Object.Instantiate((T)throwableItem!.Projectile, position, rotation);
        var settings = throwableItem.FullThrowSettings;

        projectile.transform.localScale = scale;
        projectile.Info = new(item, throwableItem.Weight, serial ?? ItemSerialGenerator.GenerateNext());

        settings.StartVelocity = force;
        settings.StartTorque = velocity;

        if (projectile is TimeGrenade timeGrenade) timeGrenade._fuseTime = fuseTime;

        if (spawn)
        {
            NetworkServer.Spawn(projectile.gameObject);
            
            LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new(projectile));
            
            if (activate)
            {
                if (projectile.TryGetRigidbody(out var rigidbody) && rigidbody != null)
                {
                    var num = 1f - Mathf.Abs(Vector3.Dot(forward, Vector3.up));
                    var vector = up * throwableItem.FullThrowSettings.UpwardsFactor;
                    var vector2 = forward + vector * num;

                    rigidbody.centerOfMass = Vector3.zero;
                    rigidbody.angularVelocity = settings.StartTorque;
                    rigidbody.linearVelocity = velocity + vector2 * force;
                }
                else
                {
                    projectile.Position = position;
                    projectile.Rotation = rotation;
                }

                projectile.ServerActivate();

                new ThrowableNetworkHandler.ThrowableItemAudioMessage(projectile.Info.Serial,
                    ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce).SendToAuthenticated();
            }
        }

        return projectile;
    }

    internal static void RegisterChamber(LockerChamber chamber, Locker locker)
    {
        if (!Lockers.Contains(locker))
            Lockers.Add(locker);

        if (!Chambers.Contains(chamber))
            Chambers.Add(chamber);

        if (!ChamberToLocker.ContainsKey(chamber))
            ChamberToLocker.Add(chamber, locker);
    }

    private static void OnRoundWaiting()
    {
        try
        {
            AmbientSoundPlayer = ExPlayer.Host.GameObject!.GetComponent<AmbientSoundPlayer>();

            Ragdolls.Clear();
            Pickups.Clear();
        }
        catch (Exception ex)
        {
            ApiLog.Error("Map API", $"Map generation failed!\n{ex.ToColoredString()}");
        }
    }

    // this is so stupid
    private static void OnDestroyingIdentity(MirrorDestroyingIdentityEventArgs args)
    {
        try
        {
            if (args.Identity == null 
                || args.Mode is not NetworkServer.DestroyMode.Destroy)
                return;

            Lockers.ForEach(l =>
            {
                if (l.netId != args.Identity.netId)
                    return;

                for (var i = 0; i < l.Chambers.Length; i++)
                {
                    var chamber = l.Chambers[i];

                    Chambers.Remove(chamber);
                    ChamberToLocker.Remove(chamber);
                }
            });

            Lockers.RemoveAll(x => x.netId == args.Identity.netId);

            if (args.Identity.TryGetComponent<IInteractable>(out var interactable))
                InteractableCollider.AllInstances.Remove(interactable);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", ex);
        }
    }

    private static void OnRagdollSpawned(BasicRagdoll ragdoll)
    {
        if (ragdoll is null || !ragdoll)
            return;

        Ragdolls.Add(ragdoll);
    }

    private static void OnRagdollRemoved(BasicRagdoll ragdoll)
    {
        if (ragdoll is null || !ragdoll)
            return;

        Ragdolls.Remove(ragdoll);
    }

    private static void OnPickupDestroyed(ItemPickupBase pickup)
    {
        Pickups.Remove(pickup);

        ExPlayer.AllPlayers.ForEach(p => p?.Inventory?.droppedItems?.Remove(pickup));
    }

    private static void OnPickupCreated(ItemPickupBase pickup)
    {
        Pickups.Add(pickup);
    }

    private static void OnGenerationStage(MapGenerationPhase phase)
    {
        if (phase is MapGenerationPhase.RoomCoordsRegistrations)
        {
            Lockers.Clear();

            Chambers.Clear();
            ChamberToLocker.Clear();
        }
    }

    private static void OnGenerated()
    {
        foreach (var structure in SpawnableStructure.AllInstances)
        {
            if (structure == null || structure is not Locker locker)
                continue;

            for (var i = 0; i < locker.Chambers.Length; i++)
                RegisterChamber(locker.Chambers[i], locker);
        }
    }

    internal static void Internal_Init()
    {
        RagdollManager.OnRagdollSpawned += OnRagdollSpawned;
        RagdollManager.OnRagdollRemoved += OnRagdollRemoved;

        ItemPickupBase.OnBeforePickupDestroyed += OnPickupDestroyed;
        ItemPickupBase.OnPickupAdded += OnPickupCreated;

        MirrorEvents.Destroying += OnDestroyingIdentity;

        InternalEvents.OnRoundWaiting += OnRoundWaiting;

        SeedSynchronizer.OnGenerationFinished += OnGenerated;

        typeof(SeedSynchronizer).InsertFirst("OnGenerationStage", OnGenerationStage);
    }
}