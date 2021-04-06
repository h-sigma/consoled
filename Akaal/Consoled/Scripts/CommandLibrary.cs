using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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

        private readonly Dictionary<Type, CommandPrefixAttribute> _prefixes =
            new Dictionary<Type, CommandPrefixAttribute>();

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


                foreach (Type commandClass in TypesCache.GetClassesWithAttribute<CommandPrefixAttribute>())
                {
                    _prefixes.Add(commandClass, commandClass.GetCustomAttribute<CommandPrefixAttribute>());
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
                bool    hasPrefix = GetAttrs(info, out var attr, out var prefixAttr, out var commandName);
                Command command   = Command.CreateCommand(info, attr, commandName);
                _commands.Add(command);
                string prefix = prefixAttr?.Prefix;
                AddCommand(hasPrefix, prefix, attr.CommandName, command);
                foreach (string alias in attr.Aliases)
                {
                    AddCommand(hasPrefix, prefix, alias, command);
                }
            }
        }

        private bool GetAttrs<TInfo>(TInfo info, out CommandAttribute attr, out CommandPrefixAttribute prefixAttribute,
            out string commandName)
            where TInfo : MemberInfo
        {
            attr = info.GetCustomAttribute<CommandAttribute>();
            bool hasPrefix = _prefixes.TryGetValue(info.DeclaringType, out prefixAttribute);
            commandName = hasPrefix ? prefixAttribute.Prefix + attr.CommandName : attr.CommandName;
            return hasPrefix;
        }

        public void AddCommand(bool hasPrefix, string prefix, string alias, Command command)
        {
            string key = hasPrefix ? prefix + alias : alias;
            if (_commandsByName.ContainsKey(key))
            {
                Debug.LogWarning(
                    $"Consoled contains duplicate commands by name {key}: {command.memberInfo.Name} and {_commandsByName[key].memberInfo.Name}. One of them will be omitted.");
            }

            _commandsByName[key] = command;
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
    }
}