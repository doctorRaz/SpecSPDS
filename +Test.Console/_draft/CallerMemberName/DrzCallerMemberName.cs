using System;
using System.Runtime.CompilerServices;

namespace drz.SpecSPDS.Test.CallerMemberName
{
    internal class DrzCallerMemberName
    {
        internal void Start([CallerMemberName] string? caller = null)
        {
            Console.WriteLine(caller);
        }
    }
}