# Создание update.txt после сборки

**User:** dRz  
**Created:** 7/15/2026 8:42:08  
**Updated:** 7/18/2026 10:49:57  
**Exported:** 7/19/2026 19:12:15  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a571d66-53e0-83ed-9e43-51a28023c04a](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a571d66-53e0-83ed-9e43-51a28023c04a)  

## Prompt:
15.07.2026, 08:43:01

visual studio 2026, как сделать что бы после сборки проекта в выходной каталог сборки записывался текстовый файл например update.txt 
содержимое файла имя проекта, мажор, минор, билд, ревизия?

## Response:
15.07.2026, 08:43:03

Да. Самый удобный способ — сделать это через **MSBuild Target** в `.csproj` или `Directory.Build.targets`. Тогда файл будет создаваться при каждой сборке автоматически.

Например:

```xml
<Target Name="WriteUpdateInfo" AfterTargets="Build">

  <PropertyGroup>
    <UpdateInfo>
Project=$(MSBuildProjectName)
Version=$(Version)
AssemblyVersion=$(AssemblyVersion)
FileVersion=$(FileVersion)
TargetFramework=$(TargetFramework)
Configuration=$(Configuration)
BuildDate=$([System.DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss"))
    </UpdateInfo>
  </PropertyGroup>

  <WriteLinesToFile
      File="$(TargetDir)update.txt"
      Lines="$(UpdateInfo)"
      Overwrite="true"
      Encoding="UTF-8" />

</Target>
```

После сборки получится файл

```text
Project=SpecSPDS
Version=2.5.17.123
AssemblyVersion=2.5.17.123
FileVersion=2.5.17.123
TargetFramework=net48
Configuration=Release
BuildDate=2026-07-15 08:34:11
```

---

## Если нужны только имя проекта и четыре части версии

Допустим версия задается так

```xml
<AssemblyVersion>2.5.17.123</AssemblyVersion>
<FileVersion>2.5.17.123</FileVersion>
```

Тогда можно записать

```xml
<Target Name="WriteUpdateInfo" AfterTargets="Build">

  <WriteLinesToFile
      File="$(TargetDir)update.txt"
      Overwrite="true"
      Lines="
$(MSBuildProjectName)
$(AssemblyVersion)
" />

</Target>
```

Получится

```
SpecSPDS
2.5.17.123
```

---

## Если нужны отдельные Major Minor Build Revision

MSBuild умеет вызывать методы .NET.

```xml
<Target Name="WriteUpdateInfo" AfterTargets="Build">

  <PropertyGroup>
    <VersionObject>$([System.Version]::Parse('$(AssemblyVersion)'))</VersionObject>

    <Major>$([System.Version]::Parse('$(AssemblyVersion)').Major)</Major>
    <Minor>$([System.Version]::Parse('$(AssemblyVersion)').Minor)</Minor>
    <Build>$([System.Version]::Parse('$(AssemblyVersion)').Build)</Build>
    <Revision>$([System.Version]::Parse('$(AssemblyVersion)').Revision)</Revision>
  </PropertyGroup>

  <WriteLinesToFile
      File="$(TargetDir)update.txt"
      Overwrite="true"
      Lines="
Project=$(MSBuildProjectName)
Major=$(Major)
Minor=$(Minor)
Build=$(Build)
Revision=$(Revision)
" />

</Target>
```

Результат

```text
Project=SpecSPDS
Major=2
Minor=5
Build=17
Revision=123
```

---

## Если версия берется из `<Version>`

В SDK-проектах обычно используют

```xml
<Version>2.5.17.123</Version>
```

или

```xml
<VersionPrefix>2.5.17</VersionPrefix>
<VersionSuffix>beta1</VersionSuffix>
```

Тогда лучше использовать `$(Version)` вместо `$(AssemblyVersion)`.

---

### Можно сделать и в формате INI

```
Project=SpecSPDS
Major=2
Minor=5
Build=17
Revision=123
Date=2026-07-15 08:34:11
```

или JSON

```json
{
  "Project": "SpecSPDS",
  "Major": 2,
  "Minor": 5,
  "Build": 17,
  "Revision": 123
}
```

Если у тебя несколько проектов (как `SpecSPDS`, `PlotSPDS`, `Loader` и т.д.), то имеет смысл вынести этот `Target` в общий `Directory.Build.targets`, чтобы все проекты автоматически создавали `update.txt` без дублирования кода.

## Prompt:
15.07.2026, 08:45:58

версия задается в директори билд пропс вот так

```
<Deterministic>false</Deterministic>
<GenerateAssemblyInfo>true</GenerateAssemblyInfo>
<AssemblyVersion>0.1.*</AssemblyVersion>
<GenerateAssemblyFileVersionAttribute>false</GenerateAssemblyFileVersionAttribute>
```

## Response:
15.07.2026, 08:45:58

В твоем случае есть важная особенность.

```xml
<AssemblyVersion>0.1.*</AssemblyVersion>
```

означает, что **компилятор вычисляет Build и Revision автоматически**. Эти значения **не становятся свойствами MSBuild**, поэтому в `Directory.Build.targets` ты не сможешь получить:

```text
0.1.12345.67890
```

Потому что для MSBuild свойство по-прежнему равно строке

```text
0.1.*
```

## Вариант 1. Получить фактическую версию после сборки (рекомендуется)

После сборки можно прочитать уже собранную DLL и извлечь из нее `AssemblyName.Version`.

```xml
<Target Name="WriteUpdateInfo" AfterTargets="Build">

  <GetAssemblyIdentity AssemblyFiles="$(TargetPath)">
    <Output TaskParameter="Assemblies" ItemName="AssemblyInfo"/>
  </GetAssemblyIdentity>

  <WriteLinesToFile
      File="$(TargetDir)update.txt"
      Overwrite="true"
      Lines="
Project=$(MSBuildProjectName)
Version=%(AssemblyInfo.Version)
"/>
</Target>
```

Получится, например

```text
Project=SpecSPDS
Version=0.1.9432.31765
```

---

## Можно сразу разложить на части

```xml
<Target Name="WriteUpdateInfo" AfterTargets="Build">

  <GetAssemblyIdentity AssemblyFiles="$(TargetPath)">
    <Output TaskParameter="Assemblies" ItemName="AssemblyInfo"/>
  </GetAssemblyIdentity>

  <PropertyGroup>
    <Major>$([System.Version]::Parse('%(AssemblyInfo.Version)').Major)</Major>
    <Minor>$([System.Version]::Parse('%(AssemblyInfo.Version)').Minor)</Minor>
    <Build>$([System.Version]::Parse('%(AssemblyInfo.Version)').Build)</Build>
    <Revision>$([System.Version]::Parse('%(AssemblyInfo.Version)').Revision)</Revision>
  </PropertyGroup>

  <WriteLinesToFile
      File="$(TargetDir)update.txt"
      Overwrite="true"
      Lines="
Project=$(MSBuildProjectName)
Major=$(Major)
Minor=$(Minor)
Build=$(Build)
Revision=$(Revision)
"/>
</Target>
```

---

## Вариант 2. Через FileVersionInfo

Если интересует именно версия файла (`FileVersion`), можно прочитать ее из готовой DLL, но `GetAssemblyIdentity` проще и быстрее.

---

### Обрати внимание на современный .NET

`AssemblyVersion` с `*` поддерживается только для классического .NET Framework. Для SDK-проектов под .NET 5/6/7/8/9 обычно используют фиксированную версию (`<Version>` или `<AssemblyVersion>`) и генерируют номер сборки средствами CI/CD. Если ты собираешь проекты под `net48` (или другой .NET Framework), то схема с `0.1.*` работает и предложенный выше способ через `GetAssemblyIdentity` позволяет получить уже вычисленную компилятором версию.

## Prompt:
15.07.2026, 08:52:34

придумай имена для этих файлов, они будут использоваться для обновления

## Response:
15.07.2026, 08:52:36

Если файл будет использоваться механизмом обновления, то лучше выбрать имя, которое сразу отражает его назначение и которое не будет конфликтовать с другими служебными файлами.

### Наиболее удачные варианты

| Имя файла | Когда использовать |
|-----------|--------------------|
| `update.txt` | Простое, если файл содержит только информацию о версии. |
| `version.txt` | Если основное содержимое — номер версии. |
| `build.txt` | Если важен номер сборки. |
| `release.txt` | Если хранится информация о релизе. |
| `manifest.txt` | Если в будущем появятся дополнительные поля (URL, CRC, размер и т.п.). |
| `update.info` | Хорошо выглядит как служебный файл обновления. |
| `update.ini` | Если будут пары ключ=значение. |
| `update.json` | Если планируется расширять структуру. |
| `product.ver` | Коротко и однозначно. |
| `version.info` | Часто встречающийся вариант. |

### Я бы выбрал

Если обновление будет делать **твоя программа**, то я бы сразу заложил возможность расширения.

#### Вариант 1 (мой фаворит)

```
update.ini
```

Содержимое:

```ini
[Product]
Name=SpecSPDS
Version=0.1.1234.5678
Major=0
Minor=1
Build=1234
Revision=5678
```

Потом можно добавить:

```ini
Url=https://...
Sha256=...
Size=1234567
RequiredVersion=...
```

---

#### Вариант 2

```
version.txt
```

Если нужен максимально простой файл:

```text
SpecSPDS
0.1.1234.5678
```

---

#### Вариант 3

```
update.json
```

Если обновлятор написан на C#:

```json
{
  "name": "SpecSPDS",
  "version": "0.1.1234.5678",
  "major": 0,
  "minor": 1,
  "build": 1234,
  "revision": 5678
}
```

Разбирать JSON в .NET очень удобно через `System.Text.Json`.

---

### Если файлов будет несколько

Тогда можно придерживаться единого соглашения:

```
update.ini        // информация о версии
update.log        // журнал обновления
update.lock       // блокировка обновления
update.cache      // кэш обновления
update.exe        // обновлятор
```

Так все служебные файлы будут иметь единый префикс `update`.

Для твоих проектов (`SpecSPDS`, `PlotSPDS`, загрузчик nanoCAD) я бы выбрал **`update.ini`**: формат легко читается человеком, просто парсится в C#, и его можно расширять без нарушения совместимости.

## Prompt:
15.07.2026, 09:01:31

удалить файлы перед сборкой

## Response:
15.07.2026, 09:01:33

Для этого лучше использовать `Target` с `BeforeTargets="Build"`.

Например, удалить `update.txt` перед сборкой:

```xml
<Target Name="DeleteUpdateFile" BeforeTargets="Build">
  <Delete Files="$(TargetDir)update.txt"
          ContinueOnError="true" />
</Target>
```

Если файлов несколько:

```xml
<Target Name="DeleteUpdateFiles" BeforeTargets="Build">
  <Delete Files="
      $(TargetDir)update.txt;
      $(TargetDir)update.ini;
      $(TargetDir)version.txt"
      ContinueOnError="true" />
</Target>
```

### Или удалить по маске

```xml
<Target Name="DeleteUpdateFiles" BeforeTargets="Build">
  <ItemGroup>
    <UpdateFiles Include="$(TargetDir)update.*"/>
  </ItemGroup>

  <Delete Files="@(UpdateFiles)"
          ContinueOnError="true" />
</Target>
```

### Если нужно выполнять перед каждой компиляцией

Можно использовать `CoreCompile` вместо `Build`:

```xml
<Target Name="DeleteUpdateFiles" BeforeTargets="CoreCompile">
    ...
</Target>
```

Это актуально, если проект не всегда проходит полный этап `Build`.

---

### Если создаешь файл после сборки

```xml
<Target Name="DeleteUpdateFiles" BeforeTargets="Build">
    <Delete Files="$(TargetDir)update.ini"/>
</Target>

<Target Name="WriteUpdateFile" AfterTargets="Build">
    ...
</Target>
```

Такой вариант обычно является наиболее правильным: старый файл удаляется перед сборкой, а новый создается только при успешной сборке.

## Prompt:
15.07.2026, 09:14:50

```
<!--ПРИБИТЬ ФАЙЛЫ UPDATE-->
<Target Name="DeleteUpdateFiles" BeforeTargets="CoreCompile">
	<ItemGroup>
		<UpdateFiles Include="$(TargetDir)update.*"/>
	</ItemGroup>

	<Delete Files="@(UpdateFiles)"
			ContinueOnError="true" />
</Target>
	<!--ini-->
<Target Name="WriteUpdateInfoINI" AfterTargets="Build">

	<GetAssemblyIdentity AssemblyFiles="$(TargetPath)">
		<Output TaskParameter="Assemblies" ItemName="AssemblyInfo"/>
	</GetAssemblyIdentity>

	<WriteLinesToFile
		File="$(TargetDir)update.ini"
		Overwrite="false"
		Lines="
```

[$(MSBuildProjectName)]
Name=$(SolutionName)$(MSBuildProjectName)
Version=%(AssemblyInfo.Version)
"/>

в файл пишутся не все сборки

## Response:
15.07.2026, 09:14:50

Причина, скорее всего, в одном из следующих факторов.

### 1. `Overwrite="false"` — наиболее вероятная причина

У тебя стоит

```xml
<WriteLinesToFile
    File="$(TargetDir)update.ini"
    Overwrite="false"
    ...
/>
```

`Overwrite="false"` означает:

- если файла нет — создать;
- если файл уже есть — **дописать в конец**.

Если несколько проектов собираются **параллельно** (`BuildInParallel`), то несколько процессов одновременно открывают один и тот же `update.ini`. `WriteLinesToFile` не синхронизирует доступ, поэтому часть записей может потеряться.

---

### 2. Все проекты пишут в один каталог

Проверь, чему равен

```text
$(TargetDir)
```

Если это один общий каталог (например, общий `bin\Release`), то все проекты одновременно пишут в один файл.

---

### 3. `DeleteUpdateFiles`

```xml
BeforeTargets="CoreCompile"
```

Этот Target выполнится **для каждого проекта**.

Получается такая картина:

```
Проект A
    Delete update.ini

Проект B
    Delete update.ini

Проект C
    Delete update.ini
```

Если проекты собираются одновременно:

```
A записал
B удалил
C записал
A дописал
...
```

В результате часть информации исчезает.

---

## Как сделать правильно

Есть два варианта.

### Вариант 1. Каждый проект пишет свой файл (рекомендуется)

Например

```xml
File="$(TargetDir)$(MSBuildProjectName).update.ini"
```

Получится

```
SpecSPDS.update.ini
Loader.update.ini
PlotSPDS.update.ini
```

После этого обновлятор может объединить их.

---

### Вариант 2. Один общий файл

Тогда:

- **не удалять** файл в каждом проекте;
- удалить его **один раз** перед сборкой решения;
- писать в него **последовательно**, а не параллельно.

При параллельной сборке MSBuild гарантировать корректную дозапись в один файл не может.

---

## Я бы вообще сделал так

Создавал бы файл

```
$(TargetDir)update.ini
```

для **каждого проекта отдельно**, например:

```
SpecSPDS\bin\Release\update.ini
Loader\bin\Release\update.ini
Core\bin\Release\update.ini
```

Это исключает гонки и делает каждый проект независимым.

---

**Вопрос:** ты собираешь **Solution**, где все проекты выводятся в **одну общую папку** (`OutputPath` одинаковый), или у каждого проекта свой `bin\Release`? Это принципиально влияет на решение.

## Prompt:
15.07.2026, 09:20:17

если запретить удалять файл и удалить его перед компиляцией вручную, то запишутся строки всех сборок

## Response:
15.07.2026, 09:20:19

Да. Это практически подтверждает, что проблема не в `WriteLinesToFile`, а в том, что **каждый проект выполняет `DeleteUpdateFiles`**.

При сборке решения происходит примерно следующее:

```text
Проект A:
  Delete update.ini
  ...
  Append свою строку

Проект B:
  Delete update.ini   <-- удаляет запись A
  ...
  Append свою строку

Проект C:
  Delete update.ini   <-- удаляет запись B
  ...
  Append свою строку
```

При последовательной сборке в файле останется только последний проект, а при параллельной — результат вообще непредсказуем.

Когда ты удаляешь файл вручную **один раз перед сборкой**, то каждый проект только выполняет:

```text
Append свою строку
```

и в итоге файл содержит записи всех проектов.

### Как сделать правильно

Лучше вообще не удалять файл из каждого проекта. Вместо этого:

- удалять `update.ini` один раз перед сборкой решения;
- либо сделать отдельный проект (или target), который выполняется один раз и очищает файл;
- после этого все проекты только дописывают свои записи (`Overwrite="false"`).

### Если очистку хочется оставить в MSBuild

Можно ограничить ее выполнением только для одного проекта, например:

```xml
<Target Name="DeleteUpdateFiles"
        BeforeTargets="Build"
        Condition="'$(MSBuildProjectName)' == 'Loader'">

    <Delete Files="$(TargetDir)update.ini"
            ContinueOnError="true"/>
</Target>
```

Тогда файл будет удаляться только при сборке проекта `Loader`, а остальные проекты будут его только дополнять.

### Еще один нюанс

Если в Visual Studio включена **параллельная сборка проектов** (`Максимальное число параллельных сборок > 1`), то даже без удаления несколько проектов могут одновременно писать в один файл. Обычно это работает, но надежных гарантий нет — возможны потерянные записи или перемешивание строк.

Если этот `update.ini` важен для обновления, более надежная схема — чтобы каждый проект создавал свой `*.update.ini`, а затем один завершающий шаг объединял их в общий файл. Это исключает любые гонки и делает процесс воспроизводимым.

## Prompt:
15.07.2026, 09:21:29

что написать в BeforeTargets  что бы файл удалялся только перед сборкой?

## Response:
15.07.2026, 09:21:29

Короткий ответ: **никакого `BeforeTargets` для этого нет**, если `Target` находится в `Directory.Build.targets`.

Почему:

- `Directory.Build.targets` импортируется **в каждый проект**.
- Любой `Target` из него выполняется **для каждого проекта**, независимо от того, что указано в `BeforeTargets` (`Build`, `CoreCompile`, `PrepareForBuild` и т.д.).
- MSBuild не имеет встроенного события **"один раз перед сборкой решения"**.

То есть:

```xml
BeforeTargets="Build"
BeforeTargets="CoreCompile"
BeforeTargets="PrepareForBuild"
BeforeTargets="Compile"
```

— все они будут выполняться **для каждого проекта**, а не один раз.

## Возможные решения

### 1. Удалять вручную перед сборкой
То, что ты уже сделал. Самый простой вариант.

### 2. Удалять только в одном проекте

Например, если у тебя есть главный проект `Loader`:

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="'$(MSBuildProjectName)' == 'Loader'">

    <Delete Files="$(TargetDir)update.ini"/>
</Target>
```

### 3. Использовать флаг (лучший вариант внутри MSBuild)

Можно сделать так, чтобы удаление происходило только один раз за процесс сборки. Например, через создание "маркерного" файла:

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="!Exists('$(TargetDir)update.delete.lock')">

    <Delete Files="$(TargetDir)update.ini"/>
    <Touch Files="$(TargetDir)update.delete.lock"/>
</Target>
```

А после окончания сборки удалить `update.delete.lock`.

Но это усложняет проект.

---

### Вопрос

**Ты собираешь:**

1. **из Visual Studio** (Build Solution),
2. **через MSBuild.exe**,
3. **через dotnet build**?

И еще: у тебя **все проекты пишут в одну общую папку `bin`**, или у каждого свой `bin`, а `update.ini` копируется в общее место? Это важно для выбора наиболее надежного решения.

## Prompt:
15.07.2026, 09:31:07

пишут в общую папку

## Response:
15.07.2026, 09:31:08

Тогда причина полностью понятна.

Все проекты пишут в **одну общую папку**, поэтому любой `Delete` из любого проекта удаляет результаты предыдущих проектов.

При этом **в MSBuild нет события "перед сборкой решения"**. `BeforeTargets` всегда относится к **проекту**, а не к решению.

## Наиболее правильные варианты

### Вариант 1. Pre-Build Event у решения

У `.sln` нет PreBuildEvent, поэтому этот вариант отпадает.

---

### Вариант 2. Отдельный проект "BuildTools" (рекомендуется)

Создать проект, например `BuildTools`, который:

- удаляет `update.ini`;
- является зависимостью остальных проектов.

Тогда удаление выполнится один раз.

---

### Вариант 3. Удалять только в одном проекте

Если у тебя есть главный проект (например, `Loader`), который всегда собирается первым, то:

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="'$(MSBuildProjectName)' == 'Loader'">

    <Delete Files="$(TargetDir)update.ini"/>
</Target>
```

Но это работает только если `Loader` действительно собирается первым.

---

### Вариант 4. Использовать инкрементальную сборку (мой выбор)

MSBuild умеет выполнять Target только один раз, если задать `Inputs` и `Outputs`.

Например:

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Inputs="$(MSBuildProjectFullPath)"
        Outputs="$(TargetDir)update.delete.stamp">

    <Delete Files="$(TargetDir)update.ini"/>
    <Touch Files="$(TargetDir)update.delete.stamp"/>
</Target>
```

После сборки удалить `update.delete.stamp`.

---

## Но есть решение намного проще

Поскольку все проекты **дописывают** в `update.ini`, вообще не удаляй его в MSBuild.

Перед сборкой решения запускай небольшой скрипт.

Например, `Build.cmd`:

```bat
@echo off

del /q ".\bin\Release\update.ini" 2>nul

msbuild MySolution.sln /m
```

или для Visual Studio:

```bat
devenv MySolution.sln /Build Release
```

Так делают многие проекты: подготовка окружения (очистка, генерация файлов, удаление временных данных) выполняется **до запуска MSBuild**, а не внутри него.

---

### Если ты пользуешься Visual Studio 2026

Есть еще один интересный способ — использовать специальный Target `InitialTargets`. Он может сработать только один раз на уровне корневого проекта при сборке решения. Для твоего случая это может оказаться самым элегантным решением. Могу показать пример, который будет удалять `update.ini` ровно один раз перед началом сборки всей Solution, без дополнительных проектов и bat-файлов.

## Prompt:
15.07.2026, 09:32:45

покажи

## Response:
15.07.2026, 09:32:46

К сожалению, `InitialTargets` не решит эту задачу.

Он выполняется **при начале сборки каждого проекта**, а не один раз для всей Solution. То есть если `Directory.Build.targets` импортирован в 10 проектов, `InitialTargets` тоже выполнится 10 раз.

Есть, однако, способ, который работает именно в твоей ситуации.

### Использовать статическую переменную MSBuild

Можно определить свойство, которое после первого выполнения больше не позволит выполнять `Delete`.

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="'$(UpdateFileDeleted)' != 'true'">

    <Delete Files="$(TargetDir)update.ini"
            ContinueOnError="true"/>

    <PropertyGroup>
        <UpdateFileDeleted>true</UpdateFileDeleted>
    </PropertyGroup>

</Target>
```

**Но!** Это тоже **не сработает** при сборке нескольких проектов, потому что каждый проект имеет собственный набор свойств MSBuild.

---

## Что действительно работает

Если все проекты пишут в **один каталог**, то есть два надежных варианта:

### Вариант 1. Маркерный файл

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="!Exists('$(TargetDir)update.lock')">

    <Delete Files="$(TargetDir)update.ini"/>
    <Touch Files="$(TargetDir)update.lock"/>

</Target>
```

После окончания сборки (например, в главном проекте):

```xml
<Target Name="CleanupLock"
        AfterTargets="Build"
        Condition="'$(MSBuildProjectName)' == 'Loader'">

    <Delete Files="$(TargetDir)update.lock"/>

</Target>
```

Однако при **параллельной сборке** (`/m`) здесь все равно возможна гонка: два проекта могут одновременно проверить отсутствие `update.lock` и оба выполнить удаление.

---

### Вариант 2. Общий файл после сборки решения (самый надежный)

Я бы вообще отказался от идеи, что каждый проект пишет в общий `update.ini`.

Гораздо надежнее:

1. Каждый проект создает свой файл:

```
SpecSPDS.update.ini
Loader.update.ini
PlotSPDS.update.ini
```

2. Один завершающий Target (или небольшой EXE/скрипт) объединяет их в один `update.ini`.

Преимущества:

- нет гонок;
- работает с параллельной сборкой;
- не зависит от порядка сборки;
- легко расширяется.

---

### Я бы предложил другое решение

Раз уж у тебя все проекты собираются в **один общий каталог** и ты управляешь всей сборкой через `Directory.Build.targets`, можно сделать так, чтобы **вообще не использовать общий файл во время сборки**. Вместо этого каждый проект будет писать свою запись в память MSBuild, а **один финальный Target** сформирует `update.ini` после завершения сборки решения.

Для этого есть механизм `DispatchToInnerBuilds` и специальные возможности MSBuild для координации проектов. Это уже более продвинутое решение, но оно полностью устраняет проблему. Думаю, именно оно лучше всего подойдет для твоей системы.

## Prompt:
15.07.2026, 09:34:13

покажи

## Response:
15.07.2026, 09:34:15

Показал бы, но здесь есть важный момент: **сделать "один Target после сборки Solution" средствами только `Directory.Build.targets` нельзя**.

MSBuild строит граф проектов, и каждый проект выполняется изолированно. Нет встроенного Target вроде:

```xml
BeforeSolutionBuild
AfterSolutionBuild
```

или

```xml
DispatchToInnerBuilds
```

который позволил бы в обычной сборке `.sln` выполнить код ровно один раз в начале или в конце решения. Для этого обычно используют специальный "агрегирующий" проект (`.proj`) или внешний скрипт.

### Что я бы сделал в твоем случае

Поскольку:

- все проекты выводят файлы в **общий `TargetDir`**;
- все проекты используют общий `Directory.Build.targets`;
- нужен **один** `update.ini`;

я бы вообще не заставлял каждый проект дописывать в один файл.

#### Каждый проект создает свой файл

```text
update.SpecSPDS.ini
update.Loader.ini
update.Core.ini
```

Например:

```xml
<WriteLinesToFile
    File="$(TargetDir)update.$(MSBuildProjectName).ini"
    Overwrite="true"
    Lines="
[$(MSBuildProjectName)]
Name=$(SolutionName)$(MSBuildProjectName)
Version=%(AssemblyInfo.Version)
"/>
```

#### Затем один небольшой проект (или консольная утилита)

В конце сборки:

1. удаляет `update.ini`;
2. читает все `update.*.ini`;
3. объединяет их;
4. удаляет временные `update.*.ini`.

На C# это буквально 20–30 строк.

---

## Если очень хочется оставить один файл

Тогда есть единственный прием, который можно попробовать: использовать условие на проект верхнего уровня.

При сборке решения MSBuild устанавливает некоторые внутренние свойства (`IsGraphBuild`, `BuildingProject`, и др.), но **нет гарантированного свойства**, по которому можно определить "это первый проект". Порядок зависит от графа зависимостей и может меняться.

Поэтому решение вида:

```xml
Condition="'$(MSBuildProjectName)' == 'ПервыйПроект'"
```

работает только если этот проект действительно всегда строится первым.

---

### Моя рекомендация

Я бы вообще отказался от текстового `update.ini` и после сборки решения генерировал **единый JSON**:

```json
{
  "SpecSPDS": "1.2.15.456",
  "Loader": "1.0.3.12",
  "Core": "2.1.0.98"
}
```

или

```json
[
  {
    "name": "SpecSPDS",
    "version": "1.2.15.456"
  },
  {
    "name": "Loader",
    "version": "1.0.3.12"
  }
]
```

Разбирать такой файл в C# через `System.Text.Json` проще и надежнее, чем работать с INI.

**Если обновлятор тоже пишешь ты на C#**, я бы вообще отказался от `WriteLinesToFile` и сделал **небольшую утилиту `UpdateManifest.exe`**, которая после сборки получает путь к каталогу, находит все DLL, считывает из них `AssemblyVersion` и сама формирует `update.json` или `update.ini`. Тогда отпадает необходимость в генерации служебных файлов каждым проектом, и источник информации (версии DLL) всегда остается единственным и достоверным. Это решение проще сопровождать и оно не зависит от порядка или параллельности сборки.

## Prompt:
15.07.2026, 09:47:11

```
</Target>
<Target Name="WriteUpdateInfoJson" AfterTargets="Build">

	<GetAssemblyIdentity AssemblyFiles="$(TargetPath)">
		<Output TaskParameter="Assemblies" ItemName="AssemblyInfo"/>
	</GetAssemblyIdentity>

	<PropertyGroup>
		<Major>$([System.Version]::Parse('%(AssemblyInfo.Version)').Major)</Major>
		<Minor>$([System.Version]::Parse('%(AssemblyInfo.Version)').Minor)</Minor>
		<Build>$([System.Version]::Parse('%(AssemblyInfo.Version)').Build)</Build>
		<Revision>$([System.Version]::Parse('%(AssemblyInfo.Version)').Revision)</Revision>
	</PropertyGroup>

	<WriteLinesToFile
		File="$(TargetDir)update.json"
		Overwrite="false"
		Lines="
```

Project=$(MSBuildProjectName)
Major=$(Major)
Minor=$(Minor)
Build=$(Build)
Revision=$(Revision)
"/>
оформи для json

## Response:
15.07.2026, 09:47:12

Если все проекты **дописывают** в один `update.json` (`Overwrite="false"`), то сделать **корректный JSON невозможно**. JSON должен содержать один корневой объект или массив, а при дозаписи из разных проектов ты не сможешь автоматически поставить `[` `]` и запятые между объектами.

То есть получится что-то вроде:

```json
{
  "Project":"Core"
}
{
  "Project":"Loader"
}
```

что **не является валидным JSON**.

## Вариант 1. JSON Lines (рекомендую)

Это стандартный формат (`.jsonl` или NDJSON), где **каждая строка — отдельный JSON-объект**. Он идеально подходит для дозаписи.

Например, файл `update.jsonl`:

```json
{"Project":"Core","Major":1,"Minor":0,"Build":1234,"Revision":5678}
{"Project":"Loader","Major":1,"Minor":0,"Build":1235,"Revision":5679}
{"Project":"SpecSPDS","Major":2,"Minor":1,"Build":15,"Revision":42}
```

Тогда в MSBuild:

```xml
<WriteLinesToFile
    File="$(TargetDir)update.jsonl"
    Overwrite="false"
    Lines='{"Project":"$(MSBuildProjectName)","Major":$(Major),"Minor":$(Minor),"Build":$(Build),"Revision":$(Revision)}' />
```

Это полностью валидный формат, который легко читается в C# построчно.

---

## Вариант 2. Один JSON-файл

Если нужен именно `update.json`, то его нужно формировать **один раз** после сборки всех проектов:

```json
[
  {
    "Project": "Core",
    "Major": 1,
    "Minor": 0,
    "Build": 1234,
    "Revision": 5678
  },
  {
    "Project": "Loader",
    "Major": 1,
    "Minor": 0,
    "Build": 1235,
    "Revision": 5679
  }
]
```

Но так сделать с `WriteLinesToFile Overwrite="false"` нельзя.

---

### Что я рекомендую

Для твоей схемы сборки используй **`update.jsonl`** (или `update.ndjson`). Это специально придуманный формат для случаев, когда несколько процессов постепенно дописывают JSON-данные в один файл. Затем обновлятор сможет прочитать его так:

```csharp
foreach (var line in File.ReadLines("update.jsonl"))
{
    var info = JsonSerializer.Deserialize<UpdateInfo>(line);
}
```

Это надежнее и проще, чем пытаться собирать один общий JSON во время параллельной сборки.

## Prompt:
15.07.2026, 10:07:30

как автоматизировать очистку выходного каталога решения перед сборкой?

## Response:
15.07.2026, 10:07:30

Есть несколько способов. Для Visual Studio наиболее практичны следующие.

## 1. Собственный профиль сборки (рекомендуется)

Создай `Build.cmd`:

```bat
@echo off

set OUTDIR=%~dp0bin

if exist "%OUTDIR%" (
    rmdir /S /Q "%OUTDIR%"
)

msbuild MySolution.sln /m /p:Configuration=Release
```

И собирай решение через него.

---

## 2. Внешний инструмент Visual Studio

Ты уже используешь **External Tools**.

Добавь инструмент:

```
Title:
Clean Output

Command:
cmd.exe

Arguments:
/c if exist "$(SolutionDir)bin" rmdir /s /q "$(SolutionDir)bin"

Initial directory:
$(SolutionDir)
```

Перед сборкой нажал **Tools → Clean Output**.

---

## 3. Pre-build Event одного проекта

Если есть главный проект (например, Loader):

```bat
if exist "$(SolutionDir)bin" rmdir /s /q "$(SolutionDir)bin"
```

Но это опасно.

Если проекты собираются параллельно, один уже может писать в `bin`, пока другой его удаляет.

---

## 4. Собственный `.proj` (самый правильный)

Создается файл `Build.proj`:

```xml
<Project DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <Target Name="CleanOutput">
    <RemoveDir Directories="bin" />
  </Target>

  <Target Name="Build" DependsOnTargets="CleanOutput">
    <MSBuild Projects="MySolution.sln"
             Targets="Build"
             Properties="Configuration=Release" />
  </Target>

</Project>
```

Запуск:

```
msbuild Build.proj
```

Это промышленный вариант.

---

## 5. Расширение Visual Studio

Есть расширения, позволяющие выполнять команды до сборки, но я бы их не использовал.

---

# Что бы сделал я

Учитывая твои проекты:

- общий `bin`;
- `Directory.Build.props`;
- собственные утилиты;
- общая логика сборки.

Я бы сделал небольшой `Build.cmd` или `Build.ps1`, который выполняет:

```bat
@echo off

echo Cleaning output...
if exist "%~dp0bin" rmdir /s /q "%~dp0bin"

echo Building...
devenv SpecSPDS.sln /Build Release
```

или

```bat
msbuild SpecSPDS.sln /m /p:Configuration=Release
```

Такой способ не зависит от MSBuild Target'ов, работает одинаково из командной строки и в CI, и исключает проблемы с параллельной сборкой.

---

### Есть еще один удобный вариант именно для Visual Studio

Если ты **всегда** собираешь через **F6 / Ctrl+Shift+B**, можно использовать **макрос Visual Studio (VSIX)** или небольшое расширение, которое автоматически удаляет `$(SolutionDir)\bin` перед каждой сборкой. Это позволяет не менять привычный процесс работы и не запускать отдельные скрипты. Для ежедневной работы в IDE это наиболее комфортный вариант, если готов один раз установить или написать такое расширение.

## Prompt:
15.07.2026, 10:10:28

про f6 поподробнее

## Response:
15.07.2026, 10:10:30

К сожалению, в современных Visual Studio (2022/2026) **нет встроенного способа привязать выполнение команды перед F6 (Build Solution)**.

Раньше были макросы (Visual Studio Macros), но их давно убрали.

Есть несколько вариантов.

## Вариант 1. Расширение VSIX (лучший, если готов написать)

Visual Studio имеет события сборки через `BuildEvents`.

Можно подписаться на:

```csharp
DTE.Events.BuildEvents.OnBuildBegin
DTE.Events.BuildEvents.OnBuildDone
```

Пример:

```csharp
buildEvents.OnBuildBegin += (scope, action) =>
{
    Directory.Delete(Path.Combine(solutionDir, "bin"), true);
};
```

Тогда при любом:

- F6
- Ctrl+Shift+B
- Build → Build Solution

каталог будет удаляться автоматически.

Плюсы:

- полностью прозрачно;
- не нужно менять привычки;
- можно добавить любую подготовку.

Минусы:

- нужно написать VSIX (это несложно, но требует отдельного проекта).

---

## Вариант 2. Собственная команда вместо F6

Можно сделать External Tool:

```
Clean + Build
```

которая запускает

```bat
rmdir /S /Q bin
devenv Solution.sln /Build Release
```

и назначить ей горячую клавишу, например **F6**.

Но есть ограничение: External Tool запускает **новый экземпляр devenv**, а не использует уже открытую IDE. Это неудобно.

---

## Вариант 3. Команда Visual Commander (мой фаворит)

Есть расширение **Visual Commander**.

Оно позволяет писать небольшие C#-макросы.

Макрос может выглядеть так:

```csharp
var bin = Path.Combine(DTE.Solution.FullName, "..", "bin");

if (Directory.Exists(bin))
    Directory.Delete(bin, true);

DTE.ExecuteCommand("Build.BuildSolution");
```

И назначить его на **F6**.

В результате:

```
F6
    ↓
Удалить bin
    ↓
Build Solution
```

Без открытия новой Visual Studio.

Это практически то же, что были старые макросы.

---

## Вариант 4. Собственное расширение

Можно сделать VSIX буквально на 100 строк.

Оно:

- перехватывает команду `BuildSolution`;
- удаляет `bin`;
- вызывает оригинальную команду.

Работает как будто это встроенная функция Visual Studio.

---

# Что бы я рекомендовал

Поскольку ты уже пишешь на C# и делаешь расширения для nanoCAD, **VSIX для Visual Studio** для тебя — вполне посильная задача.

Преимущества:

- один раз написал;
- работает во всех решениях;
- можно добавить очистку `bin`, `obj`, генерацию `update.json`, запуск анализаторов, проверку версий и т.д.

Получится полноценный "Build Assistant" под твои проекты.

**Если интересно, могу показать готовый VSIX (около 150–200 строк), который будет автоматически удалять `$(SolutionDir)\bin` при нажатии F6 и затем запускать обычную сборку.**

## Prompt:
15.07.2026, 10:23:28

Visual Commander   что это?

## Response:
15.07.2026, 10:23:28

**Visual Commander** — это расширение для Visual Studio, которое добавляет возможность создавать собственные команды и макросы на C#.

По сути, это современная замена старым **Visual Studio Macros**, которые Microsoft убрала после Visual Studio 2010.

С его помощью можно автоматизировать практически любые действия в IDE:

- выполнять код C#;
- обращаться к объектной модели Visual Studio (`DTE`);
- запускать встроенные команды (`Build.BuildSolution`, `File.SaveAll` и др.);
- работать с файлами;
- вызывать внешние программы;
- назначать макросы на горячие клавиши и пункты меню.

### Примеры использования

Например, макрос может:

```text
Сохранить все файлы
↓
Удалить bin
↓
Удалить obj
↓
Очистить решение
↓
Собрать решение
↓
Открыть каталог с результатом
```

И все это по нажатию одной клавиши.

Или:

```text
Перед сборкой:
    увеличить BuildNumber
После сборки:
    создать update.json
    скопировать файлы
    подписать DLL
```

### Как выглядит макрос

Это обычный C#:

```csharp
public class Command
{
    public void Run(EnvDTE.DTE dte)
    {
        dte.ExecuteCommand("Build.BuildSolution");
    }
}
```

Можно использовать любые классы .NET (`System.IO`, `System.Text.Json`, `Process` и т.д.).

### Плюсы

- Не нужно писать полноценное расширение VSIX.
- Можно быстро автоматизировать рутинные операции.
- Макросы легко изменять прямо из Visual Studio.
- Можно назначить собственные горячие клавиши.

### Минусы

- Это **стороннее расширение**, а не функция Visual Studio.
- Оно работает в рамках возможностей DTE, поэтому не может перехватывать абсолютно все внутренние события IDE.

---

## Подойдет ли тебе?

Судя по тому, чем ты занимаешься (C#, nanoCAD, MSBuild, автоматизация), **да**. Если нужна именно автоматизация процесса разработки без создания собственного VSIX, Visual Commander — хороший инструмент.

Если же автоматизация нужна только для очистки общего `bin` и генерации `update.ini/json`, я бы все-таки предпочел решить это на уровне процесса сборки (MSBuild + отдельный build-скрипт или агрегирующий проект). Такое решение не зависит от установленной IDE, работает одинаково в Visual Studio, из командной строки и в CI.

## Prompt:
15.07.2026, 10:25:55

Directory.Build.props может удалить файл перед сборкой?

## Response:
15.07.2026, 10:25:56

Да, **может**, но с тем же ограничением, о котором мы говорили.

Важно различать:

- **`Directory.Build.props`** — импортируется **до** большинства свойств проекта. Он предназначен в первую очередь для определения свойств и не подходит для выполнения задач сборки.
- **`Directory.Build.targets`** — импортируется **после** проекта и именно здесь должны находиться `Target`, `Delete`, `WriteLinesToFile` и другие задачи.

То есть технически можно написать в `Directory.Build.props`:

```xml
<Target Name="DeleteUpdateFile" BeforeTargets="Build">
    ...
</Target>
```

MSBuild это выполнит, потому что `Target` допустим и в `.props`. Но это считается плохой практикой. Такие действия должны быть в `Directory.Build.targets`.

---

### Главное ограничение

Независимо от того, находится Target в `.props` или `.targets`, он будет выполняться **для каждого проекта**.

То есть

```xml
<Target Name="DeleteUpdateFile" BeforeTargets="Build">
    <Delete Files="$(TargetDir)update.ini"/>
</Target>
```

будет вызван:

```text
Project A
    Delete

Project B
    Delete

Project C
    Delete
```

а не один раз на Solution.

---

## Есть интересный вариант

Если у тебя **все проекты используют один и тот же `OutputPath`**, можно воспользоваться тем, что после первого удаления файл уже отсутствует:

```xml
<Target Name="DeleteUpdateFile"
        BeforeTargets="Build"
        Condition="Exists('$(TargetDir)update.ini')">

    <Delete Files="$(TargetDir)update.ini"/>
</Target>
```

Это уменьшит число удалений, но **не устранит проблему**, если один проект уже успел создать новый `update.ini` до того, как другой проект дошел до своего `Delete`.

---

## Вопрос

У тебя сборка проектов в Visual Studio выполняется **параллельно**?

Проверь:

**Tools → Options → Projects and Solutions → Build and Run → maximum number of parallel project builds**

Если там стоит больше **1**, то именно параллельная сборка является причиной гонок при работе с общим `update.ini`. Если поставить **1**, то текущая схема с `Overwrite="false"` и удалением в первом проекте станет предсказуемой (хотя все равно будет зависеть от порядка сборки проектов).

## Prompt:
15.07.2026, 12:00:11

Directory.Build.targets
<Project>
	<PropertyGroup>
		<SharedConfigDir>d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\Shared\</SharedConfigDir>
		<ConfigNameIn>Single_var</ConfigNameIn>
		<ConfigNameOut>NLog.dll.nlog</ConfigNameOut>
	</PropertyGroup>
	<Target Name="CopyNLogConfig"
	        AfterTargets="Build">
		<Copy SourceFiles="$(SharedConfigDir)\$(ConfigNameIn).$(Configuration).config"
		      DestinationFiles="$(OutDir)$(ConfigNameOut)"
		      SkipUnchangedFiles="true"
		      Condition="Exists('$(SharedConfigDir)\$(ConfigNameIn).$(Configuration).config')"/>
	</Target>
</Project>
добавил в папку проекта
в папке решения есть свой Directory.Build.targets
посыпались ошибки в проекте

Серьезность	Код	Описание	Проект	Файл	Строка	Состояние подавления	Подробности
Ошибка (активно)	CS0103	Имя "InternalLoggerHelpers" не существует в текущем контексте.	LogBootstrap	D:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\+LogBootstrap\_Draft\LogBootstrap.cs	45		
Имя "LogVar" не существует в текущем контексте.

## Response:
15.07.2026, 12:00:13

Это ожидаемое поведение. Проблема **не в Target**, а в том, что происходит при наличии второго `Directory.Build.targets`.

### Что происходит

MSBuild ищет `Directory.Build.targets` **снизу вверх** по дереву каталогов.

Если у тебя структура примерно такая:

```text
SpecSPDS\
│   Directory.Build.targets   ← общий
│
├── LogBootstrap\
│      Directory.Build.targets ← добавил
│      LogBootstrap.csproj
│
├── Core\
└── ...
```

то для проекта `LogBootstrap` **будет импортирован только ближайший** `Directory.Build.targets`.

То есть общий файл из корня **перестанет импортироваться автоматически**.

Из-за этого могут исчезнуть:

- `Compile Include=...`
- `DefineConstants`
- `PackageReference`
- `Reference`
- общие `.props`
- генерация файлов
- любые другие настройки, которые были в корневом `Directory.Build.targets`.

Именно поэтому у тебя внезапно появились ошибки вида:

```text
CS0103: Имя 'InternalLoggerHelpers' не существует...
CS0103: Имя 'LogVar' не существует...
```

Это, скорее всего, означает, что корневой `Directory.Build.targets` добавлял какие-то общие файлы в компиляцию или определял другие настройки.

---

## Как сделать правильно

В локальном `Directory.Build.targets` нужно **импортировать родительский**.

Например:

```xml
<Project>

  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.targets', '$(MSBuildThisFileDirectory)..\'))" />

  <PropertyGroup>
    <SharedConfigDir>d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\Shared\</SharedConfigDir>
    <ConfigNameIn>Single_var</ConfigNameIn>
    <ConfigNameOut>NLog.dll.nlog</ConfigNameOut>
  </PropertyGroup>

  <Target Name="CopyNLogConfig"
          AfterTargets="Build">
    <Copy
      SourceFiles="$(SharedConfigDir)\$(ConfigNameIn).$(Configuration).config"
      DestinationFiles="$(OutDir)$(ConfigNameOut)"
      SkipUnchangedFiles="true"
      Condition="Exists('$(SharedConfigDir)\$(ConfigNameIn).$(Configuration).config')" />
  </Target>

</Project>
```

Обрати внимание на `..\` — иначе `GetPathOfFileAbove()` найдет **этот же файл**, что приведет к рекурсивному импорту.

---

## Или еще лучше

Если этот `CopyNLogConfig` нужен только одному проекту, вообще **не создавай второй `Directory.Build.targets`**.

Просто добавь Target прямо в `LogBootstrap.csproj`:

```xml
<Target Name="CopyNLogConfig" AfterTargets="Build">
    ...
</Target>
```

Это самый чистый и рекомендуемый вариант для настроек, относящихся только к одному проекту.

### Вопрос

Покажи, пожалуйста, содержимое **корневого** `Directory.Build.targets`. Интересно посмотреть, что именно в нем определено — тогда можно точно сказать, почему исчезли `InternalLoggerHelpers` и `LogVar`.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)