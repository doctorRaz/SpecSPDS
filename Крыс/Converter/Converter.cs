using System;

namespace Converter
{
    public class Converter
    {
        public double ToDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            throw new ArgumentException();
        }

        public int ToInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            throw new ArgumentException();
        }
    }
}
