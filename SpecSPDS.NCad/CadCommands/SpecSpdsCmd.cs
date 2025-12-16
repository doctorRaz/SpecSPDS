using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.NCad.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Multicad.DatabaseServices;
using System.ComponentModel;
using System.Diagnostics;
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

            #region хотелки юзера
            //получаю маркеры
            List<Keywords> keywordsList = new List<Keywords>
            {
                new Keywords(nameof(Space.Files),Space.Files),
                new Keywords(nameof(Space.Folder),Space.Folder),
                new Keywords(nameof(Space.SubFolder),Space.SubFolder),
                new Keywords(nameof(Space.All),Space.All),
                new Keywords(nameof(Space.Document),Space.Document),
                new Keywords(nameof(Space.Layout),Space.Layout,true),
                new Keywords(nameof(Space.Select),Space.Select),

            };
            Enum propMod = KeywordAnswer(doc, keywordsList, "Выбрать маркеры...");

            if (propMod == null) return;//смысла продолжать нет

            #endregion

            #region Свойства маркера
            //в конструктор только настройки
            MultiCadProps mcUmarkerProps = new MultiCadProps(settings);

            //чего вернул кейворд
            Space spase = (Space)propMod;

            List<DefinitionMarkerProps> props = new List<DefinitionMarkerProps>();

            //собираем из папки или файлов
            if (spase == Space.Folder || spase == Space.Files || spase == Space.SubFolder)
            {
                //получили список файлов
                List<string> filenames = FetchingPatchFiles.GetFiles(spase);
                if (filenames.Count > 0) props = mcUmarkerProps.GetProps(filenames);
            }
            //собираем с открытых чертежей
            else
            {
                props = mcUmarkerProps.GetProps(spase);
            }

            ed.WriteMessage($"Собрано: {props.Count}");
            //ed.WriteMessage($"{mcUmarkerProps.ResultString}");





            #endregion

            //дальше пуляю в сортировку обработку


            //результат пишу в таблицу

            //для тестов сортировки
            PropXml propXml = new PropXml();
            propXml.Props = props;
            //props.MarkerName = "xxz";
            //props.FlagSpecRaw = "1";

            propXml.SaveProps();//для отладки


        }


    }





}
