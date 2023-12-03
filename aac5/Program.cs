using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class Program
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new Interpreter();
            if (args.Length == 0)
            {
                while (true)
                {
                    var code = Console.ReadLine();
                    if (code == "exit") break;
                    interpreter.Interpret(code);
                }
            }
            else
            {

                var code = File.ReadAllText(args[0]);//"../../../Resources/Code.txt"
                interpreter.Interpret(code);
            }
        }
    }
}
