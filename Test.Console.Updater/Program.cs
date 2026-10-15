global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;

//global using static dRz.Src.Infrastructure.AddOnContext;

using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using System;
using Sys = System;
using System.Diagnostics;

namespace dRz.Test.Console.Updater
{
    public class Program
    {
        private static void /*async Task*/ Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ConteinerCtx.Run();

                _logger = AddOnCtx.NLogFactory.GetLogger<Program>();

                _logger?.Debug("Start debug updater");

                _msg.InfoMessage($"Start update");

                ProgramAsync programAsync = new();

                _ = programAsync.MainAsync();

            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                _logger?.Info("Terminate");

                _ = ConsoleReadKey(sw);
            }
        }

        private static ConsoleKeyInfo ConsoleReadKey(Stopwatch? sw = null)
        {
            string res = "";

            if (sw != null)
            {
                res = $"Total time {sw.Elapsed}\n";
            }

            _msg.InfoMessage($"{res}Press any key to exit...");

            return Sys.Console.ReadKey();
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Program));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}