using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Updater.Models;
using dRz.Updater.Services;
using dRz.Updater.Services.SevenZip;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
                string updateJsonUrl = ResolveUrl(request.UpdateUrl, "update.json");

                await _downloader.DownloadAsync(
                    updateJsonUrl,
                    updateJsonPath,
                    cancellationToken);

                UpdateInfo update = UpdateJsonReader.Read(updateJsonPath);

                UpdateKind? updateKind = UpdateVersionChecker.Check(
                    request.CurrentVersion,
                    update);

                if (updateKind is null)
                    return false;

                // Disabled запрещает необязательные обновления, но не mandatory.
                if (request.Mode == UpdateMode.Disabled && !update.Mandatory)
                    return false;

                bool installAutomatically =
                    update.Mandatory ||
                    request.Mode == UpdateMode.CheckAndInstall;

                if (!installAutomatically)
                {
                    MessageResult result = _promptService.AskYesNo(
                        $"Доступно обновление {update.Product} до версии {update.Version.ToVersion()}.\nУстановить его?",
                        "Обновление");

                    if (result != MessageResult.Yes)
                        return false;
                }

                string assetFile = Path.Combine(
                    tempDirectory,
                    "update" + Path.GetExtension(GetAssetName(update)));

                string assetUrl = ResolveUrl(
                    request.UpdateUrl,
                    GetAsset(update));

                await _downloader.DownloadAsync(
                    assetUrl,
                    assetFile,
                    cancellationToken);

                if (!FileVerifier.Verify(
                    assetFile,
                    GetAssetSize(update),
                    GetAssetSha256(update)))
                {
                    throw new InvalidDataException(
                        "Проверка размера или SHA-256 пакета обновления не пройдена.");
                }

                MarkOfTheWebRemover.Remove(assetFile);

                string extractedDirectory = Path.Combine(
                    tempDirectory,
                    "extracted");

                SevenZipService sevenZip = new SevenZipService();

                sevenZip.Extract(
                    assetFile,
                    extractedDirectory,
                    GetAssetPassword(update));

                string sourceDirectory = Path.Combine(extractedDirectory, update.Product);

                if (!request.IsPackage)
                {
                    string moduleDirectory = Path.GetFileName(
                        Path.TrimEndingDirectorySeparator(request.PackageDirectory));
                    sourceDirectory = Path.Combine(sourceDirectory, moduleDirectory);
                }

                // Проверяем структуру архива до очистки и изменения установленного аддона.
                if (!Directory.Exists(sourceDirectory))
                {
                    throw new InvalidDataException(
                        $"В архиве не найден каталог обновления: {sourceDirectory}");
                }

                bool fullUpdate = updateKind == UpdateKind.Full;

                // Удаляем резервные копии предыдущего запуска до создания новых.
                Cleanup(request.PackageDirectory);

                try
                {
                    if (!Installer.MoveDirectoryFilesWithBackup(
                        sourceDirectory,
                        request.PackageDirectory,
                        fullUpdate))
                    {
                        throw new IOException("Не удалось установить пакет обновления.");
                    }
                }
                catch (UpdateRollbackException ex)
                {
                    // Откат не завершён: логируем обе первичные ошибки и не повторяем установку.
                    _logger.Error(ex.InstallException);
                    _logger.Error(ex.RollbackException);

                    _messageServices.WarningMessage(
                        $"Не удалось установить обновление {update.Version.ToVersion()}, и восстановление завершилось ошибкой. " +
                        $"Аддон может находиться в невалидном состоянии. Скачайте полный релиз и распакуйте его с заменой файлов. " +
                        $"Открыть страницу релиза?");

                    OpenReleaseIfRequested(request.UpdateUrl, update.Tag);
                    return false;
                }
                catch (Exception installException)
                {
                    // Installer выбросил исходную ошибку только после успешного отката.
                    _logger.Error(installException);

                    _messageServices.WarningMessage(
                        $"Не удалось установить обновление {update.Version.ToVersion()}; прежнее состояние восстановлено. " +
                        $"Попробуйте повторить обновление после перезапуска nanoCAD или скачайте релиз вручную. " +
                        $"Открыть страницу релиза?");

                    OpenReleaseIfRequested(request.UpdateUrl, update.Tag);
                    return false;
                }

                if (installAutomatically)//todo не уверен, что здесь нужно условие
                {
                    _messageServices.InfoMessage(
                        $"{update.Product} обновлен с версии {request.CurrentVersion.ToString()} до версии {update.Version.ToVersion()}\n Что бы изменения вступили в силу необходима перезагрузка.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
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
        /// Предлагает открыть страницу опубликованного релиза и открывает её при подтверждении.
        /// </summary>
        private void OpenReleaseIfRequested(string updateUrl, string tag)//todo вынести в инфраструктуру??
        {
            string releaseUrl = BuildReleaseUrl(updateUrl, tag);
            MessageResult result = _promptService.AskYesNo(
                $"Открыть страницу релиза?\n{releaseUrl}",
                "Восстановление обновления");

            if (result != MessageResult.Yes)
                return;

            try
            {
                Process.Start(new ProcessStartInfo(releaseUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _messageServices.WarningMessage(
                    $"Не удалось открыть страницу релиза автоматически. Откройте ссылку вручную: {releaseUrl}");
            }
        }

        /// <summary>
        /// Преобразует URL каталога загрузки в URL страницы конкретного релиза.
        /// </summary>
        private static string BuildReleaseUrl(string updateUrl, string tag)
        {
            if (Uri.TryCreate(updateUrl, UriKind.Absolute, out Uri? uri))
            {
                const string downloadSuffix = "/releases/latest/download/";
                string path = uri.AbsolutePath;

                if (path.EndsWith(downloadSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    string repositoryPath = path.Substring(0, path.Length - downloadSuffix.Length);
                    return new UriBuilder(uri)
                    {
                        Path = repositoryPath + "/releases/tag/" + Uri.EscapeDataString(tag),
                        Query = string.Empty,
                        Fragment = string.Empty
                    }.Uri.ToString();
                }
            }

            // Если формат URL нестандартный, хотя бы предлагаем исходный URL источника обновлений.
            return updateUrl;
        }

        /// <summary>
        /// Возвращает описание защищённого пакета, если оно задано.
        /// </summary>
        private static ProtectedPackageInfo? GetProtected(UpdateInfo update)
        {
            return update.Protected;
        }

        private static string GetAsset(UpdateInfo update)
        {
            return GetProtected(update)?.Asset ?? update.Asset;
        }

        private static string GetAssetName(UpdateInfo update)
        {
            return GetAsset(update);
        }

        private static long GetAssetSize(UpdateInfo update)
        {
            return GetProtected(update)?.Size ?? update.Size;
        }

        private static string GetAssetSha256(UpdateInfo update)
        {
            return GetProtected(update)?.Sha256 ?? update.Sha256;
        }

        private static string? GetAssetPassword(UpdateInfo update)
        {
            return GetProtected(update)?.Password;
        }

        /// <summary>
        /// Формирует URL ресурса относительно базового URL обновлений.
        /// </summary>
        private static string ResolveUrl(string baseUrl, string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                throw new InvalidDataException("В update.json не указан ресурс.");

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
                throw new InvalidDataException("Некорректный базовый URL обновлений.");

            if (Uri.TryCreate(resource, UriKind.Absolute, out Uri? absoluteUri))
                return absoluteUri.ToString();

            string normalizedBaseUrl = baseUri.AbsoluteUri.EndsWith(
                "/",
                StringComparison.Ordinal)
                ? baseUri.AbsoluteUri
                : baseUri.AbsoluteUri + "/";

            return new Uri(new Uri(normalizedBaseUrl), resource).ToString();
        }

        /// <summary>
        /// Проверяет обязательные параметры запроса.
        /// </summary>
        private static void ValidateRequest(UpdateRequest request)
        {
            if (request.CurrentVersion is null)
                throw new ArgumentException("Не указана текущая версия.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.UpdateUrl))
                throw new ArgumentException("Не указан базовый URL обновлений.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.PackageDirectory))
                throw new ArgumentException("Не указан каталог аддона.", nameof(request));
        }
    }
}