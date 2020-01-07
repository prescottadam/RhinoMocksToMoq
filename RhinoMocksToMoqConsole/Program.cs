using System;
using System.IO;
using RhinoMocksToMoq;

namespace RhinoMocksToMoqConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RhinoMocksToMoqConsole <filepath>");
                return;
            }

            string contents;

            using (var fs = new FileStream(args[0], FileMode.Open))
            using (var sr = new StreamReader(fs))
            {
                contents = sr.ReadToEnd();
            }

            var newContents = ClassConverter.Convert(contents);

            using (var fs = new FileStream(args[0], FileMode.Truncate))
            using (var sw = new StreamWriter(fs))
            {
                sw.Write(newContents);
            }
        }
    }
}
