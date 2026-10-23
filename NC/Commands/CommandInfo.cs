using dRz.Abstractions.Services.Message;
using dRz.NC.Infrastructure;
using dRz.NC.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Teigha.Runtime;

namespace dRz.NC.Commands
{
    public class CommandInfo
    {
        private static IMessageService _msg = AddOnContext.Msg;

        [CommandMethod(CommandPrefix.Value + "CommandInfo")]
        [Description("Список зарегистрированных команд")]
        public static void CmdInfo()
        {
            
            bool bMethod = false;//не выводить имена методов
#if DEBUG
            bMethod = true;
#endif
            //выводим список команд с описаниями
            CmdInfo CDI = new CmdInfo(Assembly.GetExecutingAssembly(), bMethod);//эта сборка вывод имен классов

            string sVersion = AddOnContext.AddOnInfo.ToShortString();// AI.sVersionFull;

            if (!string.IsNullOrWhiteSpace(CDI.sCmdInfo))
            {
                _msg.InfoMessage($"\n{CDI.sCmdInfo}");
            }
            else
            {
                _msg.InfoMessage("Нет зарегистрированных команд");
            }

            //дубликаты команд
            if (!string.IsNullOrEmpty(CDI.sDuplInfo))
            {
                _msg.InfoMessage($"\n{CDI.sDuplInfo}");
            }
        }
    }
}