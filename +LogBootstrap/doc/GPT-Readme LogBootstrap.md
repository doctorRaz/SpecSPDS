# Readme LogBootstrap

**User:** dRz  
**Created:** 7/22/2026 14:47:35  
**Updated:** 7/23/2026 15:35:02  
**Exported:** 7/23/2026 22:29:50  
**Link:** [https://chatgpt.com/g/g-p-6a5f946f072c819184ddfa91a3cb048b/c/6a60ad65-146c-83eb-a812-a1e955ba0bc5](https://chatgpt.com/g/g-p-6a5f946f072c819184ddfa91a3cb048b/c/6a60ad65-146c-83eb-a812-a1e955ba0bc5)  

## Prompt:
22.07.2026, 14:47:35

Составь подробное readme инструкцию по использованию библиотеки

[Attachment: LogBootstrap.zip]

## Response:
22.07.2026, 14:47:35

Да. По структуре библиотеки уже можно составить полноценный `README.md`. Ниже пример документации, которую я бы положил в репозиторий.

---

# LogBootstrap

**LogBootstrap** — библиотека для быстрой инициализации системы логирования на базе **NLog**.

Основные возможности:

- автоматическое создание `LogFactory`;
- одна фабрика логирования на продукт (`ProductName`);
- полная изоляция логирования разных дополнений;
- настройка через файлы без перекомпиляции;
- поддержка внутреннего логирования NLog (Internal Logger);
- работа через собственные интерфейсы `IDrzLogger` и `IDrzLoggerFactory`, без зависимости остальных библиотек от NLog.

---

# Возможности

- кэширование `LogFactory` по имени продукта;
- автоматическое создание конфигурации NLog;
- асинхронная запись логов;
- чтение уровня логирования из файла;
- чтение уровня InternalLogger из файла;
- вывод InternalLogger:
  - в Output Visual Studio;
  - в отдельный файл;
  - в Console;
- отсутствие зависимости прикладного кода от NLog.

---

# Структура

```
LogBootstrap
│
├── Builder
│   ├── NLogFactoryBuilder
│   └── LogKeys
│
├── Diagnostics
│   ├── InternalLoggerHelpers
│   ├── LogLevelReader
│   └── OutputDebugTextWriter
│
├── drzNLog
│   ├── NLogLoggerFactory
│   ├── NLogAdapter
│   └── NLogEventBuilderAdapter
│
└── NLogBootstrap
```

---

# Быстрый старт

Получение фабрики логирования:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetLoggerFactory(addOnInfo);
```

Получение логгера:

```csharp
IDrzLogger logger =
    loggerFactory.GetLogger<MyClass>();
```

Использование:

```csharp
logger.Trace("Trace");

logger.Debug("Debug");

logger.Info("Started");

logger.Warn("Warning");

logger.Error("Error");

logger.Fatal("Fatal");
```

или

```csharp
try
{
}
catch(Exception ex)
{
    logger.Error(ex, "Unexpected error");
}
```

---

# Получение фабрики

Главная точка входа:

```csharp
NLogBootstrap.GetLoggerFactory(IAddOnInfo addOnInfo)
```

Фабрика создается только один раз.

При повторных вызовах возвращается уже существующий экземпляр.

Ключом является

```
IAddOnInfo.ProductName
```

Таким образом каждое дополнение имеет собственную независимую систему логирования.

---

# IAddOnInfo

Для создания фабрики используются:

```csharp
ProductName
AssemblyDirectory
```

`ProductName`

используется

- для имени логов;
- для кэширования фабрики.

`AssemblyDirectory`

используется для поиска

```
log.level
diagnostic.mode
```

и хранения логов.

---

# Настройка уровня логирования

Уровень задается файлом

```
log.level
```

в каталоге сборки.

Пример:

```
Trace
```

или

```
Debug
```

или

```
Info
```

Поддерживаются стандартные уровни NLog:

```
Trace
Debug
Info
Warn
Error
Fatal
Off
```

Если файл отсутствует

```
Off
```

логирование отключается.

Если файл существует, но пустой,

используется уровень

```
Trace
```

---

# Diagnostic Mode

Внутренний логгер NLog управляется файлом

```
diagnostic.mode
```

Например

```
Trace
```

При отсутствии файла InternalLogger полностью отключен.

---

# Internal Logger

При включении Diagnostic Mode библиотека автоматически включает Internal Logger.

Сообщения могут выводиться:

- Output Window Visual Studio;
- Console;
- отдельный лог-файл.

Это позволяет диагностировать проблемы настройки NLog.

---

# Кэширование фабрик

Библиотека хранит фабрики

```csharp
ConcurrentDictionary<string, IDrzLoggerFactory>
```

где ключ

```
ProductName
```

Это гарантирует

- отсутствие повторной инициализации;
- минимальные накладные расходы;
- потокобезопасность.

---

# Потокобезопасность

Создание фабрики выполняется через

```csharp
ConcurrentDictionary.GetOrAdd()
```

Поэтому несколько потоков никогда не создадут две фабрики одного продукта.

---

# Независимость библиотек

Остальные проекты используют только интерфейсы

```csharp
IDrzLogger
```

и

```csharp
IDrzLoggerFactory
```

NLog используется исключительно внутри LogBootstrap.

Это позволяет в будущем заменить реализацию логирования без изменения остальных библиотек.

---

# Типичный сценарий использования

При запуске дополнения:

```csharp
IDrzLoggerFactory loggerFactory =
    NLogBootstrap.GetLoggerFactory(addOnInfo);

IDrzLogger logger =
    loggerFactory.GetLogger<Startup>();

logger.Info("Application started");
```

Далее фабрика передается через DI или хранится в сервисах приложения.

---

# Конфигурационные файлы

Поддерживаются следующие файлы:

| Файл | Назначение |
|-------|------------|
| `log.level` | уровень основного логирования |
| `diagnostic.mode` | уровень Internal Logger |

Оба файла должны располагаться рядом со сборкой дополнения.

---

# Архитектура

```
Application
      │
      ▼
NLogBootstrap
      │
      ▼
NLogFactoryBuilder
      │
      ▼
LogFactory
      │
      ▼
NLogLoggerFactory
      │
      ▼
IDrzLogger
```

---

# Рекомендации

- Вызывать `NLogBootstrap.GetLoggerFactory()` один раз при инициализации дополнения.
- Использовать один `ProductName` для всех сборок одного продукта.
- Не обращаться к NLog напрямую из прикладного кода.
- Для диагностики временно создавать файл `diagnostic.mode`, а после завершения диагностики удалять его.
- Управлять уровнем логирования через файл `log.level`, не изменяя код приложения.

Такой README уже подходит для публикации в GitHub/NuGet. После завершения библиотеки его можно дополнить разделами **Установка**, **Dependency Injection**, **Формат логов**, **Переменные NLog** и **Примеры конфигурации**.

## Prompt:
22.07.2026, 14:52:50

Добавь описание про файлы конфигурации
Shared\*. Config

## Response:
22.07.2026, 14:52:51

Я бы сделал отдельный раздел, поскольку это одна из ключевых возможностей библиотеки.

---

## Конфигурационные файлы

При создании `LogFactory` библиотека автоматически ищет конфигурационные файлы в каталоге **Shared**, расположенном рядом с каталогом дополнения.

Пример структуры:

```text
AddOn
│
├── MyAddOn.dll
├── log.level
├── diagnostic.mode
│
└── Shared
    ├── NLog.config
    ├── NLog.targets.config
    ├── NLog.layouts.config
    ├── NLog.rules.config
    └── ...
```

Каталог **Shared** содержит общие файлы конфигурации NLog, которые могут использоваться сразу несколькими дополнениями.

Это позволяет:

- хранить всю конфигурацию логирования в одном месте;
- использовать одинаковые правила логирования во всех продуктах;
- изменять формат логов без перекомпиляции приложений;
- переиспользовать цели (Targets), Layout'ы и Rules между проектами.

---

## Поддерживаемые файлы

При инициализации библиотеки автоматически загружаются все файлы

```text
Shared\*.config
```

в порядке, определяемом библиотекой.

Обычно конфигурация разбивается на несколько файлов:

| Файл | Назначение |
|------|------------|
| `NLog.config` | Основная конфигурация |
| `NLog.targets.config` | Описание Targets (File, Console и др.) |
| `NLog.layouts.config` | Общие Layout и переменные |
| `NLog.rules.config` | Правила маршрутизации сообщений |
| `NLog.variables.config` | Общие переменные NLog |
| `NLog.extensions.config` | Подключение расширений NLog |

Количество файлов не ограничено.

---

## Преимущества разделения конфигурации

Использование нескольких файлов позволяет:

- уменьшить размер основного `NLog.config`;
- разделить конфигурацию по назначению;
- повторно использовать общие настройки;
- упростить сопровождение больших проектов;
- обновлять отдельные части конфигурации независимо друг от друга.

---

## Пользовательская конфигурация

Любой файл, помещённый в каталог

```text
Shared
```

с расширением

```text
*.config
```

будет автоматически подключён при создании `LogFactory`.

Это позволяет добавлять новые правила, Targets и Layout без изменения исходного кода библиотеки.

---

Я бы также добавил раздел **"Приоритет конфигурации"**, где описать, в каком порядке библиотека объединяет:
1. встроенную конфигурацию;
2. `Shared\*.config`;
3. программные настройки (`NLogFactoryBuilder`);
4. файлы `log.level` и `diagnostic.mode`.

Такой раздел сразу снимает вопросы о том, какая настройка имеет приоритет.

## Prompt:
22.07.2026, 18:23:28

Какие предложения по рефакторингу?

## Response:
22.07.2026, 18:23:28

По текущей структуре библиотека уже выглядит достаточно чистой. Я бы предложил не столько переписывать код, сколько немного улучшить архитектуру и разделение ответственности.

### 1. Переименовать `drzNLog`

Это единственная папка, которая выбивается из общей структуры.

Сейчас:

```text
drzNLog
```

Лучше:

```text
Adapters
```

или

```text
NLog
```

Тогда получится:

```text
Builder
Diagnostics
NLog
Shared
```

---

### 2. `Builder` разделить

Сейчас в `Builder` лежат:

- создание `LogFactory`;
- константы.

Я бы сделал

```text
Bootstrap
    NLogBootstrap.cs

Configuration
    NLogFactoryBuilder.cs
    LogKeys.cs
```

или

```text
Bootstrap
Builder
Configuration
```

Так будет понятнее, где точка входа, а где построение конфигурации.

---

### 3. Вынести чтение файлов настроек

Сейчас есть

```
LogLevelReader
```

Но библиотека уже использует несколько файлов:

- log.level
- diagnostic.mode
- Shared/*.config

Логичнее сделать единый сервис

```text
Configuration

ConfigurationReader
    ReadLogLevel()
    ReadDiagnosticLevel()
    EnumerateConfigs()
```

Тогда вся работа с файловой системой окажется в одном месте.

---

### 4. InternalLoggerHelpers

Сейчас название немного размыто.

Если он только включает InternalLogger, лучше

```
InternalLoggerConfigurator
```

или

```
InternalLoggerSetup
```

Helpers обычно означает набор статических утилит.

---

### 5. LogKeys

Если там только строки

```
LogsDir
LogLevel
ProductName
```

лучше назвать

```
NLogConstants
```

или

```
NLogVariables
```

Потому что "Keys" не очень понятно.

---

### 6. Добавить Options

Сейчас bootstrap, вероятно, принимает только `IAddOnInfo`.

Через некоторое время наверняка появятся параметры вроде:

- использовать AsyncWrapper
- писать InternalLogger
- использовать Shared
- создавать каталог автоматически
- имя файла

Лучше заранее предусмотреть

```csharp
NLogBootstrap.Initialize(
    addOnInfo,
    options);
```

где

```csharp
class NLogBootstrapOptions
{
    bool UseSharedConfiguration;
    bool EnableInternalLogger;
    bool CreateDirectories;
    bool UseAsyncWrapper;
}
```

Тогда развитие библиотеки не будет ломать API.

---

### 7. Сделать публичной только одну точку входа

Если пользователю достаточно

```csharp
NLogBootstrap.GetLoggerFactory(...)
```

то остальные классы (`Builder`, `Diagnostics`) можно сделать `internal`.

Внешнему коду не нужно знать, как собирается конфигурация.

---

### 8. Добавить интерфейсы только там, где ожидается расширение

Например:

```csharp
ILogConfigurationLoader
```

```csharp
IInternalLoggerConfigurator
```

Это позволит позже заменить реализацию без изменения Bootstrap.

---

### 9. Разделить Diagnostics

Можно получить более понятную структуру:

```text
Diagnostics
    InternalLoggerConfigurator.cs
    LogLevelReader.cs

IO
    OutputDebugTextWriter.cs
```

`OutputDebugTextWriter` не относится к диагностике — это адаптер `TextWriter`.

---

### 10. Самое важное

Мне кажется, библиотека начинает выполнять две независимые задачи:

1. **Bootstrap NLog**
2. **Работа с конфигурацией**

Я бы уже сейчас выделил отдельный слой:

```text
LogBootstrap
│
├── Bootstrap
├── Configuration
├── Diagnostics
├── NLog
└── Shared
```

где:

- **Bootstrap** — публичный API;
- **Configuration** — поиск и объединение `Shared/*.config`, чтение `log.level` и `diagnostic.mode`;
- **Diagnostics** — InternalLogger;
- **NLog** — адаптеры и фабрика.

Это разделение хорошо масштабируется: если позже появятся поддержка Serilog или другого провайдера, слой `Configuration` останется практически без изменений, а потребуется заменить только реализацию провайдера логирования.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)