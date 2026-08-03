using drz.Abstractions.Infrastructure;
using drz.Infrastructure.Infrastructure;
using System;

namespace drz.SpecSPDS.Test
{
    /// <summary>
    /// StartRaw
    /// </summary>
    public class StartRaw
    {
      

        [STAThread]
        private static void Main(string[] args)
        {

            IAddOnInfo addOnInfo=new AddOnInfo(typeof(StartRaw).Assembly);

            var vendor = addOnInfo.GetMetadata(AssemblyMetadataKeys.Vendor);
            var url = addOnInfo.GetMetadata(AssemblyMetadataKeys.RepositoryUrl);
            var empt = addOnInfo.GetMetadata("");
            var empt2 = addOnInfo.GetMetadata("","def");

            string ret;

            var tryg=addOnInfo.TryGetMetadata("HomePage", out ret);
            Console.WriteLine(addOnInfo.ToLongString());
        }
    }
}