using System.Reflection;

using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Runners; 

using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Values;

namespace LabExtended.Commands;

using Parameters;

/// <summary>
/// Represents a method of a command.
/// </summary>
public class CommandOverload
{
    /// <summary>
    /// Gets the targeted method.
    /// </summary>
    public MethodInfo Target { get; }

    /// <summary>
    /// Gets the command path leading to this overload.
    /// </summary>
    public List<string> Path { get; } = new();
    
    /// <summary>
    /// Gets all parameters from this overload.
    /// </summary>
    public List<CommandParameter> Parameters { get; } = new();
    
    /// <summary>
    /// Gets all parameter builders from this overload.
    /// </summary>
    public Dictionary<string, CommandParameterBuilder> ParameterBuilders { get; } = new();

    /// <summary>
    /// Gets the empty buffer.
    /// </summary>
    public object[] EmptyBuffer { get; } = Array.Empty<object>();
    
    /// <summary>
    /// Whether or not this overload has any arguments.
    /// </summary>
    public bool IsEmpty { get; }
    
    /// <summary>
    /// Whether or not this overload should be executed asynchronously.
    /// </summary>
    public bool IsAsync { get; }
    
    /// <summary>
    /// Whether or not this overload is a coroutine.
    /// </summary>
    public bool IsCoroutine { get; }
    
    /// <summary>
    /// Whether or not this overload has been initialized.
    /// </summary>
    public bool IsInitialized { get; internal set; }

    /// <summary>
    /// Gets the amount of required parameters.
    /// </summary>
    public int RequiredParameters { get; }

    /// <summary>
    /// Gets the amount of parameters.
    /// </summary>
    public int ParameterCount { get; }
    
    /// <summary>
    /// Gets the overload's name.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the overload's description.
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the permission required to execute the overload.
    /// </summary>
    public string? Permission { get; internal set; }

    /// <summary>
    /// Gets the overload's buffer.
    /// </summary>
    public ReusableValue<object[]> Buffer { get; }
    
    /// <summary>
    /// Gets the assigned runner.
    /// </summary>
    public ICommandRunner Runner { get; }

    /// <summary>
    /// Creates a new <see cref="CommandOverload"/> instance.
    /// <param name="target">The method that this overload targets.</param>
    /// </summary>
    public CommandOverload(MethodInfo target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var parameters = target.GetAllParameters();

        Target = target;
        ParameterCount = parameters.Length;
        RequiredParameters = parameters.Count(x => !x.HasDefaultValue);

        IsEmpty = parameters?.Length == 0;
        IsAsync = target.ReturnType == typeof(Task);
        IsCoroutine = target.ReturnType == typeof(IEnumerator<float>);
        
        if (target.DeclaringType != null && target.DeclaringType.InheritsType<ContinuableCommandBase>())
        {
            Runner = ContinuableCommandRunner.Singleton;
        }
        else
        {
            Runner = RegularCommandRunner.Singleton;
        }
        
        foreach (var parameter in parameters!)
        {
            var builder = new CommandParameterBuilder(parameter);
            
            ParameterBuilders.Add(parameter.Name, builder);
            Parameters.Add(builder.Result);
        }

        if (!IsEmpty)
            Buffer = new(new object[ParameterCount], () => new object[ParameterCount]);
    }

    /// <summary>
    /// Invokes the underlying command or method with the specified context and arguments.
    /// </summary>
    /// <param name="ctx">The command context that provides the target instance and execution environment. Cannot be null.</param>
    /// <param name="args">An array of arguments to pass to the command or method. The number of elements must match the expected parameter
    /// count. If the command takes no parameters, this can be null or an empty array.</param>
    /// <returns>The result returned by the invoked command or method. The type and meaning of the result depend on the specific
    /// command implementation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if ctx is null, or if args is null when the command expects one or more parameters.</exception>
    /// <exception cref="ArgumentException">Thrown if the number of elements in args does not match the expected parameter count.</exception>
    public object Invoke(CommandContext ctx, object[] args)
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));

        if (args is null && !IsEmpty)
            throw new ArgumentNullException(nameof(args));

        args ??= Array.Empty<object>();

        if (args.Length != ParameterCount)
            throw new ArgumentException($"Expected {ParameterCount} arguments, but got {args.Length}.");

        return Target.Invoke(ctx.Instance, args);
    }
}