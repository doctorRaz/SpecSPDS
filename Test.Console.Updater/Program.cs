global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;

//global using static dRz.Src.Infrastructure.AddOnContext;

using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Test.Console;
using dRz.Updater;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Test
{
    /// <summary>
    /// Start
    /// </summary>
    public class Program
    {
        //[STAThread]
        private static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ConteinerCtx.Run();

                _logger!.Info("Start");

                AddOnCtx.Msg.InfoMessage(AddOnCtx.AddOnInfo.ToLongString());

                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory);

                updateManager.Cleanup(AddOnCtx.AddOnInfo.PackageDirectory);

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                    AddOnCtx.AddOnInfo.Product +
                    "/releases/latest/download/";

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.RunningVersion,

                    UpdateUrl = updateUrl,

                    Mode = UpdateMode.CheckAndInstall,

                    AddOnDirectory = AddOnCtx.AddOnInfo.PackageDirectory
                };



                Task<bool> ff = updateManager.RunAsync(request);

            }
            catch (Exception ex)
            {
                if (_isLoggerProvider)
                {
                    _logger.Fatal(ex, "Продолжение не возможно");
                }

                AddOnCtx.Msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                if (_isLoggerProvider)
                {
                    _logger.Info("Terminate");
                }

                ConsoleReadKey();
            }
        }

        private static ConsoleKeyInfo ConsoleReadKey()
        {
            Console.WriteLine("Press any key to exit...");
            return Console.ReadKey();
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Program));
        private static bool _isLoggerProvider;//логер есть
        private static IMessageService _msg;
        private static ICommandLineMessageService _msgCmd;
    }
}