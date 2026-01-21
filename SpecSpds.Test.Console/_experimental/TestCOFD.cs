using dRz.SpecSPDS.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace dRz.SpecSpds.Test._experimental
{
    public class TestCOFD
    {

        public void CofdTest()
        {
            COFD cofd = new COFD();
            cofd.IsFolderPicker = true;
            cofd.Multiselect = true;
            cofd.InitialDirectory = "";
            cofd.RestoreDirectory = true;


            var cc = cofd.ShowDialog();


            List<string> fn = cofd.FileNames.ToList();//.ToList<string>();

            cc = cofd.ShowDialog();

            List<string> fn2 = cofd.FileNames.ToList();

            //https://metanit.com/sharp/tutorial/15.4.php
            List<string> d = fn.Union(fn2).ToList();

            d.Sort();
        }
    }
}
