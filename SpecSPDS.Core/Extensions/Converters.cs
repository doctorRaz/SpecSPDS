using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace dRz.SpecSPDS.Core.Extensions
{
    public static class Converters
    {
        /// <summary>
        /// строку "1" в bool
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static bool ToBool(this string s)
        {
            //https://stackoverflow.com/questions/9742724/how-to-convert-a-string-to-a-bool

            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            return s == "1";
        }

        /// <summary>
        /// Converts to boolean. Более универсальный я тд
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();
            string[] trueStrings = { "1", "y", "yes", "true", "д", "да" };//все что не в наборе false


            if (trueStrings.Contains(s, StringComparer.InvariantCultureIgnoreCase /*OrdinalIgnoreCase*/))
            {
                return true;
            }

            return false;
        }

        public static double ToDouble(this string s)
        {
            //https://stackoverflow.com/questions/11399439/converting-string-to-double-in-c-sharp

            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];

            double result = -0.12345;

            try
            {
                if (s != null && !string.IsNullOrWhiteSpace(s))
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        return result;//   throw new Exception("Wrong string-to-double format");
                    }
                }
            }
            return result;
        }
    }
}
