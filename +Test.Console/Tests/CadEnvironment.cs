namespace dRz.SpecSpds.Test.Tests
{
    /// <summary>
    /// Универсальная среда CAD (Host + OS)
    /// </summary>
    public sealed class CadEnvironment
    {
        public CadHostInfo Cad { get; }
        public OsInfo OS { get; }

        private CadEnvironment()
        {
            Cad = CadHostInfo.GetInfo;
            OS = OsInfo.GetInfo;
        }

        public static CadEnvironment GetEnvironment() => new CadEnvironment();

        public override string ToString()
        {
            return
$@"CAD Environment
----------------
{Cad}

{OS}";
        }
    }
}
