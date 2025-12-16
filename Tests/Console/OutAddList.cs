using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSpdsConsole
{
    public class OutAddList
    {

        public void RefAdd(string[] lst , ref List<string> reflist) 
        {
            foreach (var item in lst)
            {
                reflist.Add(item);
            }

        }


    }
}
