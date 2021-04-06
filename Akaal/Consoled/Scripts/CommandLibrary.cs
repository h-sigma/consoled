using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Akaal.Consoled.Attributes;
using Akaal.Consoled.Commands;
using UnityEngine;
using UnityEngine.Profiling;

namespace Akaal.Consoled
{
    public class CommandLibrary
    {
        #region ctor

        private readonly List<Command>               _commands       = new List<Command>();
        private readonly Dictionary<string, Command> _commandsByName = new Dictionary<string, Command>();

        private readonly Dictionary<Type, CommandModule> _prefixes =
            new Dictionary<Type, CommandModule>();

        private readonly Dictionary<CommandModule, List<Command>>
            _commandsByModule = new Dictionary<CommandModule, List<Command>>();

        public bool IsReady { get; private set; }

        public CommandLibrary()
        {
            Load();
        }

        #endregion

        public void Load()
        {
            Task.Run(_Load);
        }

        private void _Load()
        {
            IsReady = false;
            _commands.Clear();
            _commandsByName.Clear();
            _prefixes.Clear();

            //todo -- command provider
            //todo -- serialize
            try
            {
                Profiler.BeginSample("Consoled.LoadCommands");


                foreach (Type commandClass in TypesCache.GetClassesWithAttribute<CommandModule>())
                {
                    _prefixes.Add(commandClass, commandClass.GetCustomAttribute<CommandModule>());
                }

                ExtractCommandsFromInfos(TypesCache.GetFieldsWithAttribute<CommandAttribute>());
                ExtractCommandsFromInfos(TypesCache.GetPropertiesWithAttribute<CommandAttribute>());
                ExtractCommandsFromInfos(TypesCache.GetMethodsWithAttribute<CommandAttribute>());
            }
            finally
            {
                IsReady = true;
                Profiler.EndSample();
            }
        }

        private void ExtractCommandsFromInfos<T>(IReadOnlyList<T> commandFields) where T : MemberInfo
        {
            foreach (var info in commandFields)
            {
                bool    hasPrefix = GetAttrs(info, out var attr, out var module, out var commandName);
                Command command   = Command.CreateCommand(info, attr, commandName);
                _commands.Add(command);
                if (module == null) module = CommandModule.DefaultModule;
                if (!_commandsByModule.TryGetValue(module, out var cmds))
                {
                    cmds                      = new List<Command>();
                    _commandsByModule[module] = cmds;
                }

                cmds.Add(command);

                AddCommand(commandName, command);
                foreach (string alias in attr.Aliases)
                {
                    AddCommand(alias, command);
                }
            }
        }

        private bool GetAttrs<TInfo>(TInfo info, out CommandAttribute attr, out CommandModule module,
            out string commandName)
            where TInfo : MemberInfo
        {
            attr = info.GetCustomAttribute<CommandAttribute>();
            bool hasPrefix = _prefixes.TryGetValue(info.DeclaringType, out module);
            commandName = hasPrefix ? CombinePrefixAndName(module.Prefix, attr.CommandName) : attr.CommandName;
            return hasPrefix;
        }

        private string CombinePrefixAndName(string prefix, string alias)
        {
            return string.IsNullOrEmpty(prefix) ? alias : $"{prefix}-{alias}";
        }

        public void AddCommand(string alias, Command command)
        {
            if (_commandsByName.ContainsKey(alias))
            {
                Debug.LogWarning(
                    $"Consoled contains duplicate commands by alias {alias}: {command.memberInfo.Name} and {_commandsByName[alias].memberInfo.Name}. One of them will be omitted.");
                return;
            }

            _commandsByName[alias] = command;
        }

        public Command GetCommand(string commandName)
        {
            if (!string.IsNullOrEmpty(commandName) && _commandsByName.TryGetValue(commandName, out Command command))
                return command;
            return null;
        }

        public IReadOnlyList<Command> GetAllCommands()
        {
            return _commands;
        }

        public Dictionary<CommandModule, IReadOnlyList<Command>> GetCommandsByModule()
        {
            var dict = new Dictionary<CommandModule, IReadOnlyList<Command>>();
            foreach (KeyValuePair<CommandModule, List<Command>> keyValuePair in _commandsByModule)
            {
                dict[keyValuePair.Key] = keyValuePair.Value;
            }

            return dict;
        }
    }
}