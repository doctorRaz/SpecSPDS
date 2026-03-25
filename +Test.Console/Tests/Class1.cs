using dRz.CAD.Runtime.Info;
using System;

namespace dRz.SpecSpds.Test.Tests
{
    internal class Class1
    {
        public void test()
        {
         Console.WriteLine(new InfoCad());

            var os = InfoOs.Current;
         Console.WriteLine(os);

        }
    }
}
