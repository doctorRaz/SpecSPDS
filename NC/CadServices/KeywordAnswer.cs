using dRz.Core;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System;
using System.Collections.Generic;

namespace dRz.NC.CadServices
{
    /// <summary>
    /// Методы для взаимодействия с командной строкой CAD.
    /// </summary>
    public class CadService
    {
        /// <summary>
        /// Показывает список ключевых слов и возвращает связанное с выбранным словом значение.
        /// </summary>
        /// <param name="doc">Документ, в котором выполняется команда.</param>
        /// <param name="keywordsList">Список доступных ключевых слов.</param>
        /// <param name="message">Текст запроса.</param>
        /// <param name="allowNone">Разрешает принять значение по умолчанию при нажатии Enter.</param>
        /// <returns>Значение выбранного ключевого слова или <see langword="null"/>, если ввод отменён или не распознан.</returns>
        public static Enum? KeywordAnswer(
            Document doc,
            IReadOnlyList<Keywords> keywordsList,
            string message,
            bool allowNone = false)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (keywordsList == null)
            {
                throw new ArgumentNullException(nameof(keywordsList));
            }

            PromptKeywordOptions options = new PromptKeywordOptions(message)
            {
                AllowNone = allowNone,
                AppendKeywordsToMessage = true
            };

            foreach (Keywords keyword in keywordsList)
            {
                options.Keywords.Add(
                    keyword.GlobalName,
                    keyword.LocalName,
                    keyword.DisplayName,
                    keyword.IsVisible,
                    keyword.IsEnabled);

                if (keyword.IsDefault)
                {
                    options.Keywords.Default = keyword.GlobalName;
                }
            }

            PromptResult result = doc.Editor.GetKeywords(options);

            if (result.Status == PromptStatus.None && allowNone)
            {
                foreach (Keywords keyword in keywordsList)
                {
                    if (keyword.IsDefault)
                    {
                        return keyword.Answer;
                    }
                }

                return null;
            }

            if (result.Status != PromptStatus.OK)
            {
                return null;
            }

            foreach (Keywords keyword in keywordsList)
            {
                if (string.Equals(
                    keyword.GlobalName,
                    result.StringResult,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return keyword.Answer;
                }
            }

            return null;
        }
    }
}
