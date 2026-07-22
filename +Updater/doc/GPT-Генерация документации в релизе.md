# Генерация документации в релизе

**User:** dRz  
**Created:** 7/19/2026 11:25:18  
**Updated:** 7/19/2026 11:25:51  
**Exported:** 7/19/2026 19:12:31  
**Link:** [https://chatgpt.com/c/6a5c89d0-86f8-83ed-951c-0aea103eb7b6](https://chatgpt.com/c/6a5c89d0-86f8-83ed-951c-0aea103eb7b6)  

## Prompt:
19.07.2026, 11:25:18

```
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
	<GenerateDocumentationFile>True</GenerateDocumentationFile>
</PropertyGroup>
```

 чтобы документация генерировалась для всех релизов не только anycpu

## Response:
19.07.2026, 11:25:18

Да. Не привязывайся к `Platform`, если это не требуется.

Достаточно проверять только конфигурацию:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

или даже так (если нужно для всех конфигураций, кроме Debug):

```xml
<PropertyGroup Condition="'$(Configuration)' != 'Debug'">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Если у тебя несколько конфигураций (`Release`, `ReleaseNC26`, `ReleaseSigned` и т.п.), то можно использовать проверку начала строки:

```xml
<PropertyGroup Condition="$([System.String]::Copy('$(Configuration)').StartsWith('Release'))">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Это сработает для:

- `Release`
- `Release|x64`
- `ReleaseSigned`
- `ReleaseNC26`

и любых других конфигураций, начинающихся с `Release`, независимо от платформы (`AnyCPU`, `x64`, `x86` и т.д.).

Для обычных проектов наиболее универсальный вариант — первый:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Он будет работать для любой платформы (`AnyCPU`, `x64`, `x86`), если имя конфигурации — `Release`.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)