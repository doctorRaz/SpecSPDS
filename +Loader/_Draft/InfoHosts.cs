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
            Cad = InfoCad.Current;
            OS = InfoOs.Current;
        }

        public static InfoHosts Current { get; } = new InfoHosts();

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
