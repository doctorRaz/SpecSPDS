using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Models;
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



            MultiCadProps mcUmarkerProps = null;


            //из папки или файлов
            Space spase = (Space)propMod;

            if (spase == Space.Folder || spase == Space.Files || spase == Space.SubFolder)
            {
                List<string> filenames = FetchingPatchFiles.GetFiles(spase);
               

                mcUmarkerProps = new MultiCadProps(filenames, settings);
            }
            // с открытых чертежей
            else
            {

                mcUmarkerProps = new MultiCadProps(spase, settings);

            }

            List<DefinitionMarkerProps> umProps = mcUmarkerProps.MarkerProps;

            ed.WriteMessage($"{mcUmarkerProps.ResultString}");

            mcUmarkerProps.ExtractAllPropertiesGetProps();

            ed.WriteMessage($"GetProps props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrGetProps}");


            mcUmarkerProps.ExtractAllPropertiesPropsSource();

            ed.WriteMessage($"PropSourse props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrPropSourse}");


            ed.WriteMessage("------------");

            mcUmarkerProps.ExtractAllPropertiesPropsSource();

            ed.WriteMessage($"PropSourse props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrPropSourse}");


            mcUmarkerProps.ExtractAllPropertiesGetProps();

            ed.WriteMessage($"GetProps props {mcUmarkerProps.Mprops.Count} in {mcUmarkerProps.TmrGetProps}");

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
