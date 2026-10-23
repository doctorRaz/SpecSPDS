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
        [Description("Список зарегистрированных команд Dubl")]
        public static void CmdInfo2()
        {
            var message = AddOnContext.Msg;
            message.InfoMessage("Duble");
        }

        [Rtm.CommandMethod(CommandPrefix.Value + "_CommandInfo")]
        [Description("Список зарегистрированных команд")]
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
                var report = new dRz.NC.Services.CmdInfo(
                    Assembly.GetExecutingAssembly(),
                    logger);

                var duplicateGroups = report.DuplicateCommands.ToList();
                var commandLines = new List<string>();

                foreach (var command in report.RegisteredCommands)
                {
                    string method = includeMethodInfo ? $" [{command.MethodName}]" : string.Empty;
                    commandLines.Add($"{command.CommandName}{method}\t{command.Description}");
                }

                if (commandLines.Count > 0)
                {
                    message.InfoMessage($"\n{string.Join(Environment.NewLine, commandLines)}");
                }
                else
                {
                    message.InfoMessage("Нет зарегистрированных команд");
                }

                // Сохраняем собственную диагностику: выводим все методы, объявившие дубликат.
                var duplicateLines = new List<string>();
                foreach (var group in duplicateGroups)
                {
                    duplicateLines.Add($"Дублированный атрибут: {group.Key}");
                    duplicateLines.AddRange(group.Select(entry => $"\t[{entry.MethodName}] {entry.Description}"));
                }

                if (duplicateLines.Count > 0)
                {
                    message.InfoMessage($"\n{string.Join(Environment.NewLine, duplicateLines)}");
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