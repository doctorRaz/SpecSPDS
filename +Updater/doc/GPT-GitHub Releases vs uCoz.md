# GitHub Releases vs uCoz

**User:** dRz  
**Created:** 7/17/2026 16:59:43  
**Updated:** 7/18/2026 10:49:44  
**Exported:** 7/19/2026 19:12:19  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5a3533-291c-83eb-a270-faef868608e9](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5a3533-291c-83eb-a270-faef868608e9)  

## Prompt:
17.07.2026, 16:59:43

Скачивание обновления
Есть возможность закачивать по fttp и скачивать без пароля по http, на ucoz.ru
Либо выкладывать на гитхаб в релизы.
Рядом с архивом лежит файлик с номером версии, именем архива, чейнджлогом.
Скачивается текстовый файл, если в нем версия новее, то качаем архив, дальше все сделано.
Как рациональнее ?

## Response:
17.07.2026, 16:59:43

Я бы выбрал **GitHub Releases**. Для твоего сценария это проще, надежнее и масштабируемее.

### Сравнение

| Критерий | uCoz (FTP + HTTP) | GitHub Releases |
|---------|--------------------|-----------------|
| Загрузка новой версии | FTP | Через веб или git |
| Скачивание | HTTP | HTTPS |
| Надежность | Зависит от хостинга | Очень высокая |
| Ограничение скорости | Может быть | Практически нет |
| История версий | Нужно делать самому | Уже есть |
| Возможность отката | Самому | Любой релиз |
| Стоимость | Есть бесплатный тариф | Бесплатно |

### Я бы сделал так

В репозитории Releases каждый релиз содержит:

```
SpecSPDS.zip
update.json
```

или

```
SpecSPDS.zip
update.txt
```

Клиент сначала скачивает только маленький файл.

Например:

```json
{
  "version": "3.8.15",
  "archive": "SpecSPDS.zip",
  "sha256": "6D5A...",
  "size": 2541234,
  "changes": [
    "Исправлена ошибка...",
    "Добавлено...",
    "Оптимизация..."
  ]
}
```

Алгоритм:

1. Скачать `update.json`.
2. Сравнить версию.
3. Если новее:
   * показать список изменений;
   * скачать архив;
   * проверить SHA-256;
   * распаковать;
   * удалить временный архив.

### Почему JSON лучше txt

TXT тоже подойдет:

```
Version=3.8.15
Archive=SpecSPDS.zip
Hash=...
```

Но через год захочется добавить:

- минимальную поддерживаемую версию;
- обязательность обновления;
- URL;
- размер;
- дату;
- подпись.

JSON расширяется без изменения формата.

### Если обновлений несколько

Еще лучше сделать один файл:

```
latest.json
```

В нем:

```json
{
  "latest": "3.8.15",
  "url": ".../SpecSPDS.zip",
  "sha256": "...",
  "published": "2026-07-17",
  "mandatory": false,
  "changes": [
    ...
  ]
}
```

Тогда клиент вообще ничего не знает о структуре сайта.

### Что бы я не делал

Не использовал бы FTP из программы. FTP нужен только тебе для публикации. Клиенту достаточно HTTPS.

---

Для твоего загрузчика оптимальная схема выглядит так:

```
Старт
    ↓
скачать latest.json
    ↓
версия новее?
    ↓ нет
завершить
    ↓ да
скачать архив
    ↓
проверить SHA-256
    ↓
распаковать
    ↓
обновление готово
```

Такая архитектура остается простой, а при необходимости легко дополняется цифровой подписью, несколькими каналами обновлений (Stable/Beta) или резервными зеркалами без изменения логики клиента.

## Prompt:
17.07.2026, 17:02:35

Для добавляемых мной врелиз архивов гитхаб сам создает sha?

## Response:
17.07.2026, 17:02:35

Нет. GitHub Releases **не вычисляет и не публикует SHA-256** для прикрепленных файлов.

Есть несколько вариантов:

1. **Самый простой** — при сборке вычислять SHA-256 архива и записывать его в `latest.json` или `update.json`. Это лучший вариант.

2. **Публиковать отдельный файл**, например:
   ```
   SpecSPDS.zip
   SpecSPDS.zip.sha256
   ```
   Многие проекты делают именно так.

3. **Не проверять хеш**. Если архив скачивается по HTTPS с GitHub, то при передаче он защищен от изменения в пути. Но хеш все равно полезен:
   - для проверки целостности после скачивания;
   - для обнаружения поврежденной загрузки;
   - как основа для дальнейшей проверки подписи.

Для твоего автообновлятора я бы рекомендовал **первый вариант**: после сборки создать ZIP, вычислить его SHA-256, записать в `latest.json`, а затем загрузить оба файла (`SpecSPDS.zip` и `latest.json`) в релиз. Это полностью автоматизируется и не требует дополнительных файлов.

## Prompt:
17.07.2026, 17:04:01

Как скачивать с гитхаб латест версию?

## Response:
17.07.2026, 17:04:01

Есть два распространенных варианта.

### Вариант 1. Через `latest/download` (если всегда одинаковые имена файлов)

Если в каждом релизе архив называется одинаково, например `SpecSPDS.zip`, а файл версии — `latest.json`, то GitHub предоставляет постоянные ссылки:

```
https://github.com/<owner>/<repo>/releases/latest/download/latest.json
```

и

```
https://github.com/<owner>/<repo>/releases/latest/download/SpecSPDS.zip
```

Например:

```
https://github.com/drz/SpecSPDS/releases/latest/download/latest.json
```

Тогда алгоритм очень простой:

1. Скачать `latest.json`.
2. Если версия новее — скачать `SpecSPDS.zip` по ссылке `.../latest/download/...`.

Это самый простой вариант.

---

### Вариант 2. Через GitHub API

Можно запросить:

```
https://api.github.com/repos/<owner>/<repo>/releases/latest
```

В ответ придет JSON с номером версии, описанием релиза и ссылками на все вложения.

Плюсы:
- не нужно знать имя архива заранее;
- можно получить changelog прямо из описания релиза.

Минусы:
- немного сложнее код.

---

### Что я бы сделал

Для твоего обновлятора достаточно первого варианта.

В каждом релизе всегда выкладывать:

```
latest.json
SpecSPDS.zip
```

Код всегда обращается к двум постоянным URL через `releases/latest/download/...`, и независимо от того, вышла версия 1.2.0 или 5.0.17, GitHub сам перенаправит на файлы последнего релиза.

Это максимально простой и надежный способ.

## Prompt:
17.07.2026, 17:05:42

Какой коммент http использовать?
Напиши код подключения

## Response:
17.07.2026, 17:05:42

Для .NET (Framework 4.6.2+, .NET 6+) лучше использовать **`HttpClient`**. Он современный, поддерживается Microsoft и хорошо работает с GitHub.

Я бы сделал один общий экземпляр:

```csharp
private static readonly HttpClient Http = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

static UpdateService()
{
    Http.DefaultRequestHeaders.UserAgent.ParseAdd(
        "SpecSPDS-Updater/1.0");
}
```

GitHub рекомендует указывать `User-Agent`.

### Скачать текстовый файл

```csharp
string text = await Http.GetStringAsync(
    "https://github.com/<owner>/<repo>/releases/latest/download/latest.json");
```

### Скачать архив

```csharp
await using var stream = await Http.GetStreamAsync(url);

await using var file = File.Create(zipFileName);

await stream.CopyToAsync(file);
```

Или еще проще:

```csharp
byte[] data = await Http.GetByteArrayAsync(url);
await File.WriteAllBytesAsync(zipFileName, data);
```

Но первый вариант лучше — он не загружает весь архив в память.

### Если нужен синхронный код

```csharp
string text = Http.GetStringAsync(url)
                  .GetAwaiter()
                  .GetResult();
```

и

```csharp
using var response = Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                         .GetAwaiter()
                         .GetResult();

response.EnsureSuccessStatusCode();

using var stream = response.Content.ReadAsStream();
using var file = File.Create(zipFileName);

stream.CopyTo(file);
```

Для обновлятора я бы использовал именно потоковое скачивание (`ResponseHeadersRead` + `CopyTo`): оно почти не расходует память и подходит даже для больших архивов.

## Prompt:
17.07.2026, 17:08:51

Напиши полный класс

## Response:
17.07.2026, 17:08:51

Я бы сделал отдельный класс без привязки к GitHub API. Он умеет скачивать любой текстовый файл и любой файл по URL, поэтому подойдет и для GitHub Releases, и для uCoz.

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SpecSPDS.Updater
{
    internal static class HttpDownloader
    {
        private static readonly HttpClient Client;

        static HttpDownloader()
        {
            Client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            Client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "SpecSPDS-Updater/1.0");
        }

        /// <summary>
        /// Скачать текстовый файл.
        /// </summary>
        public static Task<string> DownloadTextAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            return Client.GetStringAsync(url, cancellationToken);
        }

        /// <summary>
        /// Скачать файл.
        /// </summary>
        public static async Task DownloadFileAsync(
            string url,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(fileName)!);

            using var response = await Client.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var httpStream =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            await using var fileStream = new FileStream(
                fileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                true);

            await httpStream.CopyToAsync(fileStream, cancellationToken);
        }
    }
}
```

Использование:

```csharp
string info = await HttpDownloader.DownloadTextAsync(
    "https://github.com/<owner>/<repo>/releases/latest/download/latest.json");
```

```csharp
await HttpDownloader.DownloadFileAsync(
    "https://github.com/<owner>/<repo>/releases/latest/download/SpecSPDS.zip",
    @"C:\Temp\SpecSPDS.zip");
```

Если обновлятор у тебя работает под **.NET Framework 4.6.2**, имеет смысл добавить поддержку **прогресса скачивания** (`IProgress<double>`), чтобы можно было показывать пользователю процент загрузки архива. Это небольшое изменение, но оно делает класс более удобным для реального использования.

## Prompt:
17.07.2026, 17:10:01

Для net прогресс не получится?

## Response:
17.07.2026, 17:10:01

Получится. В **.NET Framework 4.6.2** тоже можно показывать прогресс. Просто `HttpClient` сам его не предоставляет — нужно читать поток вручную.

Пример метода:

```csharp
public static async Task DownloadFileAsync(
    string url,
    string fileName,
    IProgress<double>? progress = null,
    CancellationToken cancellationToken = default)
{
    using (var response = await Client.GetAsync(
        url,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken))
    {
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;

        using (var input = await response.Content.ReadAsStreamAsync())
        using (var output = File.Create(fileName))
        {
            var buffer = new byte[81920];
            long totalRead = 0;

            while (true)
            {
                int read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (read == 0)
                    break;

                await output.WriteAsync(buffer, 0, read, cancellationToken);

                totalRead += read;

                if (totalBytes.HasValue)
                    progress?.Report((double)totalRead / totalBytes.Value * 100.0);
            }
        }
    }
}
```

Использование:

```csharp
var progress = new Progress<double>(p =>
{
    Console.WriteLine($"{p:F1}%");
});

await HttpDownloader.DownloadFileAsync(url, fileName, progress);
```

Или в WinForms/WPF:

```csharp
var progress = new Progress<double>(p =>
{
    progressBar.Value = (int)p;
});
```

`Progress<T>` автоматически вызывает обработчик в UI-потоке, поэтому можно сразу обновлять `ProgressBar`.

Так что для .NET Framework 4.6.2 никаких ограничений здесь нет.

## Prompt:
17.07.2026, 17:25:51

В каде асинхронное скачивание получится?

## Response:
17.07.2026, 17:25:51

Да, получится, но есть нюансы.

Само **скачивание** через `HttpClient` можно выполнять асинхронно без проблем. Проблемы возникают только при обращении к API CAD из фонового потока.

Правильная схема:

1. Из команды nanoCAD/AutoCAD запускаешь `async`-метод.
2. Скачивание идет в фоне (`await HttpClient...`).
3. Во время скачивания **не обращаешься к API CAD**.
4. После `await` возвращаешься в исходный поток (если он имеет `SynchronizationContext`) или уже после завершения скачивания выполняешь действия с CAD.

Например:

```csharp
public async Task CheckUpdatesAsync()
{
    string json = await HttpDownloader.DownloadTextAsync(url);

    // Разбор JSON

    await HttpDownloader.DownloadFileAsync(zipUrl, zipPath);

    // Здесь уже можно показать сообщение пользователю
}
```

### Но есть особенность

В **nanoCAD и AutoCAD** часто **нет `SynchronizationContext`**, поэтому после `await` продолжение может выполниться в другом потоке. Из-за этого **нельзя гарантировать**, что после `await` безопасно вызывать API CAD.

Поэтому многие разработчики плагинов делают обновление **синхронным**:

```csharp
HttpDownloader.DownloadFileAsync(...)
              .GetAwaiter()
              .GetResult();
```

или запускают асинхронную задачу, а результат передают обратно в CAD через свой механизм.

### Для твоего обновлятора

Поскольку проверка обновлений выполняется редко (при старте или по команде), я бы сделал так:

- проверка версии — синхронно (занимает доли секунды);
- скачивание архива — синхронно, но с отображением прогресса;
- затем сразу распаковка.

Так код будет проще и не возникнет проблем с потоками и API nanoCAD. Если позже понадобится полностью неблокирующий интерфейс, архитектуру всегда можно расширить.

## Prompt:
17.07.2026, 17:27:13

Перепиши класс для синхронного скачивания

## Response:
17.07.2026, 17:27:14

Для твоего обновлятора я бы действительно сделал синхронный вариант. Код получается проще, не требует `async/await` и хорошо подходит для запуска из команды nanoCAD.

```csharp
using System;
using System.IO;
using System.Net.Http;

namespace SpecSPDS.Updater
{
    internal static class HttpDownloader
    {
        private static readonly HttpClient Client;

        static HttpDownloader()
        {
            Client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            Client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "SpecSPDS-Updater/1.0");
        }

        /// <summary>
        /// Скачать текстовый файл.
        /// </summary>
        public static string DownloadText(string url)
        {
            using (var response = Client.GetAsync(url).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();

                return response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();
            }
        }

        /// <summary>
        /// Скачать файл.
        /// </summary>
        public static void DownloadFile(
            string url,
            string fileName,
            IProgress<double>? progress = null)
        {
            string? directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using (var response = Client.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead)
                .GetAwaiter()
                .GetResult())
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                using (var input = response.Content
                    .ReadAsStreamAsync()
                    .GetAwaiter()
                    .GetResult())
                using (var output = new FileStream(
                    fileName,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    byte[] buffer = new byte[81920];
                    long totalRead = 0;

                    while (true)
                    {
                        int read = input.Read(buffer, 0, buffer.Length);

                        if (read == 0)
                            break;

                        output.Write(buffer, 0, read);

                        totalRead += read;

                        if (totalBytes.HasValue)
                            progress?.Report(
                                totalRead * 100.0 / totalBytes.Value);
                    }
                }
            }
        }
    }
}
```

Использование:

```csharp
string json = HttpDownloader.DownloadText(url);
```

```csharp
HttpDownloader.DownloadFile(
    zipUrl,
    zipFile,
    new Progress<double>(p =>
    {
        Console.WriteLine($"{p:F1}%");
    }));
```

Единственное, что я бы еще добавил — поддержку **повторной попытки скачивания** (например, 3 попытки с паузой 1–2 секунды). Для обновляторов это часто полезнее, чем асинхронность, поскольку временные сетевые сбои встречаются чаще, чем реальные ошибки сервера.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)