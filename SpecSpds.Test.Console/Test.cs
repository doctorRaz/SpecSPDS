using dRz.SpecSPDS.Core.Extensions;
using System;

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


        internal static void convert()
        {
        lb:
            Test test = new Test();
            string[] str =
                {
        "",
        "y",
        "Y",
        "t",
        "tr",
        " true",
        "TRUE ",
        "TRuE",
        "д",
        "Д",
        "Да",
        "ДА",
        "дА",
        "1",
        "-1",
        "-1.12",
        "-1,12",
        "100.10.10",
        "00000.55",
        "00000,55",
        "1.000.007E-08",
        "1.000.007E-08",
        "1.000.007E-05",
        "1000000.000001",
        };

            foreach (string s in str)
            {
                Console.WriteLine($"{s} \t{s.ToBoolean()}");
            }

            Console.WriteLine("-----------------------");
            foreach (string s in str)
            {
                Console.WriteLine($"{s} \t{s.ToDouble()}");
            }

            Console.WriteLine("-----------------------");



            //goto lb;

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
