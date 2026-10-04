using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System;
using System.Collections.Generic;
using dRz.Core.Services;

namespace drz.SpecSPDS.CadServices
{
    public class CadService
    { /// <summary>
      /// Keywords the answer.
      /// </summary>
      /// <param name="doc">The document.</param>
      /// <param name="keywordsList">The keywords list.</param>
      /// <param name="message">The message.</param>
      /// <returns></returns>
        public static Enum KeywordAnswer(Document doc, List< Keywords> keywordsList, string message)
        {
            PromptKeywordOptions options = new PromptKeywordOptions(message);

            foreach (Keywords keyword in keywordsList)
            {
                options.Keywords.Add(keyword.GlobalName,
                                     keyword.LocalName,
                                     keyword.DisplayName,
                                     keyword.IsVisible,
                                     keyword.IsEnabled);

                if (keyword.IsDefault)
                {
                    options.Keywords.Default = keyword.GlobalName;
                }
            }

            PromptResult res = doc.Editor.GetKeywords(options);

            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

             Keywords? keyItem = keywordsList.Find(p => p.GlobalName == res.StringResult);

            if (keyItem == null)//невозможно , но хз его знает)))
            {
                return null;
            }
            return keyItem.Answer;
        }
    }
}