using System;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    public class ExpressionsHellper
    {
        public static void CheckStack(Interpreter.Context context)
        {
            if (context.Tokens.Count <= 0)
                throw new Exception("Unexpected symbol.");
        }

        public static Exception ThrowUnexpectedToken(Token token)
        {
            throw new Exception($"Unexpected symbol {token.TokenString}.");
        }
    }
}
