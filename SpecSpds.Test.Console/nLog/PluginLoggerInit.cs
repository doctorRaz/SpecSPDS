using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System.IO;

/// <summary>
/// Загрузка пути напрямую в target 
/// </summary>
public static class PluginLoggerInit
{
    private static bool _initialized;

    public static void Init(string pluginAssemblyPath)
    {
        if (_initialized)
            return;

        _initialized = true;

        var config = LogManager.LoadConfiguration("NLog.config");

        var fileTarget = (FileTarget)config.Configuration.FindTargetByName("pluginFile");

        // Каталог логов рядом с DLL плагина
        string pluginDir = Path.GetDirectoryName(pluginAssemblyPath);
        string logDir = Path.Combine(pluginDir, "logs");

        Directory.CreateDirectory(logDir);

        // Часть имени файла — имя плагина
        string namePart = Path.GetFileNameWithoutExtension(pluginAssemblyPath);

        fileTarget.FileName = SimpleLayout.FromString(
            $@"{logDir}\${{shortdate}}_{namePart}.log"
        );

        LogManager.ReconfigExistingLoggers();
    }
}