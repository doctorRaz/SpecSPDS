# DynamicMessageService

**User:** dRz  
**Created:** 7/24/2026 11:57:38  
**Updated:** 7/24/2026 12:49:00  
**Exported:** 7/24/2026 12:50:50  
**Link:** [https://chatgpt.com/c/6a6328e0-3330-83ed-a4c8-6b5cf9680cd1](https://chatgpt.com/c/6a6328e0-3330-83ed-a4c8-6b5cf9680cd1)  

## Prompt:
24.07.2026, 11:57:38

Верификация контейнера 7арантирует, что все классы созданы ?

## Response:
24.07.2026, 11:57:38

Нет. Верификация контейнера не гарантирует, что **все классы будут созданы**.

Если речь о **Simple Injector**, то `container.Verify()` делает следующее:

- проверяет корректность регистраций;
- строит графы зависимостей для зарегистрированных сервисов;
- создает экземпляры **Singleton** (по умолчанию), чтобы убедиться, что их можно сконструировать;
- проверяет, что зависимости могут быть разрешены.

Но при этом:

- ❌ не создает `Transient` при обычной работе приложения (они могут быть созданы только во время проверки графа и сразу освобождены);
- ❌ не создает объекты, которые нигде не зарегистрированы;
- ❌ не вызывает все методы инициализации приложения, кроме конструктора;
- ❌ не проверяет код, который выполняется позже (например, в `Initialize()`, `Execute()`, обработчиках событий).

Например:

```csharp
container.RegisterSingleton<ILogger, NLogLogger>();
container.Register<IRepository, Repository>();
container.Register<IService, Service>();

container.Verify();
```

Во время `Verify()`:
- `NLogLogger` будет создан.
- Будет проверено, что `Service` можно создать и что для него существует `Repository`.
- Но экземпляр `Service` не будет жить до конца приложения, если он зарегистрирован как `Transient`.

### Если нужно создать все Singleton

После `Verify()` они уже обычно созданы. Либо можно явно получить их:

```csharp
container.GetInstance<IMySingleton>();
```

### Если нужно проверить абсолютно все регистрации

Можно пройтись по ним:

```csharp
foreach (var registration in container.GetCurrentRegistrations())
{
    registration.GetInstance();
}
```

Тогда будет создан экземпляр каждого зарегистрированного сервиса в соответствии с его образом жизни (`Lifestyle`).

Если вопрос связан с твоим лоадером nanoCAD, то `Verify()` имеет смысл вызывать один раз после регистрации всех сервисов — это позволяет обнаружить ошибки в зависимостях до начала работы аддона.

## Prompt:
24.07.2026, 12:02:36

При инициализации лоадера я сразу создаю контейнер и получаю для него логер на класс.
Если на этом этапе падает, то выхожу из лоадера с сообщением в консоль нанокад

Но может например упасть позже при получении мессадже сервиса
Аддон инфо ...
В принципе тоже выходить из загрузчика с выводом в консоль и логировпнием??

## Response:
24.07.2026, 12:02:36

Да, но я бы разделил ошибки на **фатальные** и **рабочие**.

### 1. Ошибка во время инициализации контейнера — фатальная

Например:
- не удалось создать контейнер;
- `Verify()` обнаружил ошибку;
- не удалось получить `IDrzLogger`;
- не зарегистрирован обязательный сервис.

В этом случае загрузчик должен:

1. записать ошибку в лог (если логгер уже доступен);
2. вывести сообщение в командную строку nanoCAD;
3. **прервать загрузку аддона**.

Это означает, что аддон находится в неконсистентном состоянии и продолжать работу нельзя.

---

### 2. Ошибка позже при получении сервиса

Например:

```csharp
var messageService = container.GetInstance<IMessageService>();
```

если здесь произошел `ActivationException`, это означает, что контейнер оказался неконсистентным. Если перед этим был успешный `Verify()`, такое практически невозможно, если только:
- регистрация происходит динамически;
- контейнер изменяется после Verify;
- сам конструктор сервиса выбрасывает исключение.

В этом случае я бы тоже считал ошибку **фатальной**.

Например:

```csharp
try
{
    var service = container.GetInstance<IMessageService>();
}
catch (Exception ex)
{
    logger.Fatal(ex);

    cadConsole.WriteLine(
        "Ошибка инициализации аддона. Загрузка прекращена.");

    return LoadResult.Failed;
}
```

---

### 3. Ошибка внутри самого сервиса

Например:

```csharp
messageService.Show(...);
```

и здесь падает WinAPI.

Это уже **не ошибка контейнера**, а ошибка работы сервиса. Здесь обычно не нужно выгружать аддон. Достаточно залогировать ошибку и, если возможно, использовать резервный способ уведомления (например, командную строку nanoCAD).

---

### Я бы сделал единое правило

Все ошибки, возникающие **до завершения `Initialize()`**, считать фатальными.

То есть:

- создание `AddonInfo`;
- создание `CadInfo`;
- создание контейнера;
- регистрация сервисов;
- `Verify()`;
- получение обязательных сервисов (`Logger`, `MessageService`, `Settings`, и т.д.).

Любое исключение на этом этапе:
- логируется;
- выводится в консоль nanoCAD;
- загрузка аддона прекращается.

После успешного завершения `Initialize()` ошибки уже относятся к рабочему времени и должны обрабатываться локально, без завершения работы аддона.

Для архитектуры твоего лоадера это наиболее предсказуемый и надежный вариант.

## Prompt:
24.07.2026, 12:09:51

Инит вызывает метод в котором
Создаётся контейнер если его нет
Из контейнера получаю логер на класс приватное поле
Мессадж сервис поле на класс
Аддон инфо 
Сад инфо и сис инфо все полями на класс
После каждого успешного получения , поднимаю соответствующие флаги
Т. Е при фатале, я в катч я проверю поднялся ли логер и тогда логирую и пишу в консоль иначе просто в консоль.
В принципе из метода можно вернуть сумму флагов и инит примет решение 
Фалсе выход из загрузки
Труе продолжить работу

## Response:
24.07.2026, 12:09:51

Идея хорошая, но я бы изменил одну вещь.

**Не возвращал бы `bool` или сумму флагов.** Это слишком малоинформативно.

Лучше вернуть объект результата, например:

```csharp
public sealed class InitializationResult
{
    public bool Success { get; init; }

    public InitFlags Flags { get; init; }

    public Exception? Exception { get; init; }
}
```

где

```csharp
[Flags]
public enum InitFlags
{
    None           = 0,
    Container       = 1 << 0,
    Logger          = 1 << 1,
    MessageService  = 1 << 2,
    AddonInfo       = 1 << 3,
    CadInfo         = 1 << 4,
    OsInfo          = 1 << 5,
}
```

Тогда алгоритм будет таким:

```text
Init()
    ↓
Initialize()
    ↓
получен InitializationResult

если Success == false
    если Logger поднялся
        Logger.Fatal(...)
    вывести в консоль nanoCAD
    return

иначе
    продолжить загрузку
```

### Еще лучше

Я бы вообще не поднимал флаги вручную после каждого шага.

Проще проверять по самим полям:

```csharp
if (_logger != null)
{
    _logger.Fatal(ex);
}

_cadConsole.WriteLine(...);
```

Флаг `Logger` фактически дублирует состояние `_logger != null`.

Исключение — если тебе нужно знать **до какого этапа дошла инициализация**. Тогда флаги полезны для диагностики, например в логе:

```
Initialization failed.
Completed stages:
✔ Container
✔ Logger
✔ AddonInfo
✘ MessageService
✘ CadInfo
✘ OsInfo
```

Такой лог намного информативнее, чем просто `false`.

Для твоего лоадера я бы оставил именно `InitFlags` как средство диагностики, а решение продолжать или прекращать загрузку принимал бы только по `Success`. Это делает код более расширяемым: позже можно добавить новые этапы инициализации без изменения логики `Init()`.

## Prompt:
24.07.2026, 12:12:33

Код может упасть на любом этапе инициализации, тогда понять что поднялось можно будет только по флагам
Либо получение логера, инфы, мессадж вынести в методы и возвращать флаги
Или это уже паранойя?

## Response:
24.07.2026, 12:12:34

Это не паранойя, но важно не переусложнить.

Я бы разделил две задачи:

1. **Управление инициализацией.**
2. **Диагностика ошибки.**

Для первой достаточно `bool` или исключения.

Для второй действительно полезно знать, до какого этапа дошла инициализация.

### Самый простой вариант

Использовать один локальный флаг этапов:

```csharp
InitFlags flags = InitFlags.None;

_container = CreateContainer();
flags |= InitFlags.Container;

_logger = _container.GetInstance<IDrzLogger>();
flags |= InitFlags.Logger;

_messageService = _container.GetInstance<IMessageService>();
flags |= InitFlags.MessageService;

_addonInfo = _container.GetInstance<IAddonInfo>();
flags |= InitFlags.AddonInfo;
```

Если любое действие выбросит исключение, в `catch` уже будет актуальное значение `flags`.

Например:

```csharp
catch (Exception ex)
{
    if (flags.HasFlag(InitFlags.Logger))
        _logger.Fatal(ex);

    _cadConsole.WriteLine(
        $"Ошибка инициализации ({flags}).");

    return false;
}
```

Это всего одна строка после каждого успешного этапа.

### Выносить каждый этап в отдельный метод

Например:

```csharp
flags |= InitializeLogger();
flags |= InitializeMessageService();
flags |= InitializeAddonInfo();
```

Я бы **не стал**, если эти методы ничего больше не делают. Получится много мелких методов, которые только вызывают `GetInstance()` и возвращают флаг. Читаемость скорее ухудшится.

### Что бы я сделал в твоем проекте

Оставил бы одну последовательную инициализацию и один `InitFlags`.

Плюс сделал бы красивый вывод:

```
Инициализация завершилась с ошибкой.

✔ Container
✔ Logger
✔ MessageService
✘ AddonInfo
✘ CadInfo
✘ OsInfo
```

Такой лог через полгода окажется гораздо полезнее, чем просто `Initialization failed`.

То есть сами флаги — хорошая идея. А вот дробить код на десяток методов только ради возврата этих флагов, на мой взгляд, уже не дает существенной пользы.

## Prompt:
24.07.2026, 12:19:46

Addoncontext возвращает message service в зависимости есть документ или нет
Я получаю сервис в поле..
Возможно это плохая идея?

## Response:
24.07.2026, 12:19:46

Да, я считаю, что в твоем случае это слабое место архитектуры.

Если `AddonContext.MessageService` **каждый раз выбирает реализацию** в зависимости от наличия активного документа, то сохранять результат в поле:

```csharp
_messageService = AddonContext.MessageService;
```

не очень хорошая идея.

Почему?

Представь сценарий:

1. Аддон загружается, документа нет.
2. В поле попадает `CommandLineMessageService`.
3. Пользователь открывает чертеж.
4. Ты продолжаешь пользоваться `_messageService`.

В итоге сообщения по-прежнему идут в командную строку, хотя уже можно показывать диалоги.

То есть поле хранит **снимок состояния** на момент инициализации.

---

### Лучше вариант 1

Не хранить сервис, а получать его непосредственно перед использованием:

```csharp
AddonContext.MessageService.Show(...);
```

или

```csharp
AddonContext.GetMessageService().Show(...);
```

Тогда выбор реализации всегда актуален.

---

### Лучше вариант 2 (мне нравится больше)

Сделать `DynamicMessageService`.

Регистрируется один Singleton:

```text
IMessageService
        │
        ▼
DynamicMessageService
```

Внутри него:

```csharp
public void Show(...)
{
    var service = _documentService.IsActive
        ? _windowMessageService
        : _commandLineMessageService;

    service.Show(...);
}
```

Тогда любой класс получает обычный `IMessageService`:

```csharp
private readonly IMessageService _messageService;
```

и никогда не думает, какая реализация сейчас нужна.

Это соответствует принципу инверсии зависимостей: **выбор стратегии скрыт внутри сервиса**, а не размазан по всему коду.

---

Для твоей архитектуры с `AddonContext` я бы выбрал именно второй вариант. Тогда контейнер всегда возвращает один и тот же `IMessageService`, а решение "в окно или в командную строку" принимает сам сервис при каждом вызове `Show()`. Это избавит от проблем с изменением состояния nanoCAD после инициализации.

## Prompt:
24.07.2026, 12:29:23

Иногда надо вывести окном
Зарегистрировать и оконный?

## Response:
24.07.2026, 12:29:23

Да, именно так я бы и сделал.

Зарегистрировал бы **три** сервиса:

```csharp
ICommandLineMessageService
IWindowMessageService
IMessageService          // динамический
```

где:

- `ICommandLineMessageService` — всегда пишет в командную строку;
- `IWindowMessageService` — всегда показывает окно;
- `IMessageService` — сам выбирает, какой из двух использовать.

Тогда:

Обычный код:

```csharp
_messageService.Show(...);
```

не думает, есть документ или нет.

А если есть особый случай:

```csharp
_windowMessageService.Show(...);
```

то окно будет показано принудительно.

Например, при:
- подтверждении удаления;
- запросе "Да/Нет";
- выборе файла;
- вводе пользователем данных.

Такой подход имеет еще одно преимущество: `IMessageService` остается "умным" сервисом по умолчанию, а специальные случаи используют специализированные сервисы. Это соответствует принципу единственной ответственности и избавляет от параметров вроде:

```csharp
Show(message, forceWindow: true);
```

или

```csharp
Show(message, MessageMode.Window);
```

которые со временем начинают расползаться по всему коду.

Для твоей архитектуры с `AddonContext` это, на мой взгляд, наиболее чистое решение.

## Prompt:
24.07.2026, 12:31:25

public void Show(...)
{
    var service = _documentService.IsActive
        ? _windowMessageService
        : _commandLineMessageService;

    service.Show(...);
}
Для инфо, еррор....
Где этот код прописывается?

## Response:
24.07.2026, 12:31:25

Этот код должен находиться **в реализации `IMessageService`**, а не в `AddonContext`.

Например:

```text
IMessageService
        ▲
        │
DynamicMessageService
```

```csharp
public sealed class DynamicMessageService : IMessageService
{
    private readonly IDocumentService _documentService;
    private readonly ICommandLineMessageService _commandLineMessageService;
    private readonly IWindowMessageService _windowMessageService;

    public DynamicMessageService(
        IDocumentService documentService,
        ICommandLineMessageService commandLineMessageService,
        IWindowMessageService windowMessageService)
    {
        _documentService = documentService;
        _commandLineMessageService = commandLineMessageService;
        _windowMessageService = windowMessageService;
    }

    private IMessageService Current =>
        _documentService.IsActive
            ? _windowMessageService
            : _commandLineMessageService;

    public void Info(string message) =>
        Current.Info(message);

    public void Warning(string message) =>
        Current.Warning(message);

    public void Error(string message) =>
        Current.Error(message);

    public MessageResult Show(
        string text,
        string caption,
        MessageButtons buttons,
        MessageIcon icon)
    {
        return Current.Show(text, caption, buttons, icon);
    }
}
```

В контейнере регистрируешь:

```csharp
container.RegisterSingleton<ICommandLineMessageService, CommandLineMessageService>();
container.RegisterSingleton<IWindowMessageService, WindowMessageService>();

container.RegisterSingleton<IMessageService, DynamicMessageService>();
```

Тогда любой класс получает:

```csharp
private readonly IMessageService _messageService;
```

и вызывает:

```csharp
_messageService.Info(...);
_messageService.Error(...);
_messageService.Show(...);
```

не заботясь о том, куда именно выводить сообщение.

А если где-то действительно нужно **обязательно окно**, то этот класс получает дополнительную зависимость:

```csharp
private readonly IWindowMessageService _windowMessageService;
```

и вызывает ее напрямую.

### Что касается `AddonContext`

Я бы вообще убрал из него логику выбора сервиса.

Плохо:

```csharp
public static IMessageService MessageService =>
    _documentService.IsActive
        ? Get<IWindowMessageService>()
        : Get<ICommandLineMessageService>();
```

Хорошо:

```csharp
public static IMessageService MessageService =>
    Get<IMessageService>();
```

Вся логика выбора остается внутри `DynamicMessageService`. Тогда `AddonContext` превращается в простой фасад над контейнером и не содержит бизнес-логики. Это делает архитектуру чище и упрощает поддержку.

## Prompt:
24.07.2026, 12:37:08

В windows message service будут еще методы да нет пропустить отменить
В консольном варианте реализовывать не предполагается
Как быть?

## Response:
24.07.2026, 12:37:09

Вот это как раз аргумент **не делать `IWindowMessageService` наследником `IMessageService`**.

Я бы разделил интерфейсы по назначению.

Например:

```csharp
public interface IMessageService
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}
```

Этот интерфейс реализуют:

- `CommandLineMessageService`
- `DynamicMessageService`

А отдельный интерфейс:

```csharp
public interface IWindowMessageService : IMessageService
{
    MessageResult Show(
        string text,
        string caption,
        MessageButtons buttons,
        MessageIcon icon);

    bool AskYesNo(string text);

    bool AskOkCancel(string text);

    MessageResult AskYesNoCancel(string text);
}
```

Именно его реализует `WindowMessageService`.

Тогда получается:

- **90% кода** использует `IMessageService` (`Info`, `Warning`, `Error`).
- Только те классы, которым действительно нужен диалог с пользователем, зависят от `IWindowMessageService`.

Это соответствует принципу ISP (Interface Segregation Principle): класс не зависит от методов, которые ему не нужны.

### Почему не стоит реализовывать диалоги в консольном сервисе

Можно, конечно, сделать:

```csharp
bool AskYesNo(...)
{
    throw new NotSupportedException();
}
```

или

```csharp
return false;
```

Но это плохое решение:

- интерфейс обещает возможность диалога, а реализация ее не поддерживает;
- легко получить неожиданное поведение в рантайме.

Поэтому я бы вообще **не включал методы `Ask...` в общий `IMessageService`**.

Общий интерфейс — только для уведомлений.

Диалоги — отдельный специализированный сервис. Для твоей архитектуры это выглядит наиболее чисто и расширяемо.

## Prompt:
24.07.2026, 12:40:58

Получается simpleinjector под один интерфейс возвращает разные реализации?

## Response:
24.07.2026, 12:40:58

Нет. **Simple Injector сам по себе так не делает.**

Для одного интерфейса обычно регистрируется **одна** реализация:

```csharp
container.RegisterSingleton<IMessageService, DynamicMessageService>();
```

А уже `DynamicMessageService` внутри решает, кого вызвать:

```text
IMessageService
      │
      ▼
DynamicMessageService
      │
      ├── CommandLineMessageService
      └── WindowMessageService
```

То есть контейнер **всегда** возвращает один и тот же экземпляр `DynamicMessageService`.

---

Если же ты хочешь получать разные реализации напрямую, то регистрируешь разные интерфейсы:

```csharp
container.RegisterSingleton<ICommandLineMessageService, CommandLineMessageService>();

container.RegisterSingleton<IWindowMessageService, WindowMessageService>();

container.RegisterSingleton<IMessageService, DynamicMessageService>();
```

Тогда можно инжектить именно то, что нужно:

```csharp
public MyClass(IMessageService messageService)       // обычные сообщения

public MyDialog(IWindowMessageService windowService) // только окна
```

---

В твоей архитектуре есть еще один вариант, который тебе, возможно, понравится больше.

Не делать `DynamicMessageService` вообще, а оставить выбор в `AddonContext`:

```csharp
AddonContext.MessageService
```

Но **не хранить его в поле**. Каждый раз обращаться к свойству:

```csharp
AddonContext.MessageService.Info(...);
```

Тогда `AddonContext` при каждом обращении будет возвращать актуальную реализацию в зависимости от наличия документа.

Это проще, но сильнее связывает код с `AddonContext`.

Если же ты стремишься к чистой DI-архитектуре, то вариант с `DynamicMessageService` выглядит предпочтительнее.

## Prompt:
24.07.2026, 12:42:52

Для DynamicMessageService напиши интерфейс, реализацию
Как использовать
Добавь комментарии

## Response:
24.07.2026, 12:42:52

Я бы сделал именно так.

## Интерфейс IMessageService

```csharp
/// <summary>
/// Предоставляет универсальный сервис вывода сообщений.
///
/// Реализация самостоятельно определяет способ отображения
/// (командная строка или окно).
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Выводит информационное сообщение.
    /// </summary>
    void Info(string message);

    /// <summary>
    /// Выводит предупреждение.
    /// </summary>
    void Warning(string message);

    /// <summary>
    /// Выводит сообщение об ошибке.
    /// </summary>
    void Error(string message);
}
```

---

## Интерфейс IWindowMessageService

```csharp
/// <summary>
/// Предоставляет сервис отображения оконных сообщений.
///
/// Используется в случаях, когда требуется взаимодействие
/// с пользователем.
/// </summary>
public interface IWindowMessageService : IMessageService
{
    MessageResult Show(
        string text,
        string caption,
        MessageButtons buttons,
        MessageIcon icon);

    bool AskYesNo(string text);

    bool AskOkCancel(string text);

    MessageResult AskYesNoCancel(string text);
}
```

---

## Интерфейс ICommandLineMessageService

```csharp
/// <summary>
/// Предоставляет сервис вывода сообщений
/// в командную строку nanoCAD.
/// </summary>
public interface ICommandLineMessageService : IMessageService
{
}
```

---

# DynamicMessageService

```csharp
/// <summary>
/// Универсальный сервис сообщений.
///
/// При каждом обращении определяет,
/// каким способом вывести сообщение:
///
/// • если открыт документ — используется окно;
/// • если документа нет — используется командная строка.
///
/// Благодаря этому пользователь класса
/// не зависит от состояния nanoCAD.
/// </summary>
public sealed class DynamicMessageService : IMessageService
{
    private readonly IDocumentService _documentService;
    private readonly ICommandLineMessageService _commandLine;
    private readonly IWindowMessageService _window;

    public DynamicMessageService(
        IDocumentService documentService,
        ICommandLineMessageService commandLine,
        IWindowMessageService window)
    {
        _documentService = documentService;
        _commandLine = commandLine;
        _window = window;
    }

    /// <summary>
    /// Возвращает актуальный сервис сообщений.
    /// </summary>
    private IMessageService Current =>
        _documentService.IsActive
            ? _window
            : _commandLine;

    public void Info(string message)
    {
        Current.Info(message);
    }

    public void Warning(string message)
    {
        Current.Warning(message);
    }

    public void Error(string message)
    {
        Current.Error(message);
    }
}
```

---

# Регистрация в контейнере

```csharp
container.RegisterSingleton<ICommandLineMessageService, CommandLineMessageService>();

container.RegisterSingleton<IWindowMessageService, WindowMessageService>();

container.RegisterSingleton<IMessageService, DynamicMessageService>();
```

---

# Использование в обычном коде

```csharp
public sealed class PlotService
{
    private readonly IMessageService _messageService;

    public PlotService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public void Execute()
    {
        _messageService.Info("Печать завершена.");
    }
}
```

Класс вообще не знает:
- открыт документ или нет;
- используется окно или командная строка.

---

# Использование, когда обязательно нужен диалог

```csharp
public sealed class DeleteService
{
    private readonly IWindowMessageService _windowMessageService;

    public DeleteService(IWindowMessageService windowMessageService)
    {
        _windowMessageService = windowMessageService;
    }

    public void Delete()
    {
        if (!_windowMessageService.AskYesNo("Удалить объект?"))
            return;

        // удаление...
    }
}
```

---

## Почему этот вариант удачный

Получается четкое разделение обязанностей:

- **`IMessageService`** — универсальный сервис для уведомлений (`Info`, `Warning`, `Error`).
- **`DynamicMessageService`** — автоматически выбирает способ вывода.
- **`IWindowMessageService`** — используется только там, где действительно нужен диалог с пользователем.
- **`ICommandLineMessageService`** — отвечает только за вывод в командную строку.

В результате большинство классов зависит только от `IMessageService`, а специальные сценарии — от `IWindowMessageService`. Это делает код проще и уменьшает количество мест, где приходится учитывать состояние nanoCAD.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)