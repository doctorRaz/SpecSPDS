using drz.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSpds.Test.Loader
{
    /// <summary>
    /// EntryPoint TEST
    /// </summary>
    internal class DrzEntryPoint
    {
        internal void Start()
        {
            EntryPoint entryPoint = new EntryPoint();

            entryPoint.Initialize();

            entryPoint.Terminate();

        }
    }
}
