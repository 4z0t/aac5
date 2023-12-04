using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    class ScanStatement : IStatement
    {
        public void Process(Interpreter interpreter, Interpreter.Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var token = context.Tokens.Pop();
            if (token.Type != TokenType.Character)
                throw new Exception($"Invalid symbol for \"scan\" {token.TokenString}.");
            var value = Int32.Parse(Console.ReadLine());
            context.Variables[token.TokenString] = value;
            ExpressionsHellper.CheckStack(context);
            token = context.Tokens.Pop();
            if (token.TokenString != ";")
                throw new Exception($"Invalid symbol for \"scan\" {token.TokenString}.");
        }
    }
}
