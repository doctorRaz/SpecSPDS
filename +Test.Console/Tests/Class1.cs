using drz.Cad.Diagnostics.Cad;
using drz.Cad.Diagnostics.Os;
using System;
using static drz.Loader.Infrastructure.AddonContext;

namespace drz.SpecSpds.Test.Tests
{
    internal class Class1
    {
        public void test()
        {

            InfoOs os = InfoOs.Current;

            Console.WriteLine(os);

            string path = InfoDll.AppDataProductLogPath;


            Console.WriteLine(InfoCad.Current);

        }
    }
}
