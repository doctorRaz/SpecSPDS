# Прогресс бар nanoCAD

**User:** dRz  
**Created:** 7/18/2026 11:44:19  
**Updated:** 7/18/2026 11:47:32  
**Exported:** 7/19/2026 19:11:45  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b3cb6-d504-83eb-8ec6-e29fe6d3432c](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b3cb6-d504-83eb-8ec6-e29fe6d3432c)  

## Prompt:
18.07.2026, 11:44:19

Прогресс бар в командной строке нанокад при скачивании обновления
Вида 
Скачиваю 10%...

## Response:
18.07.2026, 11:44:19

Для командной строки nanoCAD лучше сделать не настоящий `ProgressBar`, а обновляемую строку через `\r` (возврат каретки). Тогда в командной строке будет:

```
Скачиваю 10%...
Скачиваю 25%...
Скачиваю 73%...
Скачивание завершено
```

без засорения истории команд.

Например:

```csharp
public class DownloadProgressReporter
{
    public void Report(long received, long total)
    {
        if (total <= 0)
            return;

        int percent = (int)(received * 100 / total);

        Console.Write($"\rСкачиваю {percent}%...");
    }

    public void Complete()
    {
        Console.WriteLine("\rСкачивание завершено      ");
    }
}
```

Использование с `HttpClient`:

```csharp
using var response = await client.GetAsync(
    url,
    HttpCompletionOption.ResponseHeadersRead);

response.EnsureSuccessStatusCode();

long total = response.Content.Headers.ContentLength ?? -1;

using var stream = await response.Content.ReadAsStreamAsync();
using var file = File.Create(targetFile);

byte[] buffer = new byte[81920];
long received = 0;

int read;

while ((read = await stream.ReadAsync(buffer)) > 0)
{
    await file.WriteAsync(buffer.AsMemory(0, read));

    received += read;

    progress.Report(received, total);
}

progress.Complete();
```

Но для nanoCAD есть нюанс: `Console.Write` обычно не связан с командной строкой CAD. Если это команда nanoCAD (`CommandMethod`), то правильнее выводить через API редактора:

```csharp
doc.Editor.WriteMessage(
    $"\nСкачиваю {percent}%...");
```

Однако `WriteMessage` каждую секунду будет создавать много строк. Лучше использовать собственный `IProgress<int>` и обновлять редко:

```csharp
if (percent != lastPercent)
{
    lastPercent = percent;
    doc.Editor.WriteMessage(
        $"\rСкачиваю {percent}%...");
}
```

Но многие CAD API не поддерживают настоящий `\r` в командной строке. Тогда практичнее:

```
Скачивание обновления:
[==========          ] 50%
```

одной командой в начале и потом только проценты:

```
Скачивание обновления: 50%
```

Для твоего Updater я бы сделал интерфейс:

```csharp
public interface IProgressReporter
{
    void Report(int percent, string message);
}
```

и две реализации:

- `CommandLineProgressReporter` — для nanoCAD Editor
- `ConsoleProgressReporter` — для отладки

Тогда загрузчик архива вообще не будет знать, куда выводить прогресс.

## Prompt:
18.07.2026, 11:47:28

У нанокад есть свой прогрессбар,

## Response:
18.07.2026, 11:47:28

Да, у nanoCAD есть штатный прогресс-бар в статусной строке/интерфейсе. Для задач типа скачивания обновления он как раз подходит лучше, чем вывод в командную строку.

В MultiCAD API обычно используется `ProgressMeter` (аналогичный AutoCAD API):

```csharp
using Teigha.Runtime; // или соответствующий namespace nanoCAD

using (var progress = new ProgressMeter())
{
    progress.Start("Скачивание обновления...");

    progress.SetLimit(100);

    for (int i = 0; i <= 100; i++)
    {
        progress.MeterProgress();
        Thread.Sleep(50);
    }

    progress.Stop();
}
```

Идея такая:

```text
Скачивание обновления...
[██████████        ] 50%
```

Для реальной загрузки лучше привязать его к количеству байт:

```csharp
long totalBytes = response.Content.Headers.ContentLength ?? 0;
long downloaded = 0;

progress.SetLimit((int)totalBytes);

while ((read = await stream.ReadAsync(buffer)) > 0)
{
    await file.WriteAsync(buffer.AsMemory(0, read));

    downloaded += read;

    progress.MeterProgress(
        (int)downloaded);
}
```

Но надо учитывать: в nanoCAD `ProgressMeter` обычно рассчитан на синхронные длительные операции в основном потоке. А скачивание через `async/await` у тебя будет идти в фоне. Поэтому для Updater я бы сделал адаптер:

```csharp
public interface IProgressReporter
{
    void Start(string text, long total);
    void Report(long current);
    void Stop();
}
```

и реализацию:

```csharp
public class NanoCadProgressReporter : IProgressReporter
{
    private ProgressMeter? _meter;

    public void Start(string text, long total)
    {
        _meter = new ProgressMeter();
        _meter.Start(text);
        _meter.SetLimit((int)total);
    }

    public void Report(long current)
    {
        _meter?.MeterProgress((int)current);
    }

    public void Stop()
    {
        _meter?.Stop();
        _meter = null;
    }
}
```

Тогда твой `Updater` останется независимым от nanoCAD.

Ещё вариант — использовать штатный `Application.SetSystemVariable("MODEMACRO", ...)` для строки состояния, но для скачивания архива `ProgressMeter` выглядит правильнее.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)