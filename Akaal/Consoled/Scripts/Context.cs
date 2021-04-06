using System;
using System.Collections.Generic;

namespace Akaal.Consoled
{
    public class Context
    {
        private Dictionary<string, object> _memory = new Dictionary<string, object>();

        public CommandLibrary CommandLibrary { get; }
        public event Action   OnMemoryUpdated;

        public Context(IConsole console, CommandLibrary commandLibrary)
        {
            Console        = console;
            CommandLibrary = commandLibrary;
        }

        public IConsole Console { get; }

        public IReadOnlyDictionary<string, object> Memory => _memory;

        public object GetVariable(VariableReference varRef)
        {
            if (varRef != null && !string.IsNullOrEmpty(varRef.VariableName) &&
                _memory.TryGetValue(varRef.VariableName, out object result)) return result;
            return null;
        }

        public void SetVariable(VariableReference varRef, object value)
        {
            if (varRef == null) return;
            _memory[varRef.VariableName] = value;
            if (value == null)
            {
                UnsetVariable(varRef);
            }
            else
            {
                OnMemoryUpdated?.Invoke();
            }
        }

        public void UnsetVariable(VariableReference varRef)
        {
            if (varRef == null) return;
            _memory.Remove(varRef.VariableName);
            OnMemoryUpdated?.Invoke();
        }
    }
}