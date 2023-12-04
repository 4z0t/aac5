using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Interpreter
{
    interface IStatement
    {
        void Process(Interpreter interpreter, Interpreter.Context context);
    }
}
