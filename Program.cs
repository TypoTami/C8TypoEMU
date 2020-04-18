using System;
using System.IO;

namespace C8TypoEmu
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var app = new App())
            app.Run();
        }
    }
}
