using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using System;

namespace drz.Src.Infrastructure
{
    /// <summary>
    /// Класс чисто для укорочения вызовов service or infrastructure в коде, чтобы не писать Services.Get&lt;IService&gt;()
    /// </summary>
    internal static class AddOnContext
    {
        #region Private Fields

        private static IAddOnServices? _services;

        #endregion Private Fields

        #region Internal Properties

        /// <summary>Gets the add on information.</summary>
        /// <value>The add on information.</value>
        internal static IAddOnInfo AddOnInfo => Services.Get<IAddOnInfo>();

        /// <summary>Gets the cad information.</summary>
        /// <value>The cad information.</value>
        internal static ICadInfo CadInfo => Services.Get<ICadInfo>();

        /// <summary>Gets the document service.</summary>
        /// <value>The document service.</value>
        internal static IDocumentService DocService => Services.Get<IDocumentService>();

        /// <summary>
        /// явный вызов ком строки
        /// </summary>
        internal static ICommandLineMessageService MsgCmd => Services.Get<ICommandLineMessageService>();

        /// <summary>явный вызов окошка + Ask</summary>
        /// <value>The MSG GUI.</value>
        internal static IWindowMessageService MsgGui => Services.Get<IWindowMessageService>();

        /// <summary>Явный вызов нотифай мультикад.</summary>
        /// <value>Сервис уведомлений <see cref="IMcNotificatorMessageService"/>.</value>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если сервис <see cref="IMcNotificatorMessageService"/> не зарегистрирован.
        /// </exception>
        internal static IMcNotificatorMessageService MsgMcN => Services.Get<IMcNotificatorMessageService>();

        // общий вызов сервиса сообщений
        // реализация в классе DefaultMessageService?
        // вернет IWindowMessageService или ICommandLineMessageService
        //если документ есть отдаст консоль иначе окошко
        internal static IMessageService Msg => Services.Get<IMessageService>();

        // наследник IMessageService,IWindowMessageService расширен диалоговыми окнами: да нет пропустить дальше
        //todo internal static IWindowMessageServiceAsk MsgGUIAsc => Services.Get<IWindowMessageServiceIWindowMessageServiceAsk>();

        /// <summary>
        /// фабрика логгеров, одна на продукт из нее каждый класс получает свой логгер
        /// </summary>
        internal static IDrzLoggerFactory NLogFactory => Services.Get<IDrzLoggerFactory>();

        /// <summary>Gets the system information.</summary>
        /// <value>The system information.</value>
        internal static ISysInfo SysInfo => Services.Get<ISysInfo>();

        #endregion Internal Properties

        #region Private Properties

        /// <summary>DI services (инициализируется отдельно)</summary>
        /// <value>The services.</value>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot is not initialized</exception>
        internal static IAddOnServices Services => _services ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

        #endregion Private Properties

        #region Internal Methods

        /// <summary>
        /// Initializes the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        internal static void Initialize(IAddOnServices services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (_services != null)
                throw new InvalidOperationException("AddOnContext уже инициализирован.");

            _services = services;
        }

        #endregion Internal Methods
    }
}