using dRz.SpecSPDS.Core.Extensions;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Settings;
using System;

namespace dRz.SpecSpdsConsole
{

    public class Program
    {

        [STAThread]
        static void Main(string[] args)
        {

            AppSettings appSettings = new AppSettings();

            Console.WriteLine(appSettings.ConfigPath);
            //appSettings.Settings.ApplicationName = "SomeTestApplication";
            //appSettings.Settings.MarkerName = "Спецификация_V2.4";
            appSettings.Settings.IsSpec = true;
            appSettings.SaveSettings();


            PropXml propXml = new PropXml();
            var props = propXml.Props;
            //props.MarkerName = "xxz";
            //props.FlagSpecRaw = "1";

            //propXml.SaveProps();
        lb:
            Test test = new Test();
            string[] str =
                {
                "",
                "y",
                "Y",
                "t",
                "tr",
                " true",
                "TRUE ",
                "TRuE",
                "д",
                "Д",
                "Да",
                "ДА",
                "дА",
                "1",
                "-1",
                "-1.12",
                "-1,12",
                "100.10.10",
                "00000.55",
                "00000,55",
                "1.000.007E-08",
                "1.000.007E-08",
                "1.000.007E-05",
                "1000000.000001",
                };

            foreach (string s in str)
            {
                Console.WriteLine($"{s} \t{s.ToBoolean()}");
            }

            Console.WriteLine("-----------------------");
            //foreach (string s in str)
            //{
            //    Console.WriteLine($"{s} \t{s.ToDouble()}");
            //}

            //Console.WriteLine("-----------------------");


            { }            //  var field = new FieldNameSettings();
            goto lb;
            //var  _standardDocProperties = new Dictionary<PropName, string>();
            //  foreach (PropName docProp in Enum.GetValues(typeof(PropName)))
            //  {
            //      _standardDocProperties.Add(docProp, "");
            //  }
            //  { }
            //EnumDescription enumConverter = new EnumDescription();
            //enumConverter.StartTest();

            //return;

            //AddKeywordTest addKeywordTest = new AddKeywordTest();
            //addKeywordTest.StartTest();

            //return;
            //AppSettings appSettings = new AppSettings();

            //Console.WriteLine(appSettings.ConfigPath);
            ////appSettings.Settings.ApplicationName = "SomeTestApplication";
            ////appSettings.Settings.MarkerName = "Спецификация_V2.4";
            //appSettings.Settings.IsSpec = true;
            //appSettings.SaveSettings();

            //DoNotChangeUITest doNotChangeUITest = new DoNotChangeUITest();
            //doNotChangeUITest.StartTest();


            //BlockNormalizeUITest blockNormalizeUiTest = new BlockNormalizeUITest();
            //blockNormalizeUiTest.StartTest();

            //WindowOwnerUITest windowOwnerUITest = new WindowOwnerUITest();
            //windowOwnerUITest.StartTest();

            //проверим что там теперь с настройками
            //SettingsUITest settingsTest = new SettingsUITest();
            //settingsTest.StartTest();

            //Console.WriteLine("Hello, World!");
            Console.ReadKey();
        }
    }
}
