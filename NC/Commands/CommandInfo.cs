using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.NC.Infrastructure;
using dRz.NC.Services;
using System;
using System.ComponentModel;
using System.Reflection;
using Teigha.Runtime;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Команда вывода списка объявленных CAD-команд и отчёта о дубликатах.
    /// </summary>
    public sealed class CommandInfo
    {
        [CommandMethod(CommandPrefix.Value + "CommandInfo")]
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

                // Анализируем сборку аддона. Каждому CommandMethodAttribute соответствует отдельная запись.
                var report = new dRz.NC.Services.CmdInfo(
                    Assembly.GetExecutingAssembly(),
                    includeMethodInfo,
                    logger);

                if (!string.IsNullOrWhiteSpace(report.CommandText))
                {
                    message.InfoMessage($"\n{report.CommandText}");
                }
                else
                {
                    message.InfoMessage("Нет зарегистрированных команд");
                }

                // Собственный поиск дубликатов сохраняется: в отчёте указаны методы,
                // которые объявляют конфликтующее глобальное имя команды.
                if (!string.IsNullOrEmpty(report.DuplicateText))
                {
                    message.InfoMessage($"\n{report.DuplicateText}");
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