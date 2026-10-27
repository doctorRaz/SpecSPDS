using dRz.Abstractions.Services.Message;
using dRz.Infrastructure.NC.Services;
using dRz.NC.Infrastructure;
using System;

namespace dRz.SpecSPDS.Test
{
    /// <summary>
    /// Демонстрирует отправку сообщений через консольный и оконный сервисы.
    /// </summary>
    public class ContainerrMessage
    {
        private readonly IMessageService _msg;
        private readonly IWindowMessageService _msgGui;
        private readonly ICommandLineMessageService _msgCmd;
        private readonly DocumentService _documentService;
        private readonly System.Exception _ex;
        private const string _msgErr = "Program Error";
        private MessageResult _messageResult;
        //internal DocumentService documentService => _documentService;

        internal ContainerrMessage()
        {
            _msg = AddOnContext.Msg;
            _msgCmd = AddOnContext.MsgCmd;
            _msgGui = AddOnContext.MsgW;

            _ex = new Exception("Test Exception");

            //так делать нехорошо , но для отладки можно(((
            _documentService = (DocumentService)AddOnContext.DocService;
        }

        /// <summary>route the MSG console or window message</summary>
        internal void RunMsg()
        {
            _msg.InfoMessage(AddOnContext.SysInfo.ToString());//ex
            _msg.WarningMessage(AddOnContext.CadInfo.ToString());
            _msg.ErrorMessage(_ex);
            _msg.ErrorMessage(_msgErr, _ex);
        }

        /// <summary>Runs the MSG command.</summary>
        internal void RunMsgCmd()
        {
            _msgCmd.InfoMessage(AddOnContext.SysInfo.ToString());//ex
            _msgCmd.WarningMessage(AddOnContext.CadInfo.ToString());
            _msgCmd.ErrorMessage(_ex);
            _msgCmd.ErrorMessage(_msgErr, _ex);
        }

        /// <summary>Runs the MSG GUI.</summary>
        internal void RunMsgGui()
        {
            //info
            _msgGui.InfoMessage(AddOnContext.SysInfo.ToString());
            _msgGui.WarningMessage(AddOnContext.CadInfo.ToString());
            _msgGui.ErrorMessage(_ex);
            _msgGui.ErrorMessage(_msgErr, _ex);

            //return MessageResult
            _messageResult = _msgGui.AskOkCancel("Test Message", "Caption");
            _messageResult = _msgGui.AskAbortRetryIgnore("Test Message", "Caption");
            _messageResult = _msgGui.AskYesNoCancel("Test Message", "Caption");
            _messageResult = _msgGui.AskYesNo("Test Message", "Caption");
            _messageResult = _msgGui.AskRetryCancel("Test Message", "Caption");
        }

        internal static void Run()
        {
            ContainerrMessage containerrMessage = new ContainerrMessage();
            containerrMessage.RunMsgCmd();
            containerrMessage.RunMsgGui();

            containerrMessage._documentService.IsActive = false;
            containerrMessage.RunMsg();

            containerrMessage._documentService.IsActive = true;
            containerrMessage.RunMsg();
        }
    }
}