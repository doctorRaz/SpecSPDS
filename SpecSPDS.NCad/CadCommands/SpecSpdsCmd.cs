using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.NCad.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.ComponentModel;
using Teigha.Runtime;
using App = HostMgd.ApplicationServices;

namespace dRz.SpecSPDS.NCad.CadCommands
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

            ApplicationSettings settings = appSettings.Settings;

            Editor ed = doc.Editor;

            #region выбор маркеров
            //получаю маркеры
            List<Core.Services.Keyword> keywordsList = new List<Core.Services.Keyword>
            {
                new Core.Services.Keyword(nameof(Space.All),Space.All),
                new Core.Services.Keyword(nameof(Space.Document),Space.Document),
                new Core.Services.Keyword(nameof(Space.Layout),Space.Layout,true),
                new Core.Services.Keyword(nameof(Space.Select),Space.Select),

            };

            Enum propMod = KeywordAnswer(doc, keywordsList, "Выбрать маркер");

            if (propMod == null) return;//смысла продолжать нет

            MultiCadProps mcUmarkerProps = new MultiCadProps((Space)propMod, settings);

            List<Core.Models.DefinitionMarkerProps> umProps = mcUmarkerProps.MarkerProps;

            ed.WriteMessage($"{mcUmarkerProps.ResultString}");

            mcUmarkerProps.ExtractAllProperties();

            ed.WriteMessage($"All props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrAll}");
            ed.WriteMessage($"Ex props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrEx}");
            #endregion

            //дальше пуляю в сортировку обработку


            //результат пишу в таблицу


            PropXml propXml = new PropXml();
            propXml.Props = umProps;
            //props.MarkerName = "xxz";
            //props.FlagSpecRaw = "1";

            propXml.SaveProps();//для отладки


        }


    }





}
