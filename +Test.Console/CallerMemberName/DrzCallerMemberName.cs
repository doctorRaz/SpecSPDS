using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSpds.Test.CallerMemberName
{
    internal class DrzCallerMemberName
    {
        internal void Start([CallerMemberName] string? caller = null)
        {
            Console.WriteLine(caller);
        }
    }
}