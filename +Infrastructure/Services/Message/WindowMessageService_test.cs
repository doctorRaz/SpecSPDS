using drz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace drz.Infrastructure.Services;

/// <summary>
/// Реализация сервиса сообщений на основе WinAPI MessageBoxW.
/// </summary>
public sealed class WindowMessageService_test : IMessageService, IWindowMessageService
{
    private readonly IntPtr _owner;

    /// <summary>
    /// Создает сервис сообщений.
    /// </summary>
    /// <param name="owner">
    /// Дескриптор родительского окна.
    /// Для nanoCAD рекомендуется передавать HWND главного окна.
    /// </param>
    public WindowMessageService_test(IntPtr owner)
    {
        _owner = owner;
    }

    public MessageResult AskAbortRetryIgnore(string message, string title, [CallerMemberName] string caller = null)
    {
        throw new NotImplementedException();
        //В зависимости от контекста ошибки выберите один из двух вариантов:🔴 Error / Hand / Stop(Красный крест) — Основной вариант. Указывает на то, что произошла серьезная ошибка, которая полностью блокирует дальнейшее выполнение текущего процесса(например, Файл поврежден или Диск переполнен).⚠️ Warning(Желтый треугольник) — Альтернативный вариант. Используется, если сбой не критичен для всего приложения, но требует выбора пользователя для продолжения конкретной операции.
    }

    public MessageResult AskYesNo(string text, string? caption = null, [CallerMemberName] string caller = null)
    {
        return Show(
            text,
            caption,
            MessageButtons.YesNo,
            MessageIcon.Question);
        //В зависимости от того, насколько опасно действие, выбирается одна из двух иконок:❓ Question (Синий знак вопроса) — Для безопасных действий. Подходит для рядовых развилок, которые не ломают систему и не удаляют данные безвозвратно.Пример: «Хотите ли вы открыть лог-файл?», «Включить темную тему?»⚠️ Warning (Желтый треугольник) — Для опасных / деструктивных действий. Обязателен, если действие приведет к потере данных, удалению или выходу из системы.Пример: «Вы уверены, что хотите очистить корзину?», «Удалить аккаунт?»

    }

    public MessageResult AskRetryCancel(string message, string title, [CallerMemberName] string caller = null)
    {
        throw new NotImplementedException();
        //Иконка: Warning (Предупреждение) или Error (если операция полностью заблокирована до исправления).Смысл: «Не удалось подключиться к серверу. Попробовать еще раз?»
    }

    public MessageResult AskOkCancel(string message, string title, [CallerMemberName] string caller = null)
    {
        throw new NotImplementedException();
        //ℹ️ Information / Asterisk (Синий кружок с буквой «i») — Основной вариант. Отлично подходит для стандартных операций, чтобы подчеркнуть, что происходит штатный процесс (например, экспорт данных или отправка формы).Примеры: «Сгенерированный файл будет сохранен в папку Загрузки. Продолжить?», «Будет произведена отправка 5 писем».❓ Question (Синий знак вопроса) — Альтернативный вариант. Используется, если операция подразумевает выбор пути, но не несет рисков для данных.Примеры: «Установить обновления сейчас?», «Запустить сканирование системы?»
    }

    public MessageResult AskYesNoCancel(string message, string title, [CallerMemberName] string caller = null)
    {
        throw new NotImplementedException();
        //В 95% случаев для этого метода используется одна конкретная иконка:⚠️ Warning (Желтый треугольник с восклицательным знаком) — Основной вариант. Сигнализирует о том, что если пользователь закроет окно без сохранения (выберет «Нет»), данные будут безвозвратно утеряны.❓ Question (Синий знак вопроса) — Альтернативный вариант. Используется редко, только если закрытие окна или переход не влекут за собой потерю критически важных данных.
    }

    public void ErrorMessage(Exception ex, [CallerMemberName] string caller = null)
    {
        //Show(text, caption, MessageButtons.Ok, MessageIcon.Error);
        throw new NotImplementedException();
    }

    public void ErrorMessage(string message ,Exception ex = null, [CallerMemberName] string caller = null)
    {
        throw new NotImplementedException();
    }

    public void InfoMessage(string text, [CallerMemberName] string? caption = null)
    {
        Show(text, caption, MessageButtons.Ok, MessageIcon.Information);
    }

    public MessageResult Show(
                            string text,
        string? caption = null,
        MessageButtons buttons = MessageButtons.Ok,
        MessageIcon icon = MessageIcon.None)
    {
        int result = MessageBoxW(
            _owner,
            text,
            caption ?? string.Empty,
            (uint)buttons | (uint)icon);

        return result switch
        {
            1 => MessageResult.Ok,
            2 => MessageResult.Cancel,
            3 => MessageResult.Abort,
            4 => MessageResult.Retry,
            5 => MessageResult.Ignore,
            6 => MessageResult.Yes,
            7 => MessageResult.No,
            _ => MessageResult.None
        };
    }

    public void WarningMessage(string text, [CallerMemberName] string? caption = null)
    {
        Show(text, caption, MessageButtons.Ok, MessageIcon.Warning);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(
        IntPtr hWnd,
        string lpText,
        string lpCaption,
        uint uType);
}