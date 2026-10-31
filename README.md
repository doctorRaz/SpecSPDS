# SpecSPDS

**SpecSPDS** — .NET-приложение и набор библиотек для работы со спецификациями в nanoCAD. Основной CAD-аддон использует универсальный маркер и данные объектов чертежа для формирования и обработки спецификаций.

Проект рассчитан на nanoCAD и использует API платформы nanoCAD и MultiCAD. Репозиторий также содержит общие библиотеки, инфраструктурные компоненты и тестовые приложения.

- [Описание и архитектура проекта на DeepWiki (EN)](https://deepwiki.com/doctorRaz/SpecSPDS)
- [Описание проекта на русском](https://deepwiki-com.translate.goog/doctorRaz/SpecSPDS?_x_tr_sl=en&_x_tr_tl=ru&_x_tr_hl=ru&_x_tr_pto=wapp&_x_tr_hist=true)
- [Архитектурные договорённости](Architecture.md)

## Состав решения

Основное решение — `SpecSPDS.sln`.

| Проект / каталог | Назначение |
|---|---|
| `NC` | CAD-аддон SpecSPDS для nanoCAD. Подключает прикладной код, общие библиотеки и адаптеры API хоста. |
| `Application` | Прикладная часть SpecSPDS, не привязанная непосредственно к API nanoCAD. |
| `Core` | Общий код, используемый проектами решения. |
| `CommonLib` | Git submodule с повторно используемыми библиотеками и инфраструктурой. |
| `Test.*` | Тестовые проекты и консольные приложения для проверки отдельных компонентов и сценариев. |

### CommonLib

`CommonLib` подключается как отдельный Git-репозиторий и содержит общие компоненты:

| Проект | Назначение |
|---|---|
| `Abstractions` | Общие интерфейсы, контракты и типы. |
| `AddOnRuntime` | Компоненты времени выполнения и композиции аддона. |
| `Common.NC` | Общие команды и вспомогательные сервисы для nanoCAD. |
| `Infrastructure` | Реализации общих инфраструктурных контрактов. |
| `Infrastructure.NC` | Реализации сервисов, использующие API nanoCAD. |
| `LogBootstrap` | Настройка логирования на базе NLog. |
| `MulticadInterop` | Обёртки над API MultiCAD с использованием reflection. |
| `Settings` | Компоненты и контракты настроек. |
| `Updater` | Проверка, загрузка и установка обновлений аддона. |

Подробности и правила использования компонентов описаны в [README репозитория CommonLib](https://github.com/doctorRaz/CommonLib).

## Требования

- Windows x64.
- Visual Studio 2022 с workload **.NET desktop development** либо .NET SDK.
- .NET 6 SDK для сборки проектов решения.
- nanoCAD с поддерживаемой версией платформы для запуска аддона.
- Доступ к NuGet-источникам, указанным в `NuGet.config`.

Проект `NC` использует пакеты `nanoCAD.Platform.NET` и `MultiCAD.NET`. Для работы аддона необходима соответствующая среда nanoCAD; одной сборки проекта недостаточно для запуска вне CAD-хоста.

## Получение исходного кода

Клонируйте репозиторий вместе с субмодулями:

```bash
git clone --recurse-submodules https://github.com/doctorRaz/SpecSPDS.git
cd SpecSPDS
```

Если репозиторий уже клонирован без субмодулей, выполните:

```bash
git submodule update --init --recursive
```

Субмодуль `CommonLib` необходим для загрузки проектов общих библиотек, на которые ссылается решение.

## Сборка

Восстановить зависимости, собрать решение в конфигурации Release и запустить тестовые проекты можно командами:

```powershell
dotnet restore .\SpecSPDS.sln
dotnet build .\SpecSPDS.sln --configuration Release --no-restore
dotnet test .\SpecSPDS.sln --configuration Release --no-build
```

Для сборки конкретного проекта можно передать его `.csproj` вместо файла решения.

### Общие настройки MSBuild

В корне решения находятся:

- `Directory.Build.props` — общие свойства проектов: версия языка C#, nullable reference types, общие каталоги вывода, имя сборки и метаданные.
- `Directory.Build.targets` — общие настройки версии сборок и исключение исходников из каталогов `_draft` и `_experimental`.

MSBuild автоматически ищет `Directory.Build.props` и `Directory.Build.targets` в каталоге проекта и родительских каталогах. Поэтому настройки корня решения применяются к вложенным проектам, в том числе к проектам субмодуля `CommonLib`.

В `NuGet.config` определены источники NuGet.org, nanoDev и локальный каталог `CommonLib/Nuget`. Последний становится доступен после инициализации субмодуля.

Дополнительные настройки nanoCAD и генерация префикса команд находятся в `Shared.NC.props`.

## CI и публикация

GitHub Actions автоматизирует проверку и выпуск проекта.

- **CI** (`.github/workflows/ci.yml`) выполняет `restore → build → test` для `SpecSPDS.sln`. Запускается при PR в `master`, при push в `master` и вручную через `workflow_dispatch`.
- **Создание release tag** (`.github/workflows/create-release-tag.yml`) выполняется после слияния PR с меткой `publish` в `master` или `develop`.
- **Release** (`.github/workflows/release.yml`) запускается по release/mandatory-тегу и выполняет проверку, публикацию и формирование архивов релиза.

Параметры публикации находятся в `.github/release-settings/release.config.json`. Описание конфигурации и процесса — в [документации release settings](.github/release-settings/README.md).

## Участие в разработке

1. Создайте fork репозитория.
2. Создайте рабочую ветку от `develop`.
3. Внесите изменения и проверьте сборку и относящиеся к ним тесты.
4. Откройте Pull Request в `develop`, если для конкретной задачи не согласована другая целевая ветка.
5. Для изменений, которые должны попасть в опубликованный релиз, используйте согласованный процесс публикации с меткой `publish`.

Архитектурные принципы и ограничения изменений зафиксированы в [Architecture.md](Architecture.md).

## Лицензия

Проект распространяется на условиях [MIT License](LICENSE).
