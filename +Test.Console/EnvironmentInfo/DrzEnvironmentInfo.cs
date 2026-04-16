using drz.EnvironmentInfo;
using drz.EnvironmentInfo.App;
using drz.EnvironmentInfo.Cad;
using drz.EnvironmentInfo.Sys;
using drz.Lib_A;
using drz.Lib_B;
using drz.SpecSpds.Test.Logger;
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

            Console.WriteLine($"{InfoDll.ToStringLong()}");

            Console.WriteLine($"{RT.Info}");
            Console.WriteLine($"{RT.Os}");
            Console.WriteLine($"{RT.Cad}");

            Console.WriteLine(typeof(Start).FullName);

            RuntimeInfo runtime = RuntimeInfo.Current;
            Console.WriteLine(runtime);

            Console.WriteLine(SysInfo.Current);
            Console.WriteLine(CadInfo.Current);
            Console.WriteLine(AppInfo.Get(typeof(Start).Assembly));

            RuntimeInfo rt = RT.Info;
            AppInfo id = InfoDll;
        }
    }
}