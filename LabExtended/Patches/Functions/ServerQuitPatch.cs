using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Events;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions;

/// <summary>
/// Patches the <see cref="ExServerEvents.Quitting"/> event.
/// </summary>
public static class ServerQuitPatch
{
    /// <summary>
    /// Transpiles the <see cref="ServerConsole.OnApplicationQuit"/> method to invoke the <see cref="ExServerEvents.OnQuitting"/> event.
    /// </summary>
    /// <param name="generator">The IL generator.</param>
    /// <param name="instructions">The original IL instructions.</param>
    /// <param name="originalMethod">The original method being transpiled.</param>
    /// <returns>The modified IL instructions.</returns>
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.OnApplicationQuit))]
    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, 
        IEnumerable<CodeInstruction> instructions,
        MethodBase originalMethod)
    {
        return generator.RunTranspiler(instructions, originalMethod, ctx =>
        {
            ctx.Index = 0;
            ctx.Call(typeof(ExServerEvents), nameof(ExServerEvents.OnQuitting));
        });
    }
}