using Hazards;

using LabExtended.Extensions;

using MapGeneration.Distributors;
using MapGeneration.Holidays;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Prefabs;

/// <summary>
/// Represents a spawnable in-game prefab.
/// </summary>
public class PrefabDefinition
{
    private string normalName;

    private GameObject? prefab;
    
    private bool isLoaded;
    private bool isLoadAttempt;

    /// <summary>
    /// Gets called once a new instance of this prefab is spawned.
    /// </summary>
    public event Action<GameObject>? Spawned;

    /// <summary>
    /// Gets the prefab's name.
    /// </summary>
    public string Name
    {
        get
        {
            if (PrefabList.ActiveHoliday != HolidayType.None)
            {
                if (HolidayVariations.TryGetValue(PrefabList.ActiveHoliday, out var holidayName))
                {
                    return holidayName;
                }
            }

            return normalName;
        }
    }

    /// <summary>
    /// Checks if the prefab is successfully loaded.
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            if (!isLoadAttempt)
            {
                isLoaded = TryFindPrefab(out prefab);
                isLoadAttempt = true;
            }

            return isLoaded;
        }
    }
    
    /// <summary>
    /// Checks whether the prefab is currently active.
    /// </summary>
    public bool IsActive
    {
        get
        {
            if (ActiveHolidays.HasValue && ActiveHolidays.Value != PrefabList.ActiveHoliday)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Whether or not a holiday variation of this prefab is active.
    /// </summary>
    public bool IsHolidayActive => (ActiveHolidays.HasValue && ActiveHolidays.Value == PrefabList.ActiveHoliday) || HolidayVariations.ContainsKey(PrefabList.ActiveHoliday);
    
    /// <summary>
    /// Specifies the holidays on which this prefab is active.
    /// </summary>
    public HolidayType? ActiveHolidays { get; }

    /// <summary>
    /// Specifies the name variations for specific holidays.
    /// </summary>
    public Dictionary<HolidayType, string> HolidayVariations { get; } = new();

    /// <summary>
    /// Creates a new <see cref="PrefabDefinition"/> instance.
    /// </summary>
    /// <param name="name">Name of the prefab object.</param>
    /// <param name="activeHolidays">Holidays on which the prefab is active.</param>
    /// <param name="holidayVariations">Name variations to specific holidays.</param>
    public PrefabDefinition(string name, HolidayType? activeHolidays, params KeyValuePair<HolidayType, string>[] holidayVariations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        normalName = name;
        
        ActiveHolidays = activeHolidays;
        
        foreach (var pair in holidayVariations)
            HolidayVariations.Add(pair.Key, pair.Value);
    }

    /// <summary>
    /// Gets the prefab's instance.
    /// </summary>
    public GameObject? GameObject
    {
        get
        {
            if (!isLoadAttempt)
            {
                isLoaded = TryFindPrefab(out prefab);
                isLoadAttempt = true;
            }

            return prefab;
        }
    }

    /// <summary>
    /// Spawns a new instance from this prefab.
    /// </summary>
    /// <param name="prefabSetup"></param>
    /// <returns></returns>
    public GameObject Spawn(Action<GameObject>? prefabSetup = null)
    {
        var instance = CreateInstance()!;

        prefabSetup?.InvokeSafe(instance);

        SpawnInstance(instance.GetComponent<NetworkIdentity>());
        return instance;
    }

    /// <summary>
    /// Spawns a new instance from this prefab.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="prefabSetup"></param>
    /// <returns></returns>
    public T Spawn<T>(Action<T>? prefabSetup = null) where T : Component
    {
        var instance = CreateInstance();
        var identity = instance!.GetComponent<NetworkIdentity>();
        
        var component = default(T);

        if ((component = instance.GetComponent<T>()) is null
            && (component = instance.GetComponentInChildren<T>()) is null
            && (component = instance.GetComponentInParent<T>()) is null)
            throw new Exception($"Component of type '{typeof(T).FullName}' could not be found in prefab '{Name}'");

        prefabSetup?.InvokeSafe(component);

        SetupInstance(identity, identity.transform.position, identity.transform.localScale, identity.transform.rotation);
        SpawnInstance(instance.GetComponent<NetworkIdentity>());
        
        Spawned?.InvokeSafe(instance);
        return component;
    }

    /// <summary>
    /// Creates a new instance from this prefab.
    /// </summary>
    public GameObject? CreateInstance()
        => UnityEngine.Object.Instantiate(GameObject);

    /// <summary>
    /// Spawns an instantiated prefab instance.
    /// </summary>
    /// <param name="identity"></param>
    public void SpawnInstance(NetworkIdentity identity)
        => NetworkServer.Spawn(identity.gameObject);

    /// <summary>
    /// This method is used only for custom definitions that require a different spawn method.
    /// </summary>
    /// <param name="identity">The network identity instance.</param>
    /// <param name="position">The position to spawn at.</param>
    /// <param name="scale">The scale to spawn at.</param>
    /// <param name="rotation">The rotation to spawn with.</param>
    public virtual void SetupInstance(NetworkIdentity identity, Vector3 position, Vector3 scale, Quaternion rotation)
    {
        identity.transform.localScale = scale;
        
        if (identity.gameObject.TryFindComponent<StructurePositionSync>(out var positionSync))
        {
            positionSync.Network_position = positionSync._position = position;
            positionSync.Network_rotationY = positionSync._rotationY = (sbyte)Mathf.RoundToInt(rotation.eulerAngles.y / StructurePositionSync.ConversionRate);
        }

        if (identity.gameObject.TryFindComponent<EnvironmentalHazard>(out var hazard))
        {
            hazard.SourcePosition = position;

            if (hazard is TantrumEnvironmentalHazard tantrumHazard)
                tantrumHazard.SynchronizedPosition = new(position);
        }
    }

    /// <summary>
    /// Attempts to find the prefab in <see cref="NetworkClient.prefabs"/>.
    /// </summary>
    /// <param name="prefab">The prefab instance if found, otherwise null.</param>
    /// <returns>true if the prefab was found, otherwise false.</returns>
    public bool TryFindPrefab(out GameObject prefab)
    {
        prefab = null!;
        
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (!NetworkClient.prefabs.TryGetFirst(x => x.Value.name.Trim() == Name, out var prefabPair))
            return false;

        prefab = prefabPair.Value;
        return true;
    }
}
