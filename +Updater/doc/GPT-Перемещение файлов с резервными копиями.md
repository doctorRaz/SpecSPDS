# Перемещение файлов с резервными копиями

**User:** dRz  
**Created:** 7/17/2026 10:52:56  
**Updated:** 7/18/2026 10:50:43  
**Exported:** 7/19/2026 19:11:52  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a59df05-dca8-83ed-a03e-4b8a4207444c](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a59df05-dca8-83ed-a03e-4b8a4207444c)  

## Prompt:
17.07.2026, 10:52:54

переместит файлы в другой каталог, если существуют одноименные, то переименовать на *.bak или *(1).bak.... *(10).bak
по аналогии как тотал командер копирует

## Response:
17.07.2026, 10:52:56

Для поведения как у Total Commander лучше сделать отдельный метод генерации имени и использовать `File.Move()`.

Логика:
- если файла нет → обычное перемещение;
- если есть → пробуем:
  - `file.bak`
  - `file(1).bak`
  - `file(2).bak`
  - ...
  - `file(10).bak`
- после освобождения имени перемещаем.

Пример:

```csharp
using System;
using System.IO;

public static class FileMover
{
    public static void MoveWithBackup(string sourceFile, string targetDirectory)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException("Файл не найден", sourceFile);

        Directory.CreateDirectory(targetDirectory);

        string targetFile = Path.Combine(
            targetDirectory,
            Path.GetFileName(sourceFile));

        if (File.Exists(targetFile))
        {
            string backupFile = GetBackupName(targetFile);
            File.Move(targetFile, backupFile);
        }

        File.Move(sourceFile, targetFile);
    }

private static string GetBackupName(string file)
    {
        string directory = Path.GetDirectoryName(file)!;
        string name = Path.GetFileNameWithoutExtension(file);
        string ext = Path.GetExtension(file);

        // file.bak
        string backup = Path.Combine(directory, name + ".bak");

        if (!File.Exists(backup))
            return backup;

        // file(1).bak ... file(10).bak
        for (int i = 1; i <= 10; i++)
        {
            backup = Path.Combine(
                directory,
                $"{name}({i}).bak");

            if (!File.Exists(backup))
                return backup;
        }

        // если все занято - добавляем время
        return Path.Combine(
            directory,
            $"{name}({DateTime.Now:yyyyMMddHHmmss}).bak");
    }
}
```

Для каталога целиком:

```csharp
public static void MoveDirectoryFilesWithBackup(
    string sourceDirectory,
    string targetDirectory)
{
    foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
    {
        string relative = Path.GetRelativePath(sourceDirectory, file);
        string target = Path.Combine(targetDirectory, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        MoveWithBackup(file, Path.GetDirectoryName(target)!);
    }
}
```

Получится примерно как Total Commander:

```
test.dll
test.bak
test(1).bak
test(2).bak
...
test(10).bak
```

Для твоего updater-а под nanoCAD я бы ещё добавил:
- `.bak` делать только для `*.dll`, `*.exe`, `*.config` (ресурсы можно сразу заменять);
- при успешной распаковке старые `.bak` оставлять до следующего запуска;
- после старта nanoCAD чистить `.bak` и пустые каталоги. Это хорошо ложится на твою текущую схему обновления.

## Prompt:
17.07.2026, 11:13:51

какая  библиотека архиватора так умеет распаковывать сразу с переименованием существующих файлов?

## Response:
17.07.2026, 11:13:53

Из популярных .NET библиотек **почти никто не умеет именно такое поведение "как Total Commander" при распаковке**:

> если файл существует → автоматически переименовать старый в `.bak`, `(1).bak` и т.д. → распаковать новый.

Архиваторы обычно имеют только стратегии:
- overwrite (заменить);
- skip (пропустить);
- throw (ошибка);
- extract to another directory.

### Обзор

| Библиотека | Rename existing при Extract | Комментарий |
|---|---|---|
| **7-Zip SDK** | ❌ нет | Только callback с обработкой конфликтов |
| **SevenZipSharp** | ❌ нет | Обертка над 7z, можно перехватить события |
| **SharpCompress** | ❌ нет | Есть `Overwrite` режимы, но не rename |
| **Ionic.Zip** | ❌ нет | Старый проект, только overwrite/skip |
| **System.IO.Compression** | ❌ нет | ZIP только, минимальная логика |
| **DotNetZip** | ❌ нет | Аналогично Ionic |
| **K4os.Compression.LZ4** | ❌ | Не архиватор общего назначения |

---

### Как это обычно делают установщики

Даже большие установщики (MSI, NSIS, Inno Setup, WiX) не поручают это архиватору.

Схема:

```
archive.7z
     |
     v
Updater
     |
     +-- проверить список файлов
     |
     +-- существующие:
     |       xxx.dll
     |       ↓
     |       xxx.dll.bak
     |
     +-- Extract
             xxx.dll
```

То есть логика находится **в updater**, а не в распаковщике.

---

Для твоего случая с nanoCAD updater я бы сделал так:

1. Открыть архив без распаковки.
2. Получить список файлов.
3. Для каждого файла:

```text
addon.dll
addon.config
resources\menu.xml
```

проверить:

```
target\addon.dll существует?
```

если да:

```
addon.dll
   ↓
addon.dll.bak
   ↓
addon.dll(1).bak
```

4. После этого вызвать обычную распаковку с `Overwrite=true`.

---

Если использовать **7-Zip SDK**, это можно сделать аккуратно:

```csharp
archive.GetFiles()
       .Where(f => File.Exists(target))
       .ForEach(RenameToBackup);

archive.ExtractAll(target, overwrite:true);
```

Преимущество 7z:
- умеет AES-256;
- умеет шифровать заголовки;
- быстрый;
- хорошо подходит для твоей схемы с паролем продукта.

---

Для твоего updater-а я бы выбрал:

**7-Zip SDK + собственная логика backup-rename**.

Не стоит искать библиотеку, которая делает это внутри — потеряешь контроль над обновлением. У тебя обновляются DLL nanoCAD, поэтому лучше явно видеть каждый шаг и писать лог.

## Prompt:
17.07.2026, 11:17:19

GetRelativePath  нет такого

## Response:
17.07.2026, 11:17:21

`Path.GetRelativePath()` появился только в **.NET Core 2.0 / .NET Standard 2.0**. В **.NET Framework 4.x** (например, net462/net48, что у тебя используется для nanoCAD) его действительно нет.

Для net Framework можно сделать свой аналог:

```csharp
public static string GetRelativePath(string basePath, string path)
{
    var baseUri = new Uri(
        AppendDirectorySeparator(basePath));

    var pathUri = new Uri(path);

    return Uri.UnescapeDataString(
        baseUri.MakeRelativeUri(pathUri).ToString()
    ).Replace('/', Path.DirectorySeparatorChar);
}

private static string AppendDirectorySeparator(string path)
{
    if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
        return path + Path.DirectorySeparatorChar;

    return path;
}
```

Использование:

```csharp
string relative = GetRelativePath(sourceDirectory, file);

string target = Path.Combine(
    targetDirectory,
    relative);
```

Например:

```text
source:
D:\Addon\Package

file:
D:\Addon\Package\Lib\a.dll

relative:
Lib\a.dll

target:
D:\Addon\Install\Lib\a.dll
```

---

Для твоего updater-а я бы ещё сделал расширение:

```csharp
public static class PathEx
{
    public static string RelativeTo(this string path, string basePath)
    {
        return GetRelativePath(basePath, path);
    }
}
```

тогда код будет ближе к .NET 6:

```csharp
string relative = file.RelativeTo(sourceDirectory);
```

Такой вариант нормально работает в `net462`, `net48` и совместим с nanoCAD.

## Prompt:
17.07.2026, 11:56:41

после перемещения файлов из sourceDirectory надо ее удалить вместе с подкаталогами

## Response:
17.07.2026, 11:56:42

Да, если все файлы были **перемещены**, то исходный каталог должен стать пустым, и его можно удалить рекурсивно одной командой:

```csharp
MoveDirectoryFilesWithBackup(sourceDirectory, targetDirectory);

if (Directory.Exists(sourceDirectory))
{
    Directory.Delete(sourceDirectory, recursive: true);
}
```

Если есть вероятность, что какие-то файлы не удалось переместить (например, из-за блокировки), то `Directory.Delete(..., true)` выбросит исключение. Поэтому лучше удалять каталог только после успешного завершения всех перемещений.

Если же ты уже вызываешь свой метод удаления пустых каталогов:

```csharp
DeleteEmptyDirectories(sourceDirectory);
```

то в конце можно добавить:

```csharp
if (Directory.Exists(sourceDirectory) &&
    !Directory.EnumerateFileSystemEntries(sourceDirectory).Any())
{
    Directory.Delete(sourceDirectory);
}
```

Но если перемещение прошло без ошибок, то достаточно просто:

```csharp
Directory.Delete(sourceDirectory, true);
```

Это удалит корневой каталог и все оставшиеся пустые подкаталоги.

## Prompt:
17.07.2026, 12:02:39

даже если были ошибки, источник можно удалять

## Response:
17.07.2026, 12:02:40

Да. Если тебе не важно сохранить исходный каталог после частичного перемещения, то можно просто попытаться удалить его рекурсивно.

```csharp
try
{
    Directory.Delete(sourceDirectory, recursive: true);
}
catch
{
    // Игнорируем: могли остаться заблокированные файлы или не хватить прав.
}
```

Или ещё короче:

```csharp
if (Directory.Exists(sourceDirectory))
{
    try { Directory.Delete(sourceDirectory, true); }
    catch { }
}
```

Однако стоит учитывать поведение:
- **Все успешно перемещённые файлы** уже находятся в `targetDirectory`.
- **Неуспешно перемещённые** останутся в `sourceDirectory`.
- `Directory.Delete(sourceDirectory, true)` удалит **всё, что сможет**, но если встретит хотя бы один неудаляемый файл (например, заблокированный), то выбросит исключение, и часть каталогов может остаться.

Если цель — максимально очистить источник независимо от ошибок, то лучше пройтись по дереву и удалять файлы/папки по одному, игнорируя ошибки. Такой метод удалит всё, что возможно, даже если отдельные файлы заблокированы. Для updater-а это обычно более надежный вариант, чем один вызов `Directory.Delete(..., true)`.

## Prompt:
17.07.2026, 12:04:17

если неудачное обновление, какие есть варианты откатиться?

## Response:
17.07.2026, 12:04:19

Для обновления плагина у тебя есть несколько вариантов. Они отличаются сложностью и надежностью.

## Вариант 1. `.bak` + откат (рекомендую)

Ты уже почти к нему пришел.

Алгоритм:

1. Перед заменой:
   ```
   Addon.dll
       ↓
   Addon.dll.bak
   ```
2. Распаковать новую версию.
3. Проверить успешность обновления.
4. Если всё хорошо — при следующем запуске удалить `.bak`.
5. Если плохо — восстановить:
   ```
   удалить Addon.dll
   Addon.dll.bak → Addon.dll
   ```

Плюсы:
- быстро;
- минимум места;
- просто реализовать.

Минус:
- хранится только предыдущая версия.

---

## Вариант 2. Временный каталог

```
Addon\
    ...

Update\
    новая версия
```

Порядок:

1. Распаковать архив полностью в `Update`.
2. Проверить наличие всех файлов.
3. Начать замену файлов.

Если что-то не получилось — исходная версия еще не тронута.

Минус — всё равно во время замены нужен `.bak`.

---

## Вариант 3. Полный Backup каталога

Перед обновлением:

```
Addon\
Backup\
```

Скопировать весь каталог.

Если обновление не удалось:

```
удалить Addon
Backup → Addon
```

Плюсы:
- откатывается абсолютно всё.

Минусы:
- долго;
- требует много места.

---

## Вариант 4. Транзакция (самый надежный)

Сохранять журнал операций:

```
rename A.dll -> A.dll.bak
copy A.dll

rename B.dll -> B.dll.bak
copy B.dll

...
```

Если произошла ошибка:

идти **в обратном порядке**:

```
delete B.dll
B.dll.bak -> B.dll

delete A.dll
A.dll.bak -> A.dll
```

Это принцип работы большинства установщиков.

---

## Вариант 5. Две версии

```
Addon_v1
Addon_v2
```

Обновляется не рабочая версия.

Потом меняется только указатель.

Используют браузеры, IDE, игры.

Для nanoCAD это, скорее всего, лишнее.

---

# Что лучше для твоего updater

Я бы сделал так:

```
1. Скачать архив.

2. Распаковать во временный каталог.

3. Проверить архив
   (версия, обязательные файлы).

4. Для каждого заменяемого файла
       file.dll
          ↓
       file.dll.bak

5. Переместить новые файлы.

6. Если любой шаг завершился ошибкой
       откатить уже замененные файлы.

7. При следующем запуске,
   если всё успешно,
       удалить *.bak.
```

Такой алгоритм дает почти транзакционное обновление и не требует полного резервного копирования каталога.

---

### Я бы добавил еще один механизм

Перед началом обновления создать файл-маркер, например:

```
update.pending
```

После успешного завершения удалить его.

Если при следующем запуске `update.pending` существует, значит предыдущее обновление завершилось аварийно (например, отключили питание или процесс завершился с ошибкой). Тогда можно автоматически запустить процедуру восстановления из `.bak`, а уже после успешного отката удалить `update.pending`.

Такой маркер делает процесс устойчивым даже к сбою компьютера или принудительному завершению процесса во время обновления.

## Prompt:
17.07.2026, 12:21:48

```
   foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
   {
       string relative = file.RelativeTo(sourceDirectory);

       string target = Path.Combine(targetDirectory, relative);

       Directory.CreateDirectory(Path.GetDirectoryName(target)!);

       MoveWithBackup(file, Path.GetDirectoryName(target)!);
   }неплохо добавить проверку существует ли дирректория
```

## Response:
17.07.2026, 12:21:49

Да, стоит. Иначе `Directory.EnumerateFiles()` сразу выбросит `DirectoryNotFoundException`.

Я бы сделал проверку в самом начале метода:

```csharp
if (!Directory.Exists(sourceDirectory))
    return;

Directory.CreateDirectory(targetDirectory);

foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
{
    string relative = file.RelativeTo(sourceDirectory);

    string target = Path.Combine(targetDirectory, relative);

    string targetDir = Path.GetDirectoryName(target)!;
    Directory.CreateDirectory(targetDir);

    MoveWithBackup(file, targetDir);
}

try
{
    Directory.Delete(sourceDirectory, true);
}
catch
{
}
```

Я бы также добавил проверку и на `targetDirectory`:

```csharp
if (string.IsNullOrWhiteSpace(sourceDirectory) ||
    !Directory.Exists(sourceDirectory))
    return;

if (string.IsNullOrWhiteSpace(targetDirectory))
    throw new ArgumentNullException(nameof(targetDirectory));

Directory.CreateDirectory(targetDirectory);
```

Так метод будет корректно обрабатывать ситуацию, когда каталог с распакованным обновлением отсутствует (например, архив оказался пустым или распаковка завершилась неудачно).

## Prompt:
17.07.2026, 12:29:59

// file(1).bak ... file(10).bak
for (int i = 1; i <= 10; i++)
{
backup = Path.Combine(
directory,
$"{name}({i}).bak");

```
 if (!File.Exists(backup))
     return backup;
```

}

// если все занято - добавляем время
return Path.Combine(
directory,
$"{name}({DateTime.Now\:yyyyMMddHHmmss}).bak");

я бы переделал на бесконечный счетчик

## Response:
17.07.2026, 12:30:01

Согласен. Бесконечный счетчик проще, предсказуемее и не имеет искусственного ограничения. До десятков или сотен `.bak` дело обычно не доходит, а если дойдет — код все равно будет работать.

Я бы написал так:

```csharp
private static string GetBackupName(string file)
{
    string directory = Path.GetDirectoryName(file)!;
    string name = Path.GetFileName(file);

    string backup = Path.Combine(directory, $"{name}.bak");
    if (!File.Exists(backup))
        return backup;

    for (int i = 1; ; i++)
    {
        backup = Path.Combine(directory, $"{name}({i}).bak");

        if (!File.Exists(backup))
            return backup;
    }
}
```

Обрати внимание: здесь я использую

```csharp
string name = Path.GetFileName(file);
```

а не `GetFileNameWithoutExtension()`. Тогда получишь привычные имена:

```
SpecSPDS.dll
SpecSPDS.dll.bak
SpecSPDS.dll(1).bak
SpecSPDS.dll(2).bak
```

а не

```
SpecSPDS.bak
SpecSPDS(1).bak
```

Для DLL, EXE и других файлов первый вариант обычно удобнее — сразу видно исходное расширение файла.

## Prompt:
18.07.2026, 09:15:49

Для отката возможно проще и логичнее сделать полный архив аддона, если все ок, после загрузки удалить
Если сбой распаковать и восстановить, либо написать юзеру, что сбой, чтобы он распаковал руками.
Теоретически возможна ситуация что аддон не загрузится или загрузится не полностью

## Response:
18.07.2026, 09:15:49

Да, это вполне жизнеспособная схема, и для твоего случая она даже имеет преимущества перед `.bak`.

Разберем варианты.

### Вариант с `.bak`

Плюсы:
- быстро;
- мало места.

Минусы:
- нужно вести журнал операций или точно знать, какие файлы были переименованы;
- если процесс прервется между переименованием и копированием, каталог может оказаться в смешанном состоянии;
- если новая версия изменила структуру каталогов, откат становится сложнее.

---

### Вариант с полным архивом

Алгоритм:

1. Создать архив текущего аддона:
   ```
   backup.7z
   ```
2. Скачать обновление.
3. Распаковать обновление.
4. При следующем успешном запуске удалить `backup.7z`.
5. Если запуск не удался — восстановить из архива.

Плюсы:
- откатывается **всё**: DLL, XML, конфиги, ресурсы, меню, изображения и т.д.;
- не нужен журнал операций;
- не нужно создавать десятки `.bak`;
- восстановление очень простое.

Минусы:
- нужно место под архив;
- создание архива занимает немного времени.

---

### Для nanoCAD есть еще одна особенность

Ты не можешь гарантировать, что обновленный аддон вообще загрузится.

Например:
- отсутствует DLL;
- несовместимая версия;
- ошибка в статическом конструкторе;
- ошибка DI;
- исключение при инициализации;
- зависимость не загрузилась.

В этом случае код аддона вообще не выполнится, а значит автоматически восстановиться он уже не сможет.

---

### Поэтому я бы сделал отдельный Updater

Небольшая библиотека или EXE, которая умеет:

- создавать архив;
- восстанавливать архив;
- распаковывать обновление.

Тогда даже если основной аддон перестал запускаться, восстановление все равно возможно.

---

### Если отдельного процесса не будет

Тогда я бы оставил рядом:

```
Addon\
backup.7z
restore.cmd
```

или

```
backup.7z
```

и при следующем запуске, если обнаружено, что обновление не завершилось:

```
Не удалось завершить обновление.

Для восстановления распакуйте backup.7z
поверх каталога аддона.
```

---

### Что я бы выбрал

Для твоего проекта я бы использовал **полный архив каталога аддона**.

Причины:

- архив уже умеешь создавать и распаковывать;
- логика проще;
- откат всегда полный;
- не нужно думать, какие файлы переименовывались;
- если пользователь напишет о проблеме, достаточно сказать: *«Распакуй `backup.7z` в каталог аддона и перезапусти nanoCAD»*.

С учетом размера твоего аддона (несколько DLL и ресурсов) архив будет небольшим, а надежность такой схемы выше, чем у набора отдельных `.bak`. При этом имеет смысл хранить архив только до первого успешного запуска новой версии, после чего автоматически удалять его.

## Prompt:
18.07.2026, 09:19:18

При каждом обновлении, можно удалять старый архив и Писать новый,  в имя можно добавлять номер версии

## Response:
18.07.2026, 09:19:18

Да, это хорошая идея. Я бы даже не использовал фиксированное имя `backup.7z`, а сохранял версию в имени файла.

Например:

```text
SpecSPDS_2.3.15.0.backup.7z
```

или

```text
SpecSPDS_v2.3.15.backup.7z
```

Тогда алгоритм получается очень простой:

1. Перед обновлением удалить старые резервные архивы:
   ```text
   *.backup.7z
   ```

2. Создать новый архив текущей версии:
   ```text
   SpecSPDS_2.3.15.0.backup.7z
   ```

3. Выполнить обновление.

4. После первого успешного запуска новой версии удалить архив.

Если обновление не удалось, архив остается рядом с аддоном.

### Преимущества

- По имени сразу видно, **какую версию** можно восстановить.
- Всегда хранится только **одна** резервная копия.
- Не нужен поиск "последнего" архива.
- Пользователю проще объяснить:
  > "Если обновление прошло неудачно, распакуй `SpecSPDS_2.3.15.0.backup.7z` в каталог аддона."

### Я бы еще добавил

В архив можно положить небольшой текстовый файл, например `backup.json`:

```json
{
  "Product": "SpecSPDS",
  "Version": "2.3.15.0",
  "Created": "2026-07-18T10:35:42",
  "TargetVersion": "2.4.0.0"
}
```

Тогда при восстановлении можно проверить, что архив действительно относится к этому продукту, а также записать в лог, с какой версии выполняется обновление и к какой версии планировалось перейти. Это делает диагностику сбоев гораздо проще.

## Prompt:
18.07.2026, 09:21:15

Напиши класс упаковщика, работаем с 7z.exe

## Response:
18.07.2026, 09:21:15

Я бы сделал максимально простую обертку без зависимостей. Она принимает путь к каталогу, путь к архиву и необязательный пароль.

```csharp
using System;
using System.Diagnostics;
using System.IO;

namespace drz.Updater.Infrastructure
{
    public static class SevenZipPacker
    {
        public static void Create(
            string sourceDirectory,
            string archiveFile,
            string? password = null)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException(sourceDirectory);

            if (File.Exists(archiveFile))
                File.Delete(archiveFile);

            Directory.CreateDirectory(
                Path.GetDirectoryName(archiveFile)!);

            string sevenZip = Path.Combine(
                AppContext.BaseDirectory,
                "7z.exe");

            if (!File.Exists(sevenZip))
                throw new FileNotFoundException("Не найден 7z.exe", sevenZip);

            string arguments =
                $"a -t7z " +
                $"\"{archiveFile}\" " +
                $"\"{Path.Combine(sourceDirectory, "*")}\" " +
                $"-mx=9 " +
                $"-mmt " +
                $"-y";

            if (!string.IsNullOrWhiteSpace(password))
            {
                arguments +=
                    $" -p\"{password}\" -mhe=on";
            }

            var psi = new ProcessStartInfo
            {
                FileName = sevenZip,
                Arguments = arguments,
                WorkingDirectory = sourceDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Не удалось запустить 7z.exe");

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"7z завершился с кодом {process.ExitCode}");
        }
    }
}
```

Использование:

```csharp
SevenZipPacker.Create(
    sourceDirectory: addonDirectory,
    archiveFile: backupArchive,
    password: productName);
```

### Я бы еще изменил одну вещь

Чтобы в архив **не попадала сама папка**, а только ее содержимое, лучше использовать:

```text
WorkingDirectory = sourceDirectory

Arguments:
a archive.7z *
```

Тогда получится:

```text
SpecSPDS.dll
SpecSPDS.Core.dll
Menu\
Config\
```

а не

```text
Addon\
    SpecSPDS.dll
```

Такой архив удобнее восстанавливать поверх каталога аддона.

Еще я бы добавил параметры:

```
-mx=9     // максимальное сжатие
-mhe=on   // шифровать список файлов (если есть пароль)
-mmt      // использовать все ядра
-bb0      // минимальный вывод
-y        // отвечать Yes на все вопросы
```

Именно такой набор параметров хорошо подходит для создания резервной копии перед обновлением.

## Prompt:
18.07.2026, 09:23:40

Папка backup будет лежать рядом с package, это корень аддона

## Response:
18.07.2026, 09:23:41

Тогда я бы сделал структуру такой:

```text
AddOnRoot\
│
├── SpecSPDS.package
├── SpecSPDS.dll
├── SpecSPDS.Core.dll
├── Config\
├── Resources\
├── Backup\
│   └── SpecSPDS_2.3.15.backup.7z
└── Temp\
```

То есть `Backup` — отдельная папка в корне аддона.

Перед обновлением:

1. Создать `Backup`, если ее нет.
2. Удалить все `*.backup.7z`.
3. Создать новый архив.

Важно только **не включить папку `Backup` в архив**, иначе архив попадет сам в себя.

Проще всего передавать в 7z список того, что архивировать:

```text
SpecSPDS.dll
SpecSPDS.Core.dll
Config\
Resources\
...
```

или исключить каталог:

```text
-x!Backup
-x!Temp
```

Например:

```cmd
7z.exe a Backup\SpecSPDS_2.3.15.backup.7z * -xr!Backup -xr!Temp
```

или

```cmd
7z.exe a Backup\SpecSPDS_2.3.15.backup.7z * -x!Backup -x!Temp
```

Я бы также исключил:
- `Backup`
- `Temp`
- `Logs`
- `*.bak`

Тогда резервная копия всегда будет содержать **только рабочий аддон**, без временных файлов и предыдущих резервных копий. Это ускорит создание архива и уменьшит его размер.

## Prompt:
18.07.2026, 09:24:53

Перепиши класс с учетом этого

## Response:
18.07.2026, 09:24:53

Я бы сделал класс универсальным: он архивирует **всё содержимое корня аддона**, автоматически исключая служебные каталоги.

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace drz.Updater.Infrastructure
{
    public static class SevenZipPacker
    {
        public static void CreateBackup(
            string addonDirectory,
            string backupDirectory,
            string archiveName,
            string? password = null)
        {
            if (!Directory.Exists(addonDirectory))
                throw new DirectoryNotFoundException(addonDirectory);

            Directory.CreateDirectory(backupDirectory);

            // Удаляем предыдущие резервные копии
            foreach (string file in Directory.EnumerateFiles(backupDirectory, "*.backup.7z"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }

            string archiveFile = Path.Combine(backupDirectory, archiveName);

            string sevenZip = Path.Combine(AppContext.BaseDirectory, "7z.exe");

            if (!File.Exists(sevenZip))
                throw new FileNotFoundException("Не найден 7z.exe", sevenZip);

            string arguments =
                $"a " +
                $"-t7z " +
                $"\"{archiveFile}\" " +
                $"* " +
                $"-mx=9 " +
                $"-mmt " +
                $"-bb0 " +
                $"-y " +
                $"-xr!Backup " +
                $"-xr!Temp " +
                $"-xr!Logs " +
                $"-xr!*.bak ";

            if (!string.IsNullOrWhiteSpace(password))
                arguments += $"-p\"{password}\" -mhe=on ";

            var psi = new ProcessStartInfo
            {
                FileName = sevenZip,
                Arguments = arguments,
                WorkingDirectory = addonDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Не удалось запустить 7z.exe");

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"7z.exe завершился с кодом {process.ExitCode}");
        }
    }
}
```

Использование:

```csharp
string backupDir = Path.Combine(addonRoot, "Backup");

SevenZipPacker.CreateBackup(
    addonDirectory: addonRoot,
    backupDirectory: backupDir,
    archiveName: $"SpecSPDS_{currentVersion}.backup.7z",
    password: productName);
```

Я бы еще немного упростил API. Вместо передачи `backupDirectory` и `archiveName` отдельно сделал бы:

```csharp
CreateBackup(
    string addonDirectory,
    string productName,
    Version version,
    string? password = null)
```

Тогда класс сам будет:
- создавать `Backup`;
- удалять старые `*.backup.7z`;
- формировать имя `SpecSPDS_2.3.15.backup.7z`.

Вызов получится буквально одной строкой, а вся логика расположения резервных копий останется внутри класса.

## Prompt:
18.07.2026, 09:25:52

Удалять старые надо после создания нового

## Response:
18.07.2026, 09:25:52

Да, так безопаснее.

Если сначала удалить старый архив, а затем создание нового завершится ошибкой (нехватка места, сбой 7z, блокировка файла и т.п.), то останешься **вообще без резервной копии**.

Правильный порядок:

```text
1. Создать новый архив во временный файл.
2. Проверить, что архив успешно создан.
3. Удалить старые *.backup.7z.
4. Переименовать временный архив в окончательное имя.
```

Например:

```text
Backup\
    backup.tmp.7z
```

После успешного создания:

```text
удалить *.backup.7z
backup.tmp.7z → SpecSPDS_2.3.15.backup.7z
```

Если создание архива завершится неудачей, старый `SpecSPDS_2.3.14.backup.7z` останется на месте.

Это фактически атомарная замена резервной копии и самый надежный вариант.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)