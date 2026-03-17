namespace dRz.Loader.Cad.Infrastructure.Info
{
    /// <summary>
    /// Универсальная среда CAD (Host + OS)
    /// </summary>
    public sealed class InfoHosts
    {
        public InfoCad Cad { get; }
        public InfoOs OS { get; }

        private InfoHosts()
        {
            Cad = InfoCad.GetInfo;
            OS = InfoOs.GetInfo;
        }

        public static InfoHosts GetEnvironment() => new InfoHosts();

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
