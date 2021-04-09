using System;
using System.Collections.Generic;
using System.Text;

namespace Akaal.Consoled
{
    public class Context
    {
        private Dictionary<string, object> _memory = new Dictionary<string, object>();

        public CommandLibrary CommandLibrary { get; }
        public event Action   OnMemoryUpdated;

        public Context(IConsole console, CommandLibrary commandLibrary)
        {
            Console         =  console;
            CommandLibrary  =  commandLibrary;
            OnMemoryUpdated += () => _memoryDirty = true;
        }

        public IConsole Console { get; }

        public IReadOnlyDictionary<string, object> Memory => _memory;

        private bool          _memoryDirty;
        private StringBuilder sb                  = new StringBuilder();
        private string        _lastRepresentation = string.Empty;

        public string GetUserFriendlyMemoryRepresentation()
        {
            if (!_memoryDirty) return _lastRepresentation;
            sb.Clear();

            foreach (var kvp in _memory)
            {
                sb.Append(kvp.Key);
                sb.Append('<');
                sb.Append(kvp.Value.GetType().Name);
                sb.Append(">\n");
            }

            _lastRepresentation = sb.ToString();
            _memoryDirty        = false;
            return _lastRepresentation;
        }

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