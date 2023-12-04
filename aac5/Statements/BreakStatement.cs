using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class BreakStatement : IStatement
    {
        public void Process(Interpreter interpreter, Interpreter.Context context)
        {
            throw new BreakException();
        }
    }
}
