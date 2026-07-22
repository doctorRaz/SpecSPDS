# LogFactory доработка

**User:** dRz  
**Created:** 7/20/2026 16:43:23  
**Updated:** 7/21/2026 12:22:14  
**Exported:** 7/21/2026 12:27:30  
**Link:** [https://chatgpt.com/c/6a5e259c-e360-83eb-9316-6843d4193483](https://chatgpt.com/c/6a5e259c-e360-83eb-9316-6843d4193483)  

## Prompt:
20.07.2026, 16:43:23

В аддоне логер получаю через logbootstrap
Фабрику
Как получить этот же экземпляр логера в другой сборке, но со своим окружением?
Передавать фабрику?

## Response:
20.07.2026, 16:43:23

Да. Если другая сборка должна писать **в тот же логический контекст** (те же настройки, переменные, GDC, цели, файлы), то правильнее всего передавать **LogFactory**, а не готовый логер. Это и рекомендует архитектура NLog: `LogFactory` создает и управляет экземплярами логеров. ([NLog](https://nlog-project.org/documentation/v6.0.0/html/T_NLog_LogFactory.htm?utm_source=chatgpt.com))

Схема получается такой:

```csharp
// Addon
ILogBootstrap bootstrap = ...;

IDrzLoggerFactory loggerFactory = bootstrap.LoggerFactory;

// передаем в другую сборку
otherAssembly.Initialize(loggerFactory);
```

Во второй сборке:

```csharp
public sealed class OtherService
{
    private readonly IDrzLogger _logger;

    public OtherService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<OtherService>();
    }
}
```

Это лучше, чем передавать уже готовый `IDrzLogger`, потому что:

- каждая сборка получает логер со своей категорией (`OtherService`, `Updater`, и т.д.);
- используется одна и та же конфигурация NLog;
- сохраняется единый контекст логирования;
- не приходится создавать новый `LogBootstrap`.

Если у тебя уже есть `AddOnContext`, то можно вообще зарегистрировать `IDrzLoggerFactory` в контейнере:

```csharp
container.RegisterInstance<IDrzLoggerFactory>(bootstrap.LoggerFactory);
```

и любая сборка будет получать фабрику через DI, без передачи параметрами.

Для твоей архитектуры (`LogBootstrap`, `AddOnContext`, несколько независимых аддонов) именно **передача `IDrzLoggerFactory`** выглядит наиболее удачным решением.

---

**Sources:**

- [](https://nlog-project.org/documentation/v6.0.0/html/T_NLog_LogFactory.htm?utm_source=chatgpt.com)

## Prompt:
20.07.2026, 16:46:41

Как получить фабрику через контейнер в другой сборке?

## Response:
20.07.2026, 16:46:42

Если другая сборка использует **тот же контейнер DI**, то ничего специального делать не нужно.

В аддоне:

```csharp
container.RegisterInstance<IDrzLoggerFactory>(bootstrap.LoggerFactory);
```

В другой сборке:

```csharp
public class MyService
{
    private readonly IDrzLogger _logger;

    public MyService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<MyService>();
    }
}
```

Контейнер сам передаст тот же экземпляр `IDrzLoggerFactory`.

### Если в другой сборке нет доступа к контейнеру

Тогда есть два варианта:

1. Передать `IDrzLoggerFactory` в метод `Initialize(...)` этой сборки.
2. Передать сам `IServiceProvider`/контейнер (обычно менее предпочтительно, так как это превращается в Service Locator).

### Для твоей архитектуры

Поскольку у тебя уже есть `AddOnContext`, наиболее логично сделать так:

```csharp
public static class AddOnContext
{
    public static IDrzLoggerFactory LoggerFactory =>
        Container.GetInstance<IDrzLoggerFactory>();
}
```

Тогда в любой сборке можно написать:

```csharp
var logger = AddOnContext.LoggerFactory.GetLogger<MyService>();
```

без передачи фабрики через каждый конструктор.

Если покажешь, как сейчас устроен `AddOnContext` (он хранит `Container`, `IServiceProvider` или что-то свое), можно предложить наиболее чистый вариант именно под твою архитектуру.

## Prompt:
20.07.2026, 16:56:16

Несколько аддонов работают независимо, но используют логер с одинаковыми настройками, создают из logbootstrap, concurendictionary
Как из этих аддонов передать фабрику?

## Response:
20.07.2026, 16:56:17

Если аддоны **полностью независимы** (у каждого свой DI-контейнер и свой `LogBootstrap`), то **передавать фабрику между ними не нужно и нежелательно**.

Лучше сделать так:

- `LogBootstrap` хранит статический `ConcurrentDictionary<string, IDrzLoggerFactory>`.
- Ключ — уникальный идентификатор логической конфигурации (например, `ProductName`, `LogsDir` или `LoggerId`).
- Каждый аддон вызывает `LogBootstrap.GetLoggerFactory(...)`.
- Если фабрика уже существует — возвращается существующая.
- Если нет — создается, помещается в `ConcurrentDictionary` и возвращается.

Например:

```csharp
public static class LogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories = new();

    public static IDrzLoggerFactory GetLoggerFactory(LogOptions options)
    {
        return _factories.GetOrAdd(
            options.ProductName,
            _ => CreateFactory(options));
    }
}
```

Тогда каждый аддон делает:

```csharp
var factory = LogBootstrap.GetLoggerFactory(options);
```

и все получают **один и тот же экземпляр фабрики**, не зная друг о друге.

### Передавать фабрику стоит только если

- одна сборка создает объект из другой сборки (`new Updater(factory)`),
- или есть общий контейнер DI.

Если же аддоны загружаются nanoCAD независимо, то общий контейнер отсутствует, и **`ConcurrentDictionary` внутри `LogBootstrap` уже является реестром фабрик**. Передавать фабрику вручную в этом случае не требуется.

Именно такой подход хорошо соответствует твоей архитектуре с несколькими независимыми аддонами, использующими одинаковую инфраструктуру логирования.

## Prompt:
20.07.2026, 16:58:04

Logbootstrap надо подключать к подчиненным сборкам?

## Response:
20.07.2026, 16:58:04

Зависит от роли этих сборок.

### Если сборка — инфраструктурная библиотека

Например, `Updater`, `Backup`, `Core`, которые должны уметь логировать сами, то **нет**, `LogBootstrap` ей лучше не подключать.

Она должна зависеть только от абстракции:

```csharp
public class BackupService
{
    private readonly IDrzLogger _logger;

    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<BackupService>();
    }
}
```

или сразу получать готовый `IDrzLogger`.

### Если сборка — точка входа аддона

Только она должна знать о `LogBootstrap`:

```text
Addon
 ├── LogBootstrap
 ├── DI
 └── создает IDrzLoggerFactory
          │
          ▼
     Backup.dll
     Update.dll
     Core.dll
```

Получается правило:

- **LogBootstrap** — только в корневом аддоне (composition root).
- **Подчиненные сборки** — только `IDrzLoggerFactory` или `IDrzLogger`.
- **Никаких вызовов `LogBootstrap.GetLoggerFactory()` внутри библиотек.**

Так архитектура остается чистой: инфраструктура логирования создается в одном месте, а библиотеки не зависят от способа ее создания.

С учетом того, что у тебя несколько независимых аддонов, каждый аддон будет иметь **свой `LogBootstrap`**, но все они могут использовать одну и ту же реализацию и получать фабрики с одинаковыми настройками.

## Prompt:
21.07.2026, 03:51:37

IDrzLoggerFactory loggerFactory создается в аддоне в контейнере simpleinjector?
Передается в подчиненную сборку как параметр?

## Response:
21.07.2026, 03:51:37

Да, это наиболее правильный вариант.

В корневом аддоне:

```csharp
var loggerFactory = LogBootstrap.GetLoggerFactory(options);

container.RegisterInstance<IDrzLoggerFactory>(loggerFactory);
```

Когда создаешь объект из подчиненной сборки через SimpleInjector, контейнер сам передаст `IDrzLoggerFactory`:

```csharp
public class BackupService
{
    private readonly IDrzLogger _logger;

    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<BackupService>();
    }
}
```

И затем:

```csharp
var backupService = container.GetInstance<BackupService>();
```

Никаких параметров вручную передавать не нужно.

### Если объект создается не через DI

Тогда фабрику действительно придется передать:

```csharp
var backupService = new BackupService(loggerFactory);
```

То есть правило простое:

- **Создание через SimpleInjector** → фабрика попадает в конструктор автоматически.
- **Создание через `new`** → фабрику нужно передать вручную.

Для твоей архитектуры я бы стремился к тому, чтобы все сервисы создавались контейнером. Тогда `IDrzLoggerFactory` (как и остальные зависимости) распространяется автоматически и не требует явной передачи по цепочке.

## Prompt:
21.07.2026, 03:56:47

Для создания фабрики  <T>  не нужен?
Он требуется только для создания логера?

## Response:
21.07.2026, 03:56:47

Да, именно так.

`<T>` **не нужен при создании фабрики**. Фабрика не зависит от конкретного типа класса.

Например:

```csharp
IDrzLoggerFactory loggerFactory = LogBootstrap.GetLoggerFactory(options);
```

или

```csharp
IDrzLoggerFactory loggerFactory = new DrzLoggerFactory(...);
```

А вот когда нужен логер для конкретного класса, используется `<T>`:

```csharp
IDrzLogger logger = loggerFactory.GetLogger<MyService>();
```

или эквивалентно:

```csharp
IDrzLogger logger = loggerFactory.GetLogger(typeof(MyService));
```

То есть:

- `IDrzLoggerFactory` — одна на аддон (или на конфигурацию логирования).
- `IDrzLogger` — создается фабрикой для каждого класса.

Поэтому интерфейс фабрики обычно выглядит так:

```csharp
public interface IDrzLoggerFactory
{
    IDrzLogger GetLogger<T>();
    IDrzLogger GetLogger(Type type);
    IDrzLogger GetLogger(string name);
}
```

А `LogBootstrap` вообще не использует `<T>` — он только создает или возвращает экземпляр `IDrzLoggerFactory`.

## Prompt:
21.07.2026, 04:14:54

using drz.Abstractions.Logger;
using drz.LogServices.Diagnostics;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Concurrent;
using System.IO;

//https://replit.com/@razygraev/Log-Service-interface

namespace drz.LogServices.drzNlog
{
    /// <summary>
    /// Log Service
    /// </summary>
    /// <seealso cref="ILogService" />
    public sealed class NLogService : IDrzLogService
    {
        private static readonly ConcurrentDictionary<string, LogFactory> _factories = new();

        private readonly Func<string> _productNameProvider;

        private readonly Func<string> _assemblyDirectoryProvider;

        //private readonly string _assemblyDirectory;

        private readonly IEnvironmentInfoProvider _envInfoProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="productNameProvider">The product name provider.</param>
        /// <param name="assemblyDirectoryProvider">The assembly directory provider.</param>
        /// <param name="envInfoProvider">The env information provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// productNameProvider
        /// or
        /// assemblyDirectoryProvider
        /// </exception>
        public NLogService(
            Func<string> productNameProvider,
            Func<string> assemblyDirectoryProvider,
            IEnvironmentInfoProvider envInfoProvider = null)

        {
            _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));

            _assemblyDirectoryProvider = assemblyDirectoryProvider ?? throw new ArgumentNullException(nameof(assemblyDirectoryProvider));

            _envInfoProvider = envInfoProvider;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public IDrzLogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string productName = SafeGetProductName();
            LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);

            return new NLogAdapter(factory.GetLogger(type.FullName));
        }

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <returns></returns>
        private LogFactory CreateFactory(string productName)
        {
            string assemblyDirectory = SafeGetAssemblyDirectory();

            //путь к Diagnostic.Mode
            string baseDirDiagnostyc = Path.Combine(assemblyDirectory, LogKeys.DiagnosticMode);

            LogLevel requestedLevel = LogLevelReader.GetLevelFromFile(baseDirDiagnostyc);

            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogService).FullName}.{productName}", requestedLevel);

            string logName = productName;// ${shortdate}_{logName}.log;

            string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName, "logs"); // _appDataProductLogPathProvider();

            Exception configException = null;

            LogFactory factory = null;

            LoggingConfiguration config = null;

            bool isFallback = false;

            try
            {
                factory = new LogFactory();// Пытаемся использовать внешний конфиг

                config = factory.Configuration;
            }
            catch (NLogConfigurationException ex)
            {
                configException = ex; //потом запишем в  лог этой фабрики

                config = null;
            }

            //todo получить Level тут!!!

            //путь к Log.Level
            string baseDirLogLevel = Path.Combine(assemblyDirectory, LogKeys.LogLevel);

            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(baseDirLogLevel);

            if (config == null || config.AllTargets.Count == 0 || config.LoggingRules.Count == 0)//тут проверка конфига на нулл, конфиг если есть подгружается сам
            {
                //конфг из файла не подтянулся или битый
                isFallback = true;

                factory.Configuration = CreateConfiguration(logName, logsDir, currentLevel);
            }
            else
            {
                // Конфиг уже есть (nlog.config), просто прокидываем в него
                // наши пути через переменные
                ApplyCommonVariables(factory, logName, logsDir, currentLevel);
            }

            // писать в лог о системе о каде как этот лог сделан
            // 🔴 ВАЖНО: после того как конфиг установлен
            WriteFactoryDiagnostics(factory, productName, isFallback, configException);

            return factory;
        }

        private string SafeGetAssemblyDirectory()
        {
            try
            {
                string assemblyDirectory = _assemblyDirectoryProvider();
                return string.IsNullOrWhiteSpace(assemblyDirectory) ? string.Empty : assemblyDirectory;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the factory diagnostics.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="isFallback">if set to <c>true</c> [is fallback].</param>
        /// <param name="configException">The configuration exception.</param>
        private void WriteFactoryDiagnostics(LogFactory factory,
                                             string productName,
                                             bool isFallback,
                                             Exception configException)
        {
            try
            {
                Logger log = factory.GetLogger(typeof(NLogService).FullName);

                // Internal log level
                LogLevel internalLevel = InternalLogger.LogLevel;

                // Factory min level
                LogLevel effectiveLevel = GetEffectiveMinLevel(log);

                string msg = $"LogFactory initialized: {productName};\n" +
                                $"ConfigSource: {(isFallback ? "Fallback (programmatic)" : "External (nlog.config)")};\n" +
                                $"EffectiveMinLevel: {effectiveLevel};\n" +
                                $"InternalLoggerLevel: {internalLevel}";

                if (isFallback)
                {
                    log.Warn(msg);
                }
                else
                {
                    log.Debug(msg);
                }

                if (_envInfoProvider != null)
                {
                    log.Debug($"{_envInfoProvider.GetSummary()}");
                }

                if (configException != null)
                {
                    log.Error(configException, configException.Message);
                }
            }
            catch { }// Никогда не роняем приложение из-за диагностики логгера
        }

        /// <summary>
        /// Gets the effective minimum level.
        /// </summary>
        /// <param name="logger">The log.</param>
        /// <returns></returns>
        private static LogLevel GetEffectiveMinLevel(Logger logger)
        {
            foreach (LogLevel level in LogLevel.AllLevels) // Trace → Fatal
            {
                if (logger.IsEnabled(level))
                {
                    return level;
                }
            }

            return LogLevel.Off;
        }

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="appDataProductLogPath">The application data product log path.</param>
        /// <param name="currentLevel">The current level.</param>
        /// <returns></returns>
        private LoggingConfiguration CreateConfiguration(string filePrefix, string appDataProductLogPath, LogLevel currentLevel)
        {
            LoggingConfiguration config = new LoggingConfiguration();

#if DEBUG
            // Если файла уровня нет — Debug.
            LogLevel level = LogLevel.Debug;
#else
            // Если файла нет — Info.
            LogLevel level = LogLevel.Info;
            // Если файл есть, но пустой —Trace
            //string fallbackLevelName = "Trace";
#endif

            // Если файла уровня нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel, fallbackLevelName);

            if (currentLevel != LogLevel.Off)
            {
                level = currentLevel;
            }

            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.log"),

                ArchiveFileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.{{#}}.log"),

                ArchiveEvery = FileArchivePeriod.Day,
                //ArchiveAboveSize = 5 * 1024 * 1024,
                MaxArchiveFiles = 10,

                KeepFileOpen = false,

                OpenFileCacheTimeout = 10,

                Layout = CreateXmlLayout(),
            };

            // ---------------------------
            // Async wrapper
            // ---------------------------
            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper(fileTarget)
            {
                QueueLimit = 10000,              // размер очереди
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                BatchSize = 500,
                TimeToSleepBetweenBatches = 50,
            };

            config.AddTarget("async", asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            return config;
        }

        /// <summary>
        /// XML layout
        /// </summary>
        /// <returns></returns>
        private static XmlLayout CreateXmlLayout()
        {
            return new XmlLayout
            {
                IncludeEventProperties = true,
                IndentXml = true,
                MaxRecursionLimit = 10,
                ElementName = "logevent",

                Attributes =
                {
                    new XmlAttribute("time", "${longdate}"),
                    new XmlAttribute("level", "${level:uppercase=true}"),
                    new XmlAttribute("logger", "${logger}"),
                    new XmlAttribute("pid", "${processid}"),
                    new XmlAttribute("fullName", "${processname:fullName=true}"),
                },

                Elements =
                {
                    new XmlElement("message", "${message}"),
                    new XmlElement("exception", "${exception:format=ToString:innerFormat=ToString:maxInnerExceptionLevel=10}")
                }
            };
        }

        /// <summary>
        /// Safes the name of the get product.
        /// </summary>
        /// <returns></returns>
        private string SafeGetProductName()
        {
            try
            {
                string name = _productNameProvider();
                return string.IsNullOrWhiteSpace(name) ? "UnknownProduct" : name;
            }
            catch
            {
                return "UnknownProduct";
            }
        }

        /// <summary>
        /// Applies the common variables.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="appTitle">The application title.</param>
        /// <param name="logsDir">The logs dir.</param>
        /// <param name="currentLevel">The current level.</param>
        private void ApplyCommonVariables(LogFactory factory, string appTitle, string logsDir, LogLevel currentLevel)
        {
            LoggingConfiguration config = factory.Configuration;

            if (factory.Configuration == null)
            {
                return;
            }

            /*factory.Configuration*/
            config.Variables["AppTitle"] = appTitle;
            /*factory.Configuration*/
            config.Variables["LogsDir"] = logsDir;

            // Если файла нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel);

            //если офф, то не меняем уровень
            if (currentLevel != LogLevel.Off)
            {
                /*factory.Configuration*/
                config.Variables["LevelMay"] = currentLevel.ToString();
            }

            factory.ReconfigExistingLoggers();

#if DEBUG || TEST
            //проверка значений  var

            if (config.Variables.ContainsKey(LogVar.LevelMay))
            {
                Layout layot = config.Variables[LogVar.LevelMay];

                string finalLevel = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.AppTitle))
            {
                Layout layot = config.Variables[LogVar.AppTitle];

                string finalAppTitle = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.LogsDir))
            {
                Layout layot = config.Variables[LogVar.LogsDir];

                string finalLogsDir = layot.Render(LogEventInfo.CreateNullEvent());
            }

#endif
        }
    }
}

## Response:
21.07.2026, 04:14:54

По коду я бы предложил разделить две ответственности.

Сейчас `NLogService` одновременно:
1. создает/кэширует `LogFactory`;
2. является фабрикой логеров (`GetLogger<T>()`).

Из-за этого возникает путаница.

Лучше сделать так:

```text
NLogBootstrap
    ↓
IDrzLoggerFactory (singleton)
    ↓
GetLogger<T>()
    ↓
IDrzLogger
```

То есть:

```csharp
public interface IDrzLoggerFactory
{
    IDrzLogger GetLogger<T>();
    IDrzLogger GetLogger(Type type);
}
```

А `NLogService` (или лучше `NLogLoggerFactory`) реализует только этот интерфейс.

Создание экземпляра фабрики выполняет `NLogBootstrap`:

```csharp
var loggerFactory = NLogBootstrap.Create(
    productNameProvider,
    assemblyDirectoryProvider,
    envInfoProvider);
```

или

```csharp
var loggerFactory = NLogBootstrap.GetOrCreate(...);
```

Именно `NLogBootstrap` хранит

```csharp
ConcurrentDictionary<string, LogFactory>
```

или даже

```csharp
ConcurrentDictionary<string, IDrzLoggerFactory>
```

Тогда `NLogService` вообще не знает про `ConcurrentDictionary`.

---

### Что бы я изменил в твоем коде

Сейчас:

```csharp
public sealed class NLogService : IDrzLogService
```

я бы заменил на

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
```

А код

```csharp
_factories.GetOrAdd(productName, CreateFactory);
```

вынес в `NLogBootstrap`.

В итоге в аддоне получится:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(
        productNameProvider,
        assemblyDirectoryProvider,
        envInfoProvider);

container.RegisterInstance(loggerFactory);
```

После этого любая библиотека просто получает `IDrzLoggerFactory` через DI.

---

Мне кажется, у тебя сейчас `NLogService` фактически уже является **фабрикой логеров**, а название `Service` только запутывает. Я бы переименовал его в `NLogLoggerFactory`, а создание и кэширование фабрик полностью вынес в `NLogBootstrap`. Это сделает архитектуру более понятной и избавит от смешения ролей.

## Prompt:
21.07.2026, 04:22:23

Как библиотека получит фабрику?

## Response:
21.07.2026, 04:22:23

Есть всего два варианта.

### Вариант 1. Через DI (предпочтительный)

В аддоне:

```csharp
IDrzLoggerFactory loggerFactory = NLogBootstrap.GetOrCreate(...);

container.RegisterInstance(loggerFactory);
```

В библиотеке:

```csharp
public class BackupService
{
    private readonly IDrzLogger _logger;

    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<BackupService>();
    }
}
```

Если `BackupService` создается контейнером:

```csharp
var service = container.GetInstance<BackupService>();
```

то `IDrzLoggerFactory` будет передан автоматически.

---

### Вариант 2. Через параметр

Если библиотека не использует DI и объект создается вручную:

```csharp
var loggerFactory = NLogBootstrap.GetOrCreate(...);

var service = new BackupService(loggerFactory);
```

---

### Для твоей архитектуры

Поскольку ты уже используешь **SimpleInjector** и `AddOnContext`, я бы не передавал фабрику по цепочке вручную. Корневой аддон регистрирует `IDrzLoggerFactory` в контейнере, а все сервисы библиотек получают ее через конструктор.

Вопрос только один: **кто создает объекты библиотек?**

- Если их создает `container.GetInstance<T>()` — все уже решено, фабрика придет автоматически.
- Если в коде есть `new BackupService(...)` — тогда либо заменять создание на контейнер, либо передавать `IDrzLoggerFactory` вручную.

## Prompt:
21.07.2026, 04:25:33

IDrzLoggerFactory
Нужен интерфейс

## Response:
21.07.2026, 04:25:33

Да, интерфейс **нужен**.

Причины:

1. **Библиотеки не зависят от NLog.**
   Они знают только об `IDrzLoggerFactory` и `IDrzLogger`.

2. **Легче тестировать.**
   В тестах можно подставить фейковую фабрику.

3. **Можно заменить реализацию.**
   Например, сделать `SerilogLoggerFactory`, не меняя код библиотек.

Я бы сделал минимальный интерфейс:

```csharp
public interface IDrzLoggerFactory
{
    IDrzLogger GetLogger<T>();

    IDrzLogger GetLogger(Type type);

    IDrzLogger GetLogger(string name);
}
```

Тогда `NLogLoggerFactory` реализует этот интерфейс:

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
{
    ...
}
```

А в аддоне:

```csharp
container.RegisterInstance<IDrzLoggerFactory>(loggerFactory);
```

И библиотеки получают только:

```csharp
public BackupService(IDrzLoggerFactory loggerFactory)
{
    _logger = loggerFactory.GetLogger<BackupService>();
}
```

Это наиболее гибкая архитектура и хорошо сочетается с DI.

## Prompt:
21.07.2026, 04:32:30

private LogFactory CreateFactory(string productName)
        {...
Сделать публичным и добавить интерфейс?

## Response:
21.07.2026, 04:32:31

Нет. `CreateFactory()` лучше **не делать публичным**.

Причина в том, что `LogFactory` — это тип NLog. Если сделать его частью публичного API, абстракция начнет зависеть от NLog.

Лучше оставить `CreateFactory()` приватным, а наружу выдавать `IDrzLoggerFactory`.

Например:

```csharp
public interface IDrzLoggerBootstrap
{
    IDrzLoggerFactory GetOrCreate(
        Func<string> productNameProvider,
        Func<string> assemblyDirectoryProvider,
        IEnvironmentInfoProvider envInfoProvider = null);
}
```

или, если `NLogBootstrap` статический:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(...);
```

Внутри уже:

```csharp
private LogFactory CreateFactory(string productName)
{
    ...
}
```

Если же ты не хочешь выделять отдельный `NLogBootstrap`, тогда можно сделать публичным **не `CreateFactory()`**, а метод, возвращающий абстракцию:

```csharp
public IDrzLoggerFactory GetLoggerFactory()
{
    ...
}
```

который внутри вызовет приватный `CreateFactory()`.

Итого:
- ✅ `CreateFactory()` — `private`.
- ✅ Публичный метод должен возвращать `IDrzLoggerFactory`, а не `LogFactory`.
- ❌ Не стоит делать `LogFactory` частью интерфейса. Это привяжет все библиотеки к NLog.

## Prompt:
21.07.2026, 04:35:33

using drz.Abstractions.Logger;
using drz.LogServices.Diagnostics;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Concurrent;
using System.IO;

//https://replit.com/@razygraev/Log-Service-interface

namespace drz.LogServices.drzNlog
{
    /// <summary>
    /// Log Service
    /// </summary>
    /// <seealso cref="ILogService" />
    public sealed class NLogService : IDrzLogService
    {
        private static readonly ConcurrentDictionary<string, LogFactory> _factories = new();

        private readonly Func<string> _productNameProvider;

        private readonly Func<string> _assemblyDirectoryProvider;

        //private readonly string _assemblyDirectory;

        private readonly IEnvironmentInfoProvider _envInfoProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="productNameProvider">The product name provider.</param>
        /// <param name="assemblyDirectoryProvider">The assembly directory provider.</param>
        /// <param name="envInfoProvider">The env information provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// productNameProvider
        /// or
        /// assemblyDirectoryProvider
        /// </exception>
        public NLogService(
            Func<string> productNameProvider,
            Func<string> assemblyDirectoryProvider,
            IEnvironmentInfoProvider envInfoProvider = null)

        {
            _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));

            _assemblyDirectoryProvider = assemblyDirectoryProvider ?? throw new ArgumentNullException(nameof(assemblyDirectoryProvider));

            _envInfoProvider = envInfoProvider;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public IDrzLogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string productName = SafeGetProductName();
            LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);

            return new NLogAdapter(factory.GetLogger(type.FullName));
        }

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <returns></returns>
        private LogFactory CreateFactory(string productName)
        {
            string assemblyDirectory = SafeGetAssemblyDirectory();

            //путь к Diagnostic.Mode
            string baseDirDiagnostyc = Path.Combine(assemblyDirectory, LogKeys.DiagnosticMode);

            LogLevel requestedLevel = LogLevelReader.GetLevelFromFile(baseDirDiagnostyc);

            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogService).FullName}.{productName}", requestedLevel);

            string logName = productName;// ${shortdate}_{logName}.log;

            string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName, "logs"); // _appDataProductLogPathProvider();

            Exception configException = null;

            LogFactory factory = null;

            LoggingConfiguration config = null;

            bool isFallback = false;

            try
            {
                factory = new LogFactory();// Пытаемся использовать внешний конфиг

                config = factory.Configuration;
            }
            catch (NLogConfigurationException ex)
            {
                configException = ex; //потом запишем в  лог этой фабрики

                config = null;
            }

            //todo получить Level тут!!!

            //путь к Log.Level
            string baseDirLogLevel = Path.Combine(assemblyDirectory, LogKeys.LogLevel);

            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(baseDirLogLevel);

            if (config == null || config.AllTargets.Count == 0 || config.LoggingRules.Count == 0)//тут проверка конфига на нулл, конфиг если есть подгружается сам
            {
                //конфг из файла не подтянулся или битый
                isFallback = true;

                factory.Configuration = CreateConfiguration(logName, logsDir, currentLevel);
            }
            else
            {
                // Конфиг уже есть (nlog.config), просто прокидываем в него
                // наши пути через переменные
                ApplyCommonVariables(factory, logName, logsDir, currentLevel);
            }

            // писать в лог о системе о каде как этот лог сделан
            // 🔴 ВАЖНО: после того как конфиг установлен
            WriteFactoryDiagnostics(factory, productName, isFallback, configException);

            return factory;
        }

        private string SafeGetAssemblyDirectory()
        {
            try
            {
                string assemblyDirectory = _assemblyDirectoryProvider();
                return string.IsNullOrWhiteSpace(assemblyDirectory) ? string.Empty : assemblyDirectory;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the factory diagnostics.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="isFallback">if set to <c>true</c> [is fallback].</param>
        /// <param name="configException">The configuration exception.</param>
        private void WriteFactoryDiagnostics(LogFactory factory,
                                             string productName,
                                             bool isFallback,
                                             Exception configException)
        {
            try
            {
                Logger log = factory.GetLogger(typeof(NLogService).FullName);

                // Internal log level
                LogLevel internalLevel = InternalLogger.LogLevel;

                // Factory min level
                LogLevel effectiveLevel = GetEffectiveMinLevel(log);

                string msg = $"LogFactory initialized: {productName};\n" +
                                $"ConfigSource: {(isFallback ? "Fallback (programmatic)" : "External (nlog.config)")};\n" +
                                $"EffectiveMinLevel: {effectiveLevel};\n" +
                                $"InternalLoggerLevel: {internalLevel}";

                if (isFallback)
                {
                    log.Warn(msg);
                }
                else
                {
                    log.Debug(msg);
                }

                if (_envInfoProvider != null)
                {
                    log.Debug($"{_envInfoProvider.GetSummary()}");
                }

                if (configException != null)
                {
                    log.Error(configException, configException.Message);
                }
            }
            catch { }// Никогда не роняем приложение из-за диагностики логгера
        }

        /// <summary>
        /// Gets the effective minimum level.
        /// </summary>
        /// <param name="logger">The log.</param>
        /// <returns></returns>
        private static LogLevel GetEffectiveMinLevel(Logger logger)
        {
            foreach (LogLevel level in LogLevel.AllLevels) // Trace → Fatal
            {
                if (logger.IsEnabled(level))
                {
                    return level;
                }
            }

            return LogLevel.Off;
        }

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="appDataProductLogPath">The application data product log path.</param>
        /// <param name="currentLevel">The current level.</param>
        /// <returns></returns>
        private LoggingConfiguration CreateConfiguration(string filePrefix, string appDataProductLogPath, LogLevel currentLevel)
        {
            LoggingConfiguration config = new LoggingConfiguration();

#if DEBUG
            // Если файла уровня нет — Debug.
            LogLevel level = LogLevel.Debug;
#else
            // Если файла нет — Info.
            LogLevel level = LogLevel.Info;
            // Если файл есть, но пустой —Trace
            //string fallbackLevelName = "Trace";
#endif

            // Если файла уровня нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel, fallbackLevelName);

            if (currentLevel != LogLevel.Off)
            {
                level = currentLevel;
            }

            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.log"),

                ArchiveFileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.{{#}}.log"),

                ArchiveEvery = FileArchivePeriod.Day,
                //ArchiveAboveSize = 5 * 1024 * 1024,
                MaxArchiveFiles = 10,

                KeepFileOpen = false,

                OpenFileCacheTimeout = 10,

                Layout = CreateXmlLayout(),
            };

            // ---------------------------
            // Async wrapper
            // ---------------------------
            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper(fileTarget)
            {
                QueueLimit = 10000,              // размер очереди
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                BatchSize = 500,
                TimeToSleepBetweenBatches = 50,
            };

            config.AddTarget("async", asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            return config;
        }

        /// <summary>
        /// XML layout
        /// </summary>
        /// <returns></returns>
        private static XmlLayout CreateXmlLayout()
        {
            return new XmlLayout
            {
                IncludeEventProperties = true,
                IndentXml = true,
                MaxRecursionLimit = 10,
                ElementName = "logevent",

                Attributes =
                {
                    new XmlAttribute("time", "${longdate}"),
                    new XmlAttribute("level", "${level:uppercase=true}"),
                    new XmlAttribute("logger", "${logger}"),
                    new XmlAttribute("pid", "${processid}"),
                    new XmlAttribute("fullName", "${processname:fullName=true}"),
                },

                Elements =
                {
                    new XmlElement("message", "${message}"),
                    new XmlElement("exception", "${exception:format=ToString:innerFormat=ToString:maxInnerExceptionLevel=10}")
                }
            };
        }

        /// <summary>
        /// Safes the name of the get product.
        /// </summary>
        /// <returns></returns>
        private string SafeGetProductName()
        {
            try
            {
                string name = _productNameProvider();
                return string.IsNullOrWhiteSpace(name) ? "UnknownProduct" : name;
            }
            catch
            {
                return "UnknownProduct";
            }
        }

        /// <summary>
        /// Applies the common variables.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="appTitle">The application title.</param>
        /// <param name="logsDir">The logs dir.</param>
        /// <param name="currentLevel">The current level.</param>
        private void ApplyCommonVariables(LogFactory factory, string appTitle, string logsDir, LogLevel currentLevel)
        {
            LoggingConfiguration config = factory.Configuration;

            if (factory.Configuration == null)
            {
                return;
            }

            /*factory.Configuration*/
            config.Variables["AppTitle"] = appTitle;
            /*factory.Configuration*/
            config.Variables["LogsDir"] = logsDir;

            // Если файла нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel);

            //если офф, то не меняем уровень
            if (currentLevel != LogLevel.Off)
            {
                /*factory.Configuration*/
                config.Variables["LevelMay"] = currentLevel.ToString();
            }

            factory.ReconfigExistingLoggers();

#if DEBUG || TEST
            //проверка значений  var

            if (config.Variables.ContainsKey(LogVar.LevelMay))
            {
                Layout layot = config.Variables[LogVar.LevelMay];

                string finalLevel = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.AppTitle))
            {
                Layout layot = config.Variables[LogVar.AppTitle];

                string finalAppTitle = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.LogsDir))
            {
                Layout layot = config.Variables[LogVar.LogsDir];

                string finalLogsDir = layot.Render(LogEventInfo.CreateNullEvent());
            }

#endif
        }
    }
}
Как конкретно переделать этот класс, чтобы можно было передавать в другую библиотеку

## Response:
21.07.2026, 04:35:34

Я бы не стал пытаться передавать сам `NLogService` в другие библиотеки. У него сейчас две разные ответственности:

- создание и кэширование `LogFactory`;
- выдача логеров.

Лучше разделить их.

### 1. `NLogService` сделать реализацией `IDrzLoggerFactory`

Тогда класс практически не изменится.

```csharp
public sealed class NLogService : IDrzLoggerFactory
```

В нем остается:

```csharp
public IDrzLogger GetLogger<T>();
public IDrzLogger GetLogger(Type type);
```

Но **убирается**:

```csharp
private static readonly ConcurrentDictionary<string, LogFactory> _factories;
```

и

```csharp
private LogFactory CreateFactory(...)
```

Вместо этого появляется поле:

```csharp
private readonly LogFactory _factory;
```

которое инициализируется в конструкторе.

---

### 2. Создать отдельный `NLogBootstrap`

Именно он будет содержать твой текущий код с `ConcurrentDictionary`:

```csharp
public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories = new();

    public static IDrzLoggerFactory GetOrCreate(
        Func<string> productNameProvider,
        Func<string> assemblyDirectoryProvider,
        IEnvironmentInfoProvider envInfoProvider = null)
    {
        string product = productNameProvider();

        return _factories.GetOrAdd(product,
            _ => new NLogService(
                    CreateFactory(...)));
    }

    private static LogFactory CreateFactory(...)
    {
        // сюда переносится твой текущий CreateFactory
    }
}
```

---

### 3. В аддоне

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(
        () => addonInfo.ProductName,
        () => addonInfo.AssemblyDirectory,
        envInfoProvider);

container.RegisterInstance(loggerFactory);
```

---

### 4. В библиотеке

Никакого `NLogService`:

```csharp
public class BackupService
{
    private readonly IDrzLogger _logger;

    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<BackupService>();
    }
}
```

---

## Что это дает

Тогда библиотека вообще не знает:

- что используется NLog;
- что есть `ConcurrentDictionary`;
- что существует `CreateFactory`;
- что существует `Bootstrap`.

Она знает только про:

```csharp
IDrzLoggerFactory
```

и получает его через DI.

---

**Я бы не делал `CreateFactory()` публичным.** Этот метод — внутренняя деталь реализации NLog. Публичным должен быть только объект `IDrzLoggerFactory`, который регистрируется в контейнере и передается библиотекам. Именно это позволит использовать одну и ту же фабрику логеров во всех подчиненных сборках без привязки их к NLog.

## Prompt:
21.07.2026, 04:41:26

public sealed class NLogService : IDrzLoggerFactory
Название класса путает?

## Response:
21.07.2026, 04:41:26

Да, на мой взгляд, **`NLogService` начинает путать**.

Судя по коду, это не "сервис", а именно **фабрика логеров**. Единственная публичная функциональность класса — получить логер:

```csharp
GetLogger<T>()
GetLogger(Type)
```

Поэтому название лучше отражать его назначение.

Я бы выбрал один из вариантов:

- **`NLogLoggerFactory`** — самый понятный и соответствует интерфейсу `IDrzLoggerFactory`.
- **`DrzLoggerFactory`** — если не хочется привязывать имя к NLog.
- **`NLogLoggerProvider`** — если придерживаться терминологии Microsoft (`ILoggerProvider`), но это менее точно, так как это не `ILoggerProvider`.

Для твоей архитектуры наиболее логично:

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
```

А класс, который создает и кэширует фабрики, оставить отдельно:

```csharp
public static class NLogBootstrap
```

или

```csharp
public static class NLogFactoryProvider
```

Тогда роли становятся очевидными:

- **`NLogBootstrap`** — создает/кэширует фабрики.
- **`NLogLoggerFactory`** — выдает логеры.
- **`IDrzLoggerFactory`** — абстракция, которую получают библиотеки через DI.

Такое разделение делает архитектуру понятнее и по названию, и по ответственности.

## Prompt:
21.07.2026, 05:02:31

using drz.Abstractions.Logger;
using drz.LogServices.drzNlog;
using drz.Src.Infrastructure;
using System;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Src.Services
{
/// 
/// проброс в фабрику ProductName и получение логера для продукта
/// 
public static class LoggerProvider
{
\#region Private Fields

```
    private static readonly Lazy<IDrzLoggerFactory> _service = new(() =>
        new NLogLoggerFactory(
            productNameProvider: () => AddonInfo.ProductName,
            assemblyDirectoryProvider: () => AddonInfo.AssemblyDirectory,
            envInfoProvider: new CadEnvironmentInfoProvider()
        ));

    #endregion Private Fields

    #region Internal Methods

    internal static IDrzLogger For<T>() => _service.Value.GetLogger<T>();

    internal static IDrzLogger For(Type type) => _service.Value.GetLogger(type);

    internal static IDrzLogger For<T>(string productName) => _service.Value.GetLogger<T>(productName);


    #endregion Internal Methods
}
```

}

еще такой класс

## Response:
21.07.2026, 05:02:32

Да, этот класс тоже стоит пересмотреть.

Сейчас он выполняет роль **Service Locator** — любая библиотека может вызвать `LoggerProvider.For<T>()` и получить логер через статическое состояние (`AddOnContext`). Это создает скрытую зависимость.

Если ты переходишь на `IDrzLoggerFactory` через DI, то `LoggerProvider` становится практически не нужен.

То есть вместо:

```csharp
private readonly IDrzLogger _logger = LoggerProvider.For<MyService>();
```

будет:

```csharp
public class MyService
{
    private readonly IDrzLogger _logger;

    public MyService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<MyService>();
    }
}
```

---

### Если оставлять `LoggerProvider`

Тогда я бы хотя бы не создавал внутри него `NLogLoggerFactory` через `Lazy`.

Вместо этого сделать возможность инициализации:

```csharp
public static class LoggerProvider
{
    private static IDrzLoggerFactory _factory;

    public static void Initialize(IDrzLoggerFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public static IDrzLogger For<T>() => _factory.GetLogger<T>();
}
```

В аддоне:

```csharp
var loggerFactory = NLogBootstrap.GetOrCreate(...);

LoggerProvider.Initialize(loggerFactory);

container.RegisterInstance(loggerFactory);
```

Тогда:

- DI использует тот же экземпляр фабрики.
- Старый код через `LoggerProvider.For<T>()` тоже использует тот же экземпляр.
- Нет повторного создания `NLogLoggerFactory`.

---

### Что я бы рекомендовал

Если ты постепенно переводишь проект на DI, то:

1. Оставить `LoggerProvider` только как временный слой совместимости.
2. Новые классы писать только через `IDrzLoggerFactory`.
3. После перевода всего кода удалить `LoggerProvider`.

Так архитектура будет проще: одна фабрика создается в `NLogBootstrap`, регистрируется в SimpleInjector и используется всеми библиотеками.

## Prompt:
21.07.2026, 06:27:12

private readonly LogFactory _factory;
А как вернуть в вызывающий фабрику?

## Response:
21.07.2026, 06:27:12

Никак. `LogFactory` **не должен возвращаться наружу**, если ты строишь архитектуру через абстракции.

`LogFactory` — это внутренняя реализация `NLogLoggerFactory`.

Например:

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
{
    private readonly LogFactory _factory;

    public NLogLoggerFactory(LogFactory factory)
    {
        _factory = factory;
    }

    public IDrzLogger GetLogger<T>()
    {
        return new NLogAdapter(_factory.GetLogger(typeof(T).FullName));
    }
}
```

А наружу возвращается **не `LogFactory`**, а `IDrzLoggerFactory`:

```csharp
public static IDrzLoggerFactory GetOrCreate(...)
{
    LogFactory logFactory = ...;

    return new NLogLoggerFactory(logFactory);
}
```

Тогда:

```csharp
IDrzLoggerFactory loggerFactory = NLogBootstrap.GetOrCreate(...);

container.RegisterInstance(loggerFactory);
```

Библиотека получает `IDrzLoggerFactory`, вызывает `GetLogger<T>()`, а `LogFactory` остается полностью скрытым.

### Но в твоем случае

Я бы вообще **не стал разделять текущий класс**.

Твой `NLogService` уже является фабрикой логеров. Достаточно:

- переименовать его в `NLogLoggerFactory`;
- реализовать `IDrzLoggerFactory`;
- регистрировать его экземпляр в контейнере.

То есть вместо:

```csharp
private static ConcurrentDictionary<string, LogFactory>
```

можно даже сделать:

```csharp
private static ConcurrentDictionary<string, IDrzLoggerFactory>
```

и хранить уже готовые фабрики.

Это потребует минимальных изменений в существующем коде и хорошо впишется в твою архитектуру.

## Prompt:
21.07.2026, 06:32:42

Думаю все же надо разделить, через словарь фабрик получаем или создаем фабрику в аддоне.
Регистрируем фабрику в контейнере с интерфейсом
В библиотеку передаём готовую фабрику, там же получаем логгер

## Response:
21.07.2026, 06:32:43

Да, это как раз выглядит как чистая архитектура.

Я бы построил ее так:

```text
           NLogBootstrap
                │
      ConcurrentDictionary<string, IDrzLoggerFactory>
                │
         GetOrCreate(productName)
                │
                ▼
         NLogLoggerFactory
                │
        RegisterInstance<IDrzLoggerFactory>()
                │
      ┌─────────┴─────────┐
      ▼                   ▼
  AddonService       BackupService
                          │
                  IDrzLoggerFactory
                          │
                  GetLogger<BackupService>()
                          │
                      IDrzLogger
```

### В аддоне

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(...);

container.RegisterInstance(loggerFactory);
```

### В библиотеке

```csharp
public class BackupService
{
    private readonly IDrzLogger _logger;

    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger<BackupService>();
    }
}
```

### Ответственности

**NLogBootstrap**
- создает фабрику;
- хранит словарь фабрик;
- знает о `ConcurrentDictionary`.

**NLogLoggerFactory**
- хранит `LogFactory`;
- выдает `IDrzLogger`;
- не знает о словаре.

**Библиотеки**
- знают только об `IDrzLoggerFactory`;
- не знают ни о `NLog`, ни о `Bootstrap`.

Мне кажется, это удачнее твоего текущего варианта, потому что у каждого класса остается одна четкая ответственность. Особенно хорошо то, что библиотекам не нужно ничего знать о том, как создается и кэшируется фабрика — они получают уже готовый `IDrzLoggerFactory` через DI.

## Prompt:
21.07.2026, 11:16:02

CreateFactory в каком классе?

## Response:
21.07.2026, 11:16:02

Я бы поместил `CreateFactory()` в **`NLogBootstrap`**.

Потому что создание `LogFactory` — это часть процесса инициализации, а не работы фабрики логеров.

Получится примерно так:

```text
NLogBootstrap
 ├─ ConcurrentDictionary<string, IDrzLoggerFactory>
 ├─ GetOrCreate(...)
 └─ CreateFactory(...)
        │
        ▼
     LogFactory
        │
        ▼
NLogLoggerFactory : IDrzLoggerFactory
 ├─ private readonly LogFactory _factory;
 ├─ GetLogger<T>()
 └─ GetLogger(Type)
```

То есть:

```csharp
public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories;

    public static IDrzLoggerFactory GetOrCreate(...)
    {
        ...
    }

    private static LogFactory CreateFactory(...)
    {
        // весь твой код создания LogFactory
    }
}
```

А `NLogLoggerFactory` становится очень простым:

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
{
    private readonly LogFactory _factory;

    public NLogLoggerFactory(LogFactory factory)
    {
        _factory = factory;
    }

    public IDrzLogger GetLogger<T>() =>
        GetLogger(typeof(T));

    public IDrzLogger GetLogger(Type type) =>
        new NLogAdapter(_factory.GetLogger(type.FullName));
}
```

Так обязанности разделены максимально четко:

- **`NLogBootstrap`** — знает, *как создать и настроить* `LogFactory`.
- **`NLogLoggerFactory`** — знает только, *как получить логер* из уже готового `LogFactory`.

Именно такой вариант я бы выбрал для твоей архитектуры.

## Prompt:
21.07.2026, 12:03:25

public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories;

    public static IDrzLoggerFactory GetOrCreate(...)
    {
        ...
    }

    private static LogFactory CreateFactory(...)
    {
        // весь твой код создания LogFactory
    }
}
Полностью статический?
Допиши класс
Тип в ловаре и возвращаемый не совпадают

## Response:
21.07.2026, 12:03:26

Да, здесь есть несоответствие.

Если словарь хранит `IDrzLoggerFactory`, то `CreateFactory()` не может возвращать `LogFactory`.

Есть два варианта.

## Вариант 1 (рекомендую)

`CreateFactory()` возвращает **`IDrzLoggerFactory`**, а создание `LogFactory` скрыто внутри него.

```csharp
public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories
        = new();

    public static IDrzLoggerFactory GetOrCreate(
        Func<string> productNameProvider,
        Func<string> assemblyDirectoryProvider,
        IEnvironmentInfoProvider envInfoProvider = null)
    {
        string productName = productNameProvider();

        return _factories.GetOrAdd(productName,
            _ => CreateLoggerFactory(
                    productNameProvider,
                    assemblyDirectoryProvider,
                    envInfoProvider));
    }

    private static IDrzLoggerFactory CreateLoggerFactory(
        Func<string> productNameProvider,
        Func<string> assemblyDirectoryProvider,
        IEnvironmentInfoProvider envInfoProvider)
    {
        LogFactory logFactory = CreateLogFactory(
            productNameProvider,
            assemblyDirectoryProvider,
            envInfoProvider);

        return new NLogLoggerFactory(logFactory);
    }

    private static LogFactory CreateLogFactory(...)
    {
        // твой существующий CreateFactory()
    }
}
```

Здесь два метода с разными задачами:

- `CreateLogFactory()` → создает `LogFactory`.
- `CreateLoggerFactory()` → оборачивает его в `NLogLoggerFactory`.

---

## Вариант 2 (еще проще)

Вообще убрать `CreateLoggerFactory()`:

```csharp
return _factories.GetOrAdd(productName,
    _ => new NLogLoggerFactory(
            CreateLogFactory(...)));
```

---

### Полностью статический ли `NLogBootstrap`?

Да, в твоем случае это хороший кандидат на `static`, потому что:

- хранит общий кэш фабрик;
- не имеет собственного состояния экземпляра;
- используется как точка входа для получения фабрики.

То есть он фактически является реестром (`Registry`) фабрик, а не сервисом, который нужно регистрировать в DI.

Мне больше нравится второй вариант: он компактнее, а разделение ответственности остается понятным.

## Prompt:
21.07.2026, 12:05:00

CreateLogFactory(...)))
Напиши метод

## Response:
21.07.2026, 12:05:00

Я бы сделал его примерно таким. Это практически твой текущий `CreateFactory`, только вынесенный в `NLogBootstrap` и без обращения к полям экземпляра.

```csharp
private static LogFactory CreateLogFactory(
    string productName,
    string assemblyDirectory,
    IEnvironmentInfoProvider envInfoProvider = null)
{
    // Diagnostic.Mode
    string diagnosticModePath = Path.Combine(
        assemblyDirectory,
        LogKeys.DiagnosticMode);

    LogLevel requestedLevel =
        LogLevelReader.GetLevelFromFile(diagnosticModePath);

    InternalLoggerHelpers.ConfigureInternalLogger(
        $"{typeof(NLogBootstrap).FullName}.{productName}",
        requestedLevel);

    string logsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        productName,
        "logs");

    Exception configException = null;
    LogFactory factory = null;
    LoggingConfiguration config = null;
    bool isFallback = false;

    try
    {
        factory = new LogFactory();
        config = factory.Configuration;
    }
    catch (NLogConfigurationException ex)
    {
        configException = ex;
    }

    // Log.Level
    string logLevelPath = Path.Combine(
        assemblyDirectory,
        LogKeys.LogLevel);

    LogLevel currentLevel =
        LogLevelReader.GetLevelFromFile(logLevelPath);

    if (config == null ||
        config.AllTargets.Count == 0 ||
        config.LoggingRules.Count == 0)
    {
        isFallback = true;

        factory.Configuration =
            CreateConfiguration(productName, logsDir, currentLevel);
    }
    else
    {
        ApplyCommonVariables(factory, productName, logsDir, currentLevel);
    }

    WriteFactoryDiagnostics(
        factory,
        productName,
        isFallback,
        configException,
        envInfoProvider);

    return factory;
}
```

Но здесь есть важный момент.

После такого переноса у тебя еще останутся методы:

- `CreateConfiguration()`
- `ApplyCommonVariables()`
- `WriteFactoryDiagnostics()`
- `CreateXmlLayout()`
- `GetEffectiveMinLevel()`

Они тоже логически относятся к созданию `LogFactory`. Поэтому я бы **перенес их в `NLogBootstrap`** (или сделал внутренним вспомогательным классом `NLogFactoryBuilder`).

Тогда `NLogLoggerFactory` останется очень маленьким:

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
{
    private readonly LogFactory _factory;

    public NLogLoggerFactory(LogFactory factory)
    {
        _factory = factory;
    }

    public IDrzLogger GetLogger<T>() =>
        GetLogger(typeof(T));

    public IDrzLogger GetLogger(Type type) =>
        new NLogAdapter(_factory.GetLogger(type.FullName));
}
```

Такое разделение выглядит наиболее естественным.

## Prompt:
21.07.2026, 12:08:52

Вспомогательные методы тоже Статик?
NlogFactoryBulder static?

## Response:
21.07.2026, 12:08:53

Я бы не делал `NLogFactoryBuilder` статическим.

Причина — сейчас для построения фабрики нужны входные данные:

- `productName`;
- `assemblyDirectory`;
- `IEnvironmentInfoProvider`.

Они естественно становятся состоянием билдера.

Например:

```csharp
internal sealed class NLogFactoryBuilder
{
    private readonly string _productName;
    private readonly string _assemblyDirectory;
    private readonly IEnvironmentInfoProvider _envInfoProvider;

    public NLogFactoryBuilder(
        string productName,
        string assemblyDirectory,
        IEnvironmentInfoProvider envInfoProvider)
    {
        ...
    }

    public LogFactory Build()
    {
        ...
    }

    private LoggingConfiguration CreateConfiguration(...)
    {
        ...
    }

    private void ApplyCommonVariables(...)
    {
        ...
    }

    private void WriteFactoryDiagnostics(...)
    {
        ...
    }
}
```

Тогда `NLogBootstrap` очень простой:

```csharp
public static IDrzLoggerFactory GetOrCreate(...)
{
    return _factories.GetOrAdd(productName, _ =>
    {
        var builder = new NLogFactoryBuilder(
            productName,
            assemblyDirectory,
            envInfoProvider);

        return new NLogLoggerFactory(builder.Build());
    });
}
```

### Когда делать `static`

Если же ты не хочешь вводить `NLogFactoryBuilder`, а просто перенесешь туда код без полей, тогда да — все методы (`CreateConfiguration`, `CreateXmlLayout`, `WriteFactoryDiagnostics` и т.д.) тоже будут `private static`.

### Что я бы выбрал

Для твоего проекта я бы выбрал **обычный (`sealed`) `NLogFactoryBuilder`**, а не `static`.

Преимущества:

- не нужно таскать `productName`, `assemblyDirectory`, `envInfoProvider` параметрами через 5–6 приватных методов;
- код становится заметно чище;
- если позже появятся новые параметры (например, `LogsDir`, `LogLevel`, `CadInfo`), они добавляются в конструктор, а не во все методы сразу.

Такое решение выглядит более масштабируемым, особенно для той архитектуры логирования, которую ты сейчас строишь.

## Prompt:
21.07.2026, 12:12:12

CreateLogFactory() есть смысл сделать обычным ?

## Response:
21.07.2026, 12:12:13

Да, если ты вводишь `NLogFactoryBuilder`, то **`CreateLogFactory()` (или лучше `Build()`) имеет смысл сделать обычным экземплярным методом**.

Я бы вообще ушел от названия `CreateLogFactory()` и сделал так:

```csharp
internal sealed class NLogFactoryBuilder
{
    public LogFactory Build()
    {
        ...
    }
}
```

Тогда:

```csharp
var builder = new NLogFactoryBuilder(
    productName,
    assemblyDirectory,
    envInfoProvider);

LogFactory logFactory = builder.Build();

return new NLogLoggerFactory(logFactory);
```

### Почему экземплярный метод лучше

Потому что он использует состояние объекта:

- `_productName`
- `_assemblyDirectory`
- `_envInfoProvider`

Тогда все вспомогательные методы тоже становятся обычными:

```csharp
private LoggingConfiguration CreateConfiguration(...)

private void ApplyCommonVariables(...)

private void WriteFactoryDiagnostics(...)

private string SafeGetAssemblyDirectory()
```

и им не нужно передавать множество параметров.

### Когда оставить `static`

Если не вводить `NLogFactoryBuilder`, а оставить всю логику в `NLogBootstrap`, тогда `CreateLogFactory()` и все вспомогательные методы должны быть `static`.

---

Для твоего объема кода (около 300 строк) я бы выбрал **`NLogFactoryBuilder` с экземплярным методом `Build()`**. Получится наиболее чистое разделение:

- `NLogBootstrap` — реестр фабрик (`ConcurrentDictionary`).
- `NLogFactoryBuilder` — создает и настраивает `LogFactory`.
- `NLogLoggerFactory` — выдает `IDrzLogger` из готового `LogFactory`.

## Prompt:
21.07.2026, 12:13:30

Статик остается только словарь?

## Response:
21.07.2026, 12:13:30

Да, именно так. Это наиболее удачное разделение.

### `NLogBootstrap` (статический)

Его единственная ответственность — хранить общий кэш фабрик.

```csharp
public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories = new();

    public static IDrzLoggerFactory GetOrCreate(...)
    {
        ...
    }
}
```

Статическими остаются:
- `ConcurrentDictionary`;
- `GetOrCreate()`.

Больше ничего.

---

### `NLogFactoryBuilder` (обычный класс)

Хранит состояние и создает `LogFactory`.

```csharp
internal sealed class NLogFactoryBuilder
{
    private readonly string _productName;
    private readonly string _assemblyDirectory;
    private readonly IEnvironmentInfoProvider _envInfoProvider;

    public LogFactory Build()
    {
        ...
    }

    private LoggingConfiguration CreateConfiguration()
    {
        ...
    }

    private void ApplyCommonVariables(...)
    {
        ...
    }

    private void WriteFactoryDiagnostics(...)
    {
        ...
    }
}
```

Все методы здесь обычные, потому что используют состояние объекта.

---

### `NLogLoggerFactory`

Только оборачивает `LogFactory`.

```csharp
public sealed class NLogLoggerFactory : IDrzLoggerFactory
{
    private readonly LogFactory _factory;

    ...
}
```

---

Такое разделение выглядит очень естественно:

- **Bootstrap** — *найти или создать*.
- **Builder** — *создать и настроить*.
- **LoggerFactory** — *выдать логер*.

Я бы только подумал над именем `NLogBootstrap`. Если его задача исключительно хранить реестр фабрик, то `NLogLoggerFactoryRegistry` или `NLogFactoryRegistry` еще точнее отражают его роль. Если же предполагается, что он останется основной точкой инициализации логирования, то `NLogBootstrap` — хорошее название.

## Prompt:
21.07.2026, 12:15:53

public static IDrzLoggerFactory GetOrCreate(...)
Обращение к экземплярному методу, покажи как

## Response:
21.07.2026, 12:15:53

Да, получится примерно так.

```csharp
public static class NLogBootstrap
{
    private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories = new();

    public static IDrzLoggerFactory GetOrCreate(
        string productName,
        string assemblyDirectory,
        IEnvironmentInfoProvider envInfoProvider = null)
    {
        return _factories.GetOrAdd(productName, _ =>
        {
            var builder = new NLogFactoryBuilder(
                productName,
                assemblyDirectory,
                envInfoProvider);

            LogFactory logFactory = builder.Build();

            return new NLogLoggerFactory(logFactory);
        });
    }
}
```

`NLogFactoryBuilder`:

```csharp
internal sealed class NLogFactoryBuilder
{
    private readonly string _productName;
    private readonly string _assemblyDirectory;
    private readonly IEnvironmentInfoProvider _envInfoProvider;

    public NLogFactoryBuilder(
        string productName,
        string assemblyDirectory,
        IEnvironmentInfoProvider envInfoProvider = null)
    {
        _productName = productName;
        _assemblyDirectory = assemblyDirectory;
        _envInfoProvider = envInfoProvider;
    }

    public LogFactory Build()
    {
        // весь код из CreateFactory()
    }
}
```

В аддоне:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(
        AddonInfo.ProductName,
        AddonInfo.AssemblyDirectory,
        new CadEnvironmentInfoProvider());

container.RegisterInstance(loggerFactory);
```

### Я бы еще изменил сигнатуру

Вместо

```csharp
Func<string> productNameProvider
Func<string> assemblyDirectoryProvider
```

я бы передавал уже готовые значения:

```csharp
string productName,
string assemblyDirectory
```

Поскольку `GetOrCreate()` вызывается один раз при инициализации аддона, ленивые `Func<string>` здесь уже не дают преимуществ, а код становится проще и понятнее.

## Prompt:
21.07.2026, 12:22:08

Как получить фабрику для передачи в библиотеку?

## Response:
21.07.2026, 12:22:08

Если фабрика уже зарегистрирована в **SimpleInjector**, то получать ее нужно **из контейнера**, а не из `NLogBootstrap` повторно.

Например, в аддоне:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetOrCreate(
        AddonInfo.ProductName,
        AddonInfo.AssemblyDirectory,
        new CadEnvironmentInfoProvider());

container.RegisterInstance(loggerFactory);
```

### Если библиотека создается контейнером

Вообще ничего делать не нужно:

```csharp
public class BackupService
{
    public BackupService(IDrzLoggerFactory loggerFactory)
    {
        ...
    }
}
```

### Если библиотека создается вручную

Тогда получить фабрику из контейнера:

```csharp
IDrzLoggerFactory loggerFactory =
    container.GetInstance<IDrzLoggerFactory>();

var backup = new BackupService(loggerFactory);
```

или, если у тебя есть `AddOnContext`:

```csharp
IDrzLoggerFactory loggerFactory =
    AddOnContext.Container.GetInstance<IDrzLoggerFactory>();

var backup = new BackupService(loggerFactory);
```

---

То есть `NLogBootstrap` нужен **только один раз** — при инициализации аддона.

Дальше источник истины — **контейнер DI**. Именно из него следует получать `IDrzLoggerFactory` для передачи в любые библиотеки или сервисы.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)