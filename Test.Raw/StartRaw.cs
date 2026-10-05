using dRz.Abstractions.Infrastructure;
using dRz.Infrastructure.Infrastructure;

namespace dRz.SpecSPDS
{
    /// <summary>
    /// StartRaw
    /// </summary>
    public class StartRaw
    {


        [STAThread]
        private static void Main(string[] args)
        {

            IAddOnInfo addOnInfo = new AddOnInfo(typeof(StartRaw).Assembly);

            string? vendor = addOnInfo.GetMetadata(AssemblyMetadataKeys.Vendor);
            string? url = addOnInfo.GetMetadata(AssemblyMetadataKeys.RepositoryUrl);
            string? empt = addOnInfo.GetMetadata("");
            string empt2 = addOnInfo.GetMetadata("", "def");

            string ret;

            bool tryg = addOnInfo.TryGetMetadata("HomePage", out ret);
            Console.WriteLine(addOnInfo.ToLongString());
        }
    }
}