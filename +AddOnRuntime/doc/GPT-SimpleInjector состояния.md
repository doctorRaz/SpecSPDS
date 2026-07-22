# SimpleInjector состояния

**User:** dRz  
**Created:** 7/22/2026 6:08:01  
**Updated:** 7/22/2026 6:08:13  
**Exported:** 7/22/2026 6:56:25  
**Link:** [https://chatgpt.com/c/6a6033ee-f65c-83ed-b830-2c0d4bfa6813](https://chatgpt.com/c/6a6033ee-f65c-83ed-b830-2c0d4bfa6813)  

## Prompt:
22.07.2026, 06:08:00

SimpleInjector Singleton
Какие еще есть состояния.
Опиши подробно

## Response:
22.07.2026, 06:08:01

В SimpleInjector время жизни объекта (Lifestyle) определяет, **как долго контейнер хранит экземпляр** и когда создается новый.

Основные варианты:

| Lifestyle | Создание экземпляра | Когда использовать |
|-----------|----------------------|--------------------|
| `Singleton` | Один экземпляр на весь контейнер | Конфигурация, логирование, кэш, фабрики |
| `Transient` | Новый экземпляр при каждом `GetInstance` | Большинство сервисов без состояния |
| `Scoped` | Один экземпляр в пределах области (Scope) | Web-запросы, транзакции, единица работы |

---

## Singleton

```csharp
container.Register<ISettingsService, SettingsService>(Lifestyle.Singleton);
```

или

```csharp
container.RegisterSingleton<ISettingsService, SettingsService>();
```

Особенности:

- создается один раз;
- все получают один и тот же объект;
- потокобезопасное создание;
- живет до уничтожения контейнера.

Например:

```csharp
var s1 = container.GetInstance<ISettingsService>();
var s2 = container.GetInstance<ISettingsService>();

ReferenceEquals(s1, s2); // true
```

Подходит для:

- `ILoggerFactory`
- `ISettingsService`
- `IAddOnContext`
- `IServiceProvider`
- кэшей
- фабрик

---

## Transient

```csharp
container.Register<IMessageService, MessageService>(Lifestyle.Transient);
```

или

```csharp
container.Register<IMessageService, MessageService>();
```

`Transient` используется по умолчанию.

Каждый вызов:

```csharp
var s1 = container.GetInstance<IMessageService>();
var s2 = container.GetInstance<IMessageService>();
```

создает новый объект.

```
ReferenceEquals(s1, s2); // false
```

Используется для:

- сервисов без внутреннего состояния;
- обработчиков;
- команд;
- объектов, живущих недолго.

---

## Scoped

```csharp
container.Register<IRepository, Repository>(Lifestyle.Scoped);
```

Внутри одного Scope:

```csharp
using (AsyncScopedLifestyle.BeginScope(container))
{
    var a = container.GetInstance<IRepository>();
    var b = container.GetInstance<IRepository>();

    // один объект
}
```

После окончания Scope объект уничтожается.

Обычно используется в:

- ASP.NET;
- запросах БД;
- Unit of Work;
- транзакциях.

Для обычного CAD-плагина (`nanoCAD`, `AutoCAD`) `Scoped` обычно не нужен, если ты сам не создаешь области жизни.

---

# Instance

Можно зарегистрировать уже существующий объект.

```csharp
var settings = new SettingsService();

container.RegisterInstance<ISettingsService>(settings);
```

или

```csharp
container.RegisterSingleton(settings);
```

Контейнер не создает объект.

Полезно, когда экземпляр уже инициализирован.

Например:

```csharp
var addonContext = new AddOnContext(...);

container.RegisterInstance<IAddOnContext>(addonContext);
```

---

# Делегат (Factory)

Можно описать собственную логику создания.

```csharp
container.Register<IMessageService>(() =>
{
    return new MessageService(DateTime.Now);
});
```

или

```csharp
container.Register<IMessageService>(
    () => new MessageService(...),
    Lifestyle.Singleton);
```

---

# Коллекции

Можно зарегистрировать несколько реализаций.

```csharp
container.Collection.Register<ICommand>(
    typeof(OpenCommand),
    typeof(CloseCommand),
    typeof(PrintCommand));
```

Затем

```csharp
IEnumerable<ICommand>
```

будет содержать все реализации.

---

# Open Generic

Очень полезно.

```csharp
container.Register(typeof(IRepository<>),
                   typeof(Repository<>));
```

Тогда

```csharp
IRepository<User>
```

автоматически станет

```csharp
Repository<User>
```

---

# Когда что использовать

Для твоей архитектуры аддона я бы рекомендовал:

| Интерфейс | Lifestyle | Причина |
|-----------|-----------|---------|
| `IAddOnContext` | Singleton | Один контекст на аддон |
| `ISettingsService` | Singleton | Общие настройки |
| `ILogService` | Singleton | Один сервис логирования |
| `IMessageService` | Singleton | Если хранит только логику отображения сообщений и не имеет изменяемого состояния |
| `IAssemblyService` | Singleton | Работа с отражением и загрузкой сборок |
| `IEnvironmentService` | Singleton | Информация об окружении не меняется |
| `IUpdateService` | Singleton | Управляет процессом обновления |
| `IDocumentService` | Transient | Если оборачивает текущий документ и не хранит состояние; если должен работать с активным документом через API, выбор зависит от реализации |

Для инфраструктурных сервисов в твоем проекте (логирование, настройки, контекст, информация о CAD, фабрики сервисов) наиболее естественным выбором обычно является `Singleton`, а `Transient` стоит использовать для легковесных объектов без собственного состояния или когда требуется новый экземпляр при каждом запросе.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)