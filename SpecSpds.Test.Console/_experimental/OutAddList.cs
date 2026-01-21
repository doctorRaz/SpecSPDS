using System.Collections.Generic;

namespace dRz.SpecSpds.Test._experimental
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
