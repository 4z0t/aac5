using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    public class Interpreter
    {
        public void Interpret(string code)
        {
            var tokinizer = new Tokenizer();
            var tokens = tokinizer.Tokenize(code);
            var onlyTokens = tokens.Where(x => x.Type != TokenType.Space);
            var context = new Context(onlyTokens, Variables);
            Interpret(context);
        }


        class BreakException : Exception { };
        class ContinueException : Exception { };

        private void Interpret(Context context)
        {
            try
            {
                while (context.Tokens.Count > 0)
                    Statement(context);
            }
            catch(BreakException)
            {
                Console.WriteLine("Unexpected break statement");
            }
            catch(ContinueException)
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
            if (token == "if")
            {
                EvalIfStatement(context);
            }
            else if (token == "for")
            {
                ForStatement(context);
            }
            else if (token == "while")
            {
                WhileStatement(context);
            }
            else if (token == "print")
            {
                Print(context);
            }
            else if (token == "scan")
            {
                Scan(context);
            }
            else if (token == "continue")
            {
                Continue(context);
            }
            else if (token == "break")
            {
                Break(context);
            }
            else if (type == TokenType.Character)
            {
                context.Tokens.Push(tokenFull);
                Assign(context);
            }
            else
                throw ExpressionsHellper.ThrowUnexpectedToken(tokenFull);
        }


        private void Break(Context context)
        {
            throw new BreakException();
        }
        private void Continue(Context context)
        {
            throw new ContinueException();
        }

        private void Assign(Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var identifier = context.Tokens.Pop();
            var equalOperator = context.Tokens.Pop();
            if (equalOperator.Type != TokenType.Equal)
                throw ExpressionsHellper.ThrowUnexpectedToken(equalOperator);
            var expression = Expression.Parse(context);
            context.Variables[identifier.TokenString] = expression;
        }

        private void Scan(Context context)
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

        private void Print(Context context)
        {
            var result = PrintEnd(context);
            Console.WriteLine(result);
        }

        private string PrintEnd(Context context)
        {
            var stack = context.Tokens;
            ExpressionsHellper.CheckStack(context);
            var token = stack.Pop();
            if (token.Type == TokenType.QuoteString)
            {
                if (stack.Count > 0 && stack.Peek().TokenString == ",")
                {
                    stack.Pop();
                    return token.TokenString + " " + PrintEnd(context);
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
                    return expression + " " + PrintEnd(context);
                }
                else
                    return expression;
            }
            else
                throw new Exception("Invalid <print_end>.");
        }

        private void WhileStatement(Context context)
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
                        EvalStatement(forContext);
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

        private void ForStatement(Context context)
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
            EvalForLoop(context, charecter, right);
        }

        private void EvalForLoop(Context context, Token charecter, int right)
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
                    var forContext = new Context(commands, context.Variables);
                    EvalStatement(forContext);
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


        private void SkipStatement(Context context)
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

        private void EvalIfStatement(Context context)
        {
            var queue = context.Tokens;
            if (EvalBoolExpression(context))
            {
                EvalStatement(context);
                if (queue.Count != 0 && queue.Peek().TokenString == "else")
                {
                    queue.Pop();
                    SkipStatement(context);
                }
            }
            else
            {
                SkipStatement(context);
                ElseBlock(context);
            }
        }

        private static void PrintStack(Stack<Token> stack)
        {
            var stackCopy = new Stack<Token>(stack.Reverse());
            while(stackCopy.Count != 0)
            {
                Console.WriteLine(stackCopy.Pop());
            }
            Console.WriteLine();
        }

        private void EvalStatement(Context context)
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

        private void ElseBlock(Context context)
        {
            var queue = context.Tokens;
            if (queue.Count == 0)
                return;
            var token = queue.Peek();
            if (token.TokenString == "else")
            {
                queue.Pop();
                EvalStatement(context);
            }
        }

        private bool EvalBoolExpression(Context context)
        {
            ExpressionsHellper.CheckStack(context);
            var left = Expression.Parse(context);
            ExpressionsHellper.CheckStack(context);
            var token = context.Tokens.Pop();
            var rigth = Expression.Parse(context);
            if (token.TokenString == ">")
            {
                return left > rigth;
            }
            else if (token.TokenString == "<")
            {
                return left < rigth;
            }
            else if (token.TokenString == "==")
            {
                return left == rigth;
            }
            else if (token.TokenString == "!=")
            {
                return left != rigth;
            }
            else
                ExpressionsHellper.ThrowUnexpectedToken(token);
            return false;
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
