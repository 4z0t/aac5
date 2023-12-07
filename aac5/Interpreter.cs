using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Interpreter.Tokenizer;

namespace Interpreter
{

    class BreakException : Exception { };
    class ContinueException : Exception { };
    public class Interpreter
    {
        public void Interpret(string code)
        {
            var tokinizer = new Tokenizer();
            var tokens = tokinizer.Tokenize(code);
            var onlyTokens = tokens.Where(x => x.Type != TokenType.Space);
            Interpret(new Context(onlyTokens, Variables));
        }

        private static readonly Dictionary<string, IStatement> statements = new()
        {
            ["if"] = new IfStatement(),
            ["for"] = new ForStatement(),
            ["while"] = new WhileStatement(),
            ["print"] = new PrintStatement(),
            ["scan"] = new ScanStatement(),
            ["continue"] = new ContinueStatement(),
            ["break"] = new BreakStatement(),
        };

        private void Interpret(Context context)
        {
            try
            {
                while (context.Tokens.Count > 0)
                    Statement(context);
            }
            catch (BreakException)
            {
                Console.WriteLine("Unexpected break statement");
            }
            catch (ContinueException)
            {
                Console.WriteLine("Unexpected continue statement");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Statement(Context context)
        {
            if (context.Tokens.Count == 0)
                return;
            var tokenFull = context.Tokens.Pop();
            var type = tokenFull.Type;
            var token = tokenFull.TokenString;
            if (statements.ContainsKey(token))
            {
                statements[token].Process(this, context);
            }
            else if (type == TokenType.Character)
            {
                context.Tokens.Push(tokenFull);
                AssignStatement(context);
            }
            else
                throw ExpressionsHellper.ThrowUnexpectedToken(tokenFull);
        }

        private void AssignStatement(Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var identifier = context.Tokens.Pop();
            var equalOperator = context.Tokens.Pop();
            if (equalOperator.Type != TokenType.Equal)
                throw ExpressionsHellper.ThrowUnexpectedToken(equalOperator);
            var expression = Expression.Parse(context);
            context.Variables[identifier.TokenString] = expression;
        }

        public void SkipStatement(Context context)
        {
            var queue = context.Tokens;
            var token = queue.Pop();
            int bracketCount = 1;
            if (token.TokenString != "{")
                throw ExpressionsHellper.ThrowUnexpectedToken(token);
            while (queue.Count > 0 && bracketCount != 0)
            {
                token = queue.Pop();
                if (token.TokenString == "{")
                    bracketCount++;
                if (token.TokenString == "}")
                    bracketCount--;
            }
            if (bracketCount > 0)
                throw new Exception("Expected '}'.");
        }

        private static void PrintStack(Stack<Token> stack)
        {
            var stackCopy = new Stack<Token>(stack.Reverse());
            while (stackCopy.Count != 0)
            {
                Console.WriteLine(stackCopy.Pop());
            }
            Console.WriteLine();
        }

        public void EvalStatement(Context context)
        {
            var queue = context.Tokens;
            ExpressionsHellper.CheckStack(context);
            var token = queue.Pop();
            if (token.TokenString != "{")
                ExpressionsHellper.ThrowUnexpectedToken(token);
            while (queue.Count > 0 && queue.Peek().TokenString != "}")
            {
                Statement(context);
            }
            ExpressionsHellper.CheckStack(context);
            token = queue.Pop();
            if (token.TokenString != "}")
                ExpressionsHellper.ThrowUnexpectedToken(token);
        }

        Dictionary<string, int> Variables { get; init; } = new();

        public class Context
        {
            public Dictionary<string, int> Variables;
            public Stack<Token> Tokens;

            public Context(IEnumerable<Token> tokens, Dictionary<string, int>? vars = null)
            {
                Tokens = new Stack<Token>(tokens.Reverse());
                Variables = vars ?? new();
            }
        }
    }
}
