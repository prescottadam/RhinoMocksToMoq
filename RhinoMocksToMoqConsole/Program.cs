using System;
using System.IO;
using RhinoMocksToMoq;

namespace RhinoMocksToMoqConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!(args.Length == 1 || (args.Length == 3 && args[1] == "-out")))
            {
                Console.WriteLine("Usage: RhinoMocksToMoqConsole <filepath> [-out file]");
                Environment.Exit(1);
            }

            string sourceCode = File.ReadAllText(args[0]);

            var newSourceCode = ClassConverter.Convert(sourceCode);

            if (args.Length == 1)
                Console.WriteLine(newSourceCode);
            else
                File.WriteAllText(args[2], newSourceCode);
        }
    }
}
