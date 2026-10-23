// Сбор сведений о командах CAD и формирование диагностического отчёта.
using dRz.Abstractions.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

#if NC
using Rtm= Teigha.Runtime;
#elif AC
using Autodesk.AutoCAD.Runtime;
#endif

namespace dRz.NC.Services
{
    /// <summary>
    /// Собирает сведения о командах CAD и формирует отчёт о дубликатах.
    /// </summary>
    public sealed class CmdInfo
    {
        private readonly Assembly _assembly;
        private readonly bool _includeMethodInfo;
        private readonly IDrzLogger? _logger;

        /// <summary>
        /// Объявления команд, сгруппированные по глобальному имени команды.
        /// Для каждого CommandMethodAttribute создаётся отдельная запись.
        /// </summary>
        public Dictionary<string, List<CommandEntry>> MapInfo { get; } = new();

        /// <summary>Текст списка команд без дубликатов.</summary>
        public string CommandText { get; private set; } = string.Empty;

        /// <summary>Текст отчёта о дубликатах команд.</summary>
        public string DuplicateText { get; private set; } = string.Empty;

        /// <summary>
        /// Описание одного объявления команды.
        /// </summary>
        public sealed class CommandEntry
        {
            /// <summary>Глобальное имя команды из CommandMethodAttribute.</summary>
            public string CommandName { get; init; } = string.Empty;

            /// <summary>Полное имя типа и имя объявившего команду метода.</summary>
            public string MethodName { get; init; } = string.Empty;

            /// <summary>Описание метода; пустая строка, если DescriptionAttribute отсутствует.</summary>
            public string Description { get; init; } = string.Empty;
        }

        /// <summary>
        /// Создаёт отчёт по сборке, содержащей этот класс.
        /// </summary>
        /// <param name="includeMethodInfo">Включать ли имена методов в список команд.</param>
        public CmdInfo(bool includeMethodInfo = false)
            : this(Assembly.GetExecutingAssembly(), includeMethodInfo)
        {
        }

        /// <summary>
        /// Создаёт отчёт по указанной сборке.
        /// </summary>
        /// <param name="assembly">Сборка для анализа.</param>
        /// <param name="includeMethodInfo">Включать ли имена методов в список команд.</param>
        /// <param name="logger">Логгер для ошибок рефлексии.</param>
        public CmdInfo(Assembly assembly, bool includeMethodInfo = false, IDrzLogger? logger = null)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _includeMethodInfo = includeMethodInfo;
            _logger = logger;

            CollectCommands();
            BuildReport();
        }

        /// <summary>
        /// Собирает объявления команд. Ошибки отдельного типа или метода не прерывают весь анализ.
        /// </summary>
        private void CollectCommands()
        {
            Type[] types;

            try
            {
                types = _assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Используем доступные типы, даже если часть типов сборки не загрузилась.
                var availableTypes = new List<Type>(ex.Types.Length);
                foreach (Type? type in ex.Types)
                {
                    if (type != null)
                    {
                        availableTypes.Add(type);
                    }
                }

                types = availableTypes.ToArray();
                LogError(ex, $"Не все типы сборки '{_assembly.FullName}' удалось загрузить.");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Не удалось получить типы сборки '{_assembly.FullName}'.");
                return;
            }

            foreach (Type type in types)
            {
                if (type == null)
                {
                    continue;
                }

                MethodInfo[] methods;
                try
                {
                    methods = type.GetMethods();
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Не удалось получить методы типа '{type.FullName}'.");
                    continue;
                }

                foreach (MethodInfo method in methods)
                {
                    CollectMethodCommands(method);
                }
            }
        }

        /// <summary>
        /// Добавляет отдельную запись для каждого CommandMethodAttribute метода.
        /// </summary>
        private void CollectMethodCommands(MethodInfo method)
        {
            object[] attributes;
            try
            {
                attributes = method.GetCustomAttributes(true);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Не удалось прочитать атрибуты метода '{GetMethodName(method)}'.");
                return;
            }

            string description = string.Empty;
            foreach (object attribute in attributes)
            {
                if (attribute is DescriptionAttribute descriptionAttribute)
                {
                    description = descriptionAttribute.Description ?? string.Empty;
                    break;
                }
            }

            string methodName = GetMethodName(method);
            foreach (object attribute in attributes)
            {
                if (attribute is not Rtm.CommandMethodAttribute commandAttribute)
                {
                    continue;
                }

                string commandName = commandAttribute.GlobalName;
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    LogError(new InvalidOperationException(
                        $"CommandMethodAttribute без глобального имени у метода '{methodName}'."), null);
                    continue;
                }

                if (!MapInfo.TryGetValue(commandName, out List<CommandEntry>? entries))
                {
                    entries = new List<CommandEntry>();
                    MapInfo.Add(commandName, entries);
                }

                entries.Add(new CommandEntry
                {
                    CommandName = commandName,
                    MethodName = methodName,
                    Description = description
                });
            }
        }

        /// <summary>Формирует список команд и отдельный отчёт о дубликатах.</summary>
        private void BuildReport()
        {
            var commandLines = new List<string>();
            var duplicateLines = new List<string>();

            foreach (KeyValuePair<string, List<CommandEntry>> pair in MapInfo)
            {
                if (pair.Value.Count > 1)
                {
                    duplicateLines.Add($"Дублированный атрибут: {pair.Key}");
                    foreach (CommandEntry entry in pair.Value)
                    {
                        duplicateLines.Add($"\t[{entry.MethodName}] {entry.Description}");
                    }

                    continue;
                }

                CommandEntry command = pair.Value[0];
                string method = _includeMethodInfo ? $" [{command.MethodName}]" : string.Empty;
                commandLines.Add($"{pair.Key}{method}\t{command.Description}");
            }

            CommandText = string.Join(Environment.NewLine, commandLines);
            DuplicateText = string.Join(Environment.NewLine, duplicateLines);
        }

        private static string GetMethodName(MethodInfo method)
        {
            return $"{method.DeclaringType?.FullName ?? "<unknown>"}.{method.Name}";
        }

        /// <summary>Записывает ошибку рефлексии, не позволяя сбою логгера сорвать формирование отчёта.</summary>
        private void LogError(Exception exception, string? message)
        {
            try
            {
                _logger?.Error(exception, message);
            }
            catch
            {
                // Диагностика не должна влиять на инициализацию аддона или вывод отчёта.
            }
        }
    }
}