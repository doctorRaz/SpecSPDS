using dRz.Core.Services;
using dRz.NC.CadServices;
using dRz.NC.Infrastructure;
using HostMgd.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Rtm = Teigha.Runtime;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Управление уровнями логирования через файлы log.level и diagnostic.mode.
    /// </summary>
    public sealed class LogLevelCommand
    {
        private const string LogLevelFileName = "log.level";
        private const string DiagnosticModeFileName = "diagnostic.mode";

        /// <summary>
        /// Позволяет установить уровень основного логгера, удалить переопределение или посмотреть текущее значение.
        /// Команда: drz_LogLevel.
        /// </summary>
        [Rtm.CommandMethod(CommandPrefix.Value + "_LogLevel", Rtm.CommandFlags.Session)]
        [Description("Настройка уровня логирования")]
        public void SetLogLevel()
        {
            ManageLevel(LogLevelFileName, "уровня логирования");
        }

        /// <summary>
        /// Позволяет установить уровень внутренней диагностики NLog, удалить переопределение или посмотреть текущее значение.
        /// Команда: drz_DiagnosticMode.
        /// </summary>
        [Rtm.CommandMethod(CommandPrefix.Value + "_DiagnosticMode", Rtm.CommandFlags.Session)]
        [Description("Настройка внутренней диагностики логирования")]
        public void SetDiagnosticMode()
        {
            ManageLevel(DiagnosticModeFileName, "внутренней диагностики логирования");
        }

        private static void ManageLevel(string fileName, string settingName)
        {
            Document? document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            List<Keywords> actionKeywords = new List<Keywords>
            {
                new Keywords(nameof(LogLevelAction.Set), LogLevelAction.Set),
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
                case LogLevelAction.Set:
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

        private static void SetLevel(Document document, string path, string fileName, string settingName)
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

        private enum LogLevelAction
        {
            Set,
            Delete,
            Status
        }

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
