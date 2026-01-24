using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.Commands.Custom.Performance;

/// <summary>
/// Provides a command for displaying detailed performance statistics and running the profiler for a specified duration.
/// </summary>
[Command("performance", "Shows detailed performance statistics.", "perf")]
public partial class PerformanceCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Runs the profiler for the specified duration in milliseconds.
    /// </summary>
    /// <param name="duration">The length of time, in milliseconds, to run the profiler. Must be a non-negative value.</param>
    [CommandOverload(null, "performance.profiler")]
    public void InvokeCommand(int duration)
    {
        ProfilerUtils.Run(duration);
        
        Ok($"Running the profiler for '{ProfilerUtils.Duration}' milliseconds ({ProfilerUtils.Duration - ProfilerUtils.Time.ElapsedMilliseconds} remaining)");
    }

    /// <summary>
    /// Displays the current and target tick rates for the server.
    /// </summary>
    /// <remarks>This method outputs the server's current ticks per second and the configured target frame
    /// rate. Use this to monitor server performance or diagnose timing issues.</remarks>
    [CommandOverload("ticks", "Displays the current & target amount of ticks.", "performance.ticks")]
    public void InvokeTicks()
        => Ok($"Running at &1{ExServer.Tps}&r ticks per second (target: &1{Application.targetFrameRate}&r)");
}