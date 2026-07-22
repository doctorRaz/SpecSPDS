# Оптимизация получения сервиса

**User:** dRz  
**Created:** 7/21/2026 22:36:00  
**Updated:** 7/22/2026 5:55:24  
**Exported:** 7/22/2026 6:03:39  
**Link:** [https://chatgpt.com/c/6a5fc9a2-a350-83eb-a350-27600320e9c2](https://chatgpt.com/c/6a5fc9a2-a350-83eb-a350-27600320e9c2)  

## Prompt:
21.07.2026, 22:35:59

фабрика сервисов сообщений

```
    private void RegisterServices(Container container)
    {
        container.Register<IWindowHandleProvider, CadWindowProvider>(Lifestyle.Singleton);//IntPtr Handle

        container.Register<ICommandLineMessageService, CommandLineMessageService>();

        container.Register<IMcNotificatorMessageService, McNotificatorMessageServise>();

        container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

        container.Register<IDocumentService, DocumentService>(Lifestyle.Singleton);
    }
предложи как из другой сборки оптимально получать нужный сервис сообщений
использую для получения AddOnContext
```

## Response:
21.07.2026, 22:36:00

Если уже есть `AddOnContext` как точка входа в инфраструктуру, то я бы не стал передавать конкретные реализации (`IWindowMessageService`, `ICommandLineMessageService` и т.д.) между сборками. Лучше скрыть контейнер за одним интерфейсом.

Есть несколько вариантов.

### Вариант 1. AddOnContext.GetService<T>() (наиболее универсальный)

```csharp
public static class AddOnContext
{
    public static T GetService<T>() where T : class
        => Container.GetInstance<T>();
}
```

Использование:

```csharp
var messageService = AddOnContext.GetService<ICommandLineMessageService>();
messageService.ShowMessage("Hello");
```

или

```csharp
AddOnContext.GetService<IWindowMessageService>()
            .ShowMessage("Hello");
```

Плюсы:

- минимум кода;
- не нужен отдельный сервис-фабрика;
- работает для любых сервисов.

Минус — вызывающий должен знать нужный интерфейс.

---

### Вариант 2. IMessageServiceProvider (мой предпочтительный)

Если хочется скрыть конкретные интерфейсы.

```csharp
public interface IMessageServiceProvider
{
    IMessageService GetMessageService(MessageServiceType type);
}
```

Реализация

```csharp
internal sealed class MessageServiceProvider : IMessageServiceProvider
{
    private readonly ICommandLineMessageService _commandLine;
    private readonly IWindowMessageService _window;
    private readonly IMcNotificatorMessageService _notification;

    public MessageServiceProvider(
        ICommandLineMessageService commandLine,
        IWindowMessageService window,
        IMcNotificatorMessageService notification)
    {
        _commandLine = commandLine;
        _window = window;
        _notification = notification;
    }

    public IMessageService GetMessageService(MessageServiceType type)
    {
        switch (type)
        {
            case MessageServiceType.CommandLine:
                return _commandLine;

            case MessageServiceType.Window:
                return _window;

            case MessageServiceType.Notification:
                return _notification;

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
```

Регистрация

```csharp
container.Register<IMessageServiceProvider, MessageServiceProvider>(Lifestyle.Singleton);
```

Использование

```csharp
var provider = AddOnContext.GetService<IMessageServiceProvider>();

provider.GetMessageService(MessageServiceType.Window)
        .ShowMessage("Hello");
```

---

### Вариант 3. Специализированные свойства AddOnContext

Если сервисов немного.

```csharp
public static ICommandLineMessageService CommandLine =>
    GetService<ICommandLineMessageService>();

public static IWindowMessageService Window =>
    GetService<IWindowMessageService>();

public static IMcNotificatorMessageService Notification =>
    GetService<IMcNotificatorMessageService>();
```

Тогда

```csharp
AddOnContext.Window.ShowMessage("...");
AddOnContext.CommandLine.ShowMessage("...");
```

Очень удобно читать.

---

## Что бы я сделал

Я бы вообще отказался от `MessageServiceType`.

Оставил бы:

```csharp
AddOnContext.GetService<IWindowMessageService>();

AddOnContext.GetService<ICommandLineMessageService>();

AddOnContext.GetService<IMcNotificatorMessageService>();
```

или даже

```csharp
AddOnContext.Window
AddOnContext.CommandLine
AddOnContext.Notification
```

Это:
- без `switch`;
- без фабрики;
- без enum;
- легко расширяется;
- SimpleInjector сам гарантирует получение нужной реализации.

Фабрика с `MessageServiceType` имеет смысл только тогда, когда тип сервиса определяется динамически во время выполнения (например, считывается из настроек или передается параметром). Если же код заранее знает, что ему нужен именно `IWindowMessageService`, то фабрика лишь добавляет лишний уровень косвенности.

## Prompt:
21.07.2026, 22:39:47

все три интерфейса передавать в зависимую сборку параметром?
хотелось бы передать одним параметром и на месте получить нужный интерфейс

## Response:
21.07.2026, 22:39:48

Да, тогда фабрика как раз имеет смысл.

Но я бы передавал **не три интерфейса и не контейнер**, а один интерфейс-фабрику.

Например:

```csharp
public interface IMessageServiceFactory
{
    T Get<T>() where T : class, IMessageService;
}
```

Реализация:

```csharp
internal sealed class MessageServiceFactory : IMessageServiceFactory
{
    private readonly Container _container;

    public MessageServiceFactory(Container container)
    {
        _container = container;
    }

    public T Get<T>() where T : class, IMessageService
    {
        return _container.GetInstance<T>();
    }
}
```

или вообще без зависимости от `Container`:

```csharp
internal sealed class MessageServiceFactory : IMessageServiceFactory
{
    private readonly ICommandLineMessageService _commandLine;
    private readonly IWindowMessageService _window;
    private readonly IMcNotificatorMessageService _notification;

    public MessageServiceFactory(
        ICommandLineMessageService commandLine,
        IWindowMessageService window,
        IMcNotificatorMessageService notification)
    {
        _commandLine = commandLine;
        _window = window;
        _notification = notification;
    }

    public T Get<T>() where T : class, IMessageService
    {
        if (typeof(T) == typeof(ICommandLineMessageService))
            return (T)_commandLine;

        if (typeof(T) == typeof(IWindowMessageService))
            return (T)_window;

        if (typeof(T) == typeof(IMcNotificatorMessageService))
            return (T)_notification;

        throw new NotSupportedException(typeof(T).FullName);
    }
}
```

Использование в другой сборке:

```csharp
public class MyModule
{
    private readonly IMessageServiceFactory _messages;

    public MyModule(IMessageServiceFactory messages)
    {
        _messages = messages;
    }

    public void Execute()
    {
        _messages.Get<IWindowMessageService>()
                 .ShowMessage("Hello");

        _messages.Get<ICommandLineMessageService>()
                 .ShowMessage("Done");
    }
}
```

### Это лучше, чем enum

Не нужно поддерживать

```csharp
Get(MessageServiceType.Window)
```

и каждый раз делать `switch`.

Компилятор сам проверит:

```csharp
_messages.Get<IWindowMessageService>();
```

Если интерфейс переименуется — код не скомпилируется. С `enum` такой защиты нет.

---

### Если сервисов может стать больше

Такой интерфейс можно сделать универсальным:

```csharp
public interface IServiceProviderEx
{
    T Get<T>() where T : class;
}
```

Тогда через один параметр можно получать **любые** сервисы:

```csharp
_services.Get<IWindowMessageService>();
_services.Get<ICommandLineMessageService>();
_services.Get<IDocumentService>();
_services.Get<ILogger>();
```

Для архитектуры с `AddOnContext` этот вариант мне нравится больше всего: зависимая сборка знает только про `IServiceProviderEx`, а конкретные сервисы получает по мере необходимости, не завися от контейнера DI напрямую.

## Prompt:
21.07.2026, 22:44:14

как его регистрировать в контейнере?

## Response:
21.07.2026, 22:44:15

Если использовать **SimpleInjector**, есть несколько вариантов.

### Вариант 1. Через `Func<Type, object>` (самый чистый)

Интерфейс:

```csharp
public interface IServiceProviderEx
{
    T Get<T>() where T : class;
}
```

Реализация:

```csharp
internal sealed class ServiceProviderEx : IServiceProviderEx
{
    private readonly Func<Type, object> _resolver;

    public ServiceProviderEx(Func<Type, object> resolver)
    {
        _resolver = resolver;
    }

    public T Get<T>() where T : class
    {
        return (T)_resolver(typeof(T));
    }
}
```

Регистрация:

```csharp
container.RegisterInstance<Func<Type, object>>(container.GetInstance);

container.Register<IServiceProviderEx, ServiceProviderEx>(Lifestyle.Singleton);
```

---

### Вариант 2. Передать Container

Если не смущает зависимость от SimpleInjector:

```csharp
internal sealed class ServiceProviderEx : IServiceProviderEx
{
    private readonly Container _container;

    public ServiceProviderEx(Container container)
    {
        _container = container;
    }

    public T Get<T>() where T : class
    {
        return _container.GetInstance<T>();
    }
}
```

Регистрация:

```csharp
container.RegisterInstance(container);

container.Register<IServiceProviderEx, ServiceProviderEx>(Lifestyle.Singleton);
```

Но этот вариант привязывает класс к SimpleInjector.

---

### Вариант 3. Вообще без класса

Если нужен только один экземпляр, можно зарегистрировать готовый объект:

```csharp
public sealed class ServiceProviderEx : IServiceProviderEx
{
    private readonly Container _container;

    public ServiceProviderEx(Container container)
    {
        _container = container;
    }

    public T Get<T>() where T : class
    {
        return _container.GetInstance<T>();
    }
}
```

```csharp
container.RegisterInstance<IServiceProviderEx>(
    new ServiceProviderEx(container));
```

---

## Что я бы выбрал

Я бы не называл интерфейс `IServiceProviderEx`, так как есть стандартный `IServiceProvider`.

Лучше назвать его по назначению, например:

```csharp
public interface IAddOnServices
{
    T Get<T>() where T : class;
}
```

или

```csharp
public interface IServiceResolver
{
    T Get<T>() where T : class;
}
```

Регистрация тогда выглядит очень просто:

```csharp
container.RegisterSingleton<IAddOnServices>(
    () => new AddOnServices(container));
```

или

```csharp
container.RegisterInstance<IAddOnServices>(
    new AddOnServices(container));
```

Так зависимые сборки получают **один параметр** (`IAddOnServices`) и могут запрашивать любые зарегистрированные сервисы, не зная ничего о `Container`.

## Prompt:
21.07.2026, 22:48:17

т.е. AddOnContext  получается не нужен, по интерфейсу можно вытащить любой объект из аддона?

## Response:
21.07.2026, 22:48:18

Именно. Если в зависимые сборки передается `IAddOnServices` (или `IServiceResolver`), то **`AddOnContext` внутри этих сборок становится практически не нужен**.

Например:

```csharp
public interface IAddOnServices
{
    T Get<T>() where T : class;
}
```

Использование:

```csharp
public class MyCommand
{
    private readonly IAddOnServices _services;

    public MyCommand(IAddOnServices services)
    {
        _services = services;
    }

    public void Execute()
    {
        _services.Get<IWindowMessageService>()
                 .ShowMessage("Hello");

        _services.Get<IDocumentService>()
                 .Open(...);

        _services.Get<IDrzLogger>()
                 .Info("Done");
    }
}
```

### Но есть нюанс

Это фактически **Service Locator**. Обычно DI рекомендует его избегать, потому что:

```csharp
public class MyCommand
{
    public MyCommand(IAddOnServices services)
    {
    }
}
```

По конструктору совершенно непонятно, от каких сервисов реально зависит класс. Они могут появляться внутри метода:

```csharp
_services.Get<IDocumentService>();
_services.Get<IWindowMessageService>();
_services.Get<ISettingsService>();
_services.Get<ILogger>();
```

Если же зависимости передаются явно:

```csharp
public MyCommand(
    IDocumentService documentService,
    IWindowMessageService messageService)
```

то архитектура получается прозрачнее.

---

## Для твоей архитектуры

У тебя ситуация немного отличается:

- есть **граница между сборками**;
- одна сборка не должна знать обо всех сервисах заранее;
- `AddOnContext` уже выполняет роль сервис-локатора.

Поэтому заменить `AddOnContext` на `IAddOnServices` — вполне логичное решение.

Получится примерно так:

```
AddOn
 ├── Container (SimpleInjector)
 ├── IAddOnServices
 └── RegisterServices()

          │
          ▼

Library A
Library B
Library C

получают только IAddOnServices
```

Никаких статических классов.

---

### Я бы даже ограничил возможности

Вместо универсального

```csharp
T Get<T>();
```

сделал бы

```csharp
public interface IAddOnServices
{
    T GetRequiredService<T>() where T : class;
}
```

или

```csharp
bool TryGet<T>(out T service) where T : class;
```

Так сразу понятно, что это именно получение зарегистрированного сервиса.

---

Для твоего проекта, где аддон состоит из нескольких сборок и есть единая точка композиции (`CompositionRoot`), **передача одного `IAddOnServices` выглядит архитектурно чище, чем статический `AddOnContext`**. Статический контекст можно оставить лишь как удобную обертку для кода, который нельзя создавать через DI (например, статические команды, точки входа SDK, обработчики, создаваемые самим nanoCAD).

## Prompt:
21.07.2026, 22:53:08

using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.CadServices.Services;
using drz.Infrastructure.Infrastructure;
using drz.Infrastructure.Services;
using drz.LogBootstrap;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Reflection;

namespace drz.AddOnRuntime
{
///  Наполнение SimpleInjector объектами 
/// 
public class AddOnCompositionRoot : IDisposable
{
\#region Private Fields

```
    private readonly Container _container;

    private bool _disposed;

    #endregion Private Fields

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AddOnCompositionRoot"/> class.
    /// </summary>
    /// <param name="addOnAssembly">The add on assembly.</param>
    public AddOnCompositionRoot(Assembly addOnAssembly)
    {
        _container = new Container();

        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        RegisterInfrastructure(_container, addOnAssembly);

        RegisterServices(_container);

        _container.Verify();
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Создать скоуп для Scoped-сервисов
    /// </summary>
    public Scope BeginScope() => AsyncScopedLifestyle.BeginScope(_container);

    /// <summary>
    /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _container.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Получить сервис
    /// </summary>
    public TService Get<TService>() where TService : class
    {
        return _container.GetInstance<TService>();
    }

    #endregion Public Methods

    #region Private Methods

    private void RegisterInfrastructure(Container container, Assembly addOnAssembly)
    {
        container.RegisterInstance(addOnAssembly);

        container.Register<IAddOnInfo, AddOnInfo>(Lifestyle.Singleton);

        container.Register<ISysInfo, SysInfo>(Lifestyle.Singleton);

        container.Register<ICadInfo, CadInfo>(Lifestyle.Singleton);

        container.RegisterSingleton<IDrzLoggerFactory>(() => NLogBootstrap.GetLoggerFactory(container.GetInstance<IAddOnInfo>()));
    }

    private void RegisterServices(Container container)
    {
        container.Register<IWindowHandleProvider, CadWindowProvider>(Lifestyle.Singleton);//IntPtr Handle

        container.Register<ICommandLineMessageService, CommandLineMessageService>();

        container.Register<IMcNotificatorMessageService, McNotificatorMessageServise>();

        container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

        container.Register<IDocumentService, DocumentService>(Lifestyle.Singleton);
    }

    #endregion Private Methods
}
```

}

сейчас так, добавь все необходимое в контейнер

## Response:
21.07.2026, 22:53:08

У тебя уже есть практически всё необходимое. Я бы сделал так:

1. Вынес `Get<T>()` из `AddOnCompositionRoot` в отдельный интерфейс.
2. Зарегистрировал сам `AddOnCompositionRoot` как реализацию этого интерфейса.

Например.

### Интерфейс

```csharp
public interface IAddOnServices
{
    T Get<T>() where T : class;
}
```

### CompositionRoot

```csharp
public class AddOnCompositionRoot : IAddOnServices, IDisposable
{
    ...
    public T Get<T>() where T : class
    {
        return _container.GetInstance<T>();
    }
}
```

### Регистрация

После регистрации остальных сервисов добавь:

```csharp
container.RegisterInstance<IAddOnServices>(this);
```

То есть конструктор станет примерно таким:

```csharp
public AddOnCompositionRoot(Assembly addOnAssembly)
{
    _container = new Container();

    _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

    RegisterInfrastructure(_container, addOnAssembly);

    RegisterServices(_container);

    _container.RegisterInstance<IAddOnServices>(this);

    _container.Verify();
}
```

---

## Или еще лучше

Чтобы `AddOnCompositionRoot` не становился сервисом, сделать небольшую обертку.

```csharp
internal sealed class AddOnServices : IAddOnServices
{
    private readonly Container _container;

    public AddOnServices(Container container)
    {
        _container = container;
    }

    public T Get<T>() where T : class
    {
        return _container.GetInstance<T>();
    }
}
```

Тогда регистрация:

```csharp
container.RegisterInstance<IAddOnServices>(
    new AddOnServices(container));
```

или

```csharp
container.RegisterSingleton<IAddOnServices>(
    () => new AddOnServices(container));
```

---

## Мне второй вариант нравится больше

Потому что тогда:

- `AddOnCompositionRoot` отвечает только за построение контейнера;
- `IAddOnServices` — только за получение сервисов;
- нет лишнего метода `Get<T>()` у `AddOnCompositionRoot`.

В итоге зависимым сборкам передается **только `IAddOnServices`**, а `AddOnCompositionRoot` вообще не покидает границы инфраструктурной сборки. Это хорошо разделяет ответственность и упрощает дальнейшее развитие архитектуры.

## Prompt:
22.07.2026, 05:35:17

Как передать объект IAddOnServices между сборками?

## Response:
22.07.2026, 05:35:17

Есть несколько способов, но для твоей архитектуры один из них выглядит наиболее естественным.

### 1. Передавать через конструктор (предпочтительно)

Если класс создаешь ты сам:

```csharp
public class MyModule
{
    private readonly IAddOnServices _services;

    public MyModule(IAddOnServices services)
    {
        _services = services;
    }
}
```

Создание:

```csharp
var module = new MyModule(compositionRoot.Get<IAddOnServices>());
```

или вообще

```csharp
var module = container.GetInstance<MyModule>();
```

если `MyModule` зарегистрирован в контейнере.

---

### 2. Передать в метод и сохранить

Если объект создается сторонним SDK (например, nanoCAD):

```csharp
public class MyCommands
{
    private IAddOnServices _services;

    public void Initialize(IAddOnServices services)
    {
        _services = services;
    }
}
```

Вызывается один раз при инициализации аддона.

---

### 3. Передать в контекст библиотеки

Если библиотека использует статические методы:

```csharp
public static class LibraryContext
{
    public static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }
}
```

Тогда один раз:

```csharp
LibraryContext.Initialize(
    compositionRoot.Get<IAddOnServices>());
```

и дальше в библиотеке:

```csharp
var logger = LibraryContext.Services.Get<IDrzLoggerFactory>();
```

---

## Что я бы сделал в твоем проекте

Поскольку у тебя уже есть `AddOnContext`, я бы не убирал его полностью, а превратил в **тонкую обертку** над `IAddOnServices`.

```csharp
public static class AddOnContext
{
    private static IAddOnServices _services;

    public static void Initialize(IAddOnServices services)
    {
        _services = services;
    }

    public static T Get<T>() where T : class
    {
        return _services.Get<T>();
    }
}
```

При запуске аддона:

```csharp
AddOnContext.Initialize(
    compositionRoot.Get<IAddOnServices>());
```

Тогда в любой сборке:

```csharp
var logger = AddOnContext.Get<IDrzLoggerFactory>();
var msg = AddOnContext.Get<IWindowMessageService>();
```

При этом сама реализация больше не знает о `Container` или `SimpleInjector`, а `AddOnContext` становится лишь удобной точкой доступа к зарегистрированным сервисам, а не владельцем контейнера. Это позволяет при необходимости заменить реализацию `IAddOnServices` без изменения остального кода.

## Prompt:
22.07.2026, 05:55:11

Напиши интерфейс, реализацию и примеры передачи между сборками и пример использования.
Добавь подробные комментарии

## Response:
22.07.2026, 05:55:12

Я бы сделал так. Это не привязывает библиотеки к `SimpleInjector`, позволяет передавать **один объект** между сборками и при этом сохраняет возможность получать любой зарегистрированный сервис.

## 1. Интерфейс (drz.Abstractions)

```csharp
namespace drz.Abstractions.Infrastructure
{
    /// <summary>
    /// Предоставляет доступ к сервисам, зарегистрированным в контейнере аддона.
    /// </summary>
    /// <remarks>
    /// Интерфейс предназначен для передачи между сборками.
    /// Сборки не должны иметь зависимость от SimpleInjector и не должны
    /// получать доступ к контейнеру напрямую.
    ///
    /// Обычно экземпляр интерфейса передается один раз при инициализации
    /// библиотеки или через конструктор класса.
    /// </remarks>
    public interface IAddOnServices
    {
        /// <summary>
        /// Возвращает зарегистрированный сервис указанного типа.
        /// </summary>
        /// <typeparam name="TService">
        /// Тип требуемого сервиса.
        /// </typeparam>
        /// <returns>
        /// Экземпляр зарегистрированного сервиса.
        /// </returns>
        /// <exception cref="ActivationException">
        /// Сервис данного типа отсутствует в контейнере.
        /// </exception>
        TService Get<TService>() where TService : class;
    }
}
```

---

## 2. Реализация (Infrastructure)

```csharp
using drz.Abstractions.Infrastructure;
using SimpleInjector;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// Реализация <see cref="IAddOnServices"/>,
    /// предоставляющая доступ к сервисам контейнера.
    /// </summary>
    internal sealed class AddOnServices : IAddOnServices
    {
        private readonly Container _container;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        /// <param name="container">
        /// Контейнер SimpleInjector.
        /// </param>
        public AddOnServices(Container container)
        {
            _container = container;
        }

        /// <inheritdoc/>
        public TService Get<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }
    }
}
```

---

## 3. Регистрация в CompositionRoot

После регистрации всех сервисов добавить:

```csharp
container.RegisterInstance<IAddOnServices>(
    new AddOnServices(container));
```

Теперь `IAddOnServices` сам становится сервисом контейнера.

---

## 4. Получение интерфейса

В `AddOnCompositionRoot` уже есть

```csharp
public TService Get<TService>() where TService : class
{
    return _container.GetInstance<TService>();
}
```

Поэтому получить интерфейс очень просто:

```csharp
IAddOnServices services =
    compositionRoot.Get<IAddOnServices>();
```

---

# Пример 1. Передача через конструктор

Главная сборка

```csharp
IAddOnServices services =
    compositionRoot.Get<IAddOnServices>();

var module = new MyModule(services);
```

Зависимая сборка

```csharp
public class MyModule
{
    private readonly IAddOnServices _services;

    /// <summary>
    /// Инициализирует модуль.
    /// </summary>
    public MyModule(IAddOnServices services)
    {
        _services = services;
    }

    public void Execute()
    {
        var message =
            _services.Get<IWindowMessageService>();

        message.ShowMessage("Hello");
    }
}
```

---

# Пример 2. Инициализация библиотеки

Если библиотека содержит статические классы.

```csharp
public static class LibraryContext
{
    /// <summary>
    /// Сервисы текущего аддона.
    /// </summary>
    public static IAddOnServices Services { get; private set; }

    /// <summary>
    /// Выполняет инициализацию библиотеки.
    /// </summary>
    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }
}
```

Главная сборка

```csharp
LibraryContext.Initialize(
    compositionRoot.Get<IAddOnServices>());
```

Любой код библиотеки

```csharp
var logger =
    LibraryContext.Services.Get<IDrzLoggerFactory>();

var msg =
    LibraryContext.Services.Get<ICommandLineMessageService>();

msg.ShowMessage("Ready");
```

---

# Пример 3. Получение нескольких сервисов

```csharp
public class DocumentProcessor
{
    private readonly IAddOnServices _services;

    public DocumentProcessor(IAddOnServices services)
    {
        _services = services;
    }

    public void Process()
    {
        var documentService =
            _services.Get<IDocumentService>();

        var loggerFactory =
            _services.Get<IDrzLoggerFactory>();

        var logger =
            loggerFactory.GetLogger<DocumentProcessor>();

        var messageService =
            _services.Get<IWindowMessageService>();

        logger.Info("Начало обработки.");

        documentService.Save();

        messageService.ShowMessage("Готово.");
    }
}
```

## Структура зависимостей

```
drz.Abstractions
│
├── IAddOnServices
├── IMessageService
├── IDocumentService
└── ...

                ▲

drz.Infrastructure
│
├── AddOnCompositionRoot
├── AddOnServices
└── SimpleInjector

                ▲

Главный AddOn
│
├── создает CompositionRoot
├── получает IAddOnServices
└── передает его другим сборкам

                ▲

Любая зависимая сборка
│
└── использует только IAddOnServices
```

Такое решение хорошо масштабируется: при появлении новых сервисов (`ISettingsService`, `IUpdateService`, `ILicenseService` и т.д.) код `IAddOnServices` не изменяется, а зависимые сборки могут получать их без каких-либо дополнительных фабрик или перечислений.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)