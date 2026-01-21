using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core._experimental
{
    public class ModuleNameTest2
    {
        public void CallerName(string message,
                   [CallerMemberName] string member = "",
                   [CallerFilePath] string file = "")
        {
            string cls = Path.GetFileNameWithoutExtension(file);
            Debug.WriteLine($"{cls}.{member}: {message}");

            Debug.WriteLine($"GetExecutingAssembly\t{Assembly.GetExecutingAssembly().GetName().Name}");

            Debug.WriteLine($"GetEntryAssembly\t{Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name}");



        }
    }
}
