using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Updater.Services;

namespace dRz.Updater
{
    /// <summary>
    /// Менеджер обновлений.
    /// </summary>
    public class UpdateManager
    {
        private readonly IDrzLogger _logger;
        private readonly IMessageService _messageServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateManager"/> class.
        /// </summary>
        /// <param name="messageServices">Сервис сообщений.</param>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        public UpdateManager(
            IMessageService messageServices,
            IDrzLoggerFactory loggerFactory)
        {
            _messageServices = messageServices;
            _logger = loggerFactory.GetLogger<UpdateManager>();
        }

        /// <summary>
        /// Очищает каталог аддона от резервных копий и пустых каталогов.
        /// </summary>
        /// <param name="addOnDirectory">Полный путь к каталогу аддона.</param>
        public void Cleanup(string addOnDirectory)
        {
            if (string.IsNullOrWhiteSpace(addOnDirectory))
                throw new ArgumentException("Не указан каталог аддона.", nameof(addOnDirectory));

            BackupCleaner.DeleteBackupFiles(addOnDirectory);
        }

        /// <summary>
        /// Выполняет проверку и установку обновления согласно переданным параметрам.
        /// </summary>
        /// <param name="request">Параметры обновления, сформированные основным аддоном.</param>
        /// <returns><see langword="true"/>, если операция выполнена успешно.</returns>
        public bool Run(UpdateRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.Debug("Run");

            // TODO: Реализация проверки и установки обновления.
            return true;
        }
    }
}
