# LogFactoryInfo

**User:** dRz  
**Created:** 7/23/2026 10:21:37  
**Updated:** 7/23/2026 11:08:54  
**Exported:** 7/23/2026 11:10:40  
**Link:** [https://chatgpt.com/c/6a61c0d1-9db8-83eb-a349-2ed106355c79](https://chatgpt.com/c/6a61c0d1-9db8-83eb-a349-2ed106355c79)  

## Prompt:
23.07.2026, 10:21:36

Nlog как получить имя файла из которого загружена конфигурация?

## Response:
23.07.2026, 10:21:37

Если конфигурация была загружена из файла, то в NLog можно получить путь к нему через свойство `FileName` у `XmlLoggingConfiguration`.

```csharp
using NLog;
using NLog.Config;

var config = LogManager.Configuration;

if (config is XmlLoggingConfiguration xmlConfig)
{
    string configFile = xmlConfig.FileName;
}
```

Если используешь собственный `LogFactory`:

```csharp
LogFactory factory = ...;

if (factory.Configuration is XmlLoggingConfiguration xmlConfig)
{
    string configFile = xmlConfig.FileName;
}
```

### Важно

- `FileName` будет заполнено только если конфигурация действительно загружена из файла (`LoadConfigurationFromFile`, `XmlLoggingConfiguration(path)` и т.п.).
- Если конфигурация создана программно, `FileName` будет `null`.

Например:

```csharp
var factory = new LogFactory();
factory.Setup().LoadConfigurationFromFile(configPath);

var file = ((XmlLoggingConfiguration)factory.Configuration).FileName;
// file == configPath
```

Если тебе нужно это внутри своей библиотеки `LogBootstrap`, то лучше сделать вспомогательное свойство:

```csharp
public static string? GetConfigurationFile(LogFactory factory)
{
    return (factory.Configuration as XmlLoggingConfiguration)?.FileName;
}
```

Это будет работать для любой конфигурации, загруженной из XML-файла.

## Prompt:
23.07.2026, 10:43:44

Как получить путь к файлам лога

## Response:
23.07.2026, 10:43:44

Если речь про NLog, то универсального свойства "путь к файлам лога" нет. Нужно получить его из `FileTarget`.

Для одного файла:

```csharp
using NLog;
using NLog.Config;
using NLog.Targets;

var config = LogManager.Configuration;

var fileTarget = config.AllTargets
    .OfType<FileTarget>()
    .FirstOrDefault();

if (fileTarget != null)
{
    var logFile = fileTarget.FileName.Render(LogEventInfo.CreateNullEvent());

    // Полный путь к текущему файлу лога
}
```

Если файловых таргетов несколько:

```csharp
var files = LogManager.Configuration.AllTargets
    .OfType<FileTarget>()
    .Select(t => t.FileName.Render(LogEventInfo.CreateNullEvent()))
    .ToList();
```

### Если используется AsyncWrapper

Тогда нужно сначала "распаковать" обертку:

```csharp
var fileTargets = LogManager.Configuration.AllTargets
    .SelectMany(t =>
    {
        if (t is WrapperTargetBase wrapper && wrapper.WrappedTarget is FileTarget ft)
            return new[] { ft };

        if (t is FileTarget file)
            return new[] { file };

        return Enumerable.Empty<FileTarget>();
    });
```

### Для получения каталога

```csharp
string directory = Path.GetDirectoryName(logFile)!;
```

Метод `Render()` подставляет все переменные (`${basedir}`, `${gdc:item=LogsDir}`, `${shortdate}` и т.д.), поэтому возвращает фактический путь для текущего события. Для твоей библиотеки `LogBootstrap` это наиболее надежный способ.

## Prompt:
23.07.2026, 10:46:43

//todo logger.For_XXXX_Event

   /*

     _logger.ForErrorEvent()

       .Message("Properties is null")

       .Property("name", 10)

       .Property("null", "Properties is null")

       .Property("null", "Properties is null")

       .Exception(ex)

       .Log();

    */

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

       log.Info(msg);

   }
Надо переделать на ForInfoEvent, ForWarnEvent

## Response:
23.07.2026, 10:46:43

Да, такой Fluent API здесь будет уместен. Он позволит избавиться от ручной сборки строк и сделает запись структурированной.

Я бы сделал примерно так:

```csharp
log.ForInfoEvent()
    .Message("LogFactory initialized")
    .Property("ProductName", productName)
    .Property("ConfigSource", isFallback ? "Fallback (programmatic)" : "External (nlog.config)")
    .Property("EffectiveMinLevel", effectiveLevel)
    .Property("InternalLoggerLevel", internalLevel)
    .Log();
```

И для fallback:

```csharp
log.ForWarnEvent()
    .Message("LogFactory initialized")
    .Property("ProductName", productName)
    .Property("ConfigSource", "Fallback (programmatic)")
    .Property("EffectiveMinLevel", effectiveLevel)
    .Property("InternalLoggerLevel", internalLevel)
    .Log();
```

Либо вообще выбрать тип события один раз:

```csharp
var evt = isFallback
    ? log.ForWarnEvent()
    : log.ForInfoEvent();

evt.Message("LogFactory initialized")
   .Property("ProductName", productName)
   .Property("ConfigSource", isFallback ? "Fallback (programmatic)" : "External (nlog.config)")
   .Property("EffectiveMinLevel", effectiveLevel)
   .Property("InternalLoggerLevel", internalLevel)
   .Log();
```

### Я бы еще немного расширил событие

Поскольку это инициализация логирования, полезно добавить:

```csharp
evt.Message("LogFactory initialized")
   .Property("ProductName", productName)
   .Property("ConfigurationFile", configFile)
   .Property("LogDirectory", logDirectory)
   .Property("ConfigSource", ...)
   .Property("EffectiveMinLevel", effectiveLevel)
   .Property("InternalLoggerLevel", internalLevel)
   .Log();
```

Тогда в логе сразу будет видно:
- какой продукт инициализировался;
- из какого файла загружена конфигурация;
- куда пишутся логи;
- какой минимальный уровень используется;
- используется ли резервная (programmatic) конфигурация.

Такой формат лучше подходит для последующего анализа, чем многострочное текстовое сообщение.

## Prompt:
23.07.2026, 10:49:09

logDirectory как получить

## Response:
23.07.2026, 10:49:09

Если каталог определяется `FileTarget`, то получить его можно так:

```csharp
using NLog;
using NLog.Targets;

var fileTarget = LogManager.Configuration.AllTargets
    .OfType<FileTarget>()
    .FirstOrDefault();

string? logDirectory = null;

if (fileTarget != null)
{
    string logFile = fileTarget.FileName.Render(LogEventInfo.CreateNullEvent());
    logDirectory = Path.GetDirectoryName(logFile);
}
```

Если у тебя несколько `FileTarget`, лучше получить все каталоги:

```csharp
var logDirectories = LogManager.Configuration.AllTargets
    .OfType<FileTarget>()
    .Select(t => Path.GetDirectoryName(
        t.FileName.Render(LogEventInfo.CreateNullEvent())))
    .Where(d => !string.IsNullOrEmpty(d))
    .Distinct()
    .ToList();
```

### Если используется `AsyncWrapper`

Нужно найти вложенный `FileTarget`:

```csharp
static IEnumerable<FileTarget> GetFileTargets(LoggingConfiguration config)
{
    foreach (var target in config.AllTargets)
    {
        if (target is FileTarget file)
            yield return file;

        if (target is WrapperTargetBase wrapper &&
            wrapper.WrappedTarget is FileTarget wrapped)
            yield return wrapped;
    }
}
```

---

**Но для твоей архитектуры `LogBootstrap` я бы не вычислял каталог через NLog.** Ты ведь сам задаешь `LogsDir` при построении конфигурации (через `${gdc:item=LogsDir}` или переменную `LogsDir`). Гораздо надежнее сохранить этот путь в своем `LogFactoryInfo` или аналогичном классе и использовать его напрямую. Тогда он будет доступен независимо от того, XML-конфигурация используется или программная, и не придется анализировать таргеты.

## Prompt:
23.07.2026, 11:06:28

var file = ((XmlLoggingConfiguration)factory.Configuration).FileName;
FileName нет такого
Есть AutoReloadFileNames

## Response:
23.07.2026, 11:06:29

Это зависит от версии NLog.

В **NLog 6.x** свойство `FileName` у `XmlLoggingConfiguration` было удалено. Вместо него есть, например:

- `AutoReloadFileNames`
- `AutoReload`
- другие свойства, связанные с автоперезагрузкой.

Если конфигурация загружалась через

```csharp
factory.Setup().LoadConfigurationFromFile(configPath);
```

то API **не предоставляет** способ получить исходный `configPath` обратно.

### Что лучше сделать

Если путь к конфигурации тебе нужен позже (например, для логирования), сохрани его самостоятельно:

```csharp
string configFile = configPath;

factory.Setup().LoadConfigurationFromFile(configFile);

// ...
logger.ForInfoEvent()
    .Property("ConfigurationFile", configFile)
    .Log();
```

Либо оберни `LogFactory`:

```csharp
public sealed class LogFactoryContext
{
    public LogFactory Factory { get; }
    public string? ConfigurationFile { get; }

    public LogFactoryContext(LogFactory factory, string? configurationFile)
    {
        Factory = factory;
        ConfigurationFile = configurationFile;
    }
}
```

Это надежнее и не зависит от внутренних изменений NLog.

**Вопрос:** какую именно версию NLog ты сейчас используешь? (Например, 6.0.7, 6.0.8 и т.д.)

## Prompt:
23.07.2026, 11:08:50

6.1.3
AutoReloadFileNames возвращает список
FirstOrDefault вполне
Вряд ли может быть загружено больше одного конфига

## Response:
23.07.2026, 11:08:50

Да, для **NLog 6.1.3** это вполне рабочее решение.

`AutoReloadFileNames` содержит список файлов, за которыми NLog следит для автоперезагрузки. Если конфигурация загружена из одного `nlog.config`, то список обычно содержит единственный элемент.

Можно написать так:

```csharp
var configFile = (factory.Configuration as XmlLoggingConfiguration)?
    .AutoReloadFileNames
    .FirstOrDefault();
```

или чуть безопаснее:

```csharp
string? configFile = null;

if (factory.Configuration is XmlLoggingConfiguration xmlConfig)
{
    configFile = xmlConfig.AutoReloadFileNames.FirstOrDefault();
}
```

Для твоего случая этого достаточно. Действительно, в большинстве приложений загружается один файл конфигурации.

Единственное, я бы не полагался на то, что это **всегда** единственный файл. Теоретически NLog может отслеживать несколько файлов (например, если используются `include`), поэтому название свойства — `AutoReloadFileNames`. Но если тебе нужен именно **основной** конфигурационный файл, то `FirstOrDefault()` — разумный практический выбор.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)
