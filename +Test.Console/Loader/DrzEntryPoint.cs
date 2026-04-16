using drz.Loader;

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
