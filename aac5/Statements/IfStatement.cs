using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interpreter.Interpreter;

namespace Interpreter
{
    class IfStatement : IStatement
    {
        public virtual void Process(Interpreter interpreter, Context context)
        {
            var queue = context.Tokens;
            if (EvalBoolExpression(context))
            {
                interpreter.EvalStatement(context);
                if (queue.Count != 0 && queue.Peek().TokenString == "else")
                {
                    queue.Pop();
                    interpreter.SkipStatement(context);
                }
            }
            else
            {
                interpreter.SkipStatement(context);
                ElseBlock(interpreter, context);
            }
        }

        protected bool EvalBoolExpression(Context context)
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

        private void ElseBlock(Interpreter interpreter, Context context)
        {
            var queue = context.Tokens;
            if (queue.Count == 0)
                return;
            var token = queue.Peek();
            if (token.TokenString == "else")
            {
                queue.Pop();
                interpreter.EvalStatement(context);
            }
        }

    }
}
