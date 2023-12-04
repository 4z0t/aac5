using Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interpreter.Interpreter;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    class ForStatement : IStatement
    {
        public void Process(Interpreter interpreter, Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var stack = context.Tokens;
            var charecter = stack.Pop();
            if (charecter.Type != TokenType.Character)
                throw ExpressionsHellper.ThrowUnexpectedToken(charecter);
            ExpressionsHellper.CheckStack(context);
            var equal = stack.Pop();
            if (equal.Type != TokenType.Equal)
                throw ExpressionsHellper.ThrowUnexpectedToken(equal);
            var left = Expression.Parse(context);
            ExpressionsHellper.CheckStack(context);
            var to = stack.Pop();
            if (to.TokenString != "to")
                throw ExpressionsHellper.ThrowUnexpectedToken(equal);
            var right = Expression.Parse(context);
            context.Variables[charecter.TokenString] = left;
            EvalForLoop(interpreter, context, charecter, right);
        }

        private void EvalForLoop(Interpreter interpreter, Context context, Token charecter, int right)
        {
            var queue = context.Tokens;
            var commands = new List<Token>();
            int bracketCount = 1;
            var token = queue.Pop();
            if (token.TokenString != "{")
                throw ExpressionsHellper.ThrowUnexpectedToken(token);
            commands.Add(token);
            while (queue.Count > 0 && bracketCount != 0)
            {
                token = queue.Pop();
                commands.Add(token);
                if (token.TokenString == "{")
                    bracketCount++;
                if (token.TokenString == "}")
                    bracketCount--;
            }
            if (bracketCount > 0)
                throw new Exception("Expected '}'");

            while (context.Variables[charecter.TokenString] < right)
            {
                try
                {
                    interpreter.EvalStatement(new Context(commands, context.Variables));
                    context.Variables[charecter.TokenString]++;
                }
                catch (ContinueException)
                {
                    context.Variables[charecter.TokenString]++;
                    continue;
                }
                catch (BreakException)
                {
                    break;
                }
            }
        }

    }
}
