using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Settings;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Multicad;
using Multicad.DatabaseServices;
using System.ComponentModel;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using App = HostMgd.ApplicationServices;

namespace dRz.SpecSPDS.CadCommands
{
    /// <summary>
    /// получение маркеров.. выбором с пространства с документа с нескольких документов 
    /// <br>
    /// определяем в какую таблицу выводить результат... существующую или из базы, файла созданную на лету (указать точку вставки)
    /// </br>
    /// </summary>

    public partial class SpecSpdsCmd
    {

        /// <summary>
        /// спека с маркеров
        /// </summary>
#if DEBUG
        [CommandMethod("тсс")]
#endif
        [CommandMethod("drz_SpecSpds")]
        [Description("Импорт свойств из стороннего файла в текущий документ")]
        public static void SpecSpds()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            AppSettings appSettings = new AppSettings();//настройки

            Editor ed = doc.Editor;

            #region выбор маркеров

            List<Core.Services.Keyword> keywordsList = new List<Core.Services.Keyword>
            {
                new Core.Services.Keyword(nameof(Space.All),Space.All),
                new Core.Services.Keyword(nameof(Space.Document),Space.Document),
                new Core.Services.Keyword(nameof(Space.Layout),Space.Layout,true),
                new Core.Services.Keyword(nameof(Space.Select),Space.Select),

            };

            Enum propMod = KeywordAnswer(doc, keywordsList, "Выбрать маркер");

            if (propMod == null) return;//смысла продолжать нет

            MulticadProps mcUmarkerProps = new MulticadProps((Space)propMod, appSettings);

            List<Core.Models.DefinitionMarkerProps> umProps = mcUmarkerProps.MarkerProps;

            #endregion

            ed.WriteMessage($"{mcUmarkerProps.ResultString}");

            PropXml propXml = new PropXml();
            propXml.Props = umProps;
            //props.MarkerName = "xxz";
            //props.FlagSpecRaw = "1";

            propXml.SaveProps();


        }

        [CommandMethod("PaintFile" )]
        static public void PaintFile()
        {
            string filName = @"d:\@Developers\Programmers\!NET\!SpecSPDS\res\test.dwg";
            McDocument pDoc = McDocumentsManager.OpenDocument(filName, false, true);

            using ( Database db0 = new Database(false, false))
            {
                  db0. ReadDwgFile(filName, FileOpenMode.OpenForReadAndAllShare, false, "", false);

            }
            McDocument pOldWD = McDocument.WorkingDocument;

            McDocument.WorkingDocument = pDoc;
            ObjectFilter of = ObjectFilter.Create(false).AddDoc(pDoc);
            of.AllObjects = true;
            List<McObjectId> ids = of.GetObjects();
            foreach (McObjectId id in ids)
            {
                McDbEntity pEnt = id.GetObject().Cast<McDbEntity>();
                if (pEnt == null)
                    continue;
                pEnt.Color = Color.Red;
            }
            McObjectManager.UpdateAll();
            pDoc.SaveAs(filName);
            pDoc.Close();
            McDocument.WorkingDocument = pOldWD;
        }

    }

}
