using System.Collections.Generic;

namespace dRz.SpecSPDS.Core.Extensions
{
    public static class Declension
    {
        /// <summary>
        /// Возвращает слова в падеже, зависимом от заданного числа
        /// </summary>
        /// <param name="number">Число от которого зависит выбранное слово</param>
        /// <param name="nominativ">Именительный падеж слова. Например "день"</param>
        /// <param name="genetiv">Родительный падеж слова. Например "дня"</param>
        /// <param name="plural">Множественное число слова. Например "дней"</param>
        /// <returns>строка в правильном падеже</returns>
        public static string DeclensionGenerator(int number, string nominativ, string genetiv, string plural)
        {
            string[] titles = new[] { nominativ, genetiv, plural };
            int[] cases = new[] { 2, 0, 1, 1, 1, 2 };
            return titles[number % 100 > 4 && number % 100 < 20 ? 2 : cases[number % 10 < 5 ? number % 10 : 5]];
        }

        public static string Declens(this List<string> titles, int number)
        {
            List<int> cases = new List<int> { 2, 0, 1, 1, 1, 2 };
            return titles[number % 100 > 4 && number % 100 < 20 ? 2 : cases[number % 10 < 5 ? number % 10 : 5]];
        }
    }
}
