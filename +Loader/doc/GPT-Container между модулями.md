# Container между модулями

**User:** dRz  
**Created:** 7/24/2026 6:01:12  
**Updated:** 7/24/2026 7:04:38  
**Exported:** 7/24/2026 7:08:08  
**Link:** [https://chatgpt.com/c/6a62d4b7-e3a4-83eb-a3d1-f4398b99cb62](https://chatgpt.com/c/6a62d4b7-e3a4-83eb-a3d1-f4398b99cb62)  

## Prompt:
24.07.2026, 06:01:12

Loader создает Addon Context
Ищет подходящую сборку в соответствии с версией nanoCad, например SpecSPDS.23
И делает ей loadfrom
В SpecSPDS.23 создастся свой контейнер или можно как-то получить созданный загрузчиком?

## Response:
24.07.2026, 06:01:12

Да. Более того, я бы именно так и сделал - **контейнер должен быть один**, созданный Loader'ом. Это соответствует идее Composition Root: контейнер создается один раз при старте приложения. ([Simple Injector](https://docs.simpleinjector.org/en/latest/using.html?utm_source=chatgpt.com))

Схема может выглядеть так:

```text
Loader
 ├─ Container
 ├─ AddonContext
 └─ LoadFrom("SpecSPDS.23.dll")
              │
              ▼
        SpecSPDS.23
```

`SpecSPDS.23` не создает новый `Container`, а получает уже существующий.

Например:

```csharp
public sealed class AddonContext
{
    public Container Container { get; }

    public AddonContext(Container container)
    {
        Container = container;
    }
}
```

В Loader:

```csharp
var container = new Container();

var context = new AddonContext(container);

var asm = Assembly.LoadFrom(path);

var startupType = asm.GetType("SpecSPDS.Startup");
var startup = (IAddonStartup)Activator.CreateInstance(startupType);

startup.Initialize(context);
```

В адаптере:

```csharp
public class Startup : IAddonStartup
{
    public void Initialize(IAddonContext context)
    {
        context.Container.Register<...>();

        context.Container.Verify();

        var app = context.Container.GetInstance<SpecApplication>();
        app.Run();
    }
}
```

### Еще лучше

Я бы вообще **не отдавал наружу `Container`**.

В `IAddonContext` оставить только:

```csharp
public interface IAddonContext
{
    IServiceProvider Services { get; }
}
```

или

```csharp
public interface IAddonContext
{
    T GetService<T>();
}
```

Тогда Loader остается единственным местом, которое знает о SimpleInjector. Если однажды захочешь заменить его на Autofac или Microsoft DI, остальные проекты менять не придется.

### Для твоей архитектуры

Учитывая, что у тебя уже есть `AddonContext`, `LibraryContext`, `IDrzLoggerFactory`, `IMessageService` и т.д., я бы сделал так:

- **Loader**
  - создает `Container`;
  - регистрирует общие сервисы;
  - создает `AddonContext`;
  - загружает `SpecSPDS.23`.

- **SpecSPDS.23**
  - получает `AddonContext`;
  - **добавляет свои регистрации в тот же контейнер**;
  - разрешает корневой объект (`SpecApplication`, `Plugin`, `CommandManager` и т.п.).

Новый контейнер создавать не стоит. Иначе получится два разных Singleton-графа (два логгера, два `AppSettings`, два `DocumentService` и т.д.), что обычно приводит к проблемам. ([Simple Injector](https://docs.simpleinjector.org/en/latest/using.html?utm_source=chatgpt.com))

---

**Sources:**

- [Using Simple Injector — Simple Injector 5 documentation](https://docs.simpleinjector.org/en/latest/using.html?utm_source=chatgpt.com)

## Prompt:
24.07.2026, 06:05:20

Адаптер ничего не знает о лоадере, лоадер ничего не знает о адаптере
Ищет по имени файла и загружает

## Response:
24.07.2026, 06:05:20

Это даже лучше. Тогда связь должна быть не через ссылки между проектами, а через **общую контрактную сборку**.

Например:

```text
drz.Abstractions
 ├── IAddonContext
 ├── IAddonStartup
 └── ...
         ▲
         │
 ┌───────┴────────┐
 │                │
Loader       SpecSPDS.23
```

Ни `Loader` не знает о `SpecSPDS.23`, ни `SpecSPDS.23` не знает о `Loader`. Они знают только о `drz.Abstractions`.

Сценарий работы:

1. Loader создает `Container`.
2. Loader создает `AddonContext`.
3. Loader делает `Assembly.LoadFrom(...)`.
4. Loader ищет в сборке тип, реализующий `IAddonStartup`.
5. Создает его через `Activator.CreateInstance()`.
6. Вызывает `Initialize(IAddonContext context)`.

Поиск можно сделать так:

```csharp
var startupType = assembly
    .GetTypes()
    .FirstOrDefault(t =>
        typeof(IAddonStartup).IsAssignableFrom(t) &&
        !t.IsInterface &&
        !t.IsAbstract);

if (startupType == null)
    throw new InvalidOperationException("Startup not found.");

var startup = (IAddonStartup)Activator.CreateInstance(startupType);
startup.Initialize(context);
```

Адаптер при этом никак не знает, кто его загрузил.

---

### Но есть еще более интересный вариант

Если ты уже используешь `LibraryContext`, то можно вообще **не передавать `AddonContext` параметром**.

Loader после создания контейнера делает один вызов:

```csharp
LibraryContext.Initialize(addonContext);
```

Затем загружает адаптер.

В адаптере:

```csharp
var logger = LibraryContext.Services.LoggerFactory.GetLogger<Startup>();
```

или

```csharp
var container = LibraryContext.Container;
```

То есть адаптер вообще не имеет точки входа с параметром. Он берет контекст из статического класса.

---

### Однако я бы предпочел гибридный вариант

Чтобы не делать полностью глобальное состояние:

- Loader вызывает `LibraryContext.Initialize(addonContext)` **до** загрузки адаптера.
- Адаптер имеет обычный `Startup`, который вызывается без параметров:

```csharp
public interface IAddonStartup
{
    void Initialize();
}
```

Внутри `Initialize()` он получает все через `LibraryContext`.

Так Loader по-прежнему ничего не знает об адаптере, адаптер ничего не знает о Loader, а общий контейнер остается единственным на весь процесс. Это хорошо вписывается в твою текущую архитектуру с `LibraryContext` и исключает создание второго контейнера.

## Prompt:
24.07.2026, 06:27:43

Другие мои приложения PlotSPDS никак не связаны с этим, у них будет свой контейнер?

## Response:
24.07.2026, 06:27:43

Да, и это как раз правильная архитектура.

Если `SpecSPDS` и `PlotSPDS` — это **независимые аддоны**, то каждый из них должен иметь **свой контейнер DI**.

```text
nanoCAD.exe
│
├── Loader
│
├── SpecSPDS
│   ├── Container #1
│   ├── Logger
│   ├── Settings
│   └── Services
│
└── PlotSPDS
    ├── Container #2
    ├── Logger
    ├── Settings
    └── Services
```

Такой подход дает несколько преимуществ:

- каждый аддон полностью изолирован;
- можно использовать разные версии библиотек и разные регистрации;
- singleton'ы являются singleton **только внутри аддона**, а не всего процесса;
- один аддон не может случайно изменить регистрацию другого.

### Тогда возникает вопрос: что делать с Loader?

Loader не должен иметь "свой" контейнер вообще.

Его задача только:
1. определить версию nanoCAD;
2. найти нужную сборку;
3. загрузить ее;
4. вызвать точку входа.

Контейнер должен создавать **сам адаптер** (`SpecSPDS.23`, `PlotSPDS.23` и т.д.), потому что именно он знает, какие сервисы нужно зарегистрировать.

Например:

```text
Loader
    ↓
LoadFrom(SpecSPDS.23.dll)
    ↓
Startup.Initialize()
    ↓
Container = new Container()
    ↓
Register...
    ↓
Run()
```

И аналогично для `PlotSPDS`.

### Когда имеет смысл общий контейнер?

Только если `SpecSPDS` и `PlotSPDS` являются модулями **одного приложения** и должны совместно использовать экземпляры сервисов (например, общий `DocumentService`, общий кэш, общий `CommandManager`, общий `Settings`).

По тому, что ты описывал раньше, это разные продукты, поэтому **отдельный контейнер на каждый аддон** выглядит наиболее правильным решением.

## Prompt:
24.07.2026, 06:28:10

У PlotSPDS свой лоадер

## Response:
24.07.2026, 06:28:11

Тогда схема становится совсем простой и логичной.

```text
SpecSPDS
├── SpecLoader
└── SpecSPDS.23
    └── Container

PlotSPDS
├── PlotLoader
└── PlotSPDS.23
    └── Container
```

То есть:

- **SpecLoader** создает контейнер только для `SpecSPDS`.
- **PlotLoader** создает контейнер только для `PlotSPDS`.
- Контейнеры никак не пересекаются, даже если оба аддона одновременно загружены в одном процессе nanoCAD.

Это означает, что:

- `IDrzLoggerFactory` у каждого продукта свой.
- `ISettingsService` свой.
- `IMessageService` свой.
- Все `Singleton` существуют в пределах своего контейнера.

Это как раз соответствует модели SimpleInjector: контейнер является границей приложения (в твоем случае — границей одного аддона).

Единственное, что я бы изменил в ранее обсуждаемой архитектуре: **Loader не стоит делать владельцем контейнера**.

Лучше так:

1. Loader загружает адаптер.
2. Находит `IAddonStartup`.
3. Вызывает `Initialize()`.
4. **Адаптер** создает свой контейнер, регистрирует сервисы и запускается.

Тогда Loader остается универсальным и не знает вообще ничего о DI. Его единственная задача — найти и загрузить правильную сборку для текущей версии nanoCAD. Это делает его максимально простым и позволяет использовать один и тот же шаблон загрузчика для разных продуктов (`SpecLoader`, `PlotLoader` и других).

## Prompt:
24.07.2026, 06:40:07

Хотелось бы в loader вести логи и выводить сообщения.
Или это лишнее?

## Response:
24.07.2026, 06:40:07

Не лишнее. Наоборот, **Loader — это единственное место, где можно диагностировать проблемы загрузки**, когда сам аддон еще не запустился.

Я бы разделил ответственность так:

### Loader

Минимальный набор сервисов:

- `ILogger`
- `IMessageService` (или просто вывод в командную строку)
- `IAssemblyLoader`

Логировать:

- старт загрузчика;
- версию nanoCAD;
- поиск подходящей сборки;
- путь к найденной DLL;
- успешную загрузку;
- все исключения (`FileNotFoundException`, `BadImageFormatException`, `ReflectionTypeLoadException` и т.д.).

Показывать пользователю только критические ошибки:

```
Не удалось загрузить SpecSPDS.
Подробнее см. файл журнала:
...\Logs\Loader.log
```

### Адаптер

После успешной загрузки уже работает полноценная инфраструктура:

- `IDrzLogger`;
- `IUserNotificationService`;
- настройки;
- DI;
- обновления;
- и т.д.

---

### Контейнер в Loader

Я бы все-таки сделал **небольшой контейнер** и в Loader.

Не такой, как в адаптере, а только для инфраструктуры загрузчика:

```text
SpecLoader
 ├── Container
 │   ├── ILogger
 │   ├── IMessageService
 │   ├── IAssemblyResolver
 │   └── IAssemblyLoader
 └── Load SpecSPDS.23
```

А затем адаптер создает **свой** контейнер.

Получается два контейнера, но они обслуживают **разные приложения**:

- контейнер Loader живет несколько секунд и занимается только загрузкой;
- контейнер SpecSPDS живет до закрытия nanoCAD.

Это не нарушение принципов DI, потому что они не пересекаются.

### Что я бы не делал

Не стал бы пытаться передавать контейнер Loader в адаптер. Тогда Loader начнет знать о сервисах адаптера, а адаптер — зависеть от состава контейнера Loader. Ты как раз стремишься к полной независимости компонентов, и такая связь будет лишней.

Для твоей архитектуры оптимально выглядит именно такая схема:
- у каждого продукта (`SpecLoader`, `PlotLoader`) есть свой небольшой контейнер для загрузки и диагностики;
- после загрузки каждый адаптер создает собственный основной контейнер со всеми сервисами.

## Prompt:
24.07.2026, 06:42:57

Лоадер остается в памяти до закрытия нанокад, почему ты сказал что его контейнер живет меньше?

## Response:
24.07.2026, 06:42:57

Да, ты прав. В твоем случае я неверно предположил жизненный цикл.

Если Loader загружается nanoCAD при старте и остается в памяти до завершения процесса, то **его контейнер тоже живет до закрытия nanoCAD**.

Тогда получается:

```text
nanoCAD
│
├── SpecLoader
│   └── Container A
│
└── SpecSPDS.23
    └── Container B
```

Оба контейнера живут одинаково долго.

### Тогда возникает другой вопрос

Нужны ли вообще **два контейнера**?

Если Loader должен:
- логировать;
- показывать сообщения;
- работать с обновлениями;
- иметь доступ к настройкам;

то он уже превращается в полноценное приложение со своей инфраструктурой.

Тогда есть два варианта.

**Вариант 1. Два независимых контейнера.**
- Loader обслуживает только себя.
- Адаптер обслуживает только себя.
- Общих singleton нет.

Это самая независимая архитектура.

**Вариант 2. Один контейнер.**
- Loader создает контейнер.
- Передает его адаптеру.
- Адаптер лишь добавляет свои регистрации.

Именно этот вариант мы обсуждали в начале.

---

### Что бы я выбрал для твоего проекта

С учетом того, что у тебя:
- общий `LoggerFactory`;
- общий `MessageService`;
- общий `UpdateService`;
- общий `RuntimeInfo`;
- общий `AppSettings`;

я бы вернулся к **одному контейнеру на продукт**.

То есть:

```text
SpecLoader
    │
    ├── Container
    │
    ├── Logger
    ├── MessageService
    ├── UpdateService
    ├── RuntimeInfo
    │
    └── Load SpecSPDS.23
            │
            └── Register Spec services
```

`PlotLoader` при этом создаст **свой** контейнер.

В итоге получится:

- `SpecLoader` + `SpecSPDS` → **один контейнер**.
- `PlotLoader` + `PlotSPDS` → **другой контейнер**.

Мне кажется, это наиболее чистая архитектура для твоей системы: один контейнер на продукт, независимо от того, что продукт разделен на Loader и Adapter. Это избавляет от дублирования инфраструктурных сервисов и делает их общими внутри одного продукта, сохраняя полную изоляцию между разными продуктами.

## Prompt:
24.07.2026, 06:45:23

В адаптере будет свой экземпляр addoninfo
Можно сделать через concurent dictionary ключ AppTile

## Response:
24.07.2026, 06:45:23

Да, это хорошее решение.

Если `SpecLoader` и `SpecSPDS` работают с одним продуктом, то `AddonInfo` должен быть общим для них. Если же в процессе одновременно загружены `SpecSPDS` и `PlotSPDS`, то у каждого продукта должен быть свой экземпляр `AddonInfo`.

Для этого как раз подходит `ConcurrentDictionary`.

Например:

```csharp
public static class AddonContextRegistry
{
    private static readonly ConcurrentDictionary<string, IAddonContext> _contexts = new();

    public static void Register(IAddonContext context)
    {
        _contexts[context.AddonInfo.AppTitle] = context;
    }

    public static IAddonContext Get(string appTitle)
    {
        return _contexts[appTitle];
    }
}
```

Или даже лучше использовать не строку, а уникальный идентификатор продукта (`ProductId`, `Guid`), если он у тебя есть. `AppTitle` тоже подойдет, если он гарантированно уникален.

### Но я бы задумался, почему адаптер создает свой `AddonInfo`

Если Loader уже знает:
- название продукта;
- путь к продукту;
- версию;
- каталоги;
- настройки;

то логичнее создать `AddonInfo` **один раз** в Loader и передать его адаптеру через `IAddonContext`.

Тогда будет:

```
SpecLoader
    └── AddonInfo (единственный экземпляр)
            │
            ▼
       SpecSPDS
```

А не:

```
SpecLoader ── AddonInfo #1
SpecSPDS  ── AddonInfo #2
```

Даже если значения совпадают, наличие двух экземпляров может привести к расхождению состояния, если в будущем в `AddonInfo` появятся вычисляемые или изменяемые свойства.

Если же есть причина, по которой адаптер обязан создавать `AddonInfo` самостоятельно (например, Loader вообще ничего не знает о структуре продукта), то `ConcurrentDictionary<string, IAddonContext>` или `ConcurrentDictionary<string, AddonInfo>` — вполне подходящий способ хранить контексты по ключу продукта.

## Prompt:
24.07.2026, 06:46:58

У адаптера и лоадера разные title
Лоадер specspdsn.loader
Адаптер specspdsn.23

## Response:
24.07.2026, 06:46:59

Тогда `AppTitle` использовать как ключ я бы не стал.

В твоем случае:

- Loader: `SpecSPDS.Loader`
- Adapter: `SpecSPDS.23`

Это **разные сборки**, поэтому `AppTitle` отражает имя сборки, а не продукта.

Лучше ввести отдельное понятие — **ProductName** (или `ProductId`), которое одинаково для всех сборок одного продукта.

Например:

```text
ProductName = "SpecSPDS"

SpecSPDS.Loader
SpecSPDS.23
SpecSPDS.Resources
SpecSPDS.Updater
```

И для другого продукта:

```text
ProductName = "PlotSPDS"

PlotSPDS.Loader
PlotSPDS.23
PlotSPDS.Resources
```

Тогда реестр выглядит так:

```csharp
ConcurrentDictionary<string, IAddonContext>
```

где ключ — именно `ProductName`.

Такой ключ останется неизменным независимо от:
- версии nanoCAD (`.20`, `.23`, `.26`);
- Loader/Adapter;
- дополнительных сборок.

### Еще лучше

Насколько я помню твою архитектуру, у тебя уже есть класс `AddonInfo`. Я бы добавил в него два разных свойства:

```csharp
public sealed class AddonInfo
{
    public string ProductName { get; }   // SpecSPDS
    public string AssemblyName { get; }  // SpecSPDS.23
    public Version Version { get; }
}
```

или даже:

```csharp
public string ProductId { get; }     // SpecSPDS
public string ModuleName { get; }    // SpecSPDS.Loader / SpecSPDS.23
```

Тогда:
- **ProductName/ProductId** — для поиска контекста, логов, настроек, обновлений.
- **AssemblyName/ModuleName** — для диагностики и отражения фактической загруженной сборки.

Такое разделение делает модель гораздо понятнее и избавляет от привязки логики к именам DLL.

## Prompt:
24.07.2026, 06:47:57

Ты путаешь, для каждого продукта свой контейнер

## Response:
24.07.2026, 06:47:57

Да, ты прав. Я смешал понятия **продукт** и **модуль**.

Если у тебя:

- `SpecSPDS.Loader` + `SpecSPDS.23` = **один продукт** → **один контейнер**.
- `PlotSPDS.Loader` + `PlotSPDS.23` = **другой продукт** → **другой контейнер**.

То никакой `ConcurrentDictionary` для хранения контейнеров **внутри одного продукта** вообще не нужен.

Получается так:

```text
nanoCAD

SpecSPDS.Loader
    └── Container (единственный)
            │
            ├── Loader services
            └── SpecSPDS.23 services

PlotSPDS.Loader
    └── Container (единственный)
            │
            ├── Loader services
            └── PlotSPDS.23 services
```

Каждый Loader владеет своим контейнером и передает его своему адаптеру.

Поэтому вопрос сводится не к хранению нескольких контейнеров, а к тому, **как адаптер получает контейнер**, если Loader и Adapter ничего не знают друг о друге.

Именно эту задачу и нужно решить. На мой взгляд, самый чистый вариант — через общий контракт (`IAddonContext` или `IServiceProvider`), который Loader передает в точку входа адаптера. Тогда:
- Loader знает только об интерфейсе.
- Adapter знает только об интерфейсе.
- Контейнер остается один на продукт и разделяется между Loader и Adapter.

## Prompt:
24.07.2026, 06:49:08

Напиши интерфейс
Реализацию
Пример применения
Код подробно комментируй

## Response:
24.07.2026, 06:49:09

Я бы сделал так.

## 1. Общий интерфейс (drz.Abstractions)

```csharp
/// <summary>
/// Точка входа аддона.
///
/// Интерфейс находится в общей библиотеке, на которую ссылаются
/// и Loader, и Adapter.
///
/// Благодаря этому:
/// - Loader ничего не знает о реализации адаптера;
/// - Adapter ничего не знает о Loader.
/// </summary>
public interface IAddonStartup
{
    /// <summary>
    /// Выполняет инициализацию аддона.
    ///
    /// На момент вызова:
    /// - контейнер уже создан Loader;
    /// - общие сервисы зарегистрированы;
    /// - адаптер должен зарегистрировать только свои сервисы.
    /// </summary>
    /// <param name="context">
    /// Контекст приложения, содержащий контейнер и общие сервисы.
    /// </param>
    void Initialize(IAddonContext context);
}
```

---

## 2. Интерфейс контекста

```csharp
/// <summary>
/// Контекст продукта.
///
/// Создается Loader один раз.
/// Используется всеми сборками продукта.
/// </summary>
public interface IAddonContext
{
    /// <summary>
    /// Контейнер зависимостей.
    /// Один экземпляр на весь продукт.
    /// </summary>
    Container Container { get; }

    /// <summary>
    /// Информация о продукте.
    /// </summary>
    IAddonInfo AddonInfo { get; }
}
```

---

## 3. Реализация контекста (Loader)

```csharp
/// <summary>
/// Реализация контекста продукта.
/// </summary>
public sealed class AddonContext : IAddonContext
{
    /// <summary>
    /// Создает новый контекст.
    /// </summary>
    public AddonContext(
        Container container,
        IAddonInfo addonInfo)
    {
        Container = container;
        AddonInfo = addonInfo;
    }

    /// <inheritdoc/>
    public Container Container { get; }

    /// <inheritdoc/>
    public IAddonInfo AddonInfo { get; }
}
```

---

## 4. Loader

```csharp
// Создаем контейнер.
var container = new Container();

// Регистрируем инфраструктуру Loader.
container.RegisterSingleton<IDrzLoggerFactory, DrzLoggerFactory>();
container.RegisterSingleton<IMessageService, MessageService>();
container.RegisterSingleton<IUpdateService, UpdateService>();

// Создаем информацию о продукте.
var addonInfo = new AddonInfo(...);

// Создаем общий контекст.
var context = new AddonContext(container, addonInfo);

// Загружаем адаптер.
Assembly assembly = Assembly.LoadFrom(adapterPath);

// Находим класс, реализующий IAddonStartup.
Type startupType = assembly
    .GetTypes()
    .First(t => typeof(IAddonStartup).IsAssignableFrom(t));

// Создаем экземпляр.
var startup =
    (IAddonStartup)Activator.CreateInstance(startupType);

// Передаем контекст.
startup.Initialize(context);
```

---

## 5. Adapter

```csharp
/// <summary>
/// Точка входа адаптера.
/// </summary>
public sealed class Startup : IAddonStartup
{
    public void Initialize(IAddonContext context)
    {
        // Получаем контейнер Loader.
        Container container = context.Container;

        // Регистрируем сервисы,
        // специфичные только для данного адаптера.

        container.RegisterSingleton<IPlotService, PlotService>();

        container.RegisterSingleton<ISpecService, SpecService>();

        container.RegisterSingleton<CommandManager>();

        // Проверяем корректность регистрации.
        container.Verify();

        // Запускаем приложение.
        var manager =
            container.GetInstance<CommandManager>();

        manager.Initialize();
    }
}
```

---

## Что в итоге получится

```text
nanoCAD
│
└── SpecSPDS.Loader
    │
    ├── Container
    │   ├── LoggerFactory
    │   ├── MessageService
    │   ├── UpdateService
    │   ├── RuntimeInfo
    │   └── ...
    │
    ├── AddonContext
    │
    └── LoadFrom(...)
            │
            ▼
      SpecSPDS.23
            │
            ├── Register PlotService
            ├── Register Commands
            ├── Register Documents
            └── GetInstance<CommandManager>()
```

### Единственное изменение, которое я бы внес

Я бы **не делал зависимость `drz.Abstractions` от `SimpleInjector`**.

Вместо

```csharp
Container Container { get; }
```

лучше определить интерфейс, например:

```csharp
public interface IServiceResolver
{
    T GetInstance<T>();

    void RegisterSingleton<TService, TImplementation>()
        where TImplementation : class, TService
        where TService : class;
}
```

или хотя бы использовать `IServiceProvider`.

Тогда `drz.Abstractions` не будет знать, что внутри используется SimpleInjector, и при необходимости контейнер можно будет заменить без изменения контрактов. Для текущей архитектуры это сделает разделение Loader ↔ Adapter еще более чистым.

## Prompt:
24.07.2026, 06:51:08

Initialize в нанокад существует

## Response:
24.07.2026, 06:51:09

Да, в nanoCAD это важный момент. Если речь идет про стандартную точку входа через API nanoCAD, то `Initialize` уже имеет свое назначение (например, в механизме `IExtensionApplication.Initialize()` у AutoCAD-подобного API).

Поэтому называть свой интерфейс:

```csharp
public interface IAddonStartup
{
    void Initialize(IAddonContext context);
}
```

неудачно — будет путаница с API хоста.

Лучше назвать иначе:

```csharp
public interface IAddonEntryPoint
{
    /// <summary>
    /// Запуск пользовательского аддона после загрузки сборки.
    /// Вызывается Loader-ом.
    /// </summary>
    void Start(IAddonContext context);
}
```

или:

```csharp
public interface IAddonModule
{
    void Configure(IAddonContext context);
}
```

---

Тогда схема будет:

### Loader

```csharp
Assembly assembly = Assembly.LoadFrom(adapterPath);

Type type = assembly
    .GetTypes()
    .First(t => typeof(IAddonEntryPoint).IsAssignableFrom(t));

var entryPoint =
    (IAddonEntryPoint)Activator.CreateInstance(type);

entryPoint.Start(context);
```

---

### Adapter

```csharp
public sealed class SpecSpdsEntryPoint : IAddonEntryPoint
{
    public void Start(IAddonContext context)
    {
        var container = context.Container;

        // Добавляем сервисы именно этого продукта
        container.RegisterSingleton<IPlotService, PlotService>();

        container.RegisterSingleton<SpecCommand>();

        // Получаем основной объект приложения
        var application =
            container.GetInstance<SpecApplication>();

        application.Start();
    }
}
```

---

Еще один момент для nanoCAD:

Возможно, адаптер у тебя уже реализует:

```csharp
IExtensionApplication
```

Тогда лучше разделить:

```
SpecSPDS.23.dll

SpecSpdsEntryPoint : IAddonEntryPoint
        |
        | создает контекст
        |
        v

SpecApplication : IExtensionApplication
```

То есть nanoCAD вызывает свой `Initialize()`, а твой Loader вызывает свой `Start()`.

Не стоит смешивать эти два жизненных цикла. У тебя фактически есть два уровня запуска:

1. **Loader startup**
   - определить версию nanoCAD;
   - найти адаптер;
   - создать DI;
   - загрузить модуль.

2. **Adapter startup**
   - зарегистрировать команды;
   - подключить сервисы;
   - работать с API nanoCAD.

Для этого отдельное имя точки входа будет намного понятнее.

## Prompt:
24.07.2026, 06:52:01

Если реализация в лоадере, как адаптер о ней узнает?

## Response:
24.07.2026, 06:52:01

Если реализация интерфейса находится в Loader, то адаптер **сам по себе о ней не узнает**. И это нормально — как раз этого мы и хотим избежать.

Связь должна быть в обратную сторону: **Loader знает только контракт, а адаптер предоставляет реализацию**.

То есть:

```text
drz.Abstractions.dll
        |
        |
        +----------------+
        |                |
        v                v
SpecSPDS.Loader    SpecSPDS.23
        |
        |
        | Assembly.LoadFrom()
        |
        v
IAddonEntryPoint
```

### Контракт

В общей сборке:

```csharp
public interface IAddonEntryPoint
{
    void Start(IAddonContext context);
}
```

### В адаптере

`SpecSPDS.23.dll` содержит:

```csharp
public sealed class SpecEntryPoint : IAddonEntryPoint
{
    public void Start(IAddonContext context)
    {
        var container = context.Container;

        container.RegisterSingleton<ISpecService, SpecService>();

        var app = container.GetInstance<SpecApplication>();

        app.Start();
    }
}
```

### В Loader

Loader ничего не знает про `SpecEntryPoint`.

Он только ищет контракт:

```csharp
var entryType = assembly
    .GetTypes()
    .FirstOrDefault(t =>
        typeof(IAddonEntryPoint).IsAssignableFrom(t));

if (entryType == null)
    throw new Exception("Entry point not found");

var entry =
    (IAddonEntryPoint)Activator.CreateInstance(entryType);

entry.Start(context);
```

---

Если же ты имеешь в виду: "как адаптер узнает про **контекст, созданный Loader**?" — вот тут два варианта:

### Вариант 1 (лучший): через параметр

```csharp
entry.Start(context);
```

Адаптер получает уже готовый объект.

### Вариант 2: через общий статический контекст

Например:

```csharp
AddonRuntime.Current
```

Но тогда появляется глобальное состояние.

---

То есть реализация `IAddonEntryPoint` должна быть **в адаптере**, а реализация `IAddonContext` — **в Loader**.

Разделение получается таким:

| Компонент | Владелец |
|-|-|
| `IAddonEntryPoint` | `drz.Abstractions` |
| `SpecEntryPoint` | `SpecSPDS.23` |
| `AddonContext` | `SpecSPDS.Loader` |
| `Container` | `SpecSPDS.Loader` |
| Общие сервисы | `SpecSPDS.Loader` |
| Специфичные сервисы | `SpecSPDS.23` |

Так Loader остается универсальным загрузчиком, а адаптер — самостоятельным модулем.

## Prompt:
24.07.2026, 06:56:24

Где лоадер ищет контракт?
Это происходит до загрузки адаптера, в загруженных сборках контракта еще нет

## Response:
24.07.2026, 06:56:25

Хороший вопрос. Здесь как раз важна последовательность загрузки.

Контракт **не должен находиться в адаптере**. Он должен находиться в общей сборке, которую Loader уже знает и загрузил.

Например:

```text
SpecSPDS.Loader.exe
│
├── drz.Abstractions.dll       <-- контракт
│       └── IAddonEntryPoint
│
└── SpecSPDS.23.dll            <-- адаптер
        └── SpecEntryPoint : IAddonEntryPoint
```

Порядок:

1. nanoCAD загружает `SpecSPDS.Loader.dll`.
2. Loader уже имеет ссылку на `drz.Abstractions.dll`.
3. Loader делает `Assembly.LoadFrom("SpecSPDS.23.dll")`.
4. В памяти появляется сборка адаптера.
5. Loader ищет **в загруженной сборке адаптера** тип, который реализует интерфейс из `drz.Abstractions`.

Пример:

```csharp
Assembly adapterAssembly =
    Assembly.LoadFrom(adapterPath);

Type entryType = adapterAssembly
    .GetTypes()
    .FirstOrDefault(t =>
        typeof(IAddonEntryPoint).IsAssignableFrom(t));
```

Здесь:

- `IAddonEntryPoint` берется из уже загруженной `drz.Abstractions.dll`;
- `SpecEntryPoint` берется из только что загруженной `SpecSPDS.23.dll`.

То есть поиск идет не "во всех загруженных сборках", а внутри **только что загруженного адаптера**.

---

Если же сделать так:

```csharp
adapterAssembly.GetTypes()
```

до `Assembly.LoadFrom()` — конечно, ничего найти нельзя, потому что адаптера еще нет.

---

По структуре у тебя должно быть примерно:

```text
drz.Abstractions
    IAddonEntryPoint
    IAddonContext

SpecSPDS.Loader
    ссылка на drz.Abstractions
    AssemblyLoader

SpecSPDS.23
    ссылка на drz.Abstractions
    SpecEntryPoint
```

Критически важно: `drz.Abstractions.dll` должна быть **одной и той же физической сборкой** для Loader и Adapter.

Иначе будет классическая проблема:

```text
Loader:
IAddonEntryPoint, Version=1.0.0.0

Adapter:
IAddonEntryPoint, Version=1.0.1.0
```

Для CLR это будут два разных интерфейса, и:

```csharp
typeof(IAddonEntryPoint).IsAssignableFrom(type)
```

вернет `false`.

Для твоего сценария с несколькими версиями nanoCAD я бы вынес эту контрактную сборку в отдельную стабильную библиотеку, например:

```text
drz.Addon.Contracts.dll
```

и не менял ее при обновлении адаптеров.

## Prompt:
24.07.2026, 06:57:38

Лоадер DLL, хост нанокад

## Response:
24.07.2026, 06:57:38

Тогда ситуация немного другая. Если Loader — это **DLL, загружаемая в nanoCAD**, то схема такая:

```text
nanoCAD.exe
    |
    | Load
    v
SpecSPDS.Loader.dll
    |
    | Assembly.LoadFrom()
    v
SpecSPDS.23.dll
```

Loader уже находится внутри процесса nanoCAD и имеет свою загрузочную сборку.

В таком случае контракт должен находиться **не в адаптере и не в Loader**, а в отдельной общей DLL.

Например:

```text
SpecSPDS
│
├── SpecSPDS.Loader.dll       (загружается nanoCAD)
│
├── SpecSPDS.23.dll           (адаптер)
│
└── drz.Addon.Contracts.dll   (общий контракт)
```

Зависимости:

```text
SpecSPDS.Loader.dll
        |
        +--> drz.Addon.Contracts.dll

SpecSPDS.23.dll
        |
        +--> drz.Addon.Contracts.dll
```

---

### Жизненный цикл

nanoCAD вызывает Loader:

```csharp
public class LoaderEntry : IExtensionApplication
{
    public void Initialize()
    {
        // 1. Создаем контейнер продукта
        var container = new Container();

        // 2. Регистрируем сервисы Loader
        container.RegisterSingleton<IDrzLogger, DrzLogger>();
        container.RegisterSingleton<IMessageService, MessageService>();

        // 3. Создаем контекст
        var context = new AddonContext(container);

        // 4. Загружаем адаптер
        LoadAdapter(context);
    }

public void Terminate()
    {
    }
}
```

---

### Загрузка адаптера

```csharp
private void LoadAdapter(IAddonContext context)
{
    // Путь найден по версии nanoCAD
    string file =
        "SpecSPDS.23.dll";

Assembly assembly =
        Assembly.LoadFrom(file);

// Ищем реализацию контракта
    Type startupType =
        assembly.GetTypes()
            .FirstOrDefault(
                t => typeof(IAddonModule)
                    .IsAssignableFrom(t));

if (startupType == null)
        throw new Exception(
            "Не найден модуль адаптера");

var module =
        (IAddonModule)
        Activator.CreateInstance(startupType);

// Передаем ему уже созданный контейнер
    module.Configure(context);
}
```

---

### В адаптере

```csharp
public sealed class SpecSpdsModule : IAddonModule
{
    public void Configure(IAddonContext context)
    {
        var container =
            context.Container;

// Добавляем сервисы адаптера
        container.RegisterSingleton<
            ISpecService,
            SpecService>();

container.RegisterSingleton<
            SpecCommands>();

// Получаем основной объект
        var commands =
            container.GetInstance<SpecCommands>();

commands.Register();
    }
}
```

---

### Важный момент для nanoCAD

Не надо искать контракт через `AppDomain.CurrentDomain.GetAssemblies()`.

В момент запуска Loader в домене могут быть только:

```text
nanoCAD assemblies
SpecSPDS.Loader.dll
drz.Addon.Contracts.dll
```

Адаптера еще нет.

Поэтому порядок именно:

```text
LoadFrom(адаптер)
        |
        v
adapterAssembly.GetTypes()
        |
        v
найти IAddonModule
```

---

То есть архитектурно:

- nanoCAD знает только про `SpecSPDS.Loader.dll`;
- Loader знает только про `drz.Addon.Contracts.dll`;
- Adapter знает только про `drz.Addon.Contracts.dll`;
- Loader и Adapter напрямую друг о друге ничего не знают.

Это как раз подходит под твою схему с разными версиями (`SpecSPDS.23`, `SpecSPDS.26`) и разными продуктами (`PlotSPDS`, `SpecSPDS`).

## Prompt:
24.07.2026, 07:04:33

В drz.Addon.Contracts.dll и интерфейс и реализация?

## Response:
24.07.2026, 07:04:33

Нет. В `drz.Addon.Contracts.dll` должна быть **только контрактная часть**, без реализации.

То есть:

```text
drz.Addon.Contracts.dll

├── IAddonModule
├── IAddonContext
├── IAddonInfo
└── другие интерфейсы/DTO
```

Например:

```csharp
namespace drz.Addon.Contracts;

/// <summary>
/// Контракт точки входа адаптера.
/// </summary>
public interface IAddonModule
{
    /// <summary>
    /// Настройка модуля.
    /// </summary>
    void Configure(IAddonContext context);
}
```

---

А реализация должна находиться там, где она используется.

### Loader

```text
SpecSPDS.Loader.dll

├── LoaderEntry
├── AddonContext
├── AssemblyLoader
└── LoaderLogger
```

Здесь:

```csharp
public sealed class AddonContext : IAddonContext
{
    public IServiceProvider Services { get; }

    public AddonContext(IServiceProvider services)
    {
        Services = services;
    }
}
```

---

### Adapter

```text
SpecSPDS.23.dll

├── SpecSpdsModule : IAddonModule
├── SpecService
├── Commands
└── nanoCAD integration
```

Здесь:

```csharp
public sealed class SpecSpdsModule : IAddonModule
{
    public void Configure(IAddonContext context)
    {
        var services = context.Services;

        // регистрация сервисов адаптера
    }
}
```

---

Получается зависимость:

```text
               drz.Addon.Contracts
                       ▲
                       |
          +------------+------------+
          |                         |
          |                         |
SpecSPDS.Loader.dll        SpecSPDS.23.dll
(реализация Loader)        (реализация Adapter)
```

Никогда:

```text
drz.Addon.Contracts
        |
        +--> SpecSPDS.Loader
        |
        +--> SpecSPDS.23
```

---

Почему нельзя класть реализацию в `drz.Addon.Contracts`?

Потому что тогда:

1. Контрактная библиотека станет зависеть от конкретного продукта.
2. Loader и Adapter получат лишние зависимости.
3. При обновлении адаптера придется менять общую библиотеку.
4. Потеряется смысл динамической загрузки.

По сути `drz.Addon.Contracts.dll` — это как API SDK: она описывает правила взаимодействия, но не содержит самих модулей.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)