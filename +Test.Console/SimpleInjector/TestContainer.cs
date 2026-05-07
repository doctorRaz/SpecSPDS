using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;
using drz.EnvironmentInfo;
using drz.EnvironmentInfo.Cad;
using drz.EnvironmentInfo.Sys;
using System;
using static drz.Src.Infrastructure.AddOnContext;
using AC = drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.SimpleInjector
{
    internal class TestContainer
    {
        #region Private Fields

        private static bool _initialized;

        #endregion Private Fields

        #region Public Constructors

        public TestContainer()
        {
            if (_initialized)
            {
                return;
            }

            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(TestContainer).Assembly);

            AC.Initialize(root);

            _initialized = true;
        }

        #endregion Public Constructors

        #region Public Methods

        public void TestInjectorInfo()
        {
            ICadInfo cadInfo = CadInfo_NEW;
            Console.WriteLine(CadInfo_NEW);
            Console.WriteLine(CadInfo_NEW.ToString());
            Console.WriteLine(CadInfo_NEW.Copyright);

            ISysInfo sysInfo = SysInfo_NEW;
            Console.WriteLine(sysInfo);

            IApplicationInfo_NEW infoDll_NEW = InfoDll_NEW;

            Console.WriteLine(infoDll_NEW);
            Console.WriteLine(infoDll_NEW.ToShortString());
            Console.WriteLine(infoDll_NEW.ToLongString());
            //

            IApplicationInfo addon = AC.AddOn;

            IApplicationInfo_NEW infoDll = InfoDll;

            ISysInfo os = SysInfo.Current;

            ICadInfo cad = CadInfo.Current;

            IDocumentService documentService = AC.DocService;

            RuntimeInfo rt = RT.Info;


            //-----


            Console.WriteLine(os);

            string path = InfoDll.AppDataProductLogPath;

            Console.WriteLine(CadInfo.Current);
        }

        public void TestInjectorMessage()
        {
            AC.Msg.InfoMessage($"{AC.AddOn.TitlePrefix} Message");

            AC.MsgCmd.ConsoleMessage($"{AC.AddOn.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.AddOn.TitlePrefix} Info message");

            //new
            AC.Msg.InfoMessage($"{AC.InfoDll_NEW.TitlePrefix} Message NEW");

            AC.MsgCmd.ConsoleMessage($"{AC.InfoDll_NEW.TitlePrefix} Console message NEW");

            AC.MsgGUI.InfoMessage($"{AC.InfoDll_NEW.TitlePrefix} Info message NEW");





            //IApplicationInfo app = AC.Get<IApplicationInfo>();
            //IMessageService messageService = AC.Get<ICommandLineMessageService>();
            //messageService.ConsoleMessage($"{app.TitlePrefix}  Console message");

            //messageService = AC.Get<IWindowMessageService>();
            //messageService.InfoMessage("Info message");
        }

        #endregion Public Methods
    }
}