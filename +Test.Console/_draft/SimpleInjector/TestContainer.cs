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
            ICadInfo cadInfo = CadInfo;

            Console.WriteLine("CadInfo_NEW");
            Console.WriteLine(CadInfo.ToString());
            Console.WriteLine(CadInfo.ToShortString()); 
            Console.WriteLine(CadInfo.ToLongString()); 

            //ISysInfo
            ISysInfo sysInfo_NEW = SysInfo;

            Console.WriteLine("sysInfo_NEW");
            Console.WriteLine(sysInfo_NEW.ToString());
            Console.WriteLine(sysInfo_NEW.ToShortString());
            Console.WriteLine(sysInfo_NEW.ToLongString());

            //IApplicationInfo
            IAddOnInfo infoDll_NEW = AddonInfo;

            Console.WriteLine("infoDll_NEW");
            Console.WriteLine(infoDll_NEW.ToString());
            Console.WriteLine(infoDll_NEW.ToShortString());
            Console.WriteLine(infoDll_NEW.ToLongString());
            //

            IAddOnInfo addon = AC.AddonInfo;

            IAddOnInfo infoDll = AddonInfo;

            ISysInfo os = SysInfo;
            Console.WriteLine(os);

            ICadInfo cad = CadInfo;

            IDocumentService documentService = AC.DocService;

           

            //-----



            string path = AddonInfo.AppDataProductLogPath;

            Console.WriteLine(CadInfo);
        }

        public void TestInjectorMessage()
        {
            AC.Msg.InfoMessage($"{AC.AddonInfo.TitlePrefix} Message");

            AC.MsgCmd.ConsoleMessage($"{AC.AddonInfo.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.AddonInfo.TitlePrefix} Info message");

            //new
            AC.Msg.InfoMessage($"{AC.AddonInfo.TitlePrefix} Message NEW");

            AC.MsgCmd.ConsoleMessage($"{AC.AddonInfo.TitlePrefix} Console message NEW");

            AC.MsgGUI.InfoMessage($"{AC.AddonInfo.TitlePrefix} Info message NEW");





            //IApplicationInfo app = AC.Get<IApplicationInfo>();
            //IMessageService messageService = AC.Get<ICommandLineMessageService>();
            //messageService.ConsoleMessage($"{app.TitlePrefix}  Console message");

            //messageService = AC.Get<IWindowMessageService>();
            //messageService.InfoMessage("Info message");
        }

        #endregion Public Methods
    }
}