using dRz.SpecSPDS.CadServices;
using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Extensions;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using NLog;
using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teigha.Runtime;

namespace dRz.SpecSPDS.Commands
{
    /// <summary>
    /// получение маркеров.. выбором с пространства с документа с нескольких документов 
    /// <br>
    /// определяем в какую таблицу выводить результат... существующую или из базы, файла созданную на лету (указать точку вставки)
    /// </br>
    /// </summary>

    public class SpecSpdsCmd
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
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
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            AppSettings appSettings = new AppSettings();//настройки

            ApplicationSettings settings = appSettings.Settings;

            Editor ed = doc.Editor;

            #region Убрать в словарик
            //todo убрать в коре в словарик
            List<string> wordFiles = new List<string> { "файл", "файла", "файлов" };
            List<string> wordsMark = new List<string> { "маркер", "маркера", "маркеров" };
            List<string> wordsDoc = new List<string> { "документ", "документа", "документов" };

            #endregion

            #region Выбор юзера
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
            Enum propMod = CadService.KeywordAnswer(doc, keywordsList, "Выбрать маркеры...");

            if (propMod == null)
            {
                return;//смысла продолжать нет
            }

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


                ed.WriteMessage($"\nНайдено {filenames.Count} подходящих {wordFiles.Declens(filenames.Count)}");//todo вынести в интерфейс, волшебные слова через енум и словарик?

                if (filenames.Count > 0)
                {
                    props = mcUmarkerProps.GetPropsTG(filenames);
                }
                //if (filenames.Count > 0) props = mcUmarkerProps.GetPropsMC(filenames);
            }
            //собираем с открытых чертежей
            else
            {
                props = mcUmarkerProps.GetProps(spase);
            }

            #region Result GetProps

            //todo вывод статистики вынести в логгер или  класс
            string resultString = $"\nСтатистика:";

            resultString += $"\nФайлов обработано:";


            if (mcUmarkerProps.CountFilesTotal > 0)
            {
                resultString += $"\n\tВсего: {mcUmarkerProps.CountFilesTotal} {wordsDoc.Declens(mcUmarkerProps.CountFilesTotal)}";
            }

            if (mcUmarkerProps.CountFilesRead > 0)
            {
                resultString += $"\n\tПрочитано: {mcUmarkerProps.CountFilesRead} {wordsDoc.Declens(mcUmarkerProps.CountFilesRead)}";
            }


            if (mcUmarkerProps.BadFilePatchs.Count > 0)
            {
                foreach (string filename in mcUmarkerProps.BadFilePatchs)
                {
                    resultString += $"\n\tДокумент не прочитан: {filename}";
                }
            }
            resultString += $"\nМаркеров:";

            resultString += $"\n\tНайдено всего: {mcUmarkerProps.CountTotal} {wordsMark.Declens(mcUmarkerProps.CountTotal)} за {mcUmarkerProps.ElapsedID}";


            if (mcUmarkerProps.CountFalseName > 0)
            {
                resultString += $"\n\tС неподходящим именем: {mcUmarkerProps.CountFalseName} {wordsMark.Declens(mcUmarkerProps.CountFalseName)}";
            }

            if (mcUmarkerProps.CountNotFlag > 0)
            {
                resultString += $"\n\tБез признака включения в спецификацию: {mcUmarkerProps.CountNotFlag} {wordsMark.Declens(mcUmarkerProps.CountNotFlag)}";
            }

            if (mcUmarkerProps.CountIncorrectData > 0)
            {
                resultString += $"\n\tС некорректными данными: {mcUmarkerProps.CountIncorrectData} {wordsMark.Declens(mcUmarkerProps.CountIncorrectData)}";
            }
            resultString += $"\n\tВключено в набор: {mcUmarkerProps.CountAdded} {wordsMark.Declens(mcUmarkerProps.CountAdded)} за {mcUmarkerProps.ElapsedProp}";

            ed.WriteMessage($"{resultString}");

            #endregion

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



#if DEBUG
        [CommandMethod("drz_logS")]
        [Description("отладка лог")]
        public static void dRz_log()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }
            Editor ed = doc.Editor;



            log.Debug("Проверка в какой лог пойдет");


            //проверка значений  var
            LoggingConfiguration? config = LogManager.Configuration;

            if (config.Variables.ContainsKey(LogVar.FinalLevel))
            {
                Layout layot = config.Variables[LogVar.FinalLevel];

                string finalLevel = layot.Render(LogEventInfo.CreateNullEvent());
                ed.WriteMessage($"finalLevel: {finalLevel}");
            }
            if (config.Variables.ContainsKey(LogVar.FinalAppTitle))
            {
                Layout layot = config.Variables[LogVar.FinalAppTitle];

                string finalAppTitle = layot.Render(LogEventInfo.CreateNullEvent());
                ed.WriteMessage($"finalAppTitle: {finalAppTitle}");
            }
            if (config.Variables.ContainsKey(LogVar.FinalLogsDir))
            {
                Layout layot = config.Variables[LogVar.FinalLogsDir];

                string finalLogsDir = layot.Render(LogEventInfo.CreateNullEvent());
                ed.WriteMessage($"finalLogsDir: {finalLogsDir}");
            }

        }
#endif



    }

    internal static class LogVar
    {
        #region GDC

        /// <summary>
        /// префикс имени файла лога GDC
        /// </summary>
        public const string AppTitle = "AppTitle";

        /// <summary>
        /// каталог логов GDC
        /// </summary>
        public const string LogsDir = "LogsDir";

        /// <summary>
        /// level для GDC
        /// </summary>
        public const string LevelMay = "LevelMay";

        #endregion

        #region var        

        /// <summary>
        /// level для Var
        /// </summary>
        public const string FinalLevel = "FinalLevel";

        /// <summary>
        /// каталог логов var
        /// </summary>
        public const string FinalLogsDir = "FinalLogsDir";

        /// <summary>
        /// имя лога var
        /// </summary>
        public const string FinalAppTitle = "FinalAppTitle";

        #endregion

    }
}
