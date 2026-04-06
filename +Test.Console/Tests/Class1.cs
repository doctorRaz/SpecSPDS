using dRz.Cad.Diagnostics.Cad;
using dRz.Cad.Diagnostics.Os;
using System;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSpds.Test.Tests
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
