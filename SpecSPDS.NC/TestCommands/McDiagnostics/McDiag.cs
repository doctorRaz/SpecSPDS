#if DEBUG
using drz.MulticadInterop;
using drz.MulticadInterop.McNotificator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.Runtime;

namespace dRz.NC.TestCommands.McDiagnostics
{
    public class McDiag
    {

        [CommandMethod($"SPEC-NotificatorDiagnostic", CommandFlags.Session)]
        public static void NotificatorDiagnostic()
        {
            MulticadNotificatorDiagnostic.DumpCreateMessage();
        }

        [CommandMethod($"SPEC-Methods", CommandFlags.Session)]
        public static void ParamMethods()
        {
            MulticadParamManagerDiagnostic.DumpMethods();
        }


    }
}
#endif