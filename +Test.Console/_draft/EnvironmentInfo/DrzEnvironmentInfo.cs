using drz.Abstractions.Infrastructure;
using drz.Lib_A;
using drz.Lib_B;
using System;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.EnvironmentInfo
{
    /// <summary>
    /// SimpleInjector TestUpdater
    /// </summary>
    internal class DrzEnvironmentInfo
    {
        internal void Start()
        {




            CommandA cmdA = new CommandA();
            CommandB cmdB = new CommandB();

            Console.WriteLine($"{cmdA.ExecuteEnvironmentInfoProvider()}");

            Console.WriteLine($"{cmdB.ExecuteEnvironmentInfoProvider()}");

            Console.WriteLine($"{AddonInfo.ToLongString()}");

            //Console.WriteLine($"{RT.Info}");
            //Console.WriteLine($"{RT.Os}");
            //Console.WriteLine($"{RT.Cad}");

            Console.WriteLine(typeof(Start).FullName);

            

            Console.WriteLine(SysInfo);
            Console.WriteLine(CadInfo);
            Console.WriteLine(AddonInfo);

          
            IAddOnInfo id = AddonInfo;
        }
    }
}