using LabExtended.Commands.Tokens;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters.Parsers.Wrappers;

using UnityEngine;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parsers <see cref="Vector3"/>
/// </summary>
public class Vector3ParameterParser : AxisWrapperParser<Vector3>
{
    /// <inheritdoc cref="AxisWrapperParser{T}.AxisNames"/>
    public override char[] AxisNames { get; } = ['x', 'y', 'z'];

    /// <inheritdoc />
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        if (token is MethodToken methodToken && methodToken.TryExecuteMethod<Vector3>(context, out var position))
            return new(true, position, null, parameter, this);
        
        return base.Parse(tokens, token, tokenIndex, context, parameter);
    }

    /// <inheritdoc cref="AxisWrapperParser{T}.ToAxis(System.Collections.Generic.Dictionary{char,float},out string,out T)"/>
    public override bool ToAxis(Dictionary<char, float> values, out string error, out Vector3 result)
    {
        error = null!;
        
        result = new(values['x'], values['y'], values['z']);
        return true;
    }
}