#if DEBUG
using drz.MulticadInterop.McParamManager;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.ComponentModel;
using System.Drawing;
using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.McDiagnostics
{
    public class McDiag
    {
        Document doc => Application.DocumentManager.MdiActiveDocument;
        Editor ed => doc.Editor;

        /// <summary>
        /// Переключает базу данных Multicad.
        /// </summary>
        [CommandMethod("SPEC-SetParam", CommandFlags.Session)]
        [Description("Переключение базы данных Multicad")]
        public void ChangedbMod()
        {

            PromptStringOptions opts = new PromptStringOptions("enter base:")
            {
                AllowSpaces = true
            };

            PromptResult pr = ed.GetString(opts);

            if (PromptStatus.OK == pr.Status)
            {
                MulticadParamManager.SetParam(pr.StringResult, 9);
            }
        }


        [CommandMethod($"SPEC-GetStringParam", CommandFlags.Session)]
        public static void ParamMethods()
        {
            //тянем имя непечатного слоя из настроек МС
            string newLayerNameRav = MulticadParamManager.GetStringParam(1042);
            Msg.InfoMessage(newLayerNameRav);

            //имя с приставкой из настроек, 
            string newLayerName = MulticadParamManager.GetProfiledLayerName(newLayerNameRav);
            Msg.InfoMessage(newLayerNameRav);
        }

        [CommandMethod($"SPEC-GetStringParam2", CommandFlags.Session)]
        public static void ParamMethods2()
        {
            //тянем имя непечатного слоя из настроек МС
            string newLayerNameRav = MulticadParamManager.GetStringParam(1042);
            Msg.InfoMessage(newLayerNameRav);


            newLayerNameRav = "test";
            //имя с приставкой из настроек, 
            string newLayerName = MulticadParamManager.GetProfiledLayerName(newLayerNameRav);
            Msg.InfoMessage(newLayerNameRav);
        }

        [CommandMethod($"SPEC-GetStringParam3", CommandFlags.Session)]
        public static void ParamMethods3()
        {
               
            //тянем имя профиля
            string combobox_profile = MulticadParamManager.GetStringParam(25005);
            Msg.InfoMessage(combobox_profile);


           
            var combobox_bool = MulticadParamManager.GetBoolParam(1048);
            Msg.InfoMessage($"combobox_profile: {combobox_bool.ToString()}");

             //MulticadParamManager.CallOptions("",IntPtr.Zero);

            var combobox_lineweight = MulticadParamManager.GetDoubleParam(2335);
            Msg.InfoMessage($"combobox_lineweight: {combobox_lineweight.ToString()}");

            Color combobox_color = MulticadParamManager.GetColorParam(11135);
            Msg.InfoMessage($"combobox_color: {combobox_color.ToString()}");

        }


    }
}
#endif