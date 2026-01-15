using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSPDS.Core.Settings
{
    public static class AssemblyInfo
    {
        public static string Product
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                return asm
                    .GetCustomAttribute<AssemblyProductAttribute>()?.Product
                       ?? asm.GetName().Name!;
            }
        }
    }
}
