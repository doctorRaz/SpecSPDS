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

                _logger?.Debug("Start debug updater");

                _msg.InfoMessage(AddOnCtx.AddOnInfo.ToLongString());

                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory);

                updateManager.Cleanup(AddOnCtx.AddOnInfo.PackageDirectory);
                //return;
                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                    AddOnCtx.AddOnInfo.Product +
                    "/releases/latest/download/";

                updateUrl = "http://127.0.0.1:8080/mandatory/";

                UpdateMode mode = UpdateMode.CheckAndInstall;

                UpdateLaunchMode launchMode = UpdateLaunchMode.Manual;

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.RunningVersion,

                    UpdateUrl = updateUrl,

                    Mode = mode,

                    PackageDirectory = AddOnCtx.AddOnInfo.PackageDirectory,

                    IsPackage = AddOnCtx.AddOnInfo.HasPackage,

                    IsNetworkPath = AddOnCtx.AddOnInfo.IsNetworkPath(),

                    LaunchMode = launchMode,
                };

                //что с ним делать дальше????
                Task<bool> task = updateManager.RunAsync(request);
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                AddOnCtx.Msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                _logger?.Info("Terminate");

                ConsoleReadKey();
            }
        }

        private static ConsoleKeyInfo ConsoleReadKey()
        {
            Console.WriteLine("Press any key to exit...");
            return Console.ReadKey();
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Program));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}