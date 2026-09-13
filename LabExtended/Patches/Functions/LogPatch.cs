using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Attributes;

namespace LabExtended.Patches.Functions;

/// <summary>
/// Provides a Harmony patch for the ServerConsole.AddLog method to enable custom logging event handling.
/// </summary>
[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
public static class LogPatch
{
    /// <summary>
    /// Transpiler method that modifies the IL code of the ServerConsole.AddLog method to invoke the ExServerEvents.OnLogging event.
    /// </summary>
    /// <param name="generator">The IL generator used to emit new instructions during the transpilation process.</param>
    /// <param name="instructions">The original sequence of IL instructions to be modified.</param>
    /// <param name="originalMethod">The method being patched. Provides context for the transpiler and may be used to tailor the transformation.</param>
    /// <returns>An enumerable collection of CodeInstruction objects representing the modified IL instructions, including the injected logging calls.</returns>
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
    {
        return generator.RunTranspiler(instructions, originalMethod, ctx =>
        {
            ctx.FindIndex(OpCodes.Call,AccessTools.Method(typeof(ServerConsole), nameof(ServerConsole.PrintOnOutputs)));     
            ctx.LoadZeroArgument();
            ctx.Call(typeof(ExServerEvents), nameof(ExServerEvents.OnLogging), false);
        });
    }

    [LoaderInitialize]
    private static void Initialize()
    {
        ApiPatcher.labExPatchCountOffset += ApiPatcher.Harmony.CreateClassProcessor(typeof(LogPatch)).Patch().Count;
    }
}