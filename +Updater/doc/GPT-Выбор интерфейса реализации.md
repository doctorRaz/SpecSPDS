# Выбор интерфейса реализации

**User:** dRz  
**Created:** 7/20/2026 13:22:47  
**Updated:** 7/20/2026 14:48:13  
**Exported:** 7/20/2026 14:49:50  
**Link:** [https://chatgpt.com/c/6a5df6b0-8920-83eb-b5e4-354c5b9a80e0](https://chatgpt.com/c/6a5df6b0-8920-83eb-b5e4-354c5b9a80e0)  

## Prompt:
20.07.2026, 13:22:46

Есть интерфейсы и реализации
Общий интерфейс сообщений
Консольные
Оконные
Как можно реализовать выбор конкретного интерфейса, если передается общий

## Response:
20.07.2026, 13:22:47

Есть несколько вариантов. Выбор зависит от того, **кто должен принимать решение** — вызывающий код, DI-контейнер или фабрика.

### Вариант 1. Фабрика (обычно самый удачный)

```csharp
public interface IMessageService
{
    void Show(string message);
}

public class CommandLineMessageService : IMessageService
{
    ...
}

public class WindowMessageService : IMessageService
{
    ...
}

public enum MessageServiceType
{
    CommandLine,
    Window
}

public interface IMessageServiceFactory
{
    IMessageService Create(MessageServiceType type);
}
```

Реализация:

```csharp
public class MessageServiceFactory : IMessageServiceFactory
{
    private readonly CommandLineMessageService _commandLine;
    private readonly WindowMessageService _window;

    public MessageServiceFactory(
        CommandLineMessageService commandLine,
        WindowMessageService window)
    {
        _commandLine = commandLine;
        _window = window;
    }

    public IMessageService Create(MessageServiceType type)
    {
        return type switch
        {
            MessageServiceType.CommandLine => _commandLine,
            MessageServiceType.Window => _window,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
```

Использование:

```csharp
var service = factory.Create(MessageServiceType.Window);
service.Show("Привет");
```

Это хорошо сочетается с DI.

---

### Вариант 2. Зарегистрировать одну реализацию по умолчанию

Если почти всегда используется один тип сообщений:

```csharp
container.Register<IMessageService, WindowMessageService>();
```

Иногда получать другую через отдельный интерфейс или фабрику.

---

### Вариант 3. Использовать именованные регистрации

Некоторые контейнеры поддерживают регистрацию по имени или ключу.

Например:

```csharp
IMessageService service = resolver.Get("Window");
```

Но в **SimpleInjector** именованных регистраций нет, поэтому обычно делают фабрику.

---

### Вариант 4. Generic

```csharp
public interface IMessageServiceFactory
{
    T Get<T>() where T : class, IMessageService;
}
```

Использование:

```csharp
var service = factory.Get<WindowMessageService>();
```

Минус — вызывающий код знает о конкретных реализациях.

---

## Для твоего проекта

Поскольку у тебя уже есть `IMessageServiceFactory`, я бы сделал именно его ответственным за выбор реализации. Например:

```csharp
public interface IMessageServiceFactory
{
    IMessageService Get(MessageType type);
}
```

или даже лучше, если выбор зависит от среды:

```csharp
public interface IMessageServiceFactory
{
    IMessageService Create(bool useWindow);
}
```

или

```csharp
public interface IMessageServiceFactory
{
    IMessageService Create(MessageContext context);
}
```

Тогда весь остальной код работает только с `IMessageService` и не знает, консольная это реализация или оконная. Это соответствует принципу инверсии зависимостей и позволяет позже добавить, например, `SilentMessageService` или `LogMessageService` без изменения клиентского кода.

## Prompt:
20.07.2026, 13:55:53

https://github.com/doctorRaz/SpecSPDS/blob/fix/Updater/src/Infrastructure/AddonContext.cs
Сейчас так сделано, доработай

## Response:
20.07.2026, 13:55:53

Я посмотрел репозиторий, но через поиск содержимое файла получить не удалось (поиск GitHub не открыл сам исходник). ([scrapcomputers.dev](https://scrapcomputers.dev/blog/update-addon-system/?utm_source=chatgpt.com))

Пришли, пожалуйста, сам файл `AddonContext.cs` (или вставь его сюда целиком).

Тогда я предложу доработку с учетом обсуждения про `IMessageService`.

Предварительно я бы изменил архитектуру так:

```csharp
public sealed class AddonContext
{
    public IServiceProvider Services { get; }

    public IMessageServiceFactory MessageServiceFactory =>
        Services.GetRequiredService<IMessageServiceFactory>();

    public IMessageService Messages =>
        MessageServiceFactory.Get(DefaultMessageType);

    public MessageType DefaultMessageType { get; set; }
}
```

И тогда в коде можно писать:

```csharp
AddonContext.Current.Messages.Show(...);
```

или, когда нужна конкретная реализация:

```csharp
var window = AddonContext.Current
    .MessageServiceFactory
    .Get(MessageType.Window);

var console = AddonContext.Current
    .MessageServiceFactory
    .Get(MessageType.CommandLine);
```

Это избавляет от необходимости хранить в `AddonContext` две разные реализации (`WindowMessageService`, `CommandLineMessageService`) и позволит позже без изменений добавить `SilentMessageService`, `LogMessageService` и т.д.

Если покажешь текущий `AddonContext.cs`, предложу изменения прямо по существующему коду, не ломая остальную архитектуру.

---

**Sources:**

- [Update: The Addon System Update | ScrapComputers](https://scrapcomputers.dev/blog/update-addon-system/?utm_source=chatgpt.com)

## Prompt:
20.07.2026, 13:56:24

using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;

using System;

namespace drz.Src.Infrastructure
{
    internal static class AddOnContext
    {
        // metadata (всегда доступно)
        //internal static readonly AppInfo InfoDll = AppInfo.Get(typeof(AddOnContext));

        //---R
        //***B

        // DI root (инициализируется отдельно)
        private static AddOnCompositionRoot? _root;

//internal static IApplicationInfo AddOn => Root.Get<IApplicationInfo>();

        internal static IAddOnInfo InfoDll => Root.Get<IAddOnInfo>();

        internal static ISysInfo SysInfo => Root.Get<ISysInfo>();

        internal static ICadInfo CadInfo => Root.Get<ICadInfo>();

        internal static IDocumentService DocService => Root.Get<IDocumentService>();

        internal static IMessageService MsgCmd => Root.Get<ICommandLineMessageService>();

        internal static IMessageService MsgGUI => Root.Get<IWindowMessageService>();

        internal static IMessageService Msg
        {
            get
            {
                if (DocService.IsActive)
                {
                    return Root.Get<ICommandLineMessageService>();
                }
                else
                {
                    return Root.Get<IWindowMessageService>();
                }
            }
        }

        /// <summary>DI root (инициализируется отдельно)</summary>
        /// <value>The root.</value>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot is not initialized</exception>
        private static AddOnCompositionRoot Root => _root ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        internal static void Dispose()
        {
            _root?.Dispose();
            _root = null;
        }

        /// <summary>
        /// Initializes the specified root.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <exception cref="System.ArgumentNullException">root</exception>
        internal static void Initialize(AddOnCompositionRoot root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }
    }
}

## Response:
20.07.2026, 13:56:24

В целом идея хорошая, но сейчас у тебя `AddOnContext` **сам знает**, какую реализацию выбрать:

```csharp
if (DocService.IsActive)
    return Root.Get<ICommandLineMessageService>();

return Root.Get<IWindowMessageService>();
```

Получается, логика выбора размазана по инфраструктуре. Если позже появится еще один тип сообщений (StatusBar, Silent, LogOnly и т.д.), придется менять `AddOnContext`.

Я бы перенес эту логику в фабрику.

### 1. Фабрика

```csharp
public interface IMessageServiceFactory
{
    IMessageService GetDefault();
    IMessageService Get(MessageServiceType type);
}
```

```csharp
internal class MessageServiceFactory : IMessageServiceFactory
{
    private readonly IDocumentService _documentService;
    private readonly ICommandLineMessageService _cmd;
    private readonly IWindowMessageService _gui;

    public MessageServiceFactory(
        IDocumentService documentService,
        ICommandLineMessageService cmd,
        IWindowMessageService gui)
    {
        _documentService = documentService;
        _cmd = cmd;
        _gui = gui;
    }

    public IMessageService GetDefault()
    {
        return _documentService.IsActive
            ? _cmd
            : _gui;
    }

    public IMessageService Get(MessageServiceType type)
    {
        switch (type)
        {
            case MessageServiceType.CommandLine:
                return _cmd;

            case MessageServiceType.Window:
                return _gui;

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
```

### 2. `AddOnContext`

Тогда он становится намного проще:

```csharp
internal static IMessageServiceFactory MsgFactory =>
    Root.Get<IMessageServiceFactory>();

internal static IMessageService Msg =>
    MsgFactory.GetDefault();

internal static IMessageService MsgCmd =>
    MsgFactory.Get(MessageServiceType.CommandLine);

internal static IMessageService MsgGui =>
    MsgFactory.Get(MessageServiceType.Window);
```

### Почему так лучше

Сейчас `AddOnContext` отвечает сразу за две вещи:

- хранение Composition Root;
- выбор реализации сообщений.

После изменения его ответственность останется только одной — предоставлять доступ к сервисам. А решение *какой* `IMessageService` вернуть будет инкапсулировано в `MessageServiceFactory`.

### Еще одна небольшая доработка

Я бы изменил типы свойств:

```csharp
internal static ICommandLineMessageService MsgCmd =>
    Root.Get<ICommandLineMessageService>();

internal static IWindowMessageService MsgGui =>
    Root.Get<IWindowMessageService>();
```

Вместо

```csharp
internal static IMessageService MsgCmd ...
internal static IMessageService MsgGUI ...
```

Так ты не теряешь доступ к специфичным методам этих интерфейсов (если они появятся), а там, где нужен только общий контракт, используешь `Msg`. Это делает API более выразительным и не ухудшает инкапсуляцию.

## Prompt:
20.07.2026, 14:00:48

Как сделать, чтобы параметром передавать общий интерфейс, но в методе вызывать нужный

## Response:
20.07.2026, 14:00:48

Если параметр имеет тип общего интерфейса `IMessageService`, то внутри метода можно определить конкретную реализацию несколькими способами.

### 1. Pattern matching (предпочтительно)

```csharp
public void Show(IMessageService messageService)
{
    switch (messageService)
    {
        case ICommandLineMessageService cmd:
            // консольный
            break;

        case IWindowMessageService gui:
            // оконный
            break;

        default:
            throw new NotSupportedException();
    }
}
```

или

```csharp
if (messageService is ICommandLineMessageService cmd)
{
    ...
}
else if (messageService is IWindowMessageService gui)
{
    ...
}
```

---

### 2. По типу

```csharp
if (messageService.GetType() == typeof(CommandLineMessageService))
{
    ...
}
```

Этот вариант хуже, так как привязывается к реализации, а не к интерфейсу.

---

### 3. Если нужен разный алгоритм — лучше полиморфизм

Часто проверка типа говорит о том, что ответственность распределена неудачно. Например, вместо

```csharp
if (msg is ICommandLineMessageService)
    ...
else
    ...
```

лучше добавить метод в общий интерфейс:

```csharp
public interface IMessageService
{
    void Show(string message);

    bool SupportsProgress { get; }
}
```

или

```csharp
public interface IMessageService
{
    MessageServiceType Kind { get; }
}
```

Тогда:

```csharp
switch (msg.Kind)
{
    case MessageServiceType.CommandLine:
        ...
        break;

    case MessageServiceType.Window:
        ...
        break;
}
```

---

### Для твоего случая

Если тебе нужно **иногда** вызвать специфичный метод, которого нет в `IMessageService`, то самый чистый вариант — использовать `is ICommandLineMessageService` / `is IWindowMessageService`.

Если же такие проверки начинают встречаться во многих местах, значит этот специфичный метод стоит поднять в `IMessageService` либо вынести различия в отдельную стратегию/фабрику, чтобы клиентский код вообще не знал о конкретных реализациях.

## Prompt:
20.07.2026, 14:03:44

Передавать енум, чтобы вернулся нужный интерфейс

## Response:
20.07.2026, 14:03:45

Да, это хороший и достаточно распространенный вариант. Вместо передачи самого интерфейса передается `enum`, а фабрика или `AddOnContext` возвращает нужную реализацию.

Например:

```csharp
public enum MessageServiceType
{
    Default,
    CommandLine,
    Window
}
```

```csharp
public interface IMessageServiceFactory
{
    IMessageService Get(MessageServiceType type);
}
```

Реализация:

```csharp
public IMessageService Get(MessageServiceType type)
{
    switch (type)
    {
        case MessageServiceType.CommandLine:
            return _cmd;

        case MessageServiceType.Window:
            return _gui;

        case MessageServiceType.Default:
            return _documentService.IsActive ? _cmd : _gui;

        default:
            throw new ArgumentOutOfRangeException(nameof(type));
    }
}
```

Использование:

```csharp
IMessageService msg = factory.Get(MessageServiceType.CommandLine);
```

или

```csharp
IMessageService msg = factory.Get(MessageServiceType.Default);
```

### Можно даже без фабрики

Если не хочется заводить отдельную фабрику, то в `AddOnContext`:

```csharp
internal static IMessageService GetMessageService(MessageServiceType type)
{
    switch (type)
    {
        case MessageServiceType.CommandLine:
            return Root.Get<ICommandLineMessageService>();

        case MessageServiceType.Window:
            return Root.Get<IWindowMessageService>();

        case MessageServiceType.Default:
            return DocService.IsActive
                ? Root.Get<ICommandLineMessageService>()
                : Root.Get<IWindowMessageService>();

        default:
            throw new ArgumentOutOfRangeException(nameof(type));
    }
}
```

Тогда вызов выглядит лаконично:

```csharp
AddOnContext.GetMessageService(MessageServiceType.Window).Show("...");
```

Такой подход хорошо масштабируется: если позже появятся новые реализации (`StatusBar`, `Silent`, `Logger`), достаточно добавить новое значение в `enum` и одну ветку в фабрике.

## Prompt:
20.07.2026, 14:44:13

GetMessageService(MessageServiceType
Надо объявить в интерфейсе?

## Response:
20.07.2026, 14:44:14

Нет, не обязательно. Зависит от того, где должна находиться ответственность.

Если `GetMessageService(MessageServiceType)` — это **метод выбора реализации**, то его место не в `IMessageService`, а в фабрике или контексте.

Правильнее так:

```csharp
public interface IMessageService
{
    void Show(string message);
}
```

```csharp
public interface IMessageServiceFactory
{
    IMessageService Get(MessageServiceType type);
}
```

или

```csharp
internal static class AddOnContext
{
    internal static IMessageService GetMessageService(MessageServiceType type)
    {
        ...
    }
}
```

Тогда использование:

```csharp
var msg = AddOnContext.GetMessageService(MessageServiceType.Window);
msg.Show("...");
```

### Когда объявлять в интерфейсе?

Только если сам объект умеет выдавать другие `IMessageService`, что здесь не имеет смысла:

```csharp
public interface IMessageService
{
    IMessageService GetMessageService(MessageServiceType type); // ❌
}
```

Это нарушает принцип единственной ответственности: сервис сообщений начинает еще и работать как фабрика.

**Итого:**
- `IMessageService` — только методы работы с сообщениями.
- `IMessageServiceFactory` или `AddOnContext` — выбор нужной реализации по `MessageServiceType`. Это наиболее чистая архитектура.

## Prompt:
20.07.2026, 14:47:11

public enum MessageServiceType
{
    Default,
    CommandLine,
    Window
}
В какой библиотеке?

## Response:
20.07.2026, 14:47:17

Я бы разместил `MessageServiceType` там же, где находятся абстракции сервиса сообщений.

Если у тебя есть библиотека **`drz.Abstractions`**, то наиболее логично:

```
drz.Abstractions
 ├── Services
 │   ├── IMessageService.cs
 │

## Prompt:
20.07.2026, 14:48:04

Но использую их addoncontext?
Пока без фабрики

## Response:
20.07.2026, 14:48:07

Да, если **пока без фабрики**, то вполне нормально разместить метод в `AddOnContext`.

Например:

```csharp
internal static IMessageService GetMessageService(MessageServiceType type)
{
    switch (type)
    {
        case MessageServiceType.CommandLine:
            return Root.Get<ICommandLineMessageService>();

        case MessageServiceType.Window:
            return Root.Get<IWindowMessageService>();

        case MessageServiceType.Default:
        default:
            return DocService.IsActive
                ? Root.Get<ICommandLineMessageService>()
                : Root.Get<IWindowMessageService>();
    }
}
```

Тогда любой код вызывает:

```csharp
var msg = AddOnContext.GetMessageService(MessageServiceType.Default);
msg.Show("...");
```

или

```csharp
var msg = AddOnContext.GetMessageService(MessageServiceType.Window);
```

### Где разместить `MessageServiceType`

Даже если использует его только `AddOnContext`, я **не стал бы** помещать enum в инфраструктурную библиотеку.

Лучше оставить его в `drz.Abstractions`, рядом с `IMessageService`, потому что это часть **контракта**, а не реализации. Тогда и `AddOnContext`, и будущая фабрика, и любой другой код будут использовать один и тот же тип.

Если же ты абсолютно уверен, что `MessageServiceType` никогда не выйдет за пределы `AddOnContext`, можно сделать его `internal enum` в той же сборке. Но, судя по твоей архитектуре, этот enum вполне может пригодиться и другим компонентам, поэтому общий контракт (`drz.Abstractions`) выглядит более удачным местом.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)