using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interpreter.Interpreter;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    class WhileStatement : IfStatement
    {
        public override void Process(Interpreter interpreter, Interpreter.Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var stack = context.Tokens;
            var boolExprCommands = new List<Token>();
            var token = stack.Pop();
            while (token.TokenString != "{")
            {
                boolExprCommands.Add(token);
                token = stack.Pop();
            }

            int bracketCount = 1;
            var commands = new List<Token>
            {
                token
            };

            while (stack.Count > 0 && bracketCount != 0)
            {
                token = stack.Pop();
                commands.Add(token);
                if (token.TokenString == "{")
                    bracketCount++;
                if (token.TokenString == "}")
                    bracketCount--;
            }
            if (bracketCount > 0)
                throw new Exception("Expected '}'");


            while (true)
            {
                var exprContext = new Context(boolExprCommands, context.Variables);
                if (EvalBoolExpression(exprContext))
                {
                    try
                    {
                        var forContext = new Context(commands, context.Variables);
                        interpreter.EvalStatement(forContext);
                    }
                    catch (ContinueException)
                    {
                        continue;
                    }
                    catch (BreakException)
                    {
                        break;
                    }
                    catch { throw; }
                }
                else
                    break;
            }
        }
    }
}
