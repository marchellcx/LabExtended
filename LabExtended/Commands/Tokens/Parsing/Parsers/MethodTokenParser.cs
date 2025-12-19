using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Tokens.Parsing.Parsers
{
    /// <summary>
    /// Represents a parser that identifies and processes method tokens within a command input.
    /// A method token is defined as a valid identifier followed by a pair of parentheses,
    /// potentially containing arguments.
    /// </summary>
    public class MethodTokenParser : CommandTokenParser
    {
        // Method()
        // Method(Arg)
        // Method(Arg; Arg; Arg)
        /// <inheritdoc />
        public override bool ShouldStart(CommandTokenParserContext context)
        {
            if (context.PreviousCharIsEscape())
                return false;

            var openBracketIndex = context.Input.IndexOf('(', context.Index);

            if (openBracketIndex == -1 || (openBracketIndex - context.Index) < 1)
                return false;

            var closeBracketIndex = context.Input.IndexOf(')', context.Index);

            if (closeBracketIndex == -1)
                return false;

            return true;
        }

        /// <inheritdoc />
        public override bool ShouldTerminate(CommandTokenParserContext context)
        {
            if (context.PreviousCharIsEscape())
                return false;

            if (!context.CurrentCharIs(')'))
                return false;

            if (context.NextCharIs(')'))
                return false;

            return true;
        }

        /// <inheritdoc />
        public override bool ProcessContext(CommandTokenParserContext context)
        {
            if (context.CurrentParser is not MethodTokenParser
                || !context.CurrentTokenIs<MethodToken>(out var methodToken))
                return true;
            
            // Prevents a leading whitespace.
            if (context.IsCurrentWhiteSpace && context.Builder.Length < 1)
                return false;

            if (context.CurrentCharIs('\\') && !context.PreviousCharIsEscape())
                return false;

            if (methodToken.IsInName)
            {
                if (context.NextCharIs('('))
                {
                    methodToken.Name = context.Builder.ToString();

                    methodToken.IsInName = false;
                    methodToken.IsInArguments = true;
                }
                else
                {
                    context.Builder.Append(context.CurrentChar);
                }
            }

            if (methodToken.IsInArguments)
            {
                if (context.CurrentCharIs(';') && !context.PreviousCharIsEscape())
                {
                    if (context.Builder.Length > 0)
                    {
                        methodToken.Arguments.Add(context.Builder.ToString());

                        context.Builder.Clear();
                    }
                }
                else
                {
                    context.Builder.Append(context.CurrentChar);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override void OnTerminated(CommandTokenParserContext context)
        {
            base.OnTerminated(context);

            if (!context.CurrentTokenIs<MethodToken>(out var methodToken))
                return;

            if (context.Builder.Length > 1)
            {
                context.Builder.RemoveTrailingWhiteSpaces();

                methodToken.Arguments.Add(context.Builder.ToString());
            }

            methodToken.ParseArguments();
        }

        /// <inheritdoc />
        public override ICommandToken CreateToken(CommandTokenParserContext context)
            => MethodToken.Instance.NewToken();
    }
}