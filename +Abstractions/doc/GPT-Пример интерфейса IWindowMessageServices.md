# Пример интерфейса IWindowMessageServices

**User:** dRz  
**Created:** 7/23/2026 6:21:18  
**Updated:** 7/23/2026 7:18:20  
**Exported:** 7/23/2026 22:30:19  
**Link:** [https://chatgpt.com/c/6a61884e-5330-83ed-af0e-7adbf23bda74](https://chatgpt.com/c/6a61884e-5330-83ed-af0e-7adbf23bda74)  

## Prompt:
23.07.2026, 06:21:18

IWindowMessageServices
Метод может возвращать значение, yes, OK, no, retry...
Покажи пример интерфейса и реализации

## Response:
23.07.2026, 06:21:18

Если требуется универсальный сервис отображения окон, то лучше не привязываться к `DialogResult` WinForms или `MessageBoxResult` WPF. Сделай собственные перечисления в абстракциях.

```csharp
public enum MessageResult
{
    None = 0,

    Ok,
    Cancel,

    Yes,
    No,

    Retry,
    Abort,
    Ignore
}
```

```csharp
public enum MessageButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel,
    RetryCancel,
    AbortRetryIgnore
}
```

```csharp
public enum MessageIcon
{
    None,
    Information,
    Warning,
    Error,
    Question
}
```

## Интерфейс

```csharp
public interface IWindowMessageService
{
    MessageResult Show(
        string text,
        string caption = null,
        MessageButtons buttons = MessageButtons.Ok,
        MessageIcon icon = MessageIcon.None);

    void ShowInformation(
        string text,
        string caption = null);

    void ShowWarning(
        string text,
        string caption = null);

    void ShowError(
        string text,
        string caption = null);

    bool Ask(
        string text,
        string caption = null);
}
```

`Ask()` — просто удобная обертка над самым частым сценарием `Yes/No`.

---

## Реализация для WinForms

```csharp
using System.Windows.Forms;

public sealed class WindowMessageService : IWindowMessageService
{
    public MessageResult Show(
        string text,
        string caption = null,
        MessageButtons buttons = MessageButtons.Ok,
        MessageIcon icon = MessageIcon.None)
    {
        DialogResult result = MessageBox.Show(
            text,
            caption ?? string.Empty,
            Convert(buttons),
            Convert(icon));

        return Convert(result);
    }

    public void ShowInformation(string text, string caption = null)
    {
        Show(text, caption, MessageButtons.Ok, MessageIcon.Information);
    }

    public void ShowWarning(string text, string caption = null)
    {
        Show(text, caption, MessageButtons.Ok, MessageIcon.Warning);
    }

    public void ShowError(string text, string caption = null)
    {
        Show(text, caption, MessageButtons.Ok, MessageIcon.Error);
    }

    public bool Ask(string text, string caption = null)
    {
        return Show(
            text,
            caption,
            MessageButtons.YesNo,
            MessageIcon.Question) == MessageResult.Yes;
    }

    private static MessageBoxButtons Convert(MessageButtons buttons)
    {
        return buttons switch
        {
            MessageButtons.Ok => MessageBoxButtons.OK,
            MessageButtons.OkCancel => MessageBoxButtons.OKCancel,
            MessageButtons.YesNo => MessageBoxButtons.YesNo,
            MessageButtons.YesNoCancel => MessageBoxButtons.YesNoCancel,
            MessageButtons.RetryCancel => MessageBoxButtons.RetryCancel,
            MessageButtons.AbortRetryIgnore => MessageBoxButtons.AbortRetryIgnore,
            _ => MessageBoxButtons.OK
        };
    }

    private static MessageBoxIcon Convert(MessageIcon icon)
    {
        return icon switch
        {
            MessageIcon.Information => MessageBoxIcon.Information,
            MessageIcon.Warning => MessageBoxIcon.Warning,
            MessageIcon.Error => MessageBoxIcon.Error,
            MessageIcon.Question => MessageBoxIcon.Question,
            _ => MessageBoxIcon.None
        };
    }

    private static MessageResult Convert(DialogResult result)
    {
        return result switch
        {
            DialogResult.OK => MessageResult.Ok,
            DialogResult.Cancel => MessageResult.Cancel,
            DialogResult.Yes => MessageResult.Yes,
            DialogResult.No => MessageResult.No,
            DialogResult.Retry => MessageResult.Retry,
            DialogResult.Abort => MessageResult.Abort,
            DialogResult.Ignore => MessageResult.Ignore,
            _ => MessageResult.None
        };
    }
}
```

### Использование

```csharp
if (_messageService.Show(
        "Сохранить изменения?",
        "SpecSPDS",
        MessageButtons.YesNoCancel,
        MessageIcon.Question) == MessageResult.Yes)
{
    Save();
}
```

или

```csharp
if (_messageService.Ask("Удалить выбранные объекты?"))
{
    Delete();
}
```

### Что я бы добавил

Для библиотеки с долгим сроком жизни имеет смысл сделать перегрузку с владельцем окна:

```csharp
MessageResult Show(
    object owner,
    string text,
    string caption = null,
    MessageButtons buttons = MessageButtons.Ok,
    MessageIcon icon = MessageIcon.None);
```

Тогда реализация для nanoCAD сможет передавать `IntPtr`, `IWin32Window` или другой тип владельца, а интерфейс останется независимым от WinForms/WPF. Это позволит корректно показывать модальные окна поверх главного окна CAD и избежать проблем с фокусом.

## Prompt:
23.07.2026, 06:51:37

Есть смысл диалоги ком строки нанокад оборачивать в интерфейс?

## Response:
23.07.2026, 06:51:37

Да, я бы оборачивал. Но не каждую функцию в отдельный интерфейс, а сделал общий сервис взаимодействия с пользователем.

Например:

```csharp
public interface IMessageService
{
    void Write(string message);

    void WriteLine(string message);

    MessageResult Show(
        string text,
        string caption = null,
        MessageButtons buttons = MessageButtons.Ok,
        MessageIcon icon = MessageIcon.None);
}
```

А уже реализации могут быть разными:

- `CommandLineMessageService` — пишет в командную строку nanoCAD.
- `WindowMessageService` — показывает `MessageBox`.
- `SilentMessageService` — ничего не делает (для тестов или пакетного режима).
- `CompositeMessageService` — одновременно пишет в командную строку и показывает окно при необходимости.

Если же речь именно о диалогах командной строки (`Editor.GetString()`, `GetPoint()`, `GetInteger()`, `GetKeywords()` и т.п.), то **их тоже стоит абстрагировать**, но в отдельный интерфейс, потому что это уже не вывод сообщений, а взаимодействие с пользователем.

Например:

```csharp
public interface ICommandLineService
{
    void Write(string message);

    string GetString(string prompt);

    int? GetInteger(string prompt);

    double? GetDouble(string prompt);

    Point3d? GetPoint(string prompt);

    MessageResult GetKeyword(
        string prompt,
        params string[] keywords);
}
```

Это дает несколько преимуществ:

- код перестает зависеть от API nanoCAD;
- можно сделать реализацию для AutoCAD;
- легко писать unit-тесты с подменой сервиса;
- вся работа с `PromptResult`, `PromptStatus`, `PromptPointResult` и другими типами SDK оказывается скрыта внутри реализации.

### Я бы разделил ответственность так

- `IMessageService` — вывод информации пользователю (командная строка, окно, лог).
- `IWindowMessageService` — модальные окна.
- `ICommandLineService` — интерактивный ввод через командную строку CAD (`GetPoint`, `GetEntity`, `GetKeywords` и т.д.).

Такое разделение соответствует принципу единственной ответственности и делает архитектуру более расширяемой. Для аддона nanoCAD это обычно оказывается наиболее удачным вариантом.

## Prompt:
23.07.2026, 07:14:19

Допустим есть IloggerServise
Как организовать, чтобы например все error попадали в лог и выводились пользователю, Окном или ком строкой?

## Response:
23.07.2026, 07:14:20

Я бы **не смешивал логирование и показ сообщений**. Логгер должен только логировать. Иначе получится, что библиотека логирования начнет зависеть от UI.

Лучше сделать отдельный сервис уведомлений, который использует логгер.

Например:

```text
Команда
    │
    ▼
IUserNotificationService
    │
    ├── ILogger
    └── IMessageService
```

Например:

```csharp
public interface IUserNotificationService
{
    void Information(string message);

    void Warning(string message);

    void Error(string message);

    void Error(Exception exception, string message = null);
}
```

Реализация:

```csharp
public sealed class UserNotificationService : IUserNotificationService
{
    private readonly IDrzLogger _logger;
    private readonly IMessageService _messageService;

    public void Error(Exception exception, string message = null)
    {
        _logger.Error(exception, message);

        _messageService.Show(
            message ?? exception.Message,
            "Ошибка",
            MessageButtons.Ok,
            MessageIcon.Error);
    }
}
```

Тогда в коде:

```csharp
try
{
    ...
}
catch (Exception ex)
{
    _notification.Error(ex, "Не удалось открыть документ.");
}
```

Получится автоматически:

- ✔ запись в лог;
- ✔ сообщение пользователю;
- ✔ при необходимости можно добавить телеметрию, отправку отчета и т.д.

---

## Еще лучше — использовать декоратор

Если хочется, чтобы **любой вызов `ILogger.Error()` автоматически показывал сообщение**, то можно сделать декоратор:

```text
IDrzLogger
    ▲
    │
UiLoggerDecorator
    │
    ├── IDrzLogger
    └── IMessageService
```

```csharp
public void Error(Exception ex, string message = null)
{
    _inner.Error(ex, message);

    _messageService.ShowError(message ?? ex.Message);
}
```

Но я бы так **не делал**.

Почему?

Потому что не каждая ошибка должна показываться пользователю.

Например:

```csharp
_logger.Error(ex, "Не удалось удалить временный файл.");
```

Это полезно в логе, но пользователю это сообщение может быть совершенно не нужно.

---

### Для твоей архитектуры я бы разделил обязанности так:

- **`IDrzLogger`** — только пишет в лог.
- **`IMessageService`** — показывает сообщения (окно или командная строка).
- **`IUserNotificationService`** — объединяет оба сервиса для тех случаев, когда ошибку нужно и залогировать, и показать пользователю.

Такое разделение дает максимальную гибкость: можно логировать без уведомления, уведомлять без логирования (редко, но бывает) или делать и то, и другое одновременно. Именно этот вариант лучше масштабируется при развитии библиотеки.

## Prompt:
23.07.2026, 07:18:13

public sealed class UserNotificationService : IUserNotificationService
{
    private readonly IDrzLogger _logger;
    private readonly IMessageService _messageService;

    public void Error(Exception exception, string message = null)
    {
        _logger.Error(exception, message);

        _messageService.Show(
            message ?? exception.Message,
            "Ошибка",
            MessageButtons.Ok,
            MessageIcon.Error);
    }
}
Как логер и сервис сообщений попадут в класс?

## Response:
23.07.2026, 07:18:13

Через DI. У тебя же уже используется **SimpleInjector**.

Например:

```csharp
container.RegisterSingleton<IDrzLoggerFactory, DrzLoggerFactory>();

container.RegisterSingleton<IMessageService, WindowMessageService>();

container.RegisterSingleton<IUserNotificationService, UserNotificationService>();
```

Конструктор:

```csharp
public sealed class UserNotificationService : IUserNotificationService
{
    private readonly IDrzLogger _logger;
    private readonly IMessageService _messageService;

    public UserNotificationService(
        IDrzLoggerFactory loggerFactory,
        IMessageService messageService)
    {
        _logger = loggerFactory.GetLogger<UserNotificationService>();
        _messageService = messageService;
    }

    ...
}
```

### Но в твоей архитектуре я бы сделал иначе

Насколько помню, у тебя уже есть `LogFactory` и в каждом классе создается собственный логгер. Тогда внедрять `IDrzLogger` в `UserNotificationService` не очень правильно — у всех сообщений будет один источник (`UserNotificationService`), а не класс, из которого произошла ошибка.

Например:

```csharp
_command.Execute();

catch (Exception ex)
{
    _notification.Error(ex);
}
```

В логе получится:

```
ERROR UserNotificationService
...
```

Хотя ошибка произошла в `OpenDrawingCommand`.

### Более удачный вариант

Пусть класс сам пишет в свой логгер:

```csharp
catch (Exception ex)
{
    _logger.Error(ex);

    _messageService.ShowError(ex.Message);
}
```

Или сделать extension:

```csharp
_logger.ErrorAndShow(ex, _messageService);
```

или

```csharp
_logger.NotifyError(ex, _messageService);
```

Тогда:

- лог останется с правильным контекстом (`OpenDrawingCommand`);
- пользователю покажется сообщение;
- не появится "центральный" логгер, который скрывает источник ошибки.

Для твоей библиотеки, где большое значение имеет контекст логирования (`GetLogger<T>()`), я бы **не выносил логирование внутрь `UserNotificationService`**. Лучше пусть этот сервис отвечает только за отображение сообщений, а решение о том, что именно логировать, остается в вызывающем коде. Это сохранит правильное имя логгера и сделает логи значительно полезнее.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)