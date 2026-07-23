
### Предполагаемая структура зависимостей

Chat GPT

не совсем верно но пойдет

```text
                           Abstractions
                                 │
          ┌──────────────┬───────┼──────────────┬────────┬──────┐
          │              │       │              │        │      │
          │              │       │              │        │      │
   LogBootstrap   Test.CadServices   Test.Infrastructure │  Test.LibB
    │     │                                              │                                              
   NLog   │                                         Test.LibA
          │
          └──────────────┐
                         ▼
                 Test.AddOnRuntime
                 │      │      │
                 │      │      └────────────── Test.Infrastructure
                 │      └───────────────────── Test.CadServices
                 └──────────────────────────── LogBootstrap

                              ▲
                              │
                        Test.Console
                  ┌────────┼────────┐
                  │        │        │
                  │        │        ▼
                  │        │    Updater
                  │        ▼
                  │   Test.AddOnRuntime
                  ▼
              Test.LibA──┐
                         |
				    Test.LibB
```

## Логические слои

Я бы организовал решение так.

```text
Solution
│
├── 00.Core
│   ├── Abstractions
│   ├── AddOnRuntime
│   └── LogBootstrap
│
├── 01.Infrastructure
│   ├── CadServices
│   ├── Infrastructure
│   └── Updater
│
├── 02.Extensions
│   ├── LibA
│   └── LibB
│
└── 99.Tests
    └── Console
```

Или более красиво:

```text
src
│
├── Core
│   ├── Abstractions
│   ├── Runtime
│   └── Logging
│
├── Infrastructure
│   ├── Cad
│   ├── Services
│   └── Update
│
├── Libraries
│   ├── LibA
│   └── LibB
│
└── Samples
    └── Console
```

---

## Что бросается в глаза

Архитектура уже достаточно чистая:

- **Abstractions** — фундамент.
- **LogBootstrap** зависит только от Abstractions ✔️
- **CadServices** зависит только от Abstractions ✔️
- **Infrastructure** зависит только от Abstractions ✔️
- **AddOnRuntime** объединяет инфраструктурные сервисы ✔️
- **Console** является только точкой входа ✔️

---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)