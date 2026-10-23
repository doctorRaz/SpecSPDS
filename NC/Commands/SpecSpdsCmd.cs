using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.Application.Enums;
using dRz.Application.Models;
using dRz.Application.Services;
using dRz.Application.Settings;
using dRz.Core.Extensions;
using dRz.Core.Services;
using dRz.NC.CadServices;
using dRz.NC.Infrastructure;
using dRz.NC.Services;
using HostMgd.EditorInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teigha.Runtime;
using App = HostMgd.ApplicationServices;

namespace dRz.NC.Commands
{
    /// <summary>
    /// получение маркеров.. выбором с пространства с документа с нескольких документов
    /// <br>
    /// определяем в какую таблицу выводить результат... существующую или из базы, файла созданную на лету (указать точку вставки)
    /// </br>
    /// </summary>

    public class SpecSpdsCmd
    {
        private static IDrzLogger _logger = AddOnContext.NLogFactory.GetLogger(typeof(UpdateModeCommand));

        private static IMessageService _msg = AddOnContext.Msg;

        private static IAddonSettings? _settings;

        /// <summary>
        /// спека с маркеров
        /// </summary>
#if DEBUG

        [CommandMethod(CommandPrefix.Value + "_тсс")]
#endif
        [CommandMethod(CommandPrefix.Value + "")]
        [Description("Спецификация по универсальным маркерам MultiCAD")]
        public static void SpecSpds()
        {
            App.Document doc = App.Application.DocumentManager.MdiActiveDocument;
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

            #endregion Убрать в словарик

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
            Enum? propMod = CadService.KeywordAnswer(doc, keywordsList, "Выбрать маркеры...");

            if (propMod == null)
            {
                return;//смысла продолжать нет
            }

            #endregion Выбор юзера

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

            #endregion Result GetProps

            #endregion Свойства маркера

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