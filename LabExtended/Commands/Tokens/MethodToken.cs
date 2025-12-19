using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Commands.Tokens
{
    /// <summary>
    /// Used to parse string formatted like 'method(arg, arg)'.
    /// </summary>
    public class MethodToken : PoolObject, ICommandToken
    {
        /// <summary>
        /// Represents a delegate used to execute a method with a specified command context
        /// and method token while producing an output result.
        /// Returns a boolean value indicating success or failure of the execution.
        /// </summary>
        /// <param name="context">The command execution context providing necessary execution details.</param>
        /// <param name="token">The method token representing the method and its parsed arguments.</param>
        /// <param name="result">The output of the method execution, if any.</param>
        /// <returns>
        /// A boolean value indicating whether the method execution was successful.
        /// </returns>
        public delegate bool MethodDelegate(CommandContext context, MethodToken token, out object result);
        
        /// <summary>
        /// Gets the instance of the method token.
        /// </summary>
        public static MethodToken Instance { get; } = new();

        /// <summary>
        /// A collection of all registered methods.
        /// </summary>
        public static Dictionary<string, MethodDelegate> Methods { get; } = new();
        
        private MethodToken() { }

        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether or not the parser is currently processing the method's name.
        /// </summary>
        public bool IsInName { get; set; }
        
        /// <summary>
        /// Whether or not the parser is currently processing the method's arguments.
        /// </summary>
        public bool IsInArguments { get; set; }

        /// <summary>
        /// Gets a list of parsed method arguments.
        /// </summary>
        public List<string> Arguments { get; } = new();

        /// <summary>
        /// A collection of parsed arguments, either a <see cref="StringToken"/> or a <see cref="MethodToken"/>.
        /// </summary>
        public List<ICommandToken> ParsedArguments { get; } = new();

        /// <summary>
        /// Parses the arguments of the method token, extracting them into individual components
        /// and converting them into appropriate command tokens based on their structure.
        /// </summary>
        /// <remarks>
        /// This method processes the <see cref="Arguments"/> list, removing any empty or whitespace
        /// entries, and creates parsed tokens for each valid entry. If an argument contains nested
        /// method tokens (indicated by parentheses), it recursively parses them into
        /// <see cref="MethodToken"/> instances. Otherwise, it creates <see cref="StringToken"/>
        /// instances for simple arguments and adds them to the <see cref="ParsedArguments"/> list.
        /// </remarks>
        public void ParseArguments()
        {
            Arguments.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            
            if (Arguments.Count > 0)
            {
                for (var x = 0; x < Arguments.Count; x++)
                {
                    var argument = Arguments[x];
                    var openIndex = argument.IndexOf('(');

                    if (openIndex == -1)
                    {
                        var stringToken = StringToken.Instance.NewToken<StringToken>();

                        stringToken.Value = argument;
                        
                        ParsedArguments.Add(stringToken);
                        continue;
                    }
                    
                    var closeIndex = argument.IndexOf(')', openIndex);

                    if (closeIndex == -1)
                    {
                        var stringToken = StringToken.Instance.NewToken<StringToken>();

                        stringToken.Value = argument;
                        
                        ParsedArguments.Add(stringToken);
                        continue;
                    }

                    var name = argument.Substring(0, openIndex);
                    var args = (argument.Substring(openIndex, closeIndex) ?? string.Empty).Split(';');
                    
                    var token = this.NewToken<MethodToken>();

                    token.Name = name;

                    token.IsInName = false;
                    token.IsInArguments = true;
                    
                    token.Arguments.AddRange(args);
                    token.ParseArguments();
                    
                    ParsedArguments.Add(token);
                }
            }
        }

        /// <summary>
        /// Attempts to execute the method associated with the current token within
        /// the provided command context and retrieves the result as the specified type.
        /// </summary>
        /// <typeparam name="T">The type expected for the result of the method execution.</typeparam>
        /// <param name="ctx">The context in which the command is being executed, providing necessary information.</param>
        /// <param name="result">The output parameter that contains the result of the method execution if successful; otherwise, it will be <c>default</c>.</param>
        /// <returns><c>true</c> if the method was executed successfully and the result could be cast to the specified type; otherwise, <c>false</c>.</returns>
        public bool TryExecuteMethod<T>(CommandContext ctx, out T? result)
        {
            result = default;

            if (!TryExecuteMethod(ctx, out var obj))
                return false;

            if (obj is not T value)
                return false;

            result = value;
            return true;
        }

        /// <summary>
        /// Attempts to execute a method associated with this <see cref="MethodToken"/> using the provided
        /// <see cref="CommandContext"/>. If a method corresponding to the token's name exists, it is invoked and
        /// its result is returned.
        /// </summary>
        /// <param name="ctx">The <see cref="CommandContext"/> providing execution context for the method, such as
        /// arguments and runtime information.</param>
        /// <param name="result">When the method exists and executes successfully, this parameter is assigned its
        /// output result. If execution fails or no method matches, this is set to <c>null</c>.</param>
        /// <returns>
        /// <c>true</c> if a matching method name is found and its execution generates a result successfully;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool TryExecuteMethod(CommandContext ctx, out object? result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (!Methods.TryGetValue(Name, out var method))
                return false;

            return method(ctx, this, out result) && result != null;
        }

        /// <inheritdoc />
        public override void OnReturned()
        {
            base.OnReturned();

            Name = string.Empty;

            IsInName = true;
            IsInArguments = false;
            
            Arguments.Clear();
            
            ParsedArguments.ForEach(t => t.ReturnToken());
            ParsedArguments.Clear();
        }

        /// <inheritdoc />
        public ICommandToken NewToken()
            => ObjectPool<MethodToken>.Shared.Rent();

        /// <inheritdoc />
        public void ReturnToken()
            => ObjectPool<MethodToken>.Shared.Return(this);
    }
}