using HarmonyLib;

using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.API.Custom.Teams;

// There may be a lot of bugs in this API, I was literally falling asleep while writing this ..

/// <summary>
/// Contains all registered custom teams.
/// </summary>
public static class CustomTeamRegistry
{
    /// <summary>
    /// Gets a list of all registered handlers.
    /// </summary>
    public static Dictionary<string, CustomTeamHandler> RegisteredHandlers { get; } = new();

    /// <summary>
    /// Attempts to find a custom team handler by it's name or ID.
    /// </summary>
    /// <param name="nameOrId">The name or ID of the type.</param>
    /// <param name="teamHandler">The found team handler.</param>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGet<THandler>(string nameOrId, out THandler teamHandler) where THandler : CustomTeamHandler
    {
        if (string.IsNullOrEmpty(nameOrId))
        {
            teamHandler = null!;
            return false;
        }

        foreach (var pair in RegisteredHandlers)
        {
            if (string.Equals(nameOrId, pair.Value.Name, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(nameOrId, pair.Value.Id, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(nameOrId, pair.Value.GetType().Name, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(nameOrId, pair.Value.GetType().Name.Replace("Handler", string.Empty), StringComparison.InvariantCultureIgnoreCase))
            {
                if (pair.Value is THandler handler)
                {
                    teamHandler = handler;
                    return true;
                }
            }
        }
        
        teamHandler = null!;
        return false;
    }

    /// <summary>
    /// Attempts to find a registered handler.
    /// </summary>
    /// <param name="handler">The found handler.</param>
    /// <typeparam name="THandler">The type of handler to find.</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGet<THandler>(out THandler handler) where THandler : CustomTeamHandler
    {
        if (RegisteredHandlers.TryGetFirst(x => x.Value is THandler, out var result))
        {
            handler = (THandler)result.Value;
            return true;
        }
        
        handler = null!;
        return false;
    }

    /// <summary>
    /// Registers a new handler.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static THandler Register<THandler>() where THandler : CustomTeamHandler
        => (THandler)Register(typeof(THandler));
    
    /// <summary>
    /// Registers a new handler.
    /// </summary>
    /// <param name="type">The type to register.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static CustomTeamHandler Register(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (Activator.CreateInstance(type) is not CustomTeamHandler handler)
            throw new Exception($"Type {type.FullName} could not be instantiated as a CustomTeamHandler");

        if (RegisteredHandlers.ContainsKey(handler.Id))
            throw new Exception($"A handler with the ID {handler.Id} is already registered.");

        RegisteredHandlers.Add(handler.Id, handler);
        
        handler.OnRegistered();
        return handler;
    }

    /// <summary>
    /// Registers an already instantiated handler.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="handler">The handler instance to register.</param>
    /// <returns>The registered handler instance.</returns>
    public static THandler Register<THandler>(THandler handler) where THandler : CustomTeamHandler
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (RegisteredHandlers.ContainsKey(handler.Id))
            throw new Exception($"A handler with the ID {handler.Id} is already registered.");

        RegisteredHandlers.Add(handler.Id, handler);

        handler.OnRegistered();
        return handler;
    }
}