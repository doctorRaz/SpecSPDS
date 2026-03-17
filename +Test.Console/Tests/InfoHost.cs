namespace dRz.SpecSpds.Test.Tests
{
    /// <summary>
    /// Универсальная среда CAD (Host + OS)
    /// </summary>
    public sealed class InfoHost
    {
        public InfoCad Cad { get; }
        public InfoOs OS { get; }

        private InfoHost()
        {
            Cad = InfoCad.GetInfo;
            OS = InfoOs.GetInfo;
        }

        public static InfoHost GetEnvironment() => new InfoHost();

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
