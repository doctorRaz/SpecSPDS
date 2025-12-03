using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.ComponentModel;
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

            Editor ed = doc.Editor;

            #region выбор маркеров

            List<Keywords> keywordsList = new List<Keywords>
            {
                new Keywords(nameof(Space.All),Space.All),
                new Keywords(nameof(Space.Layout),Space.Layout,true),
                new Keywords(nameof(Space.Select),Space.Select),

            };

            Enum propMod = KeywordAnswer(doc, keywordsList, "Выбрать маркер");

            if (propMod == null) return;//смысла продолжать нет

            McUmarkerProps mcUmarkerProps = new McUmarkerProps((Space)propMod);

            List<Core.Models.DefinitionMarkerProps> umProps = mcUmarkerProps.MarkerProps;

            #endregion

            ed.WriteMessage($"{mcUmarkerProps.ResultString}");

            PropXml propXml = new PropXml();
            propXml.Props=umProps;
            //props.MarkerName = "xxz";
            //props.FlagSpecRaw = "1";

            propXml.SaveProps();


        }

    }

}
