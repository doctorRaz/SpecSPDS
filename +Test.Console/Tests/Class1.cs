using dRz.Cad.Diagnostics.AddOn;
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

            var path = InfoDll.AppDataProductLogPath;


            Console.WriteLine(new InfoCad());

            var os = InfoOs.Current;
            
            Console.WriteLine(os);

        }
    }
}
