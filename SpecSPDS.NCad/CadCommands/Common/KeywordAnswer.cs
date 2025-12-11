using dRz.SpecSPDS.Core.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;

namespace dRz.SpecSPDS.NCad.CadCommands
{
    public partial class SpecSpdsCmd
    { /// <summary>
      /// Keyword the answer.
      /// </summary>
      /// <param name="doc">The document.</param>
      /// <param name="keywordsList">The keywords list.</param>
      /// <param name="message">The message.</param>
      /// <returns></returns>
        public static Enum KeywordAnswer(Document doc, List<Core.Services.Keyword> keywordsList, string message)
        {
            PromptKeywordOptions options = new PromptKeywordOptions(message);

            foreach (Core.Services.Keyword keyword in keywordsList)
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

            Core.Services.Keyword? keyItem = keywordsList.Find(p => p.GlobalName == res.StringResult);

            if (keyItem == null)//невозможно , но хз его знает)))
            {
                return null;
            }
            return keyItem.Answer;


        }

    }
}
