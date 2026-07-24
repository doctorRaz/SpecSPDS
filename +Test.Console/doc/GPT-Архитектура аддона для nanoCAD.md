# Архитектура аддона для nanoCAD

**User:** dRz  
**Created:** 7/22/2026 16:30:42  
**Updated:** 7/22/2026 16:30:54  
**Exported:** 7/23/2026 22:30:45  
**Link:** [https://chatgpt.com/c/6a60c5ee-37f0-83eb-bbbf-ca5b8cac6783](https://chatgpt.com/c/6a60c5ee-37f0-83eb-bbbf-ca5b8cac6783)  

## Prompt:
22.07.2026, 16:30:41

Напиши архитектуру аддона.

## Response:
22.07.2026, 16:30:41

С учетом того, что мы обсуждали ранее (nanoCAD, несколько продуктов, общий код, SimpleInjector, NLog, собственный загрузчик, обновлятор), я бы строил архитектуру аддона следующим образом.

```text
                     nanoCAD
                        │
              ICommand / Extension
                        │
                EntryPoint (Loader)
                        │
        ┌───────────────┴────────────────┐
        │                                │
  Проверка версии CAD              Поиск зависимостей
        │                                │
        └───────────────┬────────────────┘
                        │
              AddOnCompositionRoot
                        │
          Регистрация SimpleInjector
                        │
                  AddOnContext
                        │
     ┌───────────┬─────────────┬─────────────┐
     │           │             │             │
 Runtime     Infrastructure   Services     Modules
     │           │             │             │
     └───────────┴──────┬──────┴─────────────┘
                         │
                  Бизнес-логика
```

## Структура библиотек

```
Product
│
├── Product.Loader
│   ├── EntryPoint
│   ├── AssemblyResolver
│   ├── Проверка версии CAD
│   └── Запуск CompositionRoot
│
├── Product
│   ├── Команды nanoCAD
│   ├── UI
│   ├── Меню
│   └── Инициализация
│
├── Product.Core
│   ├── Бизнес логика
│   ├── Документы
│   ├── Команды
│   └── Сценарии работы
│
├── Product.Services
│   ├── Работа с файлами
│   ├── Настройки
│   ├── Обновления
│   ├── Документы
│   └── CAD сервисы
│
├── Product.Infrastructure
│   ├── SimpleInjector
│   ├── NLog
│   ├── Json
│   ├── IO
│   └── Внешние библиотеки
│
├── Product.Resources
│
└── Product.Abstractions
    ├── Интерфейсы
    ├── DTO
    ├── Enums
    └── Events
```

---

## Общие библиотеки (drz)

```
drz.Abstractions
    Интерфейсы

drz.Infrastructure
    Общие реализации

drz.Logging
    LogFactory
    IDrzLogger

drz.Update
    Проверка обновлений
    Установка

drz.Configuration
    ConfigReader

drz.Common
    Утилиты

drz.Cad
    Общие сервисы CAD

drz.Runtime
    RuntimeInfo
```

---

## Время жизни объектов

```
Singleton
    LogFactory
    UpdateService
    ConfigService
    EnvironmentInfo
    RuntimeInfo

Scoped (если появятся документы)
    DocumentContext

Transient
    Команды
    Обработчики
    Диалоги
```

---

## Поток запуска

```
nanoCAD
    ↓
EntryPoint
    ↓
AssemblyResolver
    ↓
Проверка версии
    ↓
Создание Container
    ↓
Регистрация сервисов
    ↓
Initialize()
    ↓
Регистрация команд
    ↓
Работа пользователя
```

---

## AddOnContext

`AddOnContext` должен быть единственной точкой доступа к инфраструктуре.

Например:

```
AddOnContext
 ├── Container
 ├── LoggerFactory
 ├── Logger
 ├── Runtime
 ├── Services
 ├── Configuration
 └── ProductInfo
```

При этом бизнес-код должен получать зависимости через конструктор, а `AddOnContext` использовать только в точках входа (команды nanoCAD, статические методы, интеграция с API). Это уменьшает связность и облегчает тестирование.

---

## Обновление

```
Product
    │
    ├── Проверка версии
    ├── Download
    ├── Verify
    ├── Backup
    ├── Extract
    └── Restart
```

Обновлятор должен быть полностью независимой библиотекой, чтобы его можно было использовать во всех продуктах.

---

## Принципы

- `Abstractions` не знают ни о каких реализациях.
- `Core` не знает о nanoCAD, NLog и SimpleInjector.
- `Infrastructure` реализует интерфейсы из `Abstractions`.
- Только `Loader` знает, как собрать приложение.
- Все внешние зависимости (NLog, Json, SevenZip и т.д.) изолированы в `Infrastructure`.
- Каждый продукт имеет собственный `CompositionRoot`.
- Все общие компоненты (логирование, обновления, конфигурация, Runtime, сервисы ОС) переиспользуются между продуктами. Такая модульная архитектура хорошо соответствует плагинной модели .NET и позволяет независимо развивать и обновлять отдельные компоненты. ([blog.nashtechglobal.com](https://blog.nashtechglobal.com/plugin-architecture-pattern-overview-net/?utm_source=chatgpt.com))

---

**Sources:**

- [Plugin Architecture Pattern Overview (.NET) - NashTech Blog](https://blog.nashtechglobal.com/plugin-architecture-pattern-overview-net/?utm_source=chatgpt.com)



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)