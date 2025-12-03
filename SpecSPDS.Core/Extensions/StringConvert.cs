using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core.Extensions
{
    public static class StringConvert
    {

        public static bool StrBool(this string val)
        {
            return val == "1";
        }
    }
}
