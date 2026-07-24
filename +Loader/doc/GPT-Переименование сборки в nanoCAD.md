# Переименование сборки в nanoCAD

**User:** dRz  
**Created:** 7/24/2026 7:16:23  
**Updated:** 7/24/2026 7:18:47  
**Exported:** 7/24/2026 7:19:18  
**Link:** [https://chatgpt.com/c/6a62e6e8-dbc4-83eb-8d5f-465a703085ad](https://chatgpt.com/c/6a62e6e8-dbc4-83eb-8d5f-465a703085ad)  

## Prompt:
24.07.2026, 07:16:21

если загрузить сборку в нанокад, переименовать а потом запросить полный путь сборки
что вернет?

## Response:
24.07.2026, 07:16:23

Если сборка **уже загружена в nanoCAD**, затем файл сборки на диске переименовать, а потом запросить полный путь, результат зависит от того, что именно запрашивать.

Например:

```csharp
Assembly asm = typeof(MyClass).Assembly;
string path = asm.Location;
```

или

```csharp
string path = typeof(MyClass).Assembly.CodeBase;
```

### `Assembly.Location`

После загрузки:

```
C:\Addons\SpecSPDS\SpecSPDS.Adapter.dll
```

Переименовали:

```
C:\Addons\SpecSPDS\SpecSPDS.Adapter.bak
```

Но сборка уже загружена в AppDomain.

`asm.Location` скорее всего вернет:

```
C:\Addons\SpecSPDS\SpecSPDS.Adapter.dll
```

То есть **старое имя**, потому что CLR хранит путь загрузки сборки, а не обращается каждый раз к файловой системе.

---

### Почему так?

При загрузке:

```csharp
Assembly.LoadFrom(@"C:\Addons\SpecSPDS\SpecSPDS.Adapter.dll")
```

CLR создает объект `Assembly`, внутри которого есть:

- имя сборки;
- версия;
- MVID;
- путь загрузки.

Дальше он не отслеживает переименование файла.

---

### Но есть нюансы

Если сборка загружена через `Assembly.Load(byte[])`:

```csharp
var bytes = File.ReadAllBytes(path);
Assembly.Load(bytes);
```

то:

```csharp
asm.Location
```

вернет:

```
""
```

потому что физического пути у сборки нет.

---

### Если использовать `Assembly.CodeBase`

Может вернуть:

```
file:///C:/Addons/SpecSPDS/SpecSPDS.Adapter.dll
```

Но это устаревший API (`[Obsolete]` в .NET 5+).

---

### Для твоего сценария с updater в nanoCAD

У тебя схема:

1. nanoCAD загрузил:

```
SpecSPDS.Adapter.dll
```

2. updater переименовывает:

```
SpecSPDS.Adapter.dll
→
SpecSPDS.Adapter.dll.bak
```

3. копирует новую:

```
SpecSPDS.Adapter.dll
```

Старая сборка в памяти продолжает работать.

Но:

```csharp
typeof(SomeClass).Assembly.Location
```

будет показывать **старое имя `.dll`**, даже если файла уже нет.

Это как раз удобно для поиска каталога:

```csharp
var dir = Path.GetDirectoryName(
    typeof(SomeClass).Assembly.Location);
```

Каталог останется правильным:

```
C:\Addons\SpecSPDS
```

---

Для updater я бы не использовал имя файла из `Assembly.Location` как источник истины. Лучше:

```csharp
var assemblyDir = Path.GetDirectoryName(
    typeof(AdapterEntryPoint).Assembly.Location);
```

а имя продукта/версии брать из:

```csharp
Assembly.GetName().Name
```

или из атрибутов:

```csharp
AssemblyProductAttribute
AssemblyInformationalVersionAttribute
```

Тогда переименование `.dll` в `.bak` не ломает определение каталога и продукта.

## Prompt:
24.07.2026, 07:18:40

addonInfo у loasder и адаптер отличаются, поэтому надо получать addonInfo 
  из concuren dict, ключ полный путь к сборке с именем

## Response:
24.07.2026, 07:18:41

Да, в твоей архитектуре это как раз правильнее. `AddonInfo` нельзя получать через `typeof(Loader).Assembly` или `typeof(Adapter).Assembly`, потому что это будут разные сборки и разные контексты.

Лучше сделать **реестр загруженных сборок** в loader:

```csharp
private static readonly ConcurrentDictionary<string, AddonInfo> _addons =
    new(StringComparer.OrdinalIgnoreCase);
```

Ключ:

```
FullPath + AssemblyName
```

Например:

```
C:\nanoCAD\Plugins\SpecSPDS\dRz.SpecSPDS.Adapter.dll|dRz.SpecSPDS.Adapter
```

---

### При загрузке адаптера

В loader:

```csharp
var assemblyPath = Path.GetFullPath(adapterFile);

var assembly = Assembly.LoadFrom(assemblyPath);

var key = CreateAssemblyKey(assembly);

_addons[key] = addonInfo;
```

Функция ключа:

```csharp
private static string CreateAssemblyKey(Assembly assembly)
{
    var location = assembly.Location;

    var name = assembly.GetName().Name;

    return $"{location}|{name}";
}
```

---

### В адаптере получить свой AddonInfo

Адаптер не должен знать про loader напрямую. Ему нужен контракт:

`dRz.Addon.Contracts.dll`

Например:

```csharp
public interface IAddonInfoProvider
{
    AddonInfo Get(Assembly assembly);
}
```

Loader реализует:

```csharp
public sealed class AddonInfoProvider : IAddonInfoProvider
{
    private readonly ConcurrentDictionary<string, AddonInfo> _items;

    public AddonInfoProvider(
        ConcurrentDictionary<string, AddonInfo> items)
    {
        _items = items;
    }

public AddonInfo Get(Assembly assembly)
    {
        var key = CreateKey(assembly);

        return _items.TryGetValue(key, out var info)
            ? info
            : null;
    }

private static string CreateKey(Assembly assembly)
    {
        return $"{assembly.Location}|{assembly.GetName().Name}";
    }
}
```

---

В адаптере:

```csharp
var info = addonInfoProvider.Get(
    typeof(AdapterEntryPoint).Assembly);
```

Получится именно:

```
SpecSPDS.Adapter.AddonInfo
```

а не:

```
Loader.AddonInfo
```

---

Но есть важный момент из предыдущего вопроса.

Если updater переименует:

```
Adapter.dll
↓
Adapter.dll.bak
```

то:

```csharp
assembly.Location
```

в уже загруженной сборке может остаться:

```
Adapter.dll
```

Поэтому ключ **нельзя пересчитывать позднее только по текущему файлу**.

Лучше при загрузке сразу сохранить:

```csharp
var record = new LoadedAddon
{
    Assembly = assembly,
    Path = assemblyPath,
    AddonInfo = addonInfo
};
```

и словарь:

```csharp
ConcurrentDictionary<string, LoadedAddon>
```

где ключ создается один раз.

Например:

```csharp
public sealed class LoadedAddon
{
    public string Path { get; init; }

    public Assembly Assembly { get; init; }

    public AddonInfo Info { get; init; }
}
```

Тогда даже после обновления файлов:

```
Adapter.dll
Adapter.dll.bak
```

у тебя остается корректная связь:

```
Assembly object
        ↓
LoadedAddon
        ↓
AddonInfo
```

Для nanoCAD loader это более надежная схема, потому что loader фактически является владельцем жизненного цикла адаптеров.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)