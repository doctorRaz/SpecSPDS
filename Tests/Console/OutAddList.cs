using System.Collections.Generic;

namespace dRz.SpecSpdsConsole
{
    public class OutAddList
    {

        public void RefAdd(string[] lst, ref List<string> reflist)
        {
            foreach (var item in lst)
            {
                reflist.Add(item);
            }

        }


    }
}
