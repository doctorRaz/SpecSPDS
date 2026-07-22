# 7z в 64-битной системе

**User:** dRz  
**Created:** 7/18/2026 9:39:30  
**Updated:** 7/18/2026 10:49:42  
**Exported:** 7/19/2026 19:12:24  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b1f8c-4264-83ed-b167-5d9a74f5e29c](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b1f8c-4264-83ed-b167-5d9a74f5e29c)  

## Prompt:
18.07.2026, 09:39:29

7z 32x нормально работает и в 64 битной среде

## Response:
18.07.2026, 09:39:30

Да. **7z.exe x86 (32-bit)** без проблем работает в **64-битной Windows** через подсистему WOW64.

Для твоего обновлятора это означает:

- ✅ сможет архивировать и распаковывать файлы в 64-битной системе;
- ✅ сможет работать с DLL и EXE любой разрядности (архиватору безразлично содержимое файлов);
- ✅ не требует отдельной 64-битной версии.

Единственные отличия:

- **Производительность.** На больших архивах 64-битная версия обычно быстрее.
- **Память.** При использовании методов с большим словарем (LZMA2, 1–4 ГБ словаря и т.п.) 32-битная версия ограничена адресным пространством процесса (около 2 ГБ, иногда до 4 ГБ с LargeAddressAware). Для небольших архивов аддона это не имеет значения.

Для архива обновления размером, например, **10–100 МБ** разницы практически не будет.

**Для твоего случая** (обновление плагина nanoCAD) можно спокойно положить **32-битный `7z.exe`** рядом с обновлятором и использовать его на любых современных версиях Windows. Это даже немного упрощает распространение, так как один исполняемый файл подойдет и для x86, и для x64 систем.

## Prompt:
18.07.2026, 09:45:10

```
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
чтобы несколько файлов копировал за раз, для 7z.exe и dll
```

## Response:
18.07.2026, 09:45:11

Лучше использовать `ItemGroup`. Тогда можно копировать любое количество файлов одним `Copy`.

Например:

```xml
<PropertyGroup>
    <SharedConfigDir>d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\Shared\</SharedConfigDir>
</PropertyGroup>

<ItemGroup>
    <SharedFiles Include="$(SharedConfigDir)\Single_var.$(Configuration).config">
        <TargetName>NLog.dll.nlog</TargetName>
    </SharedFiles>

    <SharedFiles Include="$(SharedConfigDir)\7z.exe">
        <TargetName>7z.exe</TargetName>
    </SharedFiles>

    <SharedFiles Include="$(SharedConfigDir)\7z.dll">
        <TargetName>7z.dll</TargetName>
    </SharedFiles>
</ItemGroup>

<Target Name="CopySharedFiles"
        AfterTargets="Build">

    <Copy
        SourceFiles="@(SharedFiles)"
        DestinationFiles="@(SharedFiles->'$(OutDir)%(TargetName)')"
        SkipUnchangedFiles="true"
        Condition="Exists('%(SharedFiles.Identity)')" />

</Target>
```

### Преимущества

- легко добавлять новые файлы;
- можно переименовывать при копировании (`TargetName`);
- один `Copy` вместо нескольких.

Чтобы добавить еще файл, достаточно одной строки:

```xml
<SharedFiles Include="$(SharedConfigDir)\7z.dll">
    <TargetName>7z.dll</TargetName>
</SharedFiles>
```

или даже переименовать:

```xml
<SharedFiles Include="$(SharedConfigDir)\7z.exe">
    <TargetName>7zip.exe</TargetName>
</SharedFiles>
```

Такой вариант считается наиболее "msbuild-ным" и хорошо масштабируется.

## Prompt:
18.07.2026, 09:46:07

если файлов много?

## Response:
18.07.2026, 09:46:07

Если файлов действительно много, то обычно используют не перечисление каждого файла, а маски (`Include`) или рекурсивные маски (`**\*`).

Например, если нужно скопировать все содержимое папки `Shared`:

```xml
<PropertyGroup>
    <SharedConfigDir>d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\Shared</SharedConfigDir>
</PropertyGroup>

<ItemGroup>
    <SharedFiles Include="$(SharedConfigDir)\**\*.*"/>
</ItemGroup>

<Target Name="CopySharedFiles"
        AfterTargets="Build">

    <Copy
        SourceFiles="@(SharedFiles)"
        DestinationFiles="@(SharedFiles->'$(OutDir)%(RecursiveDir)%(Filename)%(Extension)')"
        SkipUnchangedFiles="true" />

</Target>
```

Если нужно копировать только определенные типы файлов:

```xml
<ItemGroup>
    <SharedFiles Include="$(SharedConfigDir)\*.exe"/>
    <SharedFiles Include="$(SharedConfigDir)\*.dll"/>
    <SharedFiles Include="$(SharedConfigDir)\*.config"/>
</ItemGroup>
```

или конкретные имена через `;`:

```xml
<ItemGroup>
    <SharedFiles Include="
        $(SharedConfigDir)\7z.exe;
        $(SharedConfigDir)\7z.dll;
        $(SharedConfigDir)\License.txt;
        $(SharedConfigDir)\ReadMe.md"/>
</ItemGroup>
```

### Для твоего проекта

Я бы вообще сделал отдельную папку, например:

```
Shared
│
├── BuildFiles
│   ├── 7z.exe
│   ├── 7z.dll
│   ├── License.txt
│   ├── ...
│
├── Single_var.Debug.config
└── Single_var.Release.config
```

Тогда:

```xml
<ItemGroup>
    <SharedFiles Include="$(SharedConfigDir)\BuildFiles\**\*.*"/>
</ItemGroup>
```

и все утилиты (`7z.exe`, DLL, лицензии и т.п.) будут автоматически попадать в выходной каталог без изменения `.csproj`. Добавил файл в `BuildFiles` — он начнет копироваться при следующей сборке. Это наиболее удобный вариант, когда количество файлов со временем растет.

## Prompt:
18.07.2026, 09:56:07

SkipUnchangedFiles="true что значит

## Response:
18.07.2026, 09:56:07

`SkipUnchangedFiles="true"` означает:

> **Не копировать файл, если он не изменился.**

MSBuild сравнивает исходный и целевой файлы (по времени изменения и размеру). Если считает, что они одинаковые, операция копирования пропускается.

Например:

```
Shared\7z.exe      10:15
bin\Debug\7z.exe   10:15
```

При следующей сборке файл не будет переписан.

Если же ты заменил `Shared\7z.exe`:

```
Shared\7z.exe      11:30
bin\Debug\7z.exe   10:15
```

то `Copy` выполнится и обновит файл.

### Зачем это нужно

- ускоряет сборку;
- не изменяет дату модификации файлов без необходимости;
- уменьшает количество операций записи на диск.

### Если указать `false` или не указывать

```xml
<Copy ... />
```

или

```xml
<Copy ... SkipUnchangedFiles="false" />
```

то файл будет копироваться **при каждой сборке**, даже если он не изменился.

**Для твоего проекта `SkipUnchangedFiles="true"` — правильный выбор.**



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)