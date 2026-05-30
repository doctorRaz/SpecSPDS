using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;
using System;
using System.Diagnostics;
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

             Stopwatch sw = Stopwatch.StartNew();
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
            //ICadInfo
            ICadInfo cadInfo = CadInfo_NEW;

            Console.WriteLine("CadInfo_NEW");
            Console.WriteLine(CadInfo_NEW.ToString());
            Console.WriteLine(CadInfo_NEW.ToShortString());//todo not implement CadInfo_NEW.ToShortString
            Console.WriteLine(CadInfo_NEW.ToLongString());//todo not implement CadInfo_NEW.ToLongString

            //ISysInfo
            ISysInfo sysInfo_NEW = SysInfo_NEW;

            Console.WriteLine("sysInfo_NEW");
            Console.WriteLine(sysInfo_NEW.ToString());
            Console.WriteLine(sysInfo_NEW.ToShortString());
            Console.WriteLine(sysInfo_NEW.ToLongString());

            //IApplicationInfo_NEW
            IApplicationInfo_NEW infoDll_NEW = InfoDll_NEW;

            Console.WriteLine("infoDll_NEW");
            Console.WriteLine(infoDll_NEW.ToString());
            Console.WriteLine(infoDll_NEW.ToShortString());
            Console.WriteLine(infoDll_NEW.ToLongString());
            //

            IApplicationInfo_NEW addon = AC.InfoDll_NEW;

            IApplicationInfo_NEW infoDll = InfoDll_NEW;

            ISysInfo os = SysInfo_NEW;
            Console.WriteLine(os);

            ICadInfo cad = CadInfo_NEW;

            IDocumentService documentService = AC.DocService;

           

            //-----



            string path = InfoDll_NEW.AppDataProductLogPath;

            Console.WriteLine(CadInfo_NEW);
        }

        public void TestInjectorMessage()
        {
            AC.Msg.InfoMessage($"{AC.InfoDll_NEW.TitlePrefix} Message");

            AC.MsgCmd.ConsoleMessage($"{AC.InfoDll_NEW.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.InfoDll_NEW.TitlePrefix} Info message");

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