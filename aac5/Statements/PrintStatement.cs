using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interpreter.Interpreter;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    class PrintStatement : IStatement
    {
        public void Process(Interpreter interpreter, Interpreter.Context context)
        {
            var result = Print(context);
            Console.WriteLine(result);
        }

        private string Print(Context context)
        {
            var stack = context.Tokens;
            ExpressionsHellper.CheckStack(context);
            var token = stack.Pop();
            if (token.Type == TokenType.QuoteString)
            {
                if (stack.Count > 0 && stack.Peek().TokenString == ",")
                {
                    stack.Pop();
                    return token.TokenString + " " + Print(context);
                }
                else
                    return token.TokenString;
            }
            else if (token.Type == TokenType.Character || token.Type == TokenType.Digit)
            {
                context.Tokens.Push(token);
                var expression = Expression.Parse(context).ToString();
                bool isNextComma = stack.Count > 0 && stack.Peek().TokenString == ",";
                if (stack.Count > 0 && isNextComma)
                {
                    stack.Pop();
                    return expression + " " + Print(context);
                }
                else
                    return expression;
            }
            else
                throw new Exception("Invalid <print_end>.");
        }

    }
}
