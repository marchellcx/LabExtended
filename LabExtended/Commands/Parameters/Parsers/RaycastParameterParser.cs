using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Tokens;

using UnityEngine;

namespace LabExtended.Commands.Parameters.Parsers
{
    /// <summary>
    /// Parses command parameters by evaluating tokens that represent raycast methods.
    /// </summary>
    /// <remarks>
    /// This parser is designed specifically to handle tokens of type <see cref="MethodToken"/>
    /// that indicate a method capable of resolving a raycast operation.
    /// </remarks>
    public class RaycastParameterParser : CommandParameterParser
    {
        /// <inheritdoc />
        public override string? FriendlyAlias { get; } = "RayCast";

        /// <inheritdoc />
        public override string? UsageAlias { get; } = "ray(distance, mask)";

        /// <inheritdoc />
        public override bool AcceptsToken(ICommandToken token)
            => token is MethodToken;

        /// <inheritdoc />
        public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
            CommandParameter parameter)
        {
            if (token is not MethodToken methodToken)
                return new(false, null, "Invalid token provided", parameter, this);

            if (!methodToken.TryExecuteMethod<RaycastHit>(context, out var cast))
                return new(false, null, "Method did not resolve a raycast", parameter, this);
            
            return new(true, cast, null, parameter, this);
        }
    }
}