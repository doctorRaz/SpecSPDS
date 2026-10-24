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
    /// Управление уровнем логирования через файл log.level.
    /// </summary>
    public sealed class LogLevelCommand
    {
        private const string LogLevelFileName = "log.level";

        /// <summary>
        /// Позволяет установить уровень логирования, удалить переопределение или посмотреть текущее значение.
        /// </summary>
        [Rtm.CommandMethod(CommandPrefix.Value + "_LogLevel", Rtm.CommandFlags.Session)]
        [Description("Настройка уровня логирования")]
        public void SetLogLevel()
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
                "\nДействие с уровнем логирования: ",
                allowNone: true);

            if (action == null)
            {
                return;
            }

            string path = Path.Combine(AddOnContext.AddOnInfo.AssemblyDirectory, LogLevelFileName);

            switch ((LogLevelAction)action)
            {
                case LogLevelAction.Set:
                    SetLevel(document, path);
                    break;

                case LogLevelAction.Delete:
                    DeleteLevel(path);
                    break;

                case LogLevelAction.Status:
                    ShowStatus(path);
                    break;
            }
        }

        private static void SetLevel(Document document, string path)
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
                "\nНовый уровень логирования: ",
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
                    $"Уровень логирования установлен: {level}. Для применения изменения перезапусти nanoCAD.");
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage("Не удалось записать файл настройки уровня логирования.", ex);
            }
        }

        private static void DeleteLevel(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    AddOnContext.Msg.InfoMessage(
                        "Переопределение уровня логирования удалено. Для применения изменения перезапусти nanoCAD.");
                }
                else
                {
                    AddOnContext.Msg.InfoMessage("Файл log.level отсутствует. Используется программный уровень логирования.");
                }
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage("Не удалось удалить файл настройки уровня логирования.", ex);
            }
        }

        private static void ShowStatus(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    AddOnContext.Msg.InfoMessage("Файл log.level отсутствует. Используется программный уровень логирования.");
                    return;
                }

                string? level = File.ReadAllLines(path).Length > 0
                    ? File.ReadAllLines(path)[0].Trim()
                    : string.Empty;

                AddOnContext.Msg.InfoMessage(
                    string.IsNullOrWhiteSpace(level)
                        ? "Файл log.level существует, но первая строка пуста."
                        : $"В файле log.level указан уровень: {level}.");
            }
            catch (Exception ex)
            {
                AddOnContext.Msg.ErrorMessage("Не удалось прочитать файл настройки уровня логирования.", ex);
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
