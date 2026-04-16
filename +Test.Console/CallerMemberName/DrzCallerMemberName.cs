using System;
using System.Runtime.CompilerServices;

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