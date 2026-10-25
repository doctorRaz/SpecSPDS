using dRz.Core.Services;
using dRz.NC.CadServices;
using dRz.NC.Infrastructure;
using App = HostMgd.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Rtm = Teigha.Runtime;
using dRz.Abstractions.Infrastructure;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Предоставляет команды NanoCAD для управления уровнями логирования через файлы настройки.
    /// </summary>
    /// <remarks>
    /// <c>logger.level</c> задаёт уровень основного логгера, а <c>diagnostic.level</c> — уровень
    /// внутренней диагностики NLog. Значение читается при инициализации логирования, поэтому
    /// после изменения или удаления файла требуется перезапустить nanoCAD.
    /// </remarks>
    public sealed class LogLevelCommand
    {
        private const string LogLevelFileName = "logger.level";
        private const string DiagnosticModeFileName = "diagnostic.level";

        /// <summary>
        /// Запускает настройку уровня основного логгера через файл <c>logger.level</c>.
        /// </summary>
        /// <remarks>
        /// Пользователь может установить поддерживаемый уровень, удалить переопределение
        /// или посмотреть содержимое первой строки файла.
        /// </remarks>
        [Rtm.CommandMethod(CommandPrefix.Value + "_LogLevel", Rtm.CommandFlags.Session)]
        [Description("Настройка уровня логирования")]
        public void SetLogLevel()
        {
            ManageLevel(LogLevelFileName, "уровня логирования");
        }

        /// <summary>
        /// Запускает настройку внутренней диагностики NLog через файл <c>diagnostic.level</c>.
        /// </summary>
        /// <remarks>
        /// Пользователь может установить поддерживаемый уровень, удалить переопределение
        /// или посмотреть содержимое первой строки файла.
        /// </remarks>
        [Rtm.CommandMethod(CommandPrefix.Value + "_DiagnosticMode", Rtm.CommandFlags.Session)]
        [Description("Настройка внутренней диагностики логирования")]
        public void SetDiagnosticMode()
        {
            ManageLevel(DiagnosticModeFileName, "внутренней диагностики логирования");
        }

        /// <summary>
        /// Предлагает действие для управления файлом переопределения уровня логирования.
        /// </summary>
        /// <param name="fileName">Имя файла настройки в каталоге сборки аддона.</param>
        /// <param name="settingName">Название настройки, используемое в сообщениях пользователю.</param>
        private static void ManageLevel(string fileName, string settingName)
        {
            App.Document? document = App.Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            List<Keywords> actionKeywords = new List<Keywords>
            {
                new Keywords(nameof(LogLevelAction.Apply), LogLevelAction.Apply),
                new Keywords(nameof(LogLevelAction.Delete), LogLevelAction.Delete),
                new Keywords(nameof(LogLevelAction.Status), LogLevelAction.Status)
            };

            Enum? action = CadService.KeywordAnswer(
                document,
                actionKeywords,
                $"\nДействие для {settingName}: ",
                allowNone: true);

            if (action == null)
            {
                return;
            }

            string path = Path.Combine(AddOnContext.AddOnInfo.AssemblyDirectory, fileName);

            switch ((LogLevelAction)action)
            {
                case LogLevelAction.Apply:
                    SetLevel(document, path, fileName, settingName);
                    break;

                case LogLevelAction.Delete:
                    DeleteLevel(path, fileName, settingName);
                    break;

                case LogLevelAction.Status:
                    ShowStatus(path, fileName, settingName);
                    break;
            }
        }

        /// <summary>
        /// Предлагает уровень логирования и записывает выбранное значение в файл.
        /// </summary>
        /// <param name="document">Активный документ NanoCAD, используемый для ввода ключевого слова.</param>
        /// <param name="path">Полный путь к файлу настройки.</param>
        /// <param name="fileName">Имя файла для сообщения пользователю.</param>
        /// <param name="settingName">Название настраиваемой функции логирования.</param>
        private static void SetLevel(App.Document document, string path, string fileName, string settingName)
        {
            List<Keywords> levelKeywords = new List<Keywords>
            {
                new Keywords(nameof(LogLevelValue.Trace), LogLevelValue.Trace),
                new Keywords(nameof(LogLevelValue.Debug), LogLevelValue.Debug),
                new Keywords(nameof(LogLevelValue.Info), LogLevelValue.Info),
                new Keywords(nameof(LogLevelValue.Warn), LogLevelValue.Warn),
                new Keywords(nameof(LogLevelValue.Error), LogLevelValue.Error),
                new Keywords(nameof(LogLevelValue.Fatal), LogLevelValue.Fatal),
                new Keywords(nameof(LogLevelValue.Off), LogLevelValue.Off)
            };

            Enum? answer = CadService.KeywordAnswer(
                document,
                levelKeywords,
                $"\nНовый уровень для {settingName}: ",
                allowNone: true);

            if (answer == null)
            {
                return;
            }

            string level = answer.ToString()!;

            try
            {
                File.WriteAllText(path, level + Environment.NewLine);
                AddOnContext.Msg.InfoMessage(
                    $"Для {settingName} установлен уровень {level} в файле {fileName}. Для применения изменения необходимо перезапустить nanoCAD.");
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage($"Не удалось записать файл {fileName}.", ex);
            }
        }

        /// <summary>
        /// Удаляет файл переопределения, чтобы при следующем запуске использовалось программное значение.
        /// </summary>
        /// <param name="path">Полный путь к файлу настройки.</param>
        /// <param name="fileName">Имя файла для сообщения пользователю.</param>
        /// <param name="settingName">Название настройки для сообщения о программном значении.</param>
        private static void DeleteLevel(string path, string fileName, string settingName)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    AddOnContext.Msg.InfoMessage(
                        $"Переопределение {settingName} удалено (файл {fileName}). Для применения изменения необходимо перезапустить nanoCAD.");
                }
                else
                {
                    AddOnContext.Msg.InfoMessage(
                        $"Файл {fileName} отсутствует. Используется программное значение {settingName}.");
                }
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage($"Не удалось удалить файл {fileName}.", ex);
            }
        }

        /// <summary>
        /// Показывает наличие файла настройки и значение его первой строки.
        /// </summary>
        /// <param name="path">Полный путь к файлу настройки.</param>
        /// <param name="fileName">Имя файла для сообщения пользователю.</param>
        /// <param name="settingName">Название настройки для сообщения о программном значении.</param>
        private static void ShowStatus(string path, string fileName, string settingName)
        {
            try
            {
                if (!File.Exists(path))
                {
                    AddOnContext.Msg.InfoMessage(
                        $"Файл {fileName} отсутствует. Используется программное значение {settingName}.");
                    return;
                }

                string[] lines = File.ReadAllLines(path);
                string level = lines.Length > 0 ? lines[0].Trim() : string.Empty;

                AddOnContext.Msg.InfoMessage(
                    string.IsNullOrWhiteSpace(level)
                        ? $"Файл {fileName} существует, но первая строка пуста."
                        : $"В файле {fileName} указан уровень: {level}.");
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage($"Не удалось прочитать файл {fileName}.", ex);
            }
        }

        /// <summary>
        /// Действие пользователя с файлом переопределения уровня логирования.
        /// </summary>
        private enum LogLevelAction
        {
            [Description("Установить")]
            Apply,
            [Description("уДалить")]
            Delete,
            [Description("Статус")]
            Status
        }

        /// <summary>
        /// Поддерживаемые значения уровня логирования.
        /// </summary>
        private enum LogLevelValue
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal,
            Off
        }
    }
}
