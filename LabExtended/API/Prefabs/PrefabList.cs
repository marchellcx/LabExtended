using LabExtended.Core;
using LabExtended.Extensions;

using MapGeneration.Holidays;

using Mirror;

namespace LabExtended.API.Prefabs;

/// <summary>
/// Serves as a list of all known prefabs.
/// </summary>
public static class PrefabList
{
    /// <summary>
    /// Gets the currently active holiday.
    /// </summary>
    public static readonly HolidayType ActiveHoliday = HolidayUtils.GetActiveHoliday();
    
    private static bool prefabsLoaded;
    private static Dictionary<string, PrefabDefinition> allPrefabs = new();

    /// <summary>
    /// Gets a list of all prefabs keyed by property names.
    /// </summary>
    public static IReadOnlyDictionary<string, PrefabDefinition> AllPrefabs
    {
        get
        {
            if (!prefabsLoaded)
                ReloadPrefabs();
            
            return allPrefabs;
        }
    }

    /// <summary>
    /// Reloads all prefabs.
    /// </summary>
    public static void ReloadPrefabs()
    {
        var allDict = allPrefabs;
        var allProps = typeof(PrefabList).GetAllProperties();

        foreach (var prop in allProps)
        {
            if (allDict.ContainsKey(prop.Name))
                continue;

            if (prop.PropertyType != typeof(PrefabDefinition))
                continue;

            var value = prop.GetValue(null);

            if (value is not PrefabDefinition definition)
                continue;

            if (allDict.ContainsKey(definition.Name))
                continue;

            allDict.Add(prop.Name, definition);
        }

        var loadedCount = 0;

        foreach (var prefab in NetworkClient.prefabs)
        {
            if (!allPrefabs.TryGetFirst(x => x.Value.Name == prefab.Value.name, out _))
            {
                ApiLog.Warn("PrefabList", $"Prefab &1{prefab.Value.name}&r is not defined in PrefabList!");
            }
        }

        foreach (var prefab in allPrefabs)
        {
            if (prefab.Value.IsActive && !prefab.Value.IsLoaded)
            {
                ApiLog.Warn("PrefabList", $"Prefab &1{prefab.Key}&r has either been renamed or removed!");
            }
            else if (prefab.Value.IsActive && prefab.Value.IsLoaded)
            {
                loadedCount++;
            }
        }

        prefabsLoaded = true;
                
        ApiLog.Info("PrefabList", $"Loaded &2{loadedCount}&r / &1{NetworkClient.prefabs.Count}&r prefabs!");
    }

    #region Other Prefabs

    /// <summary>
    /// Gets the prefab of a player.
    /// </summary>
    public static PrefabDefinition Player { get; } = new("Player", null);

    /// <summary>
    /// Gets the prefab of SCP-939's Amnestic Cloud.
    /// </summary>
    public static PrefabDefinition AmnesticCloud { get; } = new("Amnestic Cloud Hazard", null);

    /// <summary>
    /// Gets the prefab of SCP-173's Tantrum.
    /// </summary>
    public static PrefabDefinition Tantrum { get; } = new("TantrumObj", null);

    #endregion

    #region Other Pickups

    /// <summary>
    /// Gets the prefab of a painkillers bottle.
    /// </summary>
    public static PrefabDefinition PainKillers { get; } = new("PainkillersPickup", null);

    /// <summary>
    /// Gets the prefab of an adrenaline.
    /// </summary>
    public static PrefabDefinition Adrenaline { get; } = new("AdrenalinePrefab", null);

    /// <summary>
    /// Gets the prefab of a medkit.
    /// </summary>
    public static PrefabDefinition Medkit { get; } = new("MedkitPickup", null);

    /// <summary>
    /// Gets the prefab of a Chaos Insurgency keycard.
    /// </summary>
    public static PrefabDefinition ChaosKeycard { get; } = new("KeycardPickup_Chaos", null);

    /// <summary>
    /// Gets the prefab of a regular keycard.
    /// </summary>
    public static PrefabDefinition Keycard { get; } = new("KeycardPickup", null);

    /// <summary>
    /// Gets the prefab of a coin.
    /// </summary>
    public static PrefabDefinition Coin { get; } = new("CoinPickup", null);

    /// <summary>
    /// Gets the prefab of a radio.
    /// </summary>
    public static PrefabDefinition Radio { get; } = new("RadioPickup", null);

    /// <summary>
    /// Gets the prefab of a flashlight.
    /// </summary>
    public static PrefabDefinition Flashlight { get; } = new("FlashlightPickup", null);

    /// <summary>
    /// Gets the prefab of a lantern.
    /// </summary>
    public static PrefabDefinition Lantern { get; } = new("LanternPickup", null);

    #endregion

    #region Ammo Pickups

    /// <summary>
    /// Gets the prefab of a 12-gauge ammo box.
    /// </summary>
    public static PrefabDefinition Ammo12ga { get; } = new("Ammo12gaPickup", null);

    /// <summary>
    /// Gets the prefab of a 44-cal ammo box.
    /// </summary>
    public static PrefabDefinition Ammo44cal { get; } = new("Ammo44calPickup", null);

    /// <summary>
    /// Gets the prefab of a 5.56mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo556mm { get; } = new("Ammo556mmPickup", null);

    /// <summary>
    /// Gets the prefab of a 7.62mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo762mm { get; } = new("Ammo762mmPickup", null);

    /// <summary>
    /// Gets the prefab of a 9mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo9mm { get; } = new("Ammo9mmPickup", null);

    #endregion

    #region Scp Items

    /// <summary>
    /// Gets the prefab of an SCP-207 bottle.
    /// </summary>
    public static PrefabDefinition Scp207 { get; } = new("SCP207Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-244-A vase.
    /// </summary>
    public static PrefabDefinition Scp244a { get; } = new("SCP244APickup Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-244-B vase.
    /// </summary>
    public static PrefabDefinition Scp244b { get; } = new("SCP244BPickup Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-268 hat.
    /// </summary>
    public static PrefabDefinition Scp268 { get; } = new("SCP268Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-330 candy bag.
    /// </summary>
    public static PrefabDefinition Scp330 { get; } = new("Scp330Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-500 bottle.
    /// </summary>
    public static PrefabDefinition Scp500 { get; } = new("SCP500Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-3114 goggles.
    /// </summary>
    public static PrefabDefinition Scp1344 { get; } = new("SCP1344Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-1576 gramophone.
    /// </summary>
    public static PrefabDefinition Scp1576 { get; } = new("SCP1576Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-1853.
    /// </summary>
    public static PrefabDefinition Scp1853 { get; } = new("SCP1853Pickup", null);

    /// <summary>
    /// Gets the prefab of an SCP-1509 pickup.
    /// </summary>
    public static PrefabDefinition Scp1509 { get; } = new("Scp1509Pickup", null);

    /// <summary>
    /// Gets the prefab of an anti-SCP-207 bottle.
    /// </summary>
    public static PrefabDefinition AntiScp207 { get; } = new("AntiSCP207Pickup", null);

    #endregion

    #region Scp Item Pedestals
    /// <summary>
    /// Gets the prefab of an SCP-2176 pedestal.
    /// </summary>
    public static PrefabDefinition Scp2176Pedestal { get; } = new("Scp2176PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-1853 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1853Pedestal { get; } = new("Scp1853PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of SCP-1576 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1576Pedestal { get; } = new("Scp1576PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-1344 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1344Pedestal { get; } = new("Scp1344PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-500 pedestal.
    /// </summary>
    public static PrefabDefinition Scp500Pedestal { get; } = new("Scp500PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-268 pedestal.
    /// </summary>
    public static PrefabDefinition Scp268Pedestal { get; } = new("Scp268PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-244 pedestal.
    /// </summary>
    public static PrefabDefinition Scp244Pedestal { get; } = new("Scp244PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-207 pedestal.
    /// </summary>
    public static PrefabDefinition Scp207Pedestal { get; } = new("Scp207PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an SCP-018 pedestal.
    /// </summary>
    public static PrefabDefinition Scp018Pedestal { get; } = new("Scp018PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of a SCP-1509 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1509Pedestal { get; } = new("Scp1509PedestalStructure Variant", null);

    /// <summary>
    /// Gets the prefab of an anti-SCP-207 pedestal.
    /// </summary>
    public static PrefabDefinition AntiScp207Pedestal { get; } = new("AntiScp207PedestalStructure Variant", null);

    #endregion

    #region Armor Pickups

    /// <summary>
    /// Gets the prefab of combat armor.
    /// </summary>
    public static PrefabDefinition CombatArmor { get; } = new("Combat Armor Pickup", null);

    /// <summary>
    /// Gets the prefab of light armor.
    /// </summary>
    public static PrefabDefinition LightArmor { get; } = new("Light Armor Pickup", null);

    /// <summary>
    /// Gets the prefab of heavy armor.
    /// </summary>
    public static PrefabDefinition HeavyArmor { get; } = new("Heavy Armor Pickup", null);

    #endregion

    #region Weapons
    /// <summary>
    /// Gets the prefab of a Jailbird.
    /// </summary>
    public static PrefabDefinition Jailbird { get; } = new("JailbirdPickup", null, new KeyValuePair<HolidayType, string>(HolidayType.Christmas, "JailbirdPickup Christmas"));

    /// <summary>
    /// Gets the prefab of a Micro-HID.
    /// </summary>
    public static PrefabDefinition MicroHid { get; } = new("MicroHidPickup", null);

    /// <summary>
    /// Gets the prefab of a firearm.
    /// </summary>
    public static PrefabDefinition Firearm { get; } = new("FirearmPickup", null);

    /// <summary>
    /// Gets the prefab of an explosive grenade.
    /// </summary>
    public static PrefabDefinition ExplosiveGrenadeItem { get; } = new("HegPickup", null);

    /// <summary>
    /// Gets the prefab of an explosive grenade projectile.
    /// </summary>
    public static PrefabDefinition ExplosiveGrenadePickup { get; } = new("HegProjectile", null);

    /// <summary>
    /// Gets the prefab of a flash grenade.
    /// </summary>
    public static PrefabDefinition FlashGrenadeItem { get; } = new("FlashbangPickup", null);

    /// <summary>
    /// Gets the prefab of a flash grenade projectile.
    /// </summary>
    public static PrefabDefinition FlashGrenadeProjectile { get; } = new("FlashbangProjectile", null);

    /// <summary>
    /// Gets the prefab of an SCP-2176 projectile.
    /// </summary>
    public static PrefabDefinition Scp2176Projectile { get; } = new("Scp2176Projectile", null);

    /// <summary>
    /// Gets the prefab of an SCP-018 projectile.
    /// </summary>
    public static PrefabDefinition Scp018Projectile { get; } = new("Scp018Projectile", null, 
        new KeyValuePair<HolidayType, string>(HolidayType.Halloween, "Scp018Projectile Halloween"), 
        new KeyValuePair<HolidayType, string>(HolidayType.Christmas, "Scp018Projectile Christmas"));
    #endregion

    #region Spawnable Map Prefabs
    /// <summary>
    /// Gets the sport version of the shooting target.
    /// </summary>
    public static PrefabDefinition SportTarget { get; } = new("sportTargetPrefab", null);

    /// <summary>
    /// Gets the prefab of the Class-D shooting target.
    /// </summary>
    public static PrefabDefinition ClassDTarget { get; } = new("dboyTargetPrefab", null);

    /// <summary>
    /// Gets the prefab of the binary shooting target.
    /// </summary>
    public static PrefabDefinition BinaryTarget { get; } = new("binaryTargetPrefab", null);

    /// <summary>
    /// Gets the prefab of the primitive object toy.
    /// </summary>
    public static PrefabDefinition Primitive { get; } = new("PrimitiveObjectToy", null);

    /// <summary>
    /// Gets the prefab of the light source toy.
    /// </summary>
    public static PrefabDefinition LightSource { get; } = new("LightSourceToy", null);

    /// <summary>
    /// Gets the prefab of the speaker toy.
    /// </summary>
    public static PrefabDefinition Speaker { get; } = new("SpeakerToy", null);

    /// <summary>
    /// Gets the prefab of the capybara toy.
    /// </summary>
    public static PrefabDefinition Capybara { get; } = new("CapybaraToy", null);
    
    /// <summary>
    /// Gets the prefab of the interactable toy.
    /// </summary>
    public static PrefabDefinition Interactable { get; } = new("InvisibleInteractableToy", null);
    
    /// <summary>
    /// Gets the prefab of the text toy.
    /// </summary>
    public static PrefabDefinition Text { get; } = new("TextToy", null);
    
    /// <summary>
    /// Gets the prefab of the waypoint toy.
    /// </summary>
    public static PrefabDefinition Waypoint { get; } = new("WaypointToy", null);

    /// <summary>
    /// Gets the prefab of <see cref="AdminToys.SpawnableCullingParent"/>
    /// </summary>
    public static PrefabDefinition CullingParent { get; } = new("CullableParentToy", null);
    
    /// <summary>
    /// Gets the prefab of the SCP-079 camera toy (Entrance Zone w/ arm version).
    /// </summary>
    public static PrefabDefinition EzArmCamera { get; } = new("EzArmCameraToy", null);

    /// <summary>
    /// Gets the prefab of the SCP-079 camera toy (Entrance Zone version).
    /// </summary>
    public static PrefabDefinition EzCamera { get; } = new("EzCameraToy", null);

    /// <summary>
    /// Gets the prefab of the SCP-079 camery toy (Surface Zone version).
    /// </summary>
    public static PrefabDefinition SurfaceCamera { get; } = new("SzCameraToy", null);

    /// <summary>
    /// Gets the prefab of the SCP-079 camera toy (Heavy Containment Zone version).
    /// </summary>
    public static PrefabDefinition HczCamera { get; } = new("HczCameraToy", null);
    
    /// <summary>
    /// Gets the prefab of the SCP-079 camera toy (Light Containment Zone version).
    /// </summary>
    public static PrefabDefinition LczCamera { get; } = new("LczCameraToy", null);

    /// <summary>
    /// Gets the prefab of SCP-106's sinkhole.
    /// </summary>
    public static PrefabDefinition Sinkhole { get; } = new("Sinkhole", null);

    /// <summary>
    /// Gets the prefab of the Entrance Zone door. 
    /// </summary>
    public static PrefabDefinition EntranceZoneDoor { get; } = new("EZ BreakableDoor", null);

    /// <summary>
    /// Gets the prefab of the Light Containment Zone door.
    /// </summary>
    public static PrefabDefinition LightZoneDoor { get; } = new("LCZ BreakableDoor", null);

    /// <summary>
    /// Gets the prefab of the Heavy Containment Zone door.
    /// </summary>
    public static PrefabDefinition HeavyZoneDoor { get; } = new("HCZ BreakableDoor", null);

    /// <summary>
    /// Gets the prefab of the Heavy Containment Zone Bulk door.
    /// </summary>
    public static PrefabDefinition HeavyZoneBulkDoor { get; } = new("HCZ BulkDoor", null);

    /// <summary>
    /// Gets the prefab of the wall mounted health box.
    /// </summary>
    public static PrefabDefinition HealthBox { get; } = new("AdrenalineMedkitStructure", null);

    /// <summary>
    /// Gets the prefab of the wall mounted adrenaline box.
    /// </summary>
    public static PrefabDefinition AdrenalineBox { get; } = new("RegularMedkitStructure", null);

    /// <summary>
    /// Gets the prefab of a SCP-079 generator.
    /// </summary>
    public static PrefabDefinition Generator { get; } = new("GeneratorStructure", null);

    /// <summary>
    /// Gets the prefab of a workstation.
    /// </summary>
    public static PrefabDefinition Workstation { get; } = new("Spawnable Work Station Structure", null);

    /// <summary>
    /// Gets the prefab of a locker.
    /// </summary>
    public static PrefabDefinition Locker { get; } = new("MiscLocker", null);

    /// <summary>
    /// Gets the prefab of an elevator's gate.
    /// </summary>
    public static PrefabDefinition ElevatorGate { get; } = new("ElevatorChamber Gates", null);

    /// <summary>
    /// Gets the prefab of an elevator's chamber.
    /// </summary>
    public static PrefabDefinition ElevatorChamber { get; } = new("ElevatorChamber", null);

    /// <summary>
    /// Gets the prefab of an elevator chamber's cargo.
    /// </summary>
    public static PrefabDefinition ElevatorChamberCargo { get; } = new("ElevatorChamberCargo", null);
    
    /// <summary>
    /// Gets the prefab of the Alpha Warhead elevator.
    /// </summary>
    public static PrefabDefinition NukeElevatorChamber { get; } = new("ElevatorChamberNuke", null);

    /// <summary>
    /// Gets the prefab of the experimental weapon locker.
    /// </summary>
    public static PrefabDefinition ExperimentalWeaponLocker { get; } = new("Experimental Weapon Locker", null);

    /// <summary>
    /// Gets the prefab of the large weapon locker.
    /// </summary>
    public static PrefabDefinition LargeGunLocker { get; } = new("LargeGunLockerStructure", null);

    /// <summary>
    /// Gets the prefab of the rifle rack.
    /// </summary>
    public static PrefabDefinition RifleRack { get; } = new("RifleRackStructure", null);

    /// <summary>
    /// Gets the prefab of an open Heavy Containment Zone hallway.
    /// </summary>
    public static PrefabDefinition OpenHallway { get; } = new("OpenHallway", null);

    /// <summary>
    /// Gets the prefab of a broken electrical box.
    /// </summary>
    public static PrefabDefinition BrokenElectricalBox { get; } = new("Broken Electrical Box Open Connector", null);

    /// <summary>
    /// Gets the prefab of a simple box collection.
    /// </summary>
    public static PrefabDefinition SimpleBoxes { get; } = new("Simple Boxes Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of short pipes.
    /// </summary>
    public static PrefabDefinition ShortPipes { get; } = new("Pipes Short Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of long pipes.
    /// </summary>
    public static PrefabDefinition LongPipes { get; } = new("Pipes Long Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of a box collection with a ladder.
    /// </summary>
    public static PrefabDefinition LadderBoxes { get; } = new("Boxes Ladder Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of tank-supported shelf.
    /// </summary>
    public static PrefabDefinition Shelf { get; } = new("Tank-Supported Shelf Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of angles fences.
    /// </summary>
    public static PrefabDefinition AngledFences { get; } = new("Angled Fences Open Connector", null);
    
    /// <summary>
    /// Gets the prefab of huge orange pipes.
    /// </summary>
    public static PrefabDefinition HugePipes { get; } = new("Huge Orange Pipes Open Connector", null);

    /// <summary>
    /// Gets the prefab of a gate door.
    /// </summary>
    public static PrefabDefinition GateDoor { get; } = new("Spawnable Unsecured Pryable GateDoor", null);
    #endregion

    #region Halloween Prefabs
    /// <summary>
    /// Gets the prefab of a prismatic cloud.
    /// </summary>
    public static PrefabDefinition PrismaticCloud { get; } = new("PrismaticCloud", HolidayType.Halloween);

    /// <summary>
    /// Gets the prefab of brown candy tantrum.
    /// </summary>
    public static PrefabDefinition BrownCandyTantrum { get; } = new("TantrumObj (Brown Candy)", HolidayType.Halloween);

    /// <summary>
    /// Gets the prefab of Hubert Moon.
    /// </summary>
    public static PrefabDefinition HubertMoon { get; } = new("Hubert Moon", HolidayType.Halloween);
    #endregion
    
    #region Christmas Prefabs
    /// <summary>
    /// Christmas variation of the regular SCP-173 tantrum.
    /// </summary>
    public static PrefabDefinition SnowPoop { get; } = new("SnowPoop - TantrumObj", HolidayType.Christmas);
    
    /// <summary>
    /// Gets the SCP-559 (cake) prefab.
    /// </summary>
    public static PrefabDefinition Scp559 { get; } = new("SCP-559 Cake", HolidayType.Christmas);

    /// <summary>
    /// Gets the prefab of the SCP-021-J bottle of water.
    /// </summary>
    public static PrefabDefinition Scp021J { get; } = new("SCP021JPickup",  HolidayType.Christmas);

    /// <summary>
    /// Gets the SCP-956 (pinata) prefab.
    /// </summary>
    public static PrefabDefinition Scp956 { get; } = new("SCP-956", HolidayType.Christmas);

    /// <summary>
    /// Gets the SCP-2536 (christmas tree) prefab.
    /// </summary>
    public static PrefabDefinition Scp2536 { get; } = new("SCP-2536 Tree", HolidayType.Christmas);

    /// <summary>
    /// Gets the snowpile prefab.
    /// </summary>
    public static PrefabDefinition Snowpile { get; } = new("Snowpile", HolidayType.Christmas);

    /// <summary>
    /// Gets the snowball prefab.
    /// </summary>
    public static PrefabDefinition Snowball { get; } = new("SnowballProjectile", HolidayType.Christmas);

    /// <summary>
    /// Gets the coal pickup prefab.
    /// </summary>
    public static PrefabDefinition Coal { get; } = new("CoalPickup", HolidayType.Christmas);

    /// <summary>
    /// Gets the coal projectile prefab.
    /// </summary>
    public static PrefabDefinition CoalProjectile { get; } = new("CoalProjectile", HolidayType.Christmas);

    /// <summary>
    /// Gets the SCP-2536 projectile (ball) prefab.
    /// </summary>
    public static PrefabDefinition Scp2536Projectile { get; } = new("Scp2536Projectile", HolidayType.Christmas);

    /// <summary>
    /// Gets the prefab of the tape player.
    /// </summary>
    public static PrefabDefinition TapePlayer { get; } = new("TapePlayerPickup", HolidayType.Christmas);

    #endregion
}