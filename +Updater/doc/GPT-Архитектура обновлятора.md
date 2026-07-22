# Архитектура обновлятора

**User:** dRz  
**Created:** 7/17/2026 17:30:02  
**Updated:** 7/18/2026 10:49:25  
**Exported:** 7/19/2026 19:12:28  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5a3c2b-ed6c-83eb-a1ae-59223a9de21b](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5a3c2b-ed6c-83eb-a1ae-59223a9de21b)  

## Prompt:
17.07.2026, 17:30:02

Скачивание, распаковку, копирование сделали
Логично написать класс который будет в обновляторе всем этим управлять?

## Response:
17.07.2026, 17:30:02

Да. Более того, это будет центральный класс обновлятора, а остальные классы должны выполнять только одну задачу.

Я бы построил архитектуру примерно так:

```text
UpdateManager
    │
    ├── GitHubDownloader      // скачать архив
    ├── ArchiveExtractor      // распаковать
    ├── BackupService         // переименовать в .bak
    ├── FileInstaller         // скопировать новые файлы
    ├── RollbackService       // откат при ошибке
    ├── CleanupService        // удалить .bak и пустые папки
    └── UpdateLogger          // журнал операций
```

Тогда `UpdateManager` будет выглядеть очень просто:

```csharp
public bool Install(UpdatePackage package)
{
    _downloader.Download(package);

    _extractor.Extract();

    _backupService.BackupFiles();

    _installer.InstallFiles();

    _cleanup.Cleanup();

    return true;
}
```

Если возникнет ошибка:

```csharp
try
{
    _downloader.Download(package);
    _extractor.Extract();
    _backupService.BackupFiles();
    _installer.InstallFiles();
    _cleanup.Commit();
}
catch
{
    _rollback.Rollback();
    throw;
}
```

Я бы еще ввел класс состояния обновления:

```csharp
UpdateContext
```

Он будет хранить все пути и общую информацию:

```csharp
public sealed class UpdateContext
{
    public string AddonDirectory { get; init; }
    public string TempDirectory { get; init; }
    public string ZipFile { get; init; }
    public string ExtractDirectory { get; init; }
    public string BackupLogFile { get; init; }
}
```

Тогда все сервисы будут принимать один параметр:

```csharp
_backupService.Backup(context);
_installer.Install(context);
_cleanup.Cleanup(context);
```

В результате код получится легко расширяемым. Если позже добавишь проверку SHA256, цифровую подпись, проверку версии, загрузку с другого сервера или уведомления пользователю, они просто станут новыми сервисами, а `UpdateManager` останется практически неизменным. Для твоего обновлятора это, на мой взгляд, наиболее удачная архитектура.

## Prompt:
17.07.2026, 17:33:19

Есть смысл передавать Simpleinjector addoninfo, logger, messageservice в updatemeneger, чтобы он получил из addoninfo пути, версию, продукт, вел лог, писал сообщения

## Response:
17.07.2026, 17:33:19

Да, но я бы передавал **не сам контейнер SimpleInjector**, а уже готовые зависимости через конструктор.

Например:

```csharp
public UpdateManager(
    IAddonInfo addonInfo,
    ILogger logger,
    IMessageService messageService)
{
    _addonInfo = addonInfo;
    _logger = logger;
    _messageService = messageService;
}
```

Это дает несколько преимуществ:

- `IAddonInfo`
  - путь к аддону;
  - название продукта;
  - текущая версия;
  - возможно, GitHub-репозиторий или URL обновлений.

- `ILogger`
  - логирование каждого этапа;
  - запись ошибок;
  - диагностика отката.

- `IMessageService`
  - показать "Найдена новая версия";
  - спросить подтверждение;
  - вывести сообщение об успешном обновлении или ошибке.

А вот передавать сам `Container` я бы не стал. Тогда `UpdateManager` начинает сам искать зависимости:

```csharp
container.GetInstance<ILogger>();
container.GetInstance<IMessageService>();
```

Это превращается в Service Locator, что усложняет тестирование и делает зависимости неявными.

### Я бы даже пошел немного дальше

Сделал бы отдельный класс контекста обновления:

```csharp
public sealed class UpdateContext
{
    public IAddonInfo AddonInfo { get; }
    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    public string AddonDirectory => AddonInfo.AssemblyDirectory;
    public string ProductName => AddonInfo.ProductName;
    public Version Version => AddonInfo.Version;
}
```

Тогда все сервисы (`Downloader`, `BackupService`, `Installer` и т.д.) будут получать только `UpdateContext`, а не по три-пять параметров.

Для твоего проекта, где обновлятор является частью аддона и тесно связан с `IAddonInfo`, `ILogger` и `IMessageService`, такой подход выглядит наиболее естественным.

## Prompt:
17.07.2026, 17:35:31

public sealed class UpdateContext
{
    public IAddonInfo AddonInfo { get; }
    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    public string AddonDirectory => AddonInfo.AssemblyDirectory;
    public string ProductName => AddonInfo.ProductName;
    public Version Version => AddonInfo.Version;
}
Покажи как заполнить данными

## Response:
17.07.2026, 17:35:32

Я бы сделал `UpdateContext` неизменяемым и заполнял его через конструктор.

```csharp
public sealed class UpdateContext
{
    public UpdateContext(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        AddonInfo = addonInfo;
        Logger = logger;
        MessageService = messageService;
    }

    public IAddonInfo AddonInfo { get; }

    public ILogger Logger { get; }

    public IMessageService MessageService { get; }

    public string AddonDirectory => AddonInfo.AssemblyDirectory;
    public string ProductName => AddonInfo.ProductName;
    public Version Version => AddonInfo.Version;
}
```

Создание через SimpleInjector будет выглядеть так:

```csharp
var context = new UpdateContext(
    container.GetInstance<IAddonInfo>(),
    container.GetInstance<ILogger>(),
    container.GetInstance<IMessageService>());
```

или, если `UpdateManager` тоже создается контейнером, то еще проще:

```csharp
public sealed class UpdateManager
{
    private readonly UpdateContext _context;

    public UpdateManager(UpdateContext context)
    {
        _context = context;
    }
}
```

А `UpdateContext` зарегистрировать в контейнере:

```csharp
container.RegisterSingleton(() =>
    new UpdateContext(
        container.GetInstance<IAddonInfo>(),
        container.GetInstance<ILogger>(),
        container.GetInstance<IMessageService>()));
```

### Но я бы сделал еще лучше

Поскольку `UpdateContext` — это не только сервисы, но и рабочие данные обновления, я бы добавил туда поля, которые вычисляются один раз:

```csharp
public sealed class UpdateContext
{
    public UpdateContext(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        AddonInfo = addonInfo;
        Logger = logger;
        MessageService = messageService;

        AddonDirectory = addonInfo.AssemblyDirectory;
        ProductName = addonInfo.ProductName;
        Version = addonInfo.Version;

        TempDirectory = Path.Combine(AddonDirectory, "_update");
        BackupDirectory = Path.Combine(AddonDirectory, "_backup");
    }

    public IAddonInfo AddonInfo { get; }
    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    public string AddonDirectory { get; }
    public string ProductName { get; }
    public Version Version { get; }

    public string TempDirectory { get; }
    public string BackupDirectory { get; }

    public string? PackageFile { get; set; }
    public string? ExtractDirectory { get; set; }
}
```

Тогда каждый этап обновления может дополнять контекст:

```csharp
_context.PackageFile = downloadedZip;
_context.ExtractDirectory = extractPath;
```

И все последующие сервисы будут использовать уже готовые данные, не вычисляя и не передавая множество отдельных параметров. Для цепочки `скачал → распаковал → установил → очистил` это очень удобный подход.

## Prompt:
17.07.2026, 17:38:39

UpdateContext передавать дальше другим сервисам?

## Response:
17.07.2026, 17:38:39

Да, но с оговоркой.

Если все сервисы относятся **только к обновлению**, то передавать `UpdateContext` вполне логично.

Например:

```csharp
_downloader.Download(context);
_extractor.Extract(context);
_backupService.Backup(context);
_installer.Install(context);
_cleanup.Cleanup(context);
```

Каждый сервис берет из контекста только то, что ему нужно:

```csharp
public void Backup(UpdateContext context)
{
    context.Logger.Info("Создание резервных копий...");

    foreach (var file in ...)
    {
        ...
    }
}
```

Но я бы не превращал `UpdateContext` в "мешок со всем подряд". Если увидишь, что там уже 30–40 свойств, лучше выделить отдельные классы, например:

```text
UpdateContext
 ├── IAddonInfo
 ├── ILogger
 ├── IMessageService
 └── UpdateState
```

где `UpdateState` содержит изменяемые данные процесса:

```csharp
public sealed class UpdateState
{
    public string? ZipFile { get; set; }
    public string? ExtractDirectory { get; set; }
    public List<BackupEntry> Backups { get; } = [];
}
```

Тогда:
- `UpdateContext` — практически неизменяемый, содержит сервисы и информацию об аддоне.
- `UpdateState` — изменяется по мере выполнения обновления.

Для твоего обновлятора, который представляет собой последовательность этапов, такой подход будет хорошо масштабироваться и упростит поддержку.

## Prompt:
17.07.2026, 17:40:05

Т.е. к обновлятору подключаю только интерфейсы?

## Response:
17.07.2026, 17:40:05

Да, именно так. Это один из главных плюсов DI.

Например:

```csharp
public sealed class UpdateManager
{
    public UpdateManager(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService,
        IDownloader downloader,
        IExtractor extractor,
        IBackupService backupService,
        IInstaller installer,
        IRollbackService rollbackService)
    {
        ...
    }
}
```

А в `CompositionRoot`:

```csharp
container.RegisterSingleton<IDownloader, GitHubDownloader>();
container.RegisterSingleton<IExtractor, SevenZipExtractor>();
container.RegisterSingleton<IBackupService, BackupService>();
container.RegisterSingleton<IInstaller, FileInstaller>();
container.RegisterSingleton<IRollbackService, RollbackService>();

container.RegisterSingleton<UpdateManager>();
```

После этого достаточно получить:

```csharp
var updater = container.GetInstance<UpdateManager>();
```

и SimpleInjector сам создаст всю цепочку зависимостей.

### Но в твоем случае я бы сделал еще проще

Поскольку обновлятор — это библиотека, которая используется только твоими аддонами, нет особого смысла делать интерфейс для каждого внутреннего класса (`IBackupService`, `IInstaller`, `IRollbackService` и т.д.), если ты не планируешь иметь несколько реализаций.

Я бы оставил интерфейсы только для внешних зависимостей:

- ✅ `IAddonInfo`
- ✅ `ILogger`
- ✅ `IMessageService`

А внутренние классы обновлятора сделал обычными:

```csharp
BackupService
FileInstaller
RollbackService
CleanupService
ArchiveExtractor
GitHubDownloader
```

Их тоже можно регистрировать в SimpleInjector, но без интерфейсов:

```csharp
container.RegisterSingleton<BackupService>();
container.RegisterSingleton<FileInstaller>();
container.RegisterSingleton<ArchiveExtractor>();
container.RegisterSingleton<UpdateManager>();
```

Такой вариант проще, код чище, и ты не создаешь десяток интерфейсов, которые никогда не будут иметь второй реализации. Интерфейсы имеет смысл вводить там, где действительно ожидается возможность замены реализации или где они являются частью публичного API библиотеки.

## Prompt:
17.07.2026, 17:41:18

Симплеинжектор необходим?

## Response:
17.07.2026, 17:41:19

Для твоего обновлятора — **нет, не необходим**.

Судя по тому, что мы обсуждали, обновлятор — это небольшой модуль с фиксированным набором классов. Если их немного, то проще создать их вручную.

Например:

```csharp
var context = new UpdateContext(addonInfo, logger, messageService);

var downloader = new GitHubDownloader();
var extractor = new ArchiveExtractor();
var backup = new BackupService();
var installer = new FileInstaller();

var updater = new UpdateManager(
    context,
    downloader,
    extractor,
    backup,
    installer);
```

Или даже так:

```csharp
var updater = new UpdateManager(context);
updater.Install();
```

А `UpdateManager` сам создаст внутренние сервисы:

```csharp
public UpdateManager(UpdateContext context)
{
    _context = context;

    _downloader = new GitHubDownloader();
    _extractor = new ArchiveExtractor();
    _backup = new BackupService();
    _installer = new FileInstaller();
}
```

### Когда SimpleInjector имеет смысл

Если обновлятор — часть большого приложения, где уже используется DI (как у тебя в аддоне), то можно зарегистрировать `UpdateManager` в контейнере и получать его оттуда. Это дает единый стиль создания объектов.

### Что бы я выбрал в твоем проекте

- **Внутри обновлятора** — без SimpleInjector. Это самодостаточная библиотека.
- **На входе** — передать только зависимости извне (`IAddonInfo`, `ILogger`, `IMessageService`) или готовый `UpdateContext`.
- Все внутренние классы (`GitHubDownloader`, `ArchiveExtractor`, `BackupService`, `FileInstaller` и т.д.) создавать обычным `new`.

Так обновлятор не зависит от какого-либо DI-контейнера и может использоваться в любом проекте, даже если там нет SimpleInjector. Это делает библиотеку более независимой и переносимой.

## Prompt:
17.07.2026, 17:43:59

готовый UpdateContext.
Т. Е наполнять его в аддоне?

## Response:
17.07.2026, 17:44:00

Да, именно так. Я считаю, что это наиболее удачный вариант для твоего случая.

Аддон знает всё о себе:
- `IAddonInfo`;
- `ILogger`;
- `IMessageService`;
- настройки обновления;
- при необходимости токен GitHub, URL репозитория и т.д.

Он создает `UpdateContext` и передает его в обновлятор.

Например:

```csharp
var context = new UpdateContext(
    addonInfo,
    logger,
    messageService);

var updater = new UpdateManager(context);
updater.Install();
```

При этом `UpdateManager` вообще ничего не знает ни о SimpleInjector, ни о том, откуда взялись эти объекты.

### Я бы даже разделил контекст на две части

1. **Неизменяемые данные**, которые заполняет аддон:

```csharp
var context = new UpdateContext(
    addonInfo,
    logger,
    messageService);
```

2. **Рабочие данные**, которые заполняет уже обновлятор:

```csharp
context.PackageFile = zipPath;
context.ExtractDirectory = tempDir;
context.Backups.Add(...);
```

То есть ответственность будет разделена:

- **Аддон** — создает и инициализирует `UpdateContext`.
- **Обновлятор** — использует его и дополняет информацией о ходе обновления.

Такое разделение хорошо соответствует архитектуре, которую ты строишь: обновлятор остается независимой библиотекой, а интеграция с аддоном сводится к передаче одного подготовленного объекта.

## Prompt:
17.07.2026, 17:46:46

var context = new UpdateContext(
    addonInfo,
    logger,
    messageService);
Гдереализация класса?

## Response:
17.07.2026, 17:46:47

Вот так я бы реализовал `UpdateContext`.

```csharp
public sealed class UpdateContext
{
    public UpdateContext(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        AddonInfo = addonInfo ?? throw new ArgumentNullException(nameof(addonInfo));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MessageService = messageService ?? throw new ArgumentNullException(nameof(messageService));

        AddonDirectory = addonInfo.AssemblyDirectory;
        ProductName = addonInfo.ProductName;
        Version = addonInfo.Version;
    }

    // Внешние сервисы
    public IAddonInfo AddonInfo { get; }
    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    // Данные аддона
    public string AddonDirectory { get; }
    public string ProductName { get; }
    public Version Version { get; }

    // Рабочие данные обновления
    public string? DownloadFile { get; set; }
    public string? ExtractDirectory { get; set; }

    // При необходимости можно добавить
    // public List<BackupEntry> Backups { get; } = new();
}
```

Использование:

```csharp
var context = new UpdateContext(
    addonInfo,
    logger,
    messageService);

var updater = new UpdateManager(context);
updater.Install();
```

Или, если используешь SimpleInjector:

```csharp
var context = new UpdateContext(
    container.GetInstance<IAddonInfo>(),
    container.GetInstance<ILogger>(),
    container.GetInstance<IMessageService>());
```

Однако я бы предложил еще один вариант, который, как мне кажется, лучше подходит твоей архитектуре. Не передавать в `UpdateContext` сами сервисы (`ILogger`, `IMessageService`), а сделать его **контекстом данных**, например:

```csharp
public sealed class UpdateContext
{
    public string AddonDirectory { get; init; }
    public string ProductName { get; init; }
    public Version Version { get; init; }

    public string? DownloadFile { get; set; }
    public string? ExtractDirectory { get; set; }
}
```

А `ILogger` и `IMessageService` передавать непосредственно в `UpdateManager`. Тогда контекст содержит только данные, а не сервисы, и его назначение становится более однозначным. Для долгосрочной поддержки такой вариант обычно оказывается проще.

## Prompt:
17.07.2026, 17:47:49

В каком модуле реализация класса?

## Response:
17.07.2026, 17:47:50

Я бы разместил `UpdateContext` **в библиотеке обновлятора**, а не в аддоне.

Например:

```
dRz.Update
│
├── UpdateManager.cs
├── UpdateContext.cs   ← здесь
├── GitHubDownloader.cs
├── ArchiveExtractor.cs
├── BackupService.cs
├── FileInstaller.cs
└── ...
```

Тогда аддон знает только публичный API обновлятора:

```csharp
using dRz.Update;

var context = new UpdateContext(
    addonInfo,
    logger,
    messageService);

var updater = new UpdateManager(context);
updater.Install();
```

Это логично, потому что `UpdateContext` является частью API обновлятора. Аддон лишь создает и заполняет его, а обновлятор использует.

Единственное условие — библиотека обновлятора должна иметь ссылки на сборки, где объявлены `IAddonInfo`, `ILogger` и `IMessageService`. Поскольку ты и так используешь эти интерфейсы во всех аддонах, это не должно быть проблемой.

Такое разделение делает ответственность понятной:
- **аддон** — предоставляет зависимости;
- **обновлятор** — определяет, какие данные ему нужны, через свой `UpdateContext`.

## Prompt:
17.07.2026, 17:48:38

Есть смысл сделать интерфейс?

## Response:
17.07.2026, 17:48:38

В данном случае **нет**.

`UpdateContext` — это обычный объект передачи данных (DTO/Context). Обычно для таких классов интерфейс не делают.

То есть:

```csharp
public sealed class UpdateContext
{
    ...
}
```

лучше, чем

```csharp
public interface IUpdateContext
{
    ...
}

public sealed class UpdateContext : IUpdateContext
{
    ...
}
```

Причины:

- реализация, скорее всего, всегда будет одна;
- подменять `UpdateContext` нет необходимости;
- интерфейс только усложнит API.

### Когда интерфейс стоит делать

Если предполагается несколько источников данных, например:

- `UpdateContext` — для обычного обновления;
- `TestUpdateContext` — для тестов;
- `RemoteUpdateContext` — данные приходят из другого процесса.

Тогда `IUpdateContext` имеет смысл.

### Что я бы сделал в твоем проекте

Оставил бы без интерфейса:

```text
dRz.Update
 ├── UpdateManager
 ├── UpdateContext   ← sealed
 ├── UpdateResult
 ├── GitHubDownloader
 ├── BackupService
 └── ...
```

А интерфейсы использовал бы только для **внешних зависимостей**, которые предоставляет аддон:

- `IAddonInfo`
- `ILogger`
- `IMessageService`

Это соответствует твоей архитектуре: обновлятор получает через интерфейсы то, что зависит от хоста, а собственные классы (`UpdateContext`, `UpdateManager`, `UpdateResult`) остаются обычными конкретными типами.

## Prompt:
17.07.2026, 17:49:58

UpdateContext это сервис, инфраструктура, другое

## Response:
17.07.2026, 17:49:58

`UpdateContext` — **не сервис** и **не инфраструктура**.

По сути это **контекст выполнения операции** (Operation Context), то есть объект, который содержит данные, необходимые для выполнения обновления.

Я бы отнес его к отдельной папке:

```text
dRz.Update
│
├── Context/
│   └── UpdateContext.cs
│
├── Services/
│   ├── GitHubDownloader.cs
│   ├── ArchiveExtractor.cs
│   ├── BackupService.cs
│   └── ...
│
├── Models/
│   ├── UpdateInfo.cs
│   └── ReleaseInfo.cs
│
└── UpdateManager.cs
```

или проще:

```text
dRz.Update
├── Contexts/
│   └── UpdateContext.cs
```

### Чем он отличается от других типов

- **Сервис** — выполняет действия.
  - `GitHubDownloader`
  - `ArchiveExtractor`
  - `BackupService`

- **Модель (Model)** — описывает предметные данные.
  - `ReleaseInfo`
  - `UpdateInfo`

- **Контекст (Context)** — хранит состояние и зависимости, необходимые для выполнения конкретной операции.
  - `UpdateContext`

- **Инфраструктура (Infrastructure)** — общие технические компоненты.
  - `HttpClientFactory`
  - `FileSystem`
  - `Sha256Helper`

Поэтому `UpdateContext` лучше рассматривать как **контекст операции обновления**, а не как сервис или инфраструктурный класс. Такое название и назначение сразу понятно любому, кто будет читать код.

## Prompt:
17.07.2026, 17:51:12

Enum контекст?

## Response:
17.07.2026, 17:51:12

Нет. `Context` — это не сокращение от `enum` и не имеет к перечислениям никакого отношения.

В программировании **Context** означает **контекст**, то есть объект, содержащий окружение или состояние, необходимое для выполнения операции.

Примеры из .NET:

- `DbContext` (Entity Framework)
- `HttpContext` (ASP.NET)
- `SynchronizationContext`
- `ExecutionContext`

Поэтому `UpdateContext` — вполне стандартное и понятное название.

Если слово `Context` тебе не нравится, можно использовать другие варианты:

- `UpdateSession` — если обновление рассматривается как сеанс.
- `UpdateEnvironment` — если объект описывает окружение.
- `UpdateOptions` — если содержит только настройки.
- `UpdateParameters` — если это просто набор входных параметров.
- `UpdateData` — если это исключительно данные.

Для твоего случая, где объект содержит:
- информацию об аддоне;
- пути;
- логгер;
- сервис сообщений;
- рабочее состояние обновления,

название **`UpdateContext`** подходит лучше всего и соответствует общепринятой практике в .NET.

## Prompt:
17.07.2026, 18:56:17

UpdateContext в конструктор передавать все, а в нем уже выбирать нужное и выносить в паблик

## Response:
17.07.2026, 18:56:17

Да, это хороший подход. Я бы именно так и сделал.

Например:

```csharp
public sealed class UpdateContext
{
    public UpdateContext(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        AddonInfo = addonInfo;
        Logger = logger;
        MessageService = messageService;

        // Копируем нужные данные
        ProductName = addonInfo.ProductName;
        Version = addonInfo.Version;
        AddonDirectory = addonInfo.AssemblyDirectory;
        AssemblyFile = addonInfo.AssemblyFile;
    }

    // При необходимости можно оставить исходные сервисы
    public IAddonInfo AddonInfo { get; }
    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    // Данные, используемые обновлятором
    public string ProductName { get; }
    public Version Version { get; }
    public string AddonDirectory { get; }
    public string AssemblyFile { get; }

    // Рабочие данные
    public string? DownloadFile { get; set; }
    public string? ExtractDirectory { get; set; }
}
```

Но если обновлятору **не нужен** сам `IAddonInfo`, то лучше его вообще не хранить:

```csharp
public sealed class UpdateContext
{
    public UpdateContext(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        Logger = logger;
        MessageService = messageService;

        ProductName = addonInfo.ProductName;
        Version = addonInfo.Version;
        AddonDirectory = addonInfo.AssemblyDirectory;
        AssemblyFile = addonInfo.AssemblyFile;
    }

    public ILogger Logger { get; }
    public IMessageService MessageService { get; }

    public string ProductName { get; }
    public Version Version { get; }
    public string AddonDirectory { get; }
    public string AssemblyFile { get; }

    public string? DownloadFile { get; set; }
    public string? ExtractDirectory { get; set; }
}
```

Так `UpdateContext` становится **независимым от аддона**. Он берет из `IAddonInfo` только нужные значения и дальше работает уже со своими свойствами. Если завтра изменится `IAddonInfo`, то менять придется только конструктор `UpdateContext`, а не весь обновлятор. Это делает архитектуру более устойчивой к изменениям.

## Prompt:
17.07.2026, 18:57:56

Передача значений по ссылке?

## Response:
17.07.2026, 18:57:56

В C# передача зависит от типа данных.

В твоем случае:

```csharp
public UpdateContext(IAddonInfo addonInfo, ILogger logger, IMessageService messageService)
```

параметры передаются **по значению**, но сами значения являются **ссылками на объекты**.

То есть:

```csharp
AddonInfo = addonInfo;
```

не копирует объект `IAddonInfo`, а копирует ссылку на него.

Схема:

```
Аддон

addonInfo  ─────────────┐
                        ▼
                  InfoAddOn объект
                        ▲
                        │
UpdateContext            │
AddonInfo ───────────────┘
```

Если внутри `UpdateContext` сделать:

```csharp
AddonInfo.ProductName = "NewName";
```

(если свойство доступно для записи), изменится исходный объект в аддоне.

А если сделать:

```csharp
ProductName = addonInfo.ProductName;
```

то это уже копия значения:

```csharp
string ProductName
Version Version
```

Например:

```csharp
ProductName = addonInfo.ProductName;
```

`string` — ссылочный тип, но строки неизменяемые (`immutable`), поэтому фактически изменение невозможно.

Для `Version` аналогично — ты просто сохраняешь ссылку на объект `Version`.

Если хочешь полностью отделить обновлятор от аддона, можно сделать копию простых данных:

```csharp
public string ProductName { get; }
public Version Version { get; }

public UpdateContext(IAddonInfo addonInfo)
{
    ProductName = string.Copy(addonInfo.ProductName);
    Version = new Version(addonInfo.Version.ToString());
}
```

Но обычно для `Version`, `string` и путей это излишне.

Для твоего случая я бы сделал так:
- `IAddonInfo`, `ILogger`, `IMessageService` — хранить как ссылки;
- пути, имя продукта, версию — вынести отдельными свойствами;
- изменяемое состояние обновления (`ZipFile`, `BackupList`) — хранить в `UpdateContext`.

То есть `UpdateContext` будет "снимком" параметров запуска + состояние текущего обновления.

## Prompt:
18.07.2026, 07:57:44

Покажи как может выглядеть запуск обновляторв из аддона

## Response:
18.07.2026, 07:57:44

Я бы сделал запуск максимально простым. Код аддона не должен знать, как устроен обновлятор.

Например, команда обновления:

```csharp
public void CheckForUpdates()
{
    var context = new UpdateContext(
        addonInfo: _addonInfo,
        logger: _logger,
        messageService: _messageService);

    var updater = new UpdateManager(context);

    updater.CheckForUpdates();
}
```

Или даже так, если `UpdateManager` сам умеет и проверять, и устанавливать:

```csharp
public void Update()
{
    var context = new UpdateContext(
        _addonInfo,
        _logger,
        _messageService);

    new UpdateManager(context).Run();
}
```

---

Я бы вообще сделал для библиотеки обновления фасад:

```csharp
public static class Updater
{
    public static void Run(UpdateContext context)
    {
        var manager = new UpdateManager(context);
        manager.Run();
    }
}
```

Тогда в аддоне будет буквально три строки:

```csharp
var context = new UpdateContext(
    _addonInfo,
    _logger,
    _messageService);

Updater.Run(context);
```

Это, на мой взгляд, наиболее удачный API:

- аддон создает только `UpdateContext`;
- вызывает один метод;
- вся остальная логика скрыта внутри библиотеки обновления.

Такой интерфейс практически не придется менять, даже если внутри появятся новые классы (`GitHubDownloader`, `RollbackService`, проверка SHA, журнал транзакций и т.д.).

## Prompt:
18.07.2026, 08:00:44

UpdateContext в обновляторе, может сразу передавать параметры в run, а он контекст сам сформирует?

## Response:
18.07.2026, 08:00:45

Да, мне этот вариант нравится даже больше.

Если `UpdateContext` используется **только внутри обновлятора**, то незачем заставлять аддон создавать его.

Тогда API библиотеки получится очень простым:

```csharp
Updater.Run(
    addonInfo,
    logger,
    messageService);
```

А внутри:

```csharp
public static class Updater
{
    public static void Run(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        var context = new UpdateContext(
            addonInfo,
            logger,
            messageService);

        var manager = new UpdateManager(context);
        manager.Run();
    }
}
```

Или вообще без отдельного класса `Updater`:

```csharp
UpdateManager.Run(
    addonInfo,
    logger,
    messageService);
```

где `Run` — статический метод.

### Плюсы такого подхода

- API обновлятора минимален.
- `UpdateContext` полностью скрыт от пользователя библиотеки.
- В любой момент можно изменить структуру `UpdateContext`, не затрагивая код аддонов.
- Аддон знает только три интерфейса, которые у него уже есть.

Для твоего проекта я бы выбрал именно этот вариант. `UpdateContext` — это внутренний класс обновлятора, а публичный API состоит из одного метода `Run(...)` (или `CheckForUpdates(...)`) с несколькими понятными параметрами. Это самый чистый и устойчивый дизайн.

## Prompt:
18.07.2026, 10:20:25

где Run — статический метод
Т. Е он будет хранить в классе полученные параметры?
Другие методы тоже статик?

## Response:
18.07.2026, 10:20:25

Нет. Я бы сделал так:

`Run()` — **статический только как точка входа**. Он **не хранит** параметры в статических полях.

```csharp
public sealed class UpdateManager
{
    private readonly UpdateContext _context;

    private UpdateManager(UpdateContext context)
    {
        _context = context;
    }

    public static void Run(
        IAddonInfo addonInfo,
        ILogger logger,
        IMessageService messageService)
    {
        var context = new UpdateContext(
            addonInfo,
            logger,
            messageService);

        var manager = new UpdateManager(context);
        manager.Run();
    }

    private void Run()
    {
        CheckForUpdates();
        Download();
        Install();
    }

    private void CheckForUpdates() { }
    private void Download() { }
    private void Install() { }
}
```

Получается:

- `Run(...)` — **статический**, создает `UpdateContext` и экземпляр `UpdateManager`.
- Все остальные методы — **обычные экземплярные**.
- Все они работают через `_context`.

Я **не рекомендую** делать так:

```csharp
private static UpdateContext _context;
```

или

```csharp
private static ILogger _logger;
```

Это создает глобальное состояние. Если одновременно будут работать два обновления (или в будущем появится многопоточность), начнутся проблемы.

Поэтому оптимальная схема для твоего проекта:

```text
UpdateManager.Run(...)   // static
        │
        ▼
создает UpdateContext
        │
        ▼
создает UpdateManager
        │
        ▼
Run()                 // экземплярный
 ├── Check()
 ├── Download()
 ├── Backup()
 ├── Install()
 └── Cleanup()
```

Такой подход сочетает удобный вызов из аддона и отсутствие статического состояния внутри обновлятора.

## Prompt:
18.07.2026, 10:41:34

И каждому методу передаем параметры нужные только ему?

## Response:
18.07.2026, 10:41:35

Да, это даже лучше, чем хранить всё в `UpdateManager`.

Например:

```csharp
private void Download(UpdateContext context)
{
    ...
}

private void Extract(UpdateContext context)
{
    ...
}

private void Backup(UpdateContext context)
{
    ...
}
```

Но еще лучше — передавать **не весь `UpdateContext`**, а только то, что действительно нужно методу.

Например:

```csharp
private void Download(
    string url,
    string downloadFile)
{
    ...
}

private void Extract(
    string archiveFile,
    string extractDirectory)
{
    ...
}

private void Backup(
    string addonDirectory)
{
    ...
}
```

### Какой подход выбрать?

Для твоего обновлятора я бы использовал оба:

- **между классами** (`UpdateManager` → `GitHubDownloader`, `BackupService`, `FileInstaller`) передавать минимально необходимые параметры;
- **внутри `UpdateManager`** использовать `UpdateContext`, чтобы не таскать десяток параметров между собственными методами.

Например:

```csharp
public void Run()
{
    CheckForUpdates(_context);
    Download(_context);
    Extract(_context);
    Install(_context);
}
```

А если вынесешь загрузку в отдельный класс:

```csharp
_downloader.Download(
    release.Url,
    _context.DownloadFile);

_extractor.Extract(
    _context.DownloadFile,
    _context.ExtractDirectory);
```

То есть `UpdateContext` — это внутреннее состояние процесса обновления, а отдельным сервисам лучше передавать только те данные, которые им действительно нужны. Так классы становятся менее связанными и их проще повторно использовать и тестировать.

## Prompt:
18.07.2026, 10:45:10

public void Run()
{
    CheckForUpdates(_context);
    Download(_context);
    Extract(_context);
    Install(_context);
}
Т. Е методы статика?

## Response:
18.07.2026, 10:45:11

Нет. В этом примере `Run()` и все остальные методы — **обычные экземплярные**.

```csharp
public sealed class UpdateManager
{
    private readonly UpdateContext _context;

    public UpdateManager(UpdateContext context)
    {
        _context = context;
    }

    public void Run()
    {
        CheckForUpdates();
        Download();
        Extract();
        Install();
    }

    private void CheckForUpdates()
    {
        // использует _context
    }

    private void Download()
    {
        // использует _context
    }

    private void Extract()
    {
        // использует _context
    }

    private void Install()
    {
        // использует _context
    }
}
```

Здесь вообще не нужно передавать `_context` — он уже является полем класса.

Если же методы сделать `static`, то придется передавать все параметры:

```csharp
private static void Download(UpdateContext context)
{
    ...
}
```

или

```csharp
private static void Download(
    ILogger logger,
    string url,
    string fileName)
{
    ...
}
```

Для твоего обновлятора я бы **не делал внутренние методы статическими**. Они работают с общим состоянием (`UpdateContext`), а значит экземпляр `UpdateManager` подходит лучше.

Итого:

- `UpdateManager.Run(...)` — **может быть статической точкой входа**.
- `new UpdateManager(context)` — создание экземпляра.
- `Run()`, `Download()`, `Install()`, `Rollback()` — **обычные методы экземпляра**, использующие `_context`. Это наиболее естественная архитектура для такого класса.

## Prompt:
18.07.2026, 10:47:12

проверкуа обновления, распаковка, перемещение файлов..
Создавать их экземпляры?

## Response:
18.07.2026, 10:47:12

Да, если это **отдельные классы**, то их экземпляры стоит создавать. Но необязательно через DI.

Например:

```csharp
public sealed class UpdateManager
{
    private readonly UpdateContext _context;

    private readonly UpdateChecker _checker;
    private readonly Downloader _downloader;
    private readonly ArchiveExtractor _extractor;
    private readonly FileInstaller _installer;

    public UpdateManager(UpdateContext context)
    {
        _context = context;

        _checker = new UpdateChecker();
        _downloader = new Downloader();
        _extractor = new ArchiveExtractor();
        _installer = new FileInstaller();
    }

    public void Run()
    {
        _checker.Check(_context);
        _downloader.Download(_context);
        _extractor.Extract(_context);
        _installer.Install(_context);
    }
}
```

Или вообще создавать их по месту использования:

```csharp
public void Run()
{
    new UpdateChecker().Check(_context);
    new Downloader().Download(_context);
    new ArchiveExtractor().Extract(_context);
    new FileInstaller().Install(_context);
}
```

### Что лучше?

Если классы **не имеют состояния** (они просто выполняют действие), то можно даже сделать их методы статическими:

```csharp
UpdateChecker.Check(_context);
Downloader.Download(_context);
ArchiveExtractor.Extract(_context);
```

Но если у класса есть внутреннее состояние, настройки или в будущем могут появиться поля, то лучше использовать обычные экземпляры.

### Для твоего проекта

Поскольку у тебя уже появляются:

- проверка версии;
- скачивание;
- резервное копирование;
- распаковка;
- установка;
- откат;
- очистка;

я бы сделал их обычными классами без `static` и создавал один раз в конструкторе `UpdateManager`. Это оставляет возможность позже добавить состояние или зависимости, не меняя архитектуру.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)