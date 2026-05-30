using drz.Abstractions.Infrastructure;
using drz.Lib_A;
using drz.Lib_B;
using System;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.EnvironmentInfo
{
    /// <summary>
    /// SimpleInjector Test
    /// </summary>
    internal class DrzEnvironmentInfo
    {
        internal void Start()
        {




            CommandA cmdA = new CommandA();
            CommandB cmdB = new CommandB();

            Console.WriteLine($"{cmdA.ExecuteEnvironmentInfoProvider()}");

            Console.WriteLine($"{cmdB.ExecuteEnvironmentInfoProvider()}");

            Console.WriteLine($"{InfoDll_NEW.ToLongString()}");

            //Console.WriteLine($"{RT.Info}");
            //Console.WriteLine($"{RT.Os}");
            //Console.WriteLine($"{RT.Cad}");

            Console.WriteLine(typeof(Start).FullName);

            

            Console.WriteLine(SysInfo_NEW);
            Console.WriteLine(CadInfo_NEW);
            Console.WriteLine(InfoDll_NEW);

          
            IApplicationInfo_NEW id = InfoDll_NEW;
        }
    }
}