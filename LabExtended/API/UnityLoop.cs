using LabExtended.Core;
using LabExtended.Extensions;
using Mirror;

using NiveraAPI.IO.Configs;

using NorthwoodLib.Pools;

using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

using static UnityEngine.LowLevel.PlayerLoopSystem;

namespace LabExtended.API;

/// <summary>
/// Provides utility methods for modifying and interacting with the Unity player loop, allowing for the injection of custom update functions and the removal of specified subsystems from the execution order.
/// </summary>
public static class UnityLoop
{
    /// <summary>
    /// Gets or sets a value indicating whether the Unity player loop should be modified by adding a custom loop and removing those specified in the configuration.
    /// </summary>
    [Config("unity-loop", "removed-loops", "A list of Unity player loop systems to remove.")]
    public static List<string> RemovedLoops { get; set; } = new()
    {
        "UnityEngine.PlayerLoop.PostLateUpdate+BatchModeUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+InputEndFrame",
        "UnityEngine.PlayerLoop.PostLateUpdate+ShaderHandleErrors",
        "UnityEngine.PlayerLoop.PostLateUpdate+ObjectDispatcherPostLateUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+GraphicsWarmupPreloadedShaders",
        "UnityEngine.PlayerLoop.PostLateUpdate+XRPreEndFrame",
        "UnityEngine.PlayerLoop.PostLateUpdate+ExecuteGameCenterCallbacks",
        "UnityEngine.PlayerLoop.PostLateUpdate+MemoryFrameMaintenance",
        "UnityEngine.PlayerLoop.PostLateUpdate+ThreadedLoadingDebug",
        "UnityEngine.PlayerLoop.PostLateUpdate+ResetInputAxis",
        "UnityEngine.PlayerLoop.PostLateUpdate+ShaderHandleError",
        "UnityEngine.PlayerLoop.PostLateUpdate+GUIClearEvents",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateResolution",
        "UnityEngine.PlayerLoop.PostLateUpdate+ClearImmediateRenderers",
        "UnityEngine.PlayerLoop.PostLateUpdate+PresentAfterDraw",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateCaptureScreenshot",
        "UnityEngine.PlayerLoop.PostLateUpdate+PhysicsSkinnedClothFinishUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+UIElementsRenderBatchModeOffscreen",
        "UnityEngine.PlayerLoop.PostLateUpdate+PlayerEmitCanvasGeometry",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateVideo",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateVideoTextures",
        "UnityEngine.PlayerLoop.PostLateUpdate+SortingGroupsUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateAllSkinnedMeshes",
        "UnityEngine.PlayerLoop.PostLateUpdate+EnlightenRuntimeUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateLightProbeProxyVolumes",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateAllRenderers",
        "UnityEngine.PlayerLoop.PostLateUpdate+XRPostLateUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateCustomRenderTextures",
        "UnityEngine.PlayerLoop.PostLateUpdate+VFXUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateAudio",
        "UnityEngine.PlayerLoop.PostLateUpdate+UIElementsRepaintPanels",
        "UnityEngine.PlayerLoop.PostLateUpdate+PlayerUpdateCanvases",
        "UnityEngine.PlayerLoop.PostLateUpdate+UpdateRectTransform",
        "UnityEngine.PlayerLoop.PostLateUpdate+PhysicsSkinnedClothBeginUpdate",
        "UnityEngine.PlayerLoop.PostLateUpdate+ParticleSystemEndUpdateAll",
        "UnityEngine.PlayerLoop.PostLateUpdate+ProfilerEndFrame",
        "UnityEngine.PlayerLoop.PostLateUpdate+ProfilerSynchronizeStats",
        "UnityEngine.PlayerLoop.PostLateUpdate+EndGraphicsJobsAfterScriptLateUpdate",
        "UnityEngine.Rendering.HighDefinition.LightLateUpdate",
        "UnityEngine.PlayerLoop.PreLateUpdate+ParticleSystemBeginUpdateAll",
        "UnityEngine.PlayerLoop.PreLateUpdate+Physics2DLateUpdate",
        "UnityEngine.PlayerLoop.PreLateUpdate+EndGraphicsJobsAfterScriptUpdate",
        "UnityEngine.PlayerLoop.PreLateUpdate+UIElementsUpdatePanels",
        "UnityEngine.PlayerLoop.PreLateUpdate+AccessibilityUpdate",
        "UnityEngine.PlayerLoop.PreLateUpdate+LegacyAnimationUpdate",
        "UnityEngine.PlayerLoop.PreLateUpdate+AIUpdatePostScript",
        "UnityEngine.PlayerLoop.PreLateUpdate+ConstraintManagerUpdate",
        "UnityEngine.Rendering.HighDefinition.LightLateUpdate",
        "UnityEngine.Rendering.HighDefinition.LocalVolumetricFogManager+RegisterLocalVolumetricFogEarlyUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+UpdateVideo",
        "UnityEngine.PlayerLoop.PreUpdate+WindUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+AIUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+SendMouseEvents",
        "UnityEngine.PlayerLoop.PreUpdate+InputForUIUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+NewInputUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+IMGUISendQueuedEvents",
        "UnityEngine.PlayerLoop.PreUpdate+CheckTexFieldInput",
        "UnityEngine.PlayerLoop.PreUpdate+PhysicsClothUpdate",
        "UnityEngine.PlayerLoop.PreUpdate+Physics2DUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+PhysicsClothFixedUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+Physics2DFixedUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+XRFixedUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+LegacyFixedAnimationUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+NewInputFixedUpdate",
        "UnityEngine.PlayerLoop.FixedUpdate+ClearLines",
        "UnityEngine.PlayerLoop.FixedUpdate+AudioFixedUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+PerformanceAnalyticsUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+SpriteAtlasManagerUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+Physics2DEarlyUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+ARCoreUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+DeliverIosPlatformEvents",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateKinect",
        "UnityEngine.PlayerLoop.EarlyUpdate+ProcessRemoteInput",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateInputManager",
        "UnityEngine.PlayerLoop.EarlyUpdate+XRUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateCanvasRectTransform",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateMainGameViewRect",
        "UnityEngine.PlayerLoop.EarlyUpdate+ProcessMouseInWindow",
        "UnityEngine.PlayerLoop.EarlyUpdate+AnalyticsCoreStatsUpdate",
        "UnityEngine.PlayerLoop.EarlyUpdate+GpuTimestamp",
        "UnityEngine.PlayerLoop.EarlyUpdate+PollPlayerConnection",
        "UnityEngine.PlayerLoop.EarlyUpdate+DispatchEventQueueEvents",
        "UnityEngine.PlayerLoop.EarlyUpdate+RendererNotifyInvisible",
        "UnityEngine.PlayerLoop.EarlyUpdate+ClearLines",
        "UnityEngine.PlayerLoop.EarlyUpdate+ClearIntermediateRenderers",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateContentLoading",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateTextureStreamingManager",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateStreamingManager",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateAsyncReadbackManager",
        "UnityEngine.PlayerLoop.EarlyUpdate+UpdateAsyncInstantiate",
        "UnityEngine.PlayerLoop.EarlyUpdate+ExecuteMainThreadJobs",
        "UnityEngine.PlayerLoop.Initialization+XREarlyUpdate",
        "UnityEngine.PlayerLoop.Initialization+SynchronizeState",
        "UnityEngine.PlayerLoop.Initialization+SynchronizeInputs",
        "UnityEngine.PlayerLoop.Initialization+UpdateCameraMotionVectors",
        "UnityEngine.PlayerLoop.Initialization+AsyncUploadTimeSlicedUpdate"
    };

    private static Action? mirrorRuntimeInitialize = typeof(NetworkLoop).FindMethod(x => x.Name == "RuntimeInitializeOnLoad")?.CreateDelegate(typeof(Action)) as Action;

    /// <summary>
    /// Represents a custom player loop system that can be injected before the default player loop.
    /// </summary>
    public struct CustomBeforePlayerLoop;

    /// <summary>
    /// Represents a custom player loop system that can be injected after the default player loop.
    /// </summary>
    public struct CustomAfterPlayerLoop;

    /// <summary>
    /// Occurs before the Unity player loop starts executing, allowing for custom logic to be executed at the beginning of each frame.
    /// </summary>
    public static event Action? BeforeLoop;

    /// <summary>
    /// Occurs after the Unity player loop has finished executing, allowing for custom logic to be executed at the end of each frame.
    /// </summary>
    public static event Action? AfterLoop;

    /// <summary>
    /// Gets the default Unity player loop system, which represents the standard execution order of subsystems in Unity.
    /// </summary>
    public static PlayerLoopSystem DefaultSystem => PlayerLoop.GetDefaultPlayerLoop();

    /// <summary>
    /// Gets or sets the current Unity player loop system, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    public static PlayerLoopSystem System
    {
        get => PlayerLoop.GetCurrentPlayerLoop();
        set => PlayerLoop.SetPlayerLoop(value);
    }

    /// <summary>
    /// Resets the Unity player loop system to its default state, removing any custom modifications and restoring the standard execution order of subsystems in Unity.
    /// </summary>
    public static void ResetSystem()
    {
        System = DefaultSystem;

        ModifySystem(x =>
        {
            x.InjectBefore<Initialization.ProfilerStartFrame>(InvokeBefore, typeof(CustomBeforePlayerLoop));
            x.InjectAfter<PostLateUpdate.UpdateVideo>(InvokeAfter, typeof(CustomAfterPlayerLoop));

            return x;
        });

        mirrorRuntimeInitialize?.Invoke();
    }

    /// <summary>
    /// Modifies the current Unity player loop system using the specified modifier function, allowing for custom changes to the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="modifier">A function that takes the current PlayerLoopSystem and returns a modified PlayerLoopSystem or null.</param>
    /// <exception cref="ArgumentNullException">Thrown when the modifier function is null.</exception>
    public static void ModifySystem(Func<PlayerLoopSystem, PlayerLoopSystem?> modifier)
    {
        if (modifier is null)
            throw new ArgumentNullException(nameof(modifier));

        var system = System;
        var newSystem = modifier(system);

        if (!newSystem.HasValue)
            return;

        System = newSystem.Value;
    }

    /// <summary>
    /// Injects a custom update function into the Unity player loop system before the specified target system type, allowing for custom logic to be executed before the target system during each frame.
    /// </summary>
    /// <typeparam name="TSystem">The type of the target system before which the custom update function will be injected.</typeparam>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <param name="systemType">The type of the system to inject before.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    public static bool InjectBefore<TSystem>(this PlayerLoopSystem system, UpdateFunction method, Type systemType)
        => InjectBefore(system, method, typeof(TSystem), systemType);

    /// <summary>
    /// Injects a custom update function into the Unity player loop system before the specified target system type, allowing for custom logic to be executed before the target system during each frame.
    /// </summary>
    /// <typeparam name="TCustom">The type of the custom system to inject.</typeparam>
    /// <typeparam name="TSystem">The type of the target system before which the custom update function will be injected.</typeparam>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    public static bool InjectBefore<TCustom, TSystem>(this PlayerLoopSystem system, UpdateFunction method)
        => InjectBefore<TSystem>(system, method, typeof(TCustom));

    /// <summary>
    /// Injects a custom update function into the Unity player loop system after the specified target system type, allowing for custom logic to be executed after the target system during each frame.
    /// </summary>
    /// <typeparam name="TSystem">The type of the target system after which the custom update function will be injected.</typeparam>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <param name="systemType">The type of the system to inject after.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    public static bool InjectAfter<TSystem>(this PlayerLoopSystem system, UpdateFunction method, Type systemType)
        => InjectAfter(system, method, typeof(TSystem), systemType);

    /// <summary>
    /// Injects a custom update function into the Unity player loop system after the specified target system type, allowing for custom logic to be executed after the target system during each frame.
    /// </summary>
    /// <typeparam name="TCustom">The type of the custom system to inject.</typeparam>
    /// <typeparam name="TSystem">The type of the target system after which the custom update function will be injected.</typeparam>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    public static bool InjectAfter<TCustom, TSystem>(this PlayerLoopSystem system, UpdateFunction method)
        => InjectAfter<TSystem>(system, method, typeof(TCustom));

    /// <summary>
    /// Gets a reference to the parent PlayerLoopSystem of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    /// <typeparam name="T">The type of the parent system to retrieve.</typeparam>
    /// <param name="system">The player loop system to search within.</param>
    /// <returns>A reference to the parent PlayerLoopSystem of the specified type.</returns>
    public static ref PlayerLoopSystem GetParentSystem<T>(this PlayerLoopSystem system)
        => ref GetParentSystem(system, typeof(T));

    /// <summary>
    /// Gets a reference to the PlayerLoopSystem of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.   
    /// </summary>
    /// <typeparam name="T">The type of the system to retrieve.</typeparam>
    /// <param name="system">The player loop system to search within.</param>
    /// <returns>A reference to the PlayerLoopSystem of the specified type.</returns>
    public static ref PlayerLoopSystem GetSystem<T>(this PlayerLoopSystem system)
        => ref GetSystem(system, typeof(T));

    /// <summary>
    /// Gets the parent type of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    /// <typeparam name="T">The type of the system to retrieve the parent type for.</typeparam>
    /// <param name="system">The player loop system to search within.</param>
    /// <returns>The parent type of the specified type.</returns>
    public static Type? GetParentType<T>(this PlayerLoopSystem system)
        => GetParentType(system, typeof(T));

    /// <summary>
    /// Gets the parent type of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to search within.</param>
    /// <param name="type">The type of the system to retrieve the parent type for.</param>
    /// <returns>The parent type of the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the type parameter is null.</exception>
    public static Type? GetParentType(this PlayerLoopSystem system, Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var systems = new Stack<PlayerLoopSystem>();

        systems.Push(system);

        while (systems.Count > 0 && ExServer.IsRunning) 
        {
            var parent = systems.Pop();

            if (parent.subSystemList != null)
            {
                for (int i = 0; i < parent.subSystemList.Length; i++)
                {
                    var subSystem = parent.subSystemList[i];

                    if (subSystem.type != null && subSystem.type == type)
                        return parent.type;

                    systems.Push(subSystem);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a reference to the parent PlayerLoopSystem of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to search within.</param>
    /// <param name="type">The type of the system to retrieve the parent system for.</param>
    /// <returns>A reference to the parent PlayerLoopSystem of the specified type.</returns>
    /// <exception cref="Exception">Thrown when the parent system of the specified type doesn't exist.</exception>
    public static ref PlayerLoopSystem GetParentSystem(this PlayerLoopSystem system, Type type)
    {
        var parentType = GetParentType(system, type);

        if (parentType != null)
            return ref GetSystem(system, parentType);

        throw new Exception($"Player loop parent type {type.FullName} doesn't exist");
    }

    /// <summary>
    /// Gets a reference to the PlayerLoopSystem of the specified type within the current PlayerLoopSystem, allowing for modifications to the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to search within.</param>
    /// <param name="type">The type of the system to retrieve.</param>
    /// <returns>A reference to the PlayerLoopSystem of the specified type.</returns>
    /// <exception cref="Exception">Thrown when the system of the specified type doesn't exist.</exception>
    public static ref PlayerLoopSystem GetSystem(this PlayerLoopSystem system, Type type)
    {
        var length = system.subSystemList.Length;

        for (int i = 0; i < length; i++)
        {
            ref var subSystemA = ref system.subSystemList[i];

            if (subSystemA.type == type)
                return ref subSystemA;

            var lengthA = subSystemA.subSystemList?.Length;

            for (int a = 0; a < lengthA; a++)
            {
                ref var subSystemB = ref subSystemA.subSystemList[a];

                if (subSystemB.type == type)
                    return ref subSystemB;

                var lengthB = subSystemB.subSystemList?.Length;

                for (int b = 0; b < lengthB; b++)
                {
                    ref var subSystemC = ref subSystemB.subSystemList[b];

                    if (subSystemC.type == type)
                        return ref subSystemC;

                    var lengthC = subSystemC.subSystemList?.Length;

                    for (int c = 0; c < lengthC; c++)
                    {
                        ref var subSystemD = ref subSystemC.subSystemList[c];

                        if (subSystemD.type == type)
                            return ref subSystemD;
                    }
                }
            }
        }

        throw new Exception($"Player loop system type {type} doesn't exist");
    }

    /// <summary>
    /// Injects a custom update function into the Unity player loop system after the specified target system type, allowing for custom logic to be executed after the target system during each frame.
    /// </summary>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <param name="targetType">The type of the target system to inject after.</param>
    /// <param name="systemType">The type of the custom system to inject.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static bool InjectAfter(this PlayerLoopSystem system, UpdateFunction method, Type targetType, Type systemType)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        if (systemType is null)
            throw new ArgumentNullException(nameof(systemType));

        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));

        ref var parentSystem = ref GetParentSystem(system, targetType);

        for (int i = 0; i < parentSystem.subSystemList.Length; i++)
        {
            ref var subSystem = ref parentSystem.subSystemList[i];

            if (subSystem.type == systemType)
                return false;
        }

        for (int i = 0; i < parentSystem.subSystemList.Length; i++)
        {
            ref var subSystem = ref parentSystem.subSystemList[i];

            if (subSystem.type == targetType)
            {
                var customSystem = new PlayerLoopSystem();

                customSystem.type = systemType;
                customSystem.updateDelegate = method;

                var subSystemList = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

                subSystemList.Insert(i, customSystem);

                parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(subSystemList);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Injects a custom update function into the Unity player loop system before the specified target system type, allowing for custom logic to be executed before the target system during each frame.
    /// </summary>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="method">The custom update function to inject.</param>
    /// <param name="targetType">The type of the target system to inject before.</param>
    /// <param name="systemType">The type of the custom system to inject.</param>
    /// <returns>True if the injection was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static bool InjectBefore(this PlayerLoopSystem system, UpdateFunction method, Type targetType, Type systemType)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        if (systemType is null)
            throw new ArgumentNullException(nameof(systemType));

        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));

        ref var parentSystem = ref GetParentSystem(system, targetType);

        for (int i = 0; i < parentSystem.subSystemList.Length; i++)
        {
            ref var subSystem = ref parentSystem.subSystemList[i];

            if (subSystem.type == systemType)
                return false;
        }

        for (int i = -1; i < parentSystem.subSystemList.Length; i++)
        {
            if ((i + 1) < parentSystem.subSystemList.Length)
            {
                ref var subSystem = ref parentSystem.subSystemList[i + 1];

                if (subSystem.type == targetType)
                {
                    var customSystem = new PlayerLoopSystem();

                    customSystem.type = systemType;
                    customSystem.updateDelegate = method;

                    var subSystemList = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

                    subSystemList.Insert(i < 0 ? 0 : i, customSystem);

                    parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(subSystemList);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Removes the specified system type from the Unity player loop system, allowing for customization of the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="targetType">The type of the system to remove.</param>
    /// <returns>True if the removal was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static bool RemoveSystem(this PlayerLoopSystem system, Type targetType)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));

        ref var parentSystem = ref GetParentSystem(system, targetType);

        var copy = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

        for (int i = 0; i < parentSystem.subSystemList.Length; i++)
        {
            ref var subSystem = ref parentSystem.subSystemList[i];

            if (subSystem.type == targetType)
            {
                copy.RemoveAt(i);

                parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(copy);
                return true;
            }
        }

        ListPool<PlayerLoopSystem>.Shared.Return(copy);
        return false;
    }

    /// <summary>
    /// Removes all systems from the Unity player loop system that match the specified predicate, allowing for customization of the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to modify.</param>
    /// <param name="predicate">The predicate used to determine which systems to remove.</param>
    public static void RemoveSystems(this PlayerLoopSystem system, Predicate<PlayerLoopSystem> predicate)
    {
        var removeList = ListPool<Type>.Shared.Rent();

        for (int i = 0; i < system.subSystemList.Length; i++)
        {
            if (system.subSystemList[i].subSystemList is null || system.subSystemList[i].type is null)
                continue;

            for (int y = 0; y < system.subSystemList[i].subSystemList.Length; y++)
            {
                var sub = system.subSystemList[i].subSystemList[y];

                if (sub.type is null)
                    continue;

                if (!predicate(sub))
                    continue;

                removeList.Add(sub.type);
            }

            if (removeList.Count > 0)
            {
                var copyList = ListPool<PlayerLoopSystem>.Shared.Rent(system.subSystemList[i].subSystemList);

                ref var copyRef = ref system.subSystemList[i];

                copyList.RemoveAll(p => p.type != null && removeList.Contains(p.type));
                copyRef.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(copyList);
            }
        }

        ListPool<Type>.Shared.Return(removeList);
    }

    /// <summary>
    /// Gets a list of the full names of all subsystems in the Unity player loop system, allowing for inspection of the execution order of subsystems in Unity.
    /// </summary>
    /// <param name="system">The player loop system to inspect.</param>
    /// <returns>A list of the full names of all subsystems.</returns>
    public static List<string> GetLoopNames(this PlayerLoopSystem system)
    {
        var loops = new List<string>();

        if (system.subSystemList != null)
        {
            var list = new Stack<PlayerLoopSystem>();

            list.Push(system);

            while (list.Count > 0 && ExServer.IsRunning)
            {
                var sys = list.Pop();

                if (sys.type != null)
                {
                    loops.Add(sys.type.FullName);
                }

                if (sys.subSystemList != null)
                {
                    for (int i = 0; i < sys.subSystemList.Length; i++)
                    {
                        list.Push(sys.subSystemList[i]);
                    }
                }
            }
        }

        return loops;
    }

    /// <summary>
    /// Gets a formatted string representation of the names of all subsystems in the Unity player loop system, allowing for inspection of the execution order of subsystems in Unity with indentation for hierarchy visualization.
    /// </summary>
    /// <param name="system">The player loop system to inspect.</param>
    /// <param name="indent">The string to use for indentation of each level in the hierarchy.</param>
    /// <returns>A formatted string representation of the names of all subsystems.</returns>
    public static string GetPlayerLoopNames(this PlayerLoopSystem system, string indent = "    ")
    {
        var builder = StringBuilderPool.Shared.Rent();

        if (system.subSystemList != null)
        {
            var list = new Stack<Tuple<int, PlayerLoopSystem>>();

            list.Push(new Tuple<int, PlayerLoopSystem>(0, system));

            while (list.Count > 0 && ExServer.IsRunning)
            {
                var tuple = list.Pop();
                var depth = tuple.Item1;

                if (tuple.Item2.type != null)
                {
                    for (int i = 0; i < depth; i++)
                        builder.Append(indent);

                    builder.Append(tuple.Item2.type.FullName);
                    builder.AppendLine();
                }
                else
                {
                    depth--;
                }

                if (tuple.Item2.subSystemList != null)
                {
                    for (int i = 0; i < tuple.Item2.subSystemList.Length; i++)
                    {
                        list.Push(new Tuple<int, PlayerLoopSystem>(depth + 1, tuple.Item2.subSystemList[i]));
                    }
                }
            }
        }

        return StringBuilderPool.Shared.ToStringReturn(builder);
    }

    private static void InvokeAfter() 
        => AfterLoop?.InvokeSafe();

    private static void InvokeBefore() 
        => BeforeLoop?.InvokeSafe();

    internal static void Internal_InitFirst()
    {
        ModifySystem(x =>
        {
            x.InjectBefore<Initialization.ProfilerStartFrame>(InvokeBefore, typeof(CustomBeforePlayerLoop));
            x.InjectAfter<PostLateUpdate.UpdateVideo>(InvokeAfter, typeof(CustomAfterPlayerLoop));

            return x;
        });
    }

    internal static void Internal_InitLast()
    {
        ModifySystem(x =>
        {
            x.RemoveSystems(s =>
            {
                if (RemovedLoops.Contains(s.type.FullName) || RemovedLoops.Contains(s.type.Name))
                    return true;

                return false;
            });

            return x;
        });
    }
}