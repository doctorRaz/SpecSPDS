> [!WARNING]
>    Сильно не доделано\
>    Писал бот
>    


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

---

Я бы также добавил раздел **"Приоритет конфигурации"**, где описать, в каком порядке библиотека объединяет:
1. встроенную конфигурацию;
2. `Shared\*.config`;
3. программные настройки (`NLogFactoryBuilder`);
4. файлы `log.level` и `diagnostic.mode`.

Такой раздел сразу снимает вопросы о том, какая настройка имеет приоритет.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)