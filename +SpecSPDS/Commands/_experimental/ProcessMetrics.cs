//#if DEBUG

using NLog;
using System;
using System.Diagnostics;











#if NC || NC26

#elif AC
using Db = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;
using App = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

#endif


namespace dRz.SpecSPDS.nCad.Commands.Test
{
    public static class ProcessMetrics
    {

        //GPT
        public static void LogProcessMetrics(ILogger log)
        {
            var p = Process.GetCurrentProcess();

            log.ForInfoEvent()
               .Message("Process performance metrics")
               .Property("memoryMb", Math.Round(p.WorkingSet64 / 1024d / 1024d, 2))
               .Property("threadCount", p.Threads.Count)
               .Property("pid", p.Id)
               .Property("processName", p.ProcessName)
               .Log();
        }
    }
}


//#endif