# Обновлятор или аддон

**User:** dRz  
**Created:** 7/19/2026 13:02:26  
**Updated:** 7/19/2026 15:02:53  
**Exported:** 7/19/2026 19:11:28  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5c9fb3-e7d8-83eb-b322-4c8c9b904a53](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5c9fb3-e7d8-83eb-b322-4c8c9b904a53)  

## Prompt:
19.07.2026, 13:02:26

Enum 
не проверять обновления
Проверять спрашивать 
Проверять устанавливать
Хранится в настройках аддона.
Эта информация передается в Updater
Но даже если стоит не проверять обновления, раз в 7 дней, обновлятор должен проверить обновление и если в json обновления стоит флаг обязательное, то обновить не уведомляя пользователя.
Вопрос, какой модуль должен следить за сроком 7 дней?
Аддон или обновлятор?

## Response:
19.07.2026, 13:02:26

В твоей архитектуре — **обновлятор**.

Причины:

1. **Это его ответственность.**
   Правило *"раз в 7 дней проверить, нет ли обязательного обновления"* относится к механизму обновления, а не к аддону. Аддон должен только сообщить свои предпочтения.

2. **Не дублируется логика.**
   Если обновлятор будет использоваться несколькими аддонами, не придется в каждом реализовывать хранение даты последней проверки.

3. **Обновлятор лучше знает, что такое "проверка".**
   Он может хранить:
   - дату последней обычной проверки;
   - дату последней принудительной проверки;
   - результат проверки;
   - версию, для которой уже показывал уведомление.

4. **Аддон остается максимально простым.**
   Он передает только режим работы.

Например:

```csharp
public enum UpdateMode
{
    Disabled,      // не проверять
    Ask,           // проверять и спрашивать
    Automatic      // проверять и устанавливать
}
```

А обновлятор внутри реализует примерно такую логику:

```text
if (Mode == Disabled)
{
    if (LastForceCheck < 7 дней назад)
        ПроверитьТолькоНаОбязательныеОбновления();
}
else
{
    ПроверитьВсеОбновления();
}
```

### Я бы даже разделил два режима проверки

```csharp
enum CheckKind
{
    Normal,
    MandatoryOnly
}
```

Тогда:

- **Normal** — скачивается весь `update.json`, анализируются все версии, учитывается `UpdateMode`.
- **MandatoryOnly** — выполняется раз в 7 дней при `Disabled`; если в `update.json` есть обязательное обновление новее текущего, оно устанавливается без участия пользователя.

### Где хранить дату?

Лучше тоже в настройках обновлятора, например:

```json
{
    "LastCheck": "2026-07-18T10:15:00Z",
    "LastMandatoryCheck": "2026-07-11T09:00:00Z"
}
```

Тогда аддон вообще не знает, когда была последняя проверка. Он знает только свое желание:

- Disabled
- Ask
- Automatic

А **обновлятор самостоятельно решает**, когда нужно проигнорировать `Disabled` ради поиска обязательного обновления. Это делает архитектуру чище и позволяет при необходимости изменить правило (например, с 7 дней на 3 или 14) без изменения кода всех аддонов.

## Prompt:
19.07.2026, 13:04:54

Т.е у обновлятора свой settings

## Response:
19.07.2026, 13:04:54

Да, я бы сделал именно так.

Разделил бы настройки по ответственности.

### Настройки аддона

То, что относится к пожеланиям пользователя:

```csharp
public class AddOnSettings
{
    public UpdateMode UpdateMode { get; set; }
}
```

### Настройки обновлятора

То, что является его внутренним состоянием:

```csharp
public class UpdaterSettings
{
    public DateTime? LastCheck { get; set; }

    public DateTime? LastMandatoryCheck { get; set; }

    public string? LastNotifiedVersion { get; set; }
}
```

Эти данные пользователь вообще не должен видеть и изменять.

### Преимущества

- Аддон ничего не знает о механизме проверки.
- Обновлятор сам решает, когда пора проверить сервер.
- В будущем можно добавить кэш, ETag, время следующей проверки, статистику ошибок и т.д., не меняя настройки аддона.

Единственное, что я бы сделал — **не отдельный файл настроек**, а файл состояния (`UpdaterState`), чтобы было понятно, что это не пользовательские настройки.

Например:

```text
UpdaterState.json
```

или

```text
.updater/state.json
```

Это уже не настройки, а служебные данные обновлятора. Такое разделение обычно получается более понятным и расширяемым.

## Prompt:
19.07.2026, 13:06:56

В обновлятор state писать последнюю установленную версию и результат обновления?

## Response:
19.07.2026, 13:06:56

Да, но я бы разделил **состояние** и **журнал**.

### В `UpdaterState`

Храни только то, что необходимо для работы алгоритма.

Например:

```csharp
public class UpdaterState
{
    public Version? InstalledVersion { get; set; }

    public DateTime? LastCheck { get; set; }

    public DateTime? LastMandatoryCheck { get; set; }

    public DateTime? LastSuccessfulUpdate { get; set; }

    public string? LastInstalledVersion { get; set; }
}
```

### А результат последнего обновления

Лучше хранить отдельно:

```csharp
public class UpdateResult
{
    public DateTime Date { get; set; }

    public Version? FromVersion { get; set; }

    public Version? ToVersion { get; set; }

    public UpdateStatus Status { get; set; }

    public string? Error { get; set; }
}
```

или вообще писать только в лог NLog.

### В твоем случае

Поскольку обновлятор запускается при старте аддона и не является отдельной службой, я бы не стал хранить `InstalledVersion`.

Почему:
- текущую версию всегда можно получить из загруженной сборки (`AssemblyInformationalVersion` или `AssemblyVersion`);
- хранить ее в `state` означает дублировать данные, которые могут рассинхронизироваться.

Поэтому я бы оставил в `UpdaterState` только действительно внутреннее состояние:

```csharp
public class UpdaterState
{
    public DateTime? LastCheck { get; set; }

    public DateTime? LastMandatoryCheck { get; set; }

    public DateTime? LastSuccessfulUpdate { get; set; }

    public string? LastInstalledVersion { get; set; }
}
```

`LastInstalledVersion` имеет смысл оставить только если тебе нужно определить, что **предыдущий запуск был уже после успешного обновления**, чтобы, например, вывести сообщение *«Продукт успешно обновлен до версии X.Y.Z»*. Если такого сценария нет, то и это поле можно не хранить — версия уже известна из самой сборки.

## Prompt:
19.07.2026, 13:10:26

Если загруженный файл был переименован и заменен, рефлекция выдаст старую версию
А fileonfo?

## Response:
19.07.2026, 13:10:26

Да, это важный момент в твоей схеме обновления.

Если сборка уже **загружена в AppDomain nanoCAD**, то:

```csharp
Assembly.GetExecutingAssembly().GetName().Version
```

или

```csharp
typeof(SomeClass).Assembly.GetName().Version
```

будет показывать **версию загруженной сборки**, даже если физический `.dll` на диске уже заменили.

То есть сценарий:

```
SpecSPDS.dll 1.0.0.0
        ↓
nanoCAD загрузил DLL
        ↓
Updater:
    SpecSPDS.dll → SpecSPDS.dll.bak
    новая SpecSPDS.dll 1.1.0.0
        ↓
старый Assembly в памяти остается 1.0.0.0
```

Рефлексия покажет:

```
1.0.0.0
```

---

А вот:

```csharp
FileVersionInfo.GetVersionInfo(path)
```

читает **файл с диска**, а не загруженную сборку.

Например:

```csharp
var info = FileVersionInfo.GetVersionInfo(
    @"C:\Addons\SpecSPDS\SpecSPDS.dll");

Console.WriteLine(info.FileVersion);
```

после замены покажет:

```
1.1.0.0
```

---

Поэтому в твоем Updater я бы разделил:

### Версия работающего аддона

```csharp
Assembly.GetExecutingAssembly()
```

Используется:
- определить текущую запущенную версию;
- отправить на сервер при проверке обновлений.

### Версия файла на диске

```csharp
FileVersionInfo.GetVersionInfo()
```

Используется:
- проверить, что обновление действительно установилось;
- определить версию после перезапуска nanoCAD;
- диагностика.

---

Для `UpdaterState` я бы добавил:

```csharp
public class UpdaterState
{
    public DateTime? LastCheck { get; set; }

    public DateTime? LastSuccessfulUpdate { get; set; }

    public string? PreviousVersion { get; set; }

    public string? InstalledVersion { get; set; }
}
```

Тогда после установки:

```
PreviousVersion = 1.0.0.0
InstalledVersion = 1.1.0.0
```

и при следующем запуске можно сравнить:

```
AssemblyVersion = 1.1.0.0
State.InstalledVersion = 1.1.0.0
```

и понять, что обновление действительно применилось.

## Prompt:
19.07.2026, 13:12:29

Алгоритм ломается если пользователь после успешного обновления еще раз запустит обновление

## Response:
19.07.2026, 13:12:29

Именно. Поэтому я бы вообще **не хранил версию в `UpdaterState`**.

В твоем случае `state` должен содержать только **служебное состояние процесса обновления**, а не информацию о продукте.

Например:

```csharp
public class UpdaterState
{
    public DateTime? LastCheck { get; set; }

    public DateTime? LastMandatoryCheck { get; set; }

    public bool PendingRestart { get; set; }

    public DateTime? LastUpdateAttempt { get; set; }
}
```

### Как определить успешное обновление?

Не по `state`.

После копирования файлов обновлятор уже знает, успешно ли завершились операции:

- архив скачан;
- распакован;
- файлы заменены;
- ошибки не было.

Если все прошло успешно — возвращает `UpdateResult.Success`.

После следующего запуска аддон просто работает уже с новой версией. Никаких сравнений со `state` делать не нужно.

### Если нужно показать один раз сообщение:

> "Программа успешно обновлена до версии 2.5.1"

Тогда можно использовать не хранение версии, а одноразовый флаг:

```csharp
public class UpdaterState
{
    public bool ShowUpdateSuccessMessage { get; set; }
}
```

или

```csharp
public Version? PendingSuccessNotificationVersion { get; set; }
```

При первом запуске после обновления сообщение показывается, затем поле очищается.

Такой алгоритм не ломается, если пользователь десять раз подряд запустит проверку обновлений — `state` не пытается хранить "истину" о текущей версии продукта, а содержит только временное состояние, необходимое самому обновлятору.

## Prompt:
19.07.2026, 13:14:27

Обновлятор будет получать версию с addoninfo и пытаться обновить
Возможно в обновляторе есть смысл вмегда получать версию как fileonfo?

## Response:
19.07.2026, 13:14:27

Я бы сделал **оба варианта**, но использовать их в разных ситуациях.

### До начала обновления

Использовать версию из `AddOnInfo`.

Это версия **запущенного продукта**. Именно ее нужно отправлять на сервер:

```text
У меня работает версия 2.4.0.
Есть ли обновление?
```

Даже если на диске уже лежит 2.5.0 (после предыдущей попытки обновления), процесс все еще работает с 2.4.0.

### После копирования файлов

Если нужно убедиться, что файлы заменились корректно, использовать:

```csharp
FileVersionInfo.GetVersionInfo(mainAssemblyPath)
```

или, что еще лучше,

```csharp
AssemblyName.GetAssemblyName(mainAssemblyPath).Version
```

`AssemblyName.GetAssemblyName()` читает метаданные сборки напрямую из файла и не зависит от того, загружена ли она в AppDomain. К тому же он возвращает именно `AssemblyVersion`, а не строковый `FileVersion`.

### Я бы оформил это так

```csharp
public interface IProductInfo
{
    Version RunningVersion { get; } // из AddOnInfo

    Version InstalledVersion { get; } // из файла
}
```

Тогда обновлятор всегда знает две версии:

- **RunningVersion** — версия, с которой сейчас работает процесс.
- **InstalledVersion** — версия, лежащая на диске.

Это избавляет от неоднозначностей и позволяет корректно работать даже после замены файлов без перезапуска.

В твоей архитектуре, где DLL заменяются "на горячую", такое разделение выглядит наиболее надежным.

## Prompt:
19.07.2026, 13:16:45

Возможно есть смысл в addoninfo добавить Version InstalledVersion как lazzy, чтобы он обновлялся при обращении?

## Response:
19.07.2026, 13:16:45

Да, это хорошая идея. Но я бы не называл это просто `InstalledVersion`, а четко разделил понятия.

Например:

```csharp
public interface IAddOnInfo
{
    Version RunningVersion { get; }      // Версия загруженной сборки
    Version InstalledVersion { get; }    // Версия файла на диске
}
```

Реализация `InstalledVersion` может быть ленивой:

```csharp
private Version? _installedVersion;

public Version InstalledVersion =>
    _installedVersion ??= ReadInstalledVersion();
```

Однако я бы добавил возможность сбросить кэш:

```csharp
public void RefreshInstalledVersion()
{
    _installedVersion = null;
}
```

или вообще не кэшировал бы.

### Почему?

Чтение версии из файла — очень дешевая операция. Зато после обновления файлов:

```text
RunningVersion   = 2.4.0
InstalledVersion = 2.5.0
```

Ты сразу увидишь разницу без вызова `Refresh()`.

Если же закэшировать `InstalledVersion`, то после обновления придется помнить, что кэш нужно сбросить. Это легко забыть.

### Поэтому я бы сделал так

- `RunningVersion` — вычисляется один раз (она не изменится до перезапуска процесса).
- `InstalledVersion` — **каждый раз читается из файла** или имеет метод `Refresh()`, если хочется кэшировать.

Для большинства приложений чтение версии из одной DLL занимает настолько мало времени, что отсутствие кэша делает код проще и надежнее. В твоем случае `InstalledVersion` — это фактически "текущее состояние файла на диске", поэтому логично, чтобы оно всегда было актуальным.

## Prompt:
19.07.2026, 13:38:47

Напиши ленивый метод без кэширования
Вызов только когда данные нужны.
Обновляются при вызове

## Response:
19.07.2026, 13:38:47

Если без кэширования, то `Lazy<T>` здесь не нужен. Достаточно сделать вычисляемое свойство, которое каждый раз читает версию из файла только при обращении.

Например:

```csharp
public Version InstalledVersion
{
    get
    {
        var version = AssemblyName.GetAssemblyName(AssemblyPath).Version;
        return version ?? new Version(0, 0);
    }
}
```

или, если уже есть путь к основной сборке:

```csharp
public Version InstalledVersion =>
    AssemblyName.GetAssemblyName(MainAssemblyPath).Version
    ?? new Version(0, 0);
```

Такое свойство обладает всеми нужными качествами:

- не читает файл, пока к нему не обратятся;
- при **каждом обращении** читает актуальную версию с диска;
- не требует `Refresh()`;
- не имеет риска устаревшего кэша.

Именно такой вариант я бы использовал для `InstalledVersion`. А вот `RunningVersion` можно вычислить один раз в конструкторе, поскольку она не изменится до завершения процесса.

## Prompt:
19.07.2026, 15:01:07

Через fileonfo?

## Response:
19.07.2026, 15:01:07

Да, можно через `FileVersionInfo`, если тебе нужна именно **FileVersion** из Win32-ресурсов.

```csharp
public Version InstalledVersion
{
    get
    {
        var info = FileVersionInfo.GetVersionInfo(MainAssemblyPath);

        return Version.TryParse(info.FileVersion, out var version)
            ? version
            : new Version(0, 0);
    }
}
```

Или более компактно:

```csharp
public Version InstalledVersion =>
    Version.TryParse(
        FileVersionInfo.GetVersionInfo(MainAssemblyPath).FileVersion,
        out var version)
            ? version
            : new Version(0, 0);
```

### Но я бы рекомендовал `AssemblyName.GetAssemblyName()`

Причина в том, что ты сравниваешь версии сборок для обновления. `AssemblyName.GetAssemblyName(path).Version` читает именно **AssemblyVersion**, которая обычно используется для логики обновления.

`FileVersionInfo` читает **FileVersion**, а она может отличаться от `AssemblyVersion` (например, если используешь `AssemblyFileVersion` отдельно).

Если в твоем проекте `AssemblyVersion` и `FileVersion` всегда синхронизированы, то `FileVersionInfo` тоже вполне подойдет. Если же есть вероятность, что они будут различаться, для обновлятора надежнее ориентироваться на `AssemblyVersion`.

## Prompt:
19.07.2026, 15:02:46

AssemblyVersion вернет версию загруженной сборки, а не файла

## Response:
19.07.2026, 15:02:47

Нет, если использовать именно:

```csharp
AssemblyName.GetAssemblyName(path)
```

то будет прочитана **сборка по указанному пути на диске**, а не уже загруженная в память.

Например:

```csharp
var version = AssemblyName
    .GetAssemblyName(@"C:\Addons\SpecSPDS\SpecSPDS.dll")
    .Version;
```

Этот метод:
- не загружает сборку в текущий `AppDomain`;
- не использует уже загруженный `Assembly`;
- читает метаданные PE-файла напрямую.

Именно поэтому его часто используют для просмотра версии DLL без ее загрузки.

А вот это:

```csharp
typeof(SomeClass).Assembly.GetName().Version
```

или

```csharp
Assembly.GetExecutingAssembly().GetName().Version
```

действительно возвращает версию **загруженной** сборки.

### Что выбрать?

Для обновлятора я бы использовал:

- **Текущая работающая версия** — из `AddOnInfo.RunningVersion` (загруженная сборка).
- **Версия файла на диске** — `AssemblyName.GetAssemblyName(path).Version`.

Это надежнее, чем `FileVersionInfo`, поскольку ты работаешь именно с версией сборки, а не с Win32-ресурсом файла.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)