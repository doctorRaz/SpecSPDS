using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Updater.Models;
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
        private readonly IMessagePromptService _promptService;
        private readonly Downloader _downloader;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateManager"/> class.
        /// </summary>
        /// <param name="messageServices">Сервис сообщений.</param>
        /// <param name="promptService">Сервис интерактивных запросов.</param>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        public UpdateManager(
            IMessageService messageServices,
            IMessagePromptService promptService,
            IDrzLoggerFactory loggerFactory)
        {
            _messageServices = messageServices;
            _promptService = promptService;
            _downloader = new Downloader();
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
        /// <returns><see langword="true"/>, если обновление установлено или проверка завершена без ошибки.</returns>
        public async Task<bool> RunAsync(
            UpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidateRequest(request);

            _logger.Debug("RunAsync");

            string tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "dRz",
                "Updater",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempDirectory);

            try
            {
                // update.json нужен даже в Disabled, чтобы mandatory update
                // оставался обнаруживаемым.
                string updateJsonPath = Path.Combine(tempDirectory, "update.json");

                await _downloader.DownloadAsync(
                    request.UpdateUrl,
                    updateJsonPath,
                    cancellationToken);

                UpdateInfo update = UpdateJsonReader.Read(updateJsonPath);

                bool? updateRequired = UpdateVersionChecker.Check(
                    request.CurrentVersion,
                    update);

                if (updateRequired is null)
                    return false;

                if (request.Mode == UpdateMode.Disabled && updateRequired == false)
                    return false;

                bool installAutomatically =
                    updateRequired == true ||
                    request.Mode == UpdateMode.CheckAndInstall;

                if (!installAutomatically)
                {
                    MessageResult result = _promptService.AskYesNo(
                        $"Доступно обновление версии {update.Version}. Установить его?",
                        "Обновление");

                    if (result != MessageResult.Yes)
                        return false;
                }

                string assetFile = Path.Combine(
                    tempDirectory,
                    "update" + Path.GetExtension(update.Asset));

                string assetUrl = ResolveUrl(request.UpdateUrl, update.Asset);

                await _downloader.DownloadAsync(
                    assetUrl,
                    assetFile,
                    cancellationToken);

                if (!FileVerifier.Verify(
                    assetFile,
                    update.Size,
                    update.Sha256))
                {
                    throw new InvalidDataException(
                        "Проверка размера или SHA-256 пакета обновления не пройдена.");
                }

                MarkOfTheWebRemover.Remove(assetFile);

                // TODO: Распаковка и установка пакета.
                return true;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDirectory))
                        Directory.Delete(tempDirectory, true);
                }
                catch
                {
                    // Временный каталог не должен маскировать результат операции.
                }
            }
        }

        /// <summary>
        /// Формирует URL ресурса относительно URL update.json.
        /// </summary>
        private static string ResolveUrl(string baseUrl, string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                throw new InvalidDataException("В update.json не указан asset.");

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
                throw new InvalidDataException("Некорректный URL update.json.");

            if (Uri.TryCreate(resource, UriKind.Absolute, out Uri? absoluteUri))
                return absoluteUri.ToString();

            return new Uri(baseUri, resource).ToString();
        }

        /// <summary>
        /// Проверяет обязательные параметры запроса.
        /// </summary>
        private static void ValidateRequest(UpdateRequest request)
        {
            if (request.CurrentVersion is null)
                throw new ArgumentException("Не указана текущая версия.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.UpdateUrl))
                throw new ArgumentException("Не указан URL проверки обновления.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.AddOnDirectory))
                throw new ArgumentException("Не указан каталог аддона.", nameof(request));
        }
    }
}