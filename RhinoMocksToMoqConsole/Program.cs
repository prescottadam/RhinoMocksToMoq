using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RhinoMocksToMoq;

namespace RhinoMocksToMoqConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!(args.Length == 1  && File.Exists(args[0]) || args.Length == 3 && args[1] == "-out" && File.Exists(args[0]) || args.Length == 2 && args[0] == "-batch" && File.Exists(args[1])))
            {
                Console.WriteLine("Usage: RhinoMocksToMoqConsole <filepath> [-out file]");
                Console.WriteLine("Usage: RhinoMocksToMoqConsole -batch filepath-containing-list-of-files]");
                Environment.Exit(1);
            }
            var writeToConsole = args.Length == 1;
            var paths = args[0] == "-batch" ? File.ReadAllLines(args[1]).Where(x => !string.IsNullOrWhiteSpace(x) && x [0] != '#').ToList() : new List<string> { args[0]};

            paths.ForEach(filename =>
            {
                var sourceCode = File.ReadAllText(filename);
                var newSourceCode = ClassConverter.Convert(sourceCode);

                if (writeToConsole)
                    Console.WriteLine(newSourceCode);
                else
                    File.WriteAllText(filename, newSourceCode);
            });
        }
    }
}
