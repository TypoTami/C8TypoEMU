using System;
using System.IO;

namespace C8TypoEmu
{
    class Program
    {
        public static string romToLoad;
        static void Main(string[] args)
        {
            // Using this for now. Should make it possible to select a rom in the program itself...
            if (args.Length == 1)
            {
                romToLoad = args[0];
                using(var app = new App())
                app.Run();
            }

        }
        
    }
}
