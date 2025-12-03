using System;
using System.Linq;

namespace dRz.SpecSpdsConsole
{
    internal class Test
    {
        internal bool IsFlag(bool isCheck, bool flag = false)

        {
            if (flag)
            {
                if (!isCheck)
                {
                    return false;
                }
            }

            return true;

        }



    }

    public static class Ext
    {
        public static double Cd(this string s)
        {
            double result = -0.12345;

            try
            {
                return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return result;
            }
        }


        
    }
}
