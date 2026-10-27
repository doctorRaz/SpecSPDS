using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace dRz.Application.Models
{
    /// <summary>
    /// Предоставляет методы преобразования строк в числовые и логические значения.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Converts to boolean. Более универсальный я тд
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            s = s.Trim();
            string[] trueStrings = { "1", "y", "yes", "true", "д", "да" };//все что не в наборе false

            if (trueStrings.Contains(s, StringComparer.InvariantCultureIgnoreCase /*OrdinalIgnoreCase*/))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Преобразует строку в число с плавающей точкой, учитывая распространённые разделители дробной части.
        /// </summary>
        /// <param name="s">Строка для преобразования.</param>
        /// <returns>Преобразованное число или значение-заполнитель, если преобразование не удалось.</returns>
        public static double ToDouble(this string s)
        {
            //https://stackoverflow.com/questions/11399439/converting-string-to-double-in-c-sharp

            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];

            double result = -0.12345;

            try
            {
                if (s != null && !string.IsNullOrWhiteSpace(s))
                {
                    if (!s.Contains(','))
                    {
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
                    }
                }
            }
            catch (Exception)
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