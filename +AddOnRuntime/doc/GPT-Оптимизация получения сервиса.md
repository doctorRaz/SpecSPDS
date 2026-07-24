# Оптимизация получения сервиса

**User:** dRz  
**Created:** 7/21/2026 22:36:00  
**Updated:** 7/22/2026 11:43:55  
**Exported:** 7/23/2026 22:31:03  
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

## Prompt:
22.07.2026, 06:05:44

Например могу создать экземпляр 
ISettingsService и все сборки получат одинаковые настройки?

## Response:
22.07.2026, 06:05:44

Да. Именно в этом одно из главных преимуществ контейнера DI.

Если зарегистрировать сервис как **Singleton**, то все сборки будут получать **один и тот же экземпляр**.

Например:

```csharp
container.Register<ISettingsService, SettingsService>(Lifestyle.Singleton);
```

или

```csharp
var settings = new SettingsService();

container.RegisterInstance<ISettingsService>(settings);
```

Тогда из любой сборки:

```csharp
var settings = _services.Get<ISettingsService>();
```

вернется **один и тот же объект**.

Например:

```csharp
// Сборка A
var settings = _services.Get<ISettingsService>();
settings.Theme = "Dark";
```

```csharp
// Сборка B
var settings = _services.Get<ISettingsService>();

Console.WriteLine(settings.Theme);
// Dark
```

`ReferenceEquals` тоже вернет `true`:

```csharp
var s1 = services.Get<ISettingsService>();
var s2 = services.Get<ISettingsService>();

Debug.Assert(ReferenceEquals(s1, s2)); // true
```

---

Это относится не только к настройкам.

Например, можно зарегистрировать:

- `ISettingsService`
- `IDrzLoggerFactory`
- `IUpdateService`
- `ILicenseService`
- `IMessageRouter`
- `IEventBus`

и все сборки будут работать с **одними и теми же экземплярами**, если они зарегистрированы как `Singleton`.

---

Для твоей архитектуры это особенно удобно: каждая сборка получает только `IAddOnServices`, а дальше может запросить любой общий сервис, зарегистрированный аддоном. Получается единое "пространство сервисов" для всего аддона без статических глобальных объектов и без зависимости библиотек от `SimpleInjector`.

## Prompt:
22.07.2026, 10:42:09

public static class LibraryContext
{
    /// <summary>
    /// Сервисы текущего аддона.
    /// </summary>
    Internal static IAddOnServices Services { get; private set; }

    /// <summary>
    /// Выполняет инициализацию библиотеки.
    /// </summary>
    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }
}
Для обычного класса так тоже можно, чтобы не объявлять в каждом классе?

## Response:
22.07.2026, 10:42:09

Да, можно. Более того, для твоей архитектуры это, вероятно, наиболее практичный вариант.

Например, сделать базовый класс:

```csharp
public abstract class AddOnObject
{
    /// <summary>
    /// Сервисы текущего аддона.
    /// </summary>
    protected IAddOnServices Services => LibraryContext.Services;
}
```

Любой класс:

```csharp
public class DocumentProcessor : AddOnObject
{
    public void Execute()
    {
        var settings = Services.Get<ISettingsService>();
        var logger = Services.Get<IDrzLoggerFactory>();
        var messages = Services.Get<IWindowMessageService>();

        ...
    }
}
```

Так не нужно в каждом классе писать

```csharp
private readonly IAddOnServices _services;
```

или передавать его через конструктор.

---

### Или даже без базового класса

Если тебя устраивает статический контекст, можно обращаться напрямую:

```csharp
var logger = LibraryContext.Services.Get<IDrzLoggerFactory>();

var settings = LibraryContext.Services.Get<ISettingsService>();
```

---

## Что я бы выбрал

Для библиотек аддона я бы сделал именно базовый класс:

```csharp
public abstract class AddOnObject
{
    protected IAddOnServices Services => LibraryContext.Services;

    protected IDrzLoggerFactory LoggerFactory =>
        Services.Get<IDrzLoggerFactory>();

    protected ISettingsService Settings =>
        Services.Get<ISettingsService>();

    protected ICommandLineMessageService CommandLine =>
        Services.Get<ICommandLineMessageService>();

    protected IWindowMessageService Window =>
        Services.Get<IWindowMessageService>();

    protected IDocumentService Documents =>
        Services.Get<IDocumentService>();
}
```

Тогда код становится очень компактным:

```csharp
public class MyCommand : AddOnObject
{
    public void Execute()
    {
        Window.ShowMessage("Hello");

        LoggerFactory.GetLogger<MyCommand>()
                     .Info("Done");

        Documents.Save();
    }
}
```

Такой подход сохраняет единый экземпляр каждого `Singleton` во всех сборках, при этом не заставляет каждый класс хранить поле `_services` или получать его через конструктор. Для архитектуры плагина, где существует один `CompositionRoot` на аддон, это вполне оправданное решение.

## Prompt:
22.07.2026, 10:44:43

public abstract class AddOnObject
{
    protected IAddOnServices Services => LibraryContext.Services;

    protected IDrzLoggerFactory LoggerFactory =>
        Services.Get<IDrzLoggerFactory>();

    protected ISettingsService Settings =>
        Services.Get<ISettingsService>();

    protected ICommandLineMessageService CommandLine =>
        Services.Get<ICommandLineMessageService>();

    protected IWindowMessageService Window =>
        Services.Get<IWindowMessageService>();

    protected IDocumentService Documents =>
        Services.Get<IDocumentService>();
}
Этот класс надо дублировать во всех сборках?

## Response:
22.07.2026, 10:44:44

Нет. Дублировать его не нужно.

Есть несколько вариантов.

### Вариант 1. Самый удачный — вынести в общую библиотеку

Например, в `drz.Abstractions` или `drz.Infrastructure` (в зависимости от того, где находится `LibraryContext`).

```
drz.Abstractions
    IAddOnServices

drz.Infrastructure
    LibraryContext
    AddOnObject
```

Тогда все сборки просто наследуются:

```csharp
public class MyCommand : AddOnObject
{
}
```

Это наиболее чистое решение.

---

### Вариант 2. Вообще отказаться от `AddOnObject`

Сделать `LibraryContext` удобнее:

```csharp
public static class LibraryContext
{
    internal static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }

    public static IDrzLoggerFactory LoggerFactory =>
        Services.Get<IDrzLoggerFactory>();

    public static ISettingsService Settings =>
        Services.Get<ISettingsService>();

    public static ICommandLineMessageService CommandLine =>
        Services.Get<ICommandLineMessageService>();

    public static IWindowMessageService Window =>
        Services.Get<IWindowMessageService>();

    public static IDocumentService Documents =>
        Services.Get<IDocumentService>();
}
```

Использование:

```csharp
LibraryContext.Window.ShowMessage(...);

LibraryContext.Settings.Load();

LibraryContext.Documents.Save();
```

Тогда никакой базовый класс вообще не нужен.

---

### Вариант 3. Универсальный базовый класс

Если в большинстве классов нужен логгер, сообщения, настройки и т.д., то один `AddOnObject` в общей библиотеке — хороший вариант.

---

## Что бы я выбрал для твоего проекта

Я бы **не стал делать `Services.Get<T>()` в каждом классе**.

Лучше один раз сделать удобный API:

```csharp
LibraryContext.LoggerFactory
LibraryContext.Settings
LibraryContext.Documents
LibraryContext.CommandLine
LibraryContext.Window
```

или

```csharp
RT.LoggerFactory
RT.Settings
RT.Documents
```

(аналогично твоему `RT.Info`, `RT.Cad`, `RT.Os`).

Тогда код получается очень читаемым:

```csharp
LibraryContext.Window.ShowMessage("Ошибка");

LibraryContext.LoggerFactory
    .GetLogger<MyClass>()
    .Error(ex);

LibraryContext.Settings.Save();
```

Для твоей архитектуры с одним экземпляром `CompositionRoot` такой подход выглядит даже естественнее, чем базовый класс: он не требует наследования, работает в обычных и статических классах и не дублируется между сборками.

## Prompt:
22.07.2026, 10:48:17

LibraryContext положить в интерфейсы?
Это единственная сборка которая подключена ко всем

## Response:
22.07.2026, 10:48:21

Я бы **не стал**.

`drz.Abstractions` (интерфейсы) лучше оставить максимально "чистой" библиотекой:

- интерфейсы;
- enum;
- DTO;
- делегаты;
- атрибуты.

Без статических классов, без логики и без хранения состояния.

`LibraryContext` — это уже **реализация инфраструктуры**, а не абстракция. Он хранит состояние (`IAddOnServices`) и выполняет инициализацию.

Лучше такая структура:

```
drz.Abstractions
│
├── IAddOnServices
├── ISettingsService
├── IMessageService
├── IDocumentService
└── ...

drz.Infrastructure
│
├── LibraryContext
├── AddOnCompositionRoot
├── AddOnServices
└── ...
```

Если проблема в том, что **не все сборки ссылаются на `drz.Infrastructure`**, то есть два варианта.

### Вариант 1 (предпочтительный)

Все библиотеки аддона подключают `drz.Infrastructure`.

Это нормально. Это инфраструктурная библиотека, и именно она должна содержать такие вещи.

---

### Вариант 2

Если принципиально не хочется, чтобы библиотеки зависели от `drz.Infrastructure`, тогда можно создать отдельную небольшую библиотеку, например:

```
drz.Runtime
```

или

```
drz.Shared
```

В ней будут:

- `LibraryContext`;
- `AddOnObject` (если нужен);
- возможно `RT`.

А `drz.Infrastructure` уже будет использовать эту библиотеку.

---

### Для твоего проекта

Судя по предыдущим обсуждениям, у тебя уже есть что-то вроде `RT.Info`, `RT.Cad`, `RT.Os`.

Я бы объединил всё это в одной небольшой библиотеке времени выполнения:

```
drz.AddOnRuntime
│
├── LibraryContext
├── RT
├── AddOnCompositionRoot
└── ...
```

А `drz.Abstractions` оставил бы только с интерфейсами.

Это позволит сохранить четкое разделение:
- **Abstractions** — контракты.
- **Runtime/Infrastructure** — реализация и глобальный контекст.

## Prompt:
22.07.2026, 10:51:32

библиотеки аддона не подключают drz.Infrastructure.  о ней знает только сам аддон
Стараюсь работать через интерфейсы

## Response:
22.07.2026, 10:51:33

Тогда я бы предложил немного изменить архитектуру.

Если **все библиотеки подключают только `drz.Abstractions`**, а про `drz.Infrastructure` знает только аддон, то именно **`drz.Abstractions` должен содержать только контракты**, а не `LibraryContext`.

Я бы сделал так:

```
drz.Abstractions
│
├── IAddOnServices
├── ISettingsService
├── IMessageService
├── ...
```

```
drz.Infrastructure
│
├── AddOnCompositionRoot
├── AddOnServices : IAddOnServices
└── ...
```

```
MyLibrary
│
└── public static class LibraryContext
```

То есть **каждая библиотека имеет свой собственный `LibraryContext`**, но он очень маленький:

```csharp
public static class LibraryContext
{
    internal static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }
}
```

Во время инициализации аддона:

```csharp
var services = compositionRoot.Get<IAddOnServices>();

LibraryA.LibraryContext.Initialize(services);
LibraryB.LibraryContext.Initialize(services);
LibraryC.LibraryContext.Initialize(services);
```

При этом **объект `IAddOnServices` один и тот же**. Он содержит ссылку на один контейнер, поэтому:

```csharp
LibraryA.LibraryContext.Services.Get<ISettingsService>()
```

и

```csharp
LibraryB.LibraryContext.Services.Get<ISettingsService>()
```

вернут **один и тот же экземпляр `ISettingsService`** (если он зарегистрирован как `Singleton`).

---

### Еще один вариант, который мне нравится даже больше

Вообще отказаться от `LibraryContext`.

Сделать во всех библиотеках базовый класс:

```csharp
public abstract class LibraryBase
{
    protected static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        Services = services;
    }
}
```

Все классы библиотеки наследуются от него.

---

### Но для твоего проекта я бы не рекомендовал помещать `LibraryContext` в `drz.Abstractions`.

Причина архитектурная:

- `IAddOnServices` — это **контракт**.
- `LibraryContext` — это **глобальное состояние** (singleton + статическое поле).

Смешивать контракты и хранение состояния в одной сборке обычно не стоит.

Я бы оставил `drz.Abstractions` полностью без реализации и без статических объектов. Это позволит в будущем использовать те же интерфейсы с другой реализацией контейнера или даже без DI, не меняя библиотеку контрактов.

## Prompt:
22.07.2026, 10:54:23

Бывают интерфейсы с реализацией?

## Response:
22.07.2026, 10:54:24

Да, бывают. Но нужно различать несколько случаев.

### 1. Интерфейс с реализацией методов (C# 8+, .NET Core 3+, .NET 5+)

Начиная с C# 8 интерфейс может содержать реализацию методов:

```csharp
public interface ILogger
{
    void Info(string message)
    {
        Console.WriteLine(message);
    }
}
```

**Но в .NET Framework 4.6 это не поддерживается.** А ты используешь .NET 4.6, поэтому этот вариант отпадает.

---

### 2. Интерфейс и вложенные типы

В интерфейсе можно объявлять вложенные типы, но это встречается редко и не подходит для `LibraryContext`.

---

### 3. Интерфейс + статический класс

Это очень распространенный подход.

Например:

```text
drz.Abstractions
├── IAddOnServices
├── ISettingsService
└── AddOnContext
```

где

```csharp
public static class AddOnContext
{
    public static IAddOnServices Services { get; internal set; }
}
```

Это **не реализация интерфейса**, а просто удобный статический класс рядом с контрактами.

---

## В твоем случае

Я бы даже рассмотрел такой вариант.

В `drz.Abstractions`:

```csharp
public interface IAddOnServices
{
    T Get<T>() where T : class;
}
```

```csharp
public static class AddOnContext
{
    public static IAddOnServices Services { get; internal set; }

    public static T Get<T>() where T : class
    {
        return Services.Get<T>();
    }
}
```

А в `drz.Infrastructure`:

```csharp
AddOnContext.Services = new AddOnServices(container);
```

Тогда любая библиотека сможет написать:

```csharp
var settings = AddOnContext.Get<ISettingsService>();

var logger = AddOnContext.Get<IDrzLoggerFactory>();

var window = AddOnContext.Get<IWindowMessageService>();
```

---

## Почему это может быть хорошим решением

Обычно я бы не советовал помещать `AddOnContext` в библиотеку с интерфейсами. Но **в твоей архитектуре**:

- существует только один экземпляр аддона;
- только аддон выполняет инициализацию;
- все библиотеки подключают `drz.Abstractions`;
- библиотеки не знают о `SimpleInjector` и `drz.Infrastructure`.

В такой ситуации `AddOnContext` становится **точкой доступа к контрактам**, а не частью реализации контейнера. Это вполне оправданный компромисс, если ты сознательно строишь архитектуру вокруг единого контекста аддона.

## Prompt:
22.07.2026, 10:57:27

Если  аддон при загрузке создаст контекст
Загрузит другой аддон loadfrom
И второй тоже создаст контекст?

## Response:
22.07.2026, 10:57:27

Да, и здесь как раз возникает проблема со **статическим `LibraryContext`**.

Все зависит от того, **как загружается `drz.Abstractions.dll`**.

### Случай 1. Общая `drz.Abstractions.dll` (наиболее вероятно)

```
nanoCAD
 ├── AddOnA
 │     └── drz.Abstractions.dll
 └── AddOnB (LoadFrom)
       └── использует ту же drz.Abstractions.dll
```

Тогда существует **один** статический объект:

```csharp
AddOnContext.Services
```

Что произойдет:

1. AddOnA:

```csharp
AddOnContext.Initialize(servicesA);
```

2. Затем AddOnB:

```csharp
AddOnContext.Initialize(servicesB);
```

В результате **все библиотеки обоих аддонов начнут использовать `servicesB`**.

То есть первый аддон потеряет свой контейнер.

---

### Случай 2. Каждый аддон загружен в отдельный `AppDomain`

Тогда у каждого будет свой `AddOnContext`.

Но в nanoCAD/AutoCAD обычно используется **один AppDomain**.

---

### Случай 3. Разные копии `drz.Abstractions.dll`

Если загрузятся именно разные сборки (например, разная версия или разный identity), то статические поля будут раздельными.

Но на это рассчитывать нельзя.

---

## Поэтому я бы **не делал глобальный `AddOnContext`**.

У тебя уже есть гораздо более надежная идея — `IAddOnServices`.

Передавай его туда, где он нужен:

```csharp
public class Library
{
    private readonly IAddOnServices _services;

    public Library(IAddOnServices services)
    {
        _services = services;
    }
}
```

или

```csharp
Library.Initialize(services);
```

где `services` хранится **внутри конкретной библиотеки**, а не в общей `drz.Abstractions`.

---

## Для твоего проекта я бы предложил еще лучшее решение

Ты уже создаешь:

```csharp
AddOnCompositionRoot
```

Добавь в него:

```csharp
public IAddOnServices Services { get; }
```

И при загрузке второго аддона будет:

```
AddOnA
    CompositionRootA
        ServicesA

AddOnB
    CompositionRootB
        ServicesB
```

Никаких конфликтов.

---

**Я бы вообще избегал любых статических полей в `drz.Abstractions`**, если есть вероятность одновременной загрузки нескольких аддонов. Иначе ты получишь очень трудноуловимые ошибки, когда второй загруженный аддон начнет "перехватывать" сервисы первого. Для поддержки нескольких независимых аддонов лучше, чтобы каждый экземпляр `IAddOnServices` жил внутри своего `CompositionRoot` и передавался явно тем библиотекам, которые относятся именно к этому аддону.

## Prompt:
22.07.2026, 11:02:58

Тогда лучший вариант передавать библиотеке сервис явно
В конструкторе библиотеки делать его internal static
Для удобства в using классов добавлять алиас

## Response:
22.07.2026, 11:02:59

Да, для твоей архитектуры это выглядит самым безопасным и масштабируемым решением.

Например, в библиотеке:

```csharp
public sealed class Library
{
    internal static IAddOnServices Services { get; private set; }

    public Library(IAddOnServices services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
```

При инициализации аддона:

```csharp
var services = compositionRoot.Get<IAddOnServices>();

new Library(services);
```

После этого любой класс библиотеки может получать сервисы:

```csharp
using S = MyLibrary.Library;

public class MyClass
{
    public void Execute()
    {
        var settings = S.Services.Get<ISettingsService>();
        var logger = S.Services.Get<IDrzLoggerFactory>();
    }
}
```

Или сделать в `Library` удобные свойства:

```csharp
public sealed class Library
{
    internal static IAddOnServices Services { get; private set; }

    public static ISettingsService Settings =>
        Services.Get<ISettingsService>();

    public static IDrzLoggerFactory LoggerFactory =>
        Services.Get<IDrzLoggerFactory>();

    public static IWindowMessageService Window =>
        Services.Get<IWindowMessageService>();

    public Library(IAddOnServices services)
    {
        Services = services;
    }
}
```

Тогда код станет еще проще:

```csharp
using S = MyLibrary.Library;

S.Window.ShowMessage("Готово");

S.LoggerFactory.GetLogger<MyClass>()
               .Info("Выполнено");
```

### Преимущества такого подхода

- ✔ каждый аддон имеет **свой экземпляр** `IAddOnServices`;
- ✔ нет глобального статического контекста в `drz.Abstractions`, который может быть перезаписан другим аддоном;
- ✔ библиотеки знают только об интерфейсах, а не о `SimpleInjector`;
- ✔ инициализация библиотеки выполняется один раз;
- ✔ код в классах остается компактным благодаря алиасу `using S = MyLibrary.Library;`.

Единственное, что я бы изменил, — сделал метод инициализации явным вместо использования конструктора:

```csharp
public static class Library
{
    internal static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        if (Services != null)
            throw new InvalidOperationException("Library уже инициализирована.");

        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
```

Тогда использование становится очевидным:

```csharp
Library.Initialize(compositionRoot.Get<IAddOnServices>());
```

По коду сразу видно, что это не создание объекта библиотеки, а ее однократная инициализация. Это лучше отражает назначение класса и исключает случайное повторное создание экземпляров.

## Prompt:
22.07.2026, 11:13:17

Если код class Library
Вынести в scr и подключить к библиотекам, где он используется?
Можно даже без namespace

## Response:
22.07.2026, 11:13:18

Да, это как раз очень хороший компромисс для твоей архитектуры.

Ты описываешь подход **Shared Source (общий исходный код)** - один `.cs` файл физически лежит в одном месте, а подключается в несколько проектов как `Compile Include`.

## Как это выглядит

### Общий файл `Library.cs`

Например:

```csharp
using System;
using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;

/// <summary>
/// Общий контекст библиотеки.
/// Подключается как Shared Source в проекты библиотек.
/// </summary>
public static class Library
{
    /// <summary>
    /// Сервисы текущего экземпляра аддона.
    /// У каждого аддона будет свой экземпляр, потому что файл компилируется
    /// внутрь конкретной библиотеки.
    /// </summary>
    internal static IAddOnServices Services { get; private set; }

    /// <summary>
    /// Инициализация библиотеки.
    /// Вызывается один раз из аддона.
    /// </summary>
    public static void Initialize(IAddOnServices services)
    {
        if (Services != null)
            throw new InvalidOperationException("Library уже инициализирована.");

        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    // Удобные свойства

    public static IDrzLoggerFactory LoggerFactory =>
        Services.Get<IDrzLoggerFactory>();

    public static ISettingsService Settings =>
        Services.Get<ISettingsService>();

    public static ICommandLineMessageService CommandLine =>
        Services.Get<ICommandLineMessageService>();

    public static IWindowMessageService Window =>
        Services.Get<IWindowMessageService>();

    public static IDocumentService Documents =>
        Services.Get<IDocumentService>();
}
```

---

## Подключение файла к проектам

В `.csproj` библиотеки:

<CodeBlock language="xml" content="&lt;ItemGroup&gt;
  &lt;Compile Include=&quot;..\Shared\Library.cs&quot; Link=&quot;Shared\Library.cs&quot; /&gt;
&lt;/ItemGroup&gt;"/>

Или через Visual Studio: **Add → Existing Item → Add As Link**.

---

# Почему это особенно хорошо у тебя

Ты боялся этого сценария:

> AddOnA загружает AddOnB через LoadFrom, и второй перезапишет глобальный контекст.

С Shared Source этого не происходит.

### Что реально получится

```text
AddOnA
 └── LibraryA.dll
      └── static Library (экземпляр A)

AddOnB
 └── LibraryB.dll
      └── static Library (экземпляр B)
```

Хотя код одинаковый, **тип `Library` компилируется внутрь каждой сборки отдельно**.

Поэтому:

```csharp
// AddOnA
Library.Initialize(servicesA);

// AddOnB
Library.Initialize(servicesB);
```

Это **два разных статических поля**.

Никакого конфликта между аддонами.

---

# Можно ли без namespace?

Технически - да.

Файл без namespace:

```csharp
public static class Library
{
}
```

Но я бы **не рекомендовал**.

### Почему

Если две библиотеки подключат один и тот же shared-файл, а затем одна библиотека будет ссылаться на другую, ты получишь конфликт типов:

```text
Library уже определен
```

### Лучше минимальный namespace

Например:

```csharp
namespace drz.Shared
{
    public static class Library
    {
    }
}
```

И в коде использовать алиас:

```csharp
using S = drz.Shared.Library;

S.Window.ShowMessage("Hello");
S.Settings.Save();
```

Это почти так же коротко, но безопасно.

---

# Я бы сделал именно так

## Финальная архитектура

### `drz.Abstractions`

Только контракты:

- `IAddOnServices`
- `ISettingsService`
- `IDocumentService`
- `IDrzLoggerFactory`
- и т.д.

### `drz.Infrastructure`

Только реализация:

- `AddOnCompositionRoot`
- `AddOnServices`
- `SimpleInjector`
- `NLogBootstrap`

### `Shared/Library.cs`

Подключается как **linked source** в библиотеки, которым нужен удобный доступ к сервисам.

### Инициализация в аддоне

```csharp
var services = compositionRoot.Get<IAddOnServices>();

drz.Shared.Library.Initialize(services);
```

### Использование

```csharp
using S = drz.Shared.Library;

public class MyCommand
{
    public void Execute()
    {
        S.Window.ShowMessage("Готово");

        S.LoggerFactory
            .GetLogger<MyCommand>()
            .Info("Выполнено");

        S.Documents.Save();
    }
}
```

Для твоего случая это, на мой взгляд, лучший баланс между:

- изоляцией аддонов,
- отсутствием глобальных конфликтов,
- работой только через интерфейсы,
- отсутствием зависимости библиотек от Infrastructure,
- и минимальным количеством кода в рабочих классах.

## Prompt:
22.07.2026, 11:17:16

Но если объект в контейнере изменится, то изменятся экземпляры во всех сборках? Синглетон ссылочный

## Response:
22.07.2026, 11:17:16

Да. Если все библиотеки используют **один и тот же `IAddOnServices`**, который обращается к **одному контейнеру**, то `Singleton` будет общим для всех.

Например:

```csharp
container.Register<ISettingsService, SettingsService>(Lifestyle.Singleton);
```

В библиотеке A:

```csharp
var settings = Library.Services.Get<ISettingsService>();
settings.Theme = "Dark";
```

В библиотеке B:

```csharp
var settings = Library.Services.Get<ISettingsService>();

Debug.Assert(ReferenceEquals(
    settings,
    Library.Services.Get<ISettingsService>()));
```

И даже между разными библиотеками:

```csharp
var a = LibraryA.Services.Get<ISettingsService>();
var b = LibraryB.Services.Get<ISettingsService>();

Debug.Assert(ReferenceEquals(a, b)); // true
```

при условии, что:

- `LibraryA.Services` и `LibraryB.Services` получили **один и тот же экземпляр** `IAddOnServices`;
- этот `IAddOnServices` работает с **одним и тем же** `Container`.

То есть цепочка выглядит так:

```text
AddOnCompositionRoot
        │
        ▼
    Container
        │
        ▼
 IAddOnServices
        │
   ┌────┴────┐
   ▼         ▼
LibraryA  LibraryB
   │         │
   └────┬────┘
        ▼
 ISettingsService (Singleton)
```

Все получают ссылку на **один объект** `SettingsService`.

---

### Если контейнеров два

Если загружены два независимых аддона:

```text
AddOnA
  ContainerA
     │
     └── SettingsServiceA

AddOnB
  ContainerB
     │
     └── SettingsServiceB
```

то даже при одинаковом коде библиотек экземпляры будут разными.

---

Именно поэтому предложенная тобой схема хороша: **каждый аддон имеет свой контейнер**, а все его библиотеки используют один экземпляр `IAddOnServices`. В результате `Singleton` действительно является ссылочным и общим **в пределах конкретного аддона**, но не "протекает" в другие аддоны.

## Prompt:
22.07.2026, 11:19:18

Студия ругается на
cref="ActivationException"
Интерфейс ничего не знает про стиплеинжектор

## Response:
22.07.2026, 11:19:20

Это правильно. `drz.Abstractions` не должен знать о `SimpleInjector`.

Поэтому в интерфейсе не стоит писать:

```csharp
/// <exception cref="ActivationException">
```

Есть несколько вариантов.

### Вариант 1. Не документировать исключение (я бы выбрал этот)

```csharp
/// <summary>
/// Возвращает сервис указанного типа.
/// </summary>
/// <typeparam name="TService">Тип сервиса.</typeparam>
/// <returns>Экземпляр зарегистрированного сервиса.</returns>
TService Get<TService>() where TService : class;
```

Интерфейс не обещает, какое исключение будет выброшено. Это зависит от реализации.

---

### Вариант 2. Документировать через `InvalidOperationException`

Реализация может перехватывать `ActivationException` и выбрасывать стандартное исключение:

```csharp
public TService Get<TService>() where TService : class
{
    try
    {
        return _container.GetInstance<TService>();
    }
    catch (ActivationException ex)
    {
        throw new InvalidOperationException(
            $"Сервис '{typeof(TService).FullName}' не зарегистрирован.",
            ex);
    }
}
```

Тогда в интерфейсе:

```csharp
/// <exception cref="InvalidOperationException">
/// Сервис не зарегистрирован.
/// </exception>
```

---

### Вариант 3. Собственное исключение

Можно определить в `drz.Abstractions`:

```csharp
public sealed class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(Type serviceType)
        : base($"Сервис '{serviceType.FullName}' не зарегистрирован.")
    {
    }
}
```

Тогда реализация преобразует исключение контейнера в свое.

Это полностью отвязывает API от DI-контейнера.

---

## Что я бы сделал

Поскольку `IAddOnServices` — это **тонкая обертка над контейнером**, я бы вообще **не документировал конкретное исключение**.

```csharp
/// <summary>
/// Возвращает зарегистрированный сервис указанного типа.
/// </summary>
/// <typeparam name="TService">
/// Тип требуемого сервиса.
/// </typeparam>
/// <returns>
/// Экземпляр сервиса.
/// </returns>
TService Get<TService>() where TService : class;
```

Это оставляет реализации свободу использовать `SimpleInjector` сегодня, а в будущем — любой другой контейнер без изменения контракта.

## Prompt:
22.07.2026, 11:39:58

Addoncontext? Вместо Library?

## Response:
22.07.2026, 11:39:58

Да, мне кажется, **`AddOnContext` — более удачное название**, чем `Library`.

`Library` не очень отражает назначение класса. По названию кажется, что это экземпляр библиотеки, хотя фактически это **контекст выполнения аддона для конкретной библиотеки**.

Я бы сделал так:

```csharp
public static class AddOnContext
{
    internal static IAddOnServices Services { get; private set; }

    public static void Initialize(IAddOnServices services)
    {
        if (Services != null)
            throw new InvalidOperationException("AddOnContext уже инициализирован.");

        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
```

Использование:

```csharp
using AC = MyLibrary.AddOnContext;

var settings = AC.Services.Get<ISettingsService>();
```

или, если добавить удобный метод:

```csharp
public static T Get<T>() where T : class
{
    return Services.Get<T>();
}
```

то получится:

```csharp
using AC = MyLibrary.AddOnContext;

var settings = AC.Get<ISettingsService>();
var logger = AC.Get<IDrzLoggerFactory>();
```

### Еще один вариант

Чтобы избежать путаницы с уже существующим `AddOnContext` в других сборках, можно назвать класс:

- `RuntimeContext`
- `ModuleContext`
- `ServiceContext`

Но если **в каждой библиотеке будет свой собственный `AddOnContext` (через Shared Source)**, то название `AddOnContext` вполне логично. Алиас:

```csharp
using AC = MyLibrary.AddOnContext;
```

делает использование коротким и понятным, а каждый аддон и каждая библиотека будут иметь свой независимый статический контекст без конфликтов.

## Prompt:
22.07.2026, 11:41:29

Переделать существующий на работу в сборках с сервисом

## Response:
22.07.2026, 11:41:30

Да, я бы именно так и сделал.

Поскольку у тебя уже есть `AddOnContext`, не стоит вводить новый класс. Лучше изменить его назначение: вместо хранения ссылок на отдельные объекты (`Info`, `Cad`, `Os` и т.п.) он будет хранить только `IAddOnServices` и предоставлять удобный доступ к сервисам.

Например:

```csharp
using System;
using drz.Abstractions.Infrastructure;

namespace MyLibrary
{
    /// <summary>
    /// Контекст текущего аддона.
    /// </summary>
    /// <remarks>
    /// Каждый аддон должен один раз вызвать <see cref="Initialize"/>.
    /// После этого любые классы библиотеки могут получать зарегистрированные
    /// сервисы через <see cref="Get{TService}"/>.
    ///
    /// Класс подключается в библиотеку как Shared Source, поэтому каждая
    /// библиотека имеет собственный экземпляр статических полей.
    /// </remarks>
    public static class AddOnContext
    {
        private static IAddOnServices _services;

        /// <summary>
        /// Инициализирует контекст библиотеки.
        /// </summary>
        /// <param name="services">
        /// Экземпляр сервисов текущего аддона.
        /// </param>
        public static void Initialize(IAddOnServices services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (_services != null)
                throw new InvalidOperationException("AddOnContext уже инициализирован.");

            _services = services;
        }

        /// <summary>
        /// Возвращает зарегистрированный сервис.
        /// </summary>
        /// <typeparam name="TService">Тип сервиса.</typeparam>
        public static TService Get<TService>() where TService : class
        {
            if (_services == null)
                throw new InvalidOperationException(
                    "AddOnContext не инициализирован.");

            return _services.Get<TService>();
        }
    }
}
```

### Инициализация

```csharp
AddOnContext.Initialize(
    compositionRoot.Get<IAddOnServices>());
```

### Использование

```csharp
using AC = MyLibrary.AddOnContext;

var logger = AC.Get<IDrzLoggerFactory>();
var settings = AC.Get<ISettingsService>();
var document = AC.Get<IDocumentService>();
```

## Я бы добавил еще один метод

Иногда удобно проверить, выполнена ли инициализация:

```csharp
public static bool IsInitialized => _services != null;
```

и при необходимости:

```csharp
public static void Reset()
{
    _services = null;
}
```

`Reset()` может пригодиться для тестов или если аддон поддерживает полную выгрузку и повторную загрузку.

---

Такой `AddOnContext` остается очень маленьким (около 30 строк), не зависит от `SimpleInjector`, работает только через `IAddOnServices` и не требует изменений при добавлении новых сервисов в контейнер. Для твоей архитектуры это выглядит наиболее удачным решением.

## Prompt:
22.07.2026, 11:43:47

Есть смысл добавить в него готовые свойства сервисов, логфабрика, и тд

## Response:
22.07.2026, 11:43:48

Да, есть. Я бы даже рекомендовал это сделать.

Но только для **часто используемых сервисов**.

Например:

```csharp
public static class AddOnContext
{
    private static IAddOnServices _services;

    public static void Initialize(IAddOnServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Получить произвольный сервис.
    /// </summary>
    public static T Get<T>() where T : class
        => _services.Get<T>();

    /// <summary>
    /// Фабрика логгеров.
    /// </summary>
    public static IDrzLoggerFactory LoggerFactory
        => Get<IDrzLoggerFactory>();

    /// <summary>
    /// Настройки аддона.
    /// </summary>
    public static ISettingsService Settings
        => Get<ISettingsService>();

    /// <summary>
    /// Сообщения в командную строку.
    /// </summary>
    public static ICommandLineMessageService CommandLine
        => Get<ICommandLineMessageService>();

    /// <summary>
    /// Оконные сообщения.
    /// </summary>
    public static IWindowMessageService Window
        => Get<IWindowMessageService>();

    /// <summary>
    /// Работа с документом CAD.
    /// </summary>
    public static IDocumentService Document
        => Get<IDocumentService>();
}
```

Использование:

```csharp
AddOnContext.Window.ShowMessage("Ошибка");

AddOnContext.CommandLine.WriteLine("Готово");

var logger = AddOnContext.LoggerFactory.GetLogger<MyClass>();

var settings = AddOnContext.Settings;
```

А для редко используемых сервисов:

```csharp
var updater = AddOnContext.Get<IUpdateService>();

var license = AddOnContext.Get<ILicenseService>();
```

## Это хороший компромисс

- **Часто используемые** сервисы доступны как свойства.
- **Редкие** — через `Get<T>()`.
- При добавлении нового сервиса не нужно менять `AddOnContext`, если он используется в одном-двух местах.

### Я бы не добавлял абсолютно все сервисы

Например:

- ✅ `LoggerFactory`
- ✅ `Settings`
- ✅ `Document`
- ✅ `Window`
- ✅ `CommandLine`

А вот специализированные сервисы (`IUpdateService`, `ILicenseService`, `IPluginCatalog` и т.п.) лучше получать через `Get<T>()`, чтобы `AddOnContext` не превратился в класс на сотни строк. Так он останется компактным и удобным.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)