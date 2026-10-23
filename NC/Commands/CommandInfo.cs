using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.NC.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Rtm = Teigha.Runtime;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Команда вывода списка объявленных CAD-команд и отчёта о дубликатах.
    /// </summary>
    public sealed class CommandInfo
    {
        [Rtm.CommandMethod(CommandPrefix.Value + "_CommandInfo")]
        [Description("Список зарегистрированных команд с описанием")]
        public static void CmdInfo()
        {
            IDrzLogger? logger = null;
            IMessageService? message = null;

            try
            {
                // Сервисы запрашиваются при выполнении команды, после инициализации AddOnContext.
                logger = AddOnContext.NLogFactory.GetLogger(typeof(CommandInfo));
                message = AddOnContext.Msg;

                bool includeMethodInfo = false;
#if DEBUG
                includeMethodInfo = true;
#endif

                // Сбор объявлений отделён от формирования текстовых отчётов.
                Services.CmdInfo report = new Services.CmdInfo(
                    Assembly.GetExecutingAssembly(),
                    logger);

                List<IGrouping<string, Services.CmdInfo.CommandEntry>> duplicateGroups = report.DuplicateCommands
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                List<string> commandLines = new List<string>();

                foreach (Services.CmdInfo.CommandEntry? command in report.RegisteredCommands
                    .OrderBy(command => command.CommandName, StringComparer.OrdinalIgnoreCase))
                {
                    string method = includeMethodInfo ? $" [{command.MethodName}]" : string.Empty;
                    commandLines.Add($"{command.CommandName}{method}\t{command.Description}");
                }

                string border = "------------------------------------------";

                message.InfoMessage(border);

                if (commandLines.Count > 0)
                {
                    //пробел в конце строки добавлен специально, вывод в консоль нанокад, почему то обрезает последний символ последней строки
                    message.InfoMessage($"{string.Join(Environment.NewLine, commandLines)} ");
                }
                else
                {
                    message.InfoMessage("Нет зарегистрированных команд");
                }
                message.InfoMessage(border);

                // Сохраняем собственную диагностику: выводим все методы, объявившие дубликат.
                List<string> duplicateLines = new List<string>();
                foreach (IGrouping<string, Services.CmdInfo.CommandEntry>? group in duplicateGroups)
                {
                    duplicateLines.Add($"Дублированный атрибут: {group.Key}");
                    duplicateLines.AddRange(group
                        .OrderBy(entry => entry.MethodName, StringComparer.OrdinalIgnoreCase)
                        .Select(entry => $"\t[{entry.MethodName}] {entry.Description}"));
                }

                if (duplicateLines.Count > 0)
                {
                    message.InfoMessage($"{string.Join(Environment.NewLine, duplicateLines)}");

                    message.InfoMessage(border);
                }
            }
            catch (Exception ex)
            {
                // Сбой диагностического отчёта не должен влиять на инициализацию аддона.
                try
                {
                    logger?.Error(ex, "Не удалось сформировать отчёт о CAD-командах.");
                }
                catch
                {
                    // Сбой логгера не должен скрывать исходную ошибку.
                }

                try
                {
                    message?.ErrorMessage("Не удалось сформировать отчёт о CAD-командах.", ex);
                }
                catch
                {
                    // Сбой уведомления также не должен распространяться в CAD-хост.
                }
            }
        }
    }
}