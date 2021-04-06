using System;
using System.Collections.Generic;
using System.Reflection;

namespace Akaal.Consoled.Adapters
{
    public class ValueAdapterCollection
    {
        private readonly Dictionary<Type, List<IValueAdapter>> _adaptersByTargetType =
            new Dictionary<Type, List<IValueAdapter>>();

        public ValueAdapterCollection()
        {
            Load();
        }

        private void Load()
        {
            var ourType = typeof(IValueAdapter);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface(nameof(IValueAdapter)) == ourType)
                    {
                        RegisterAdapter(type);
                    }
                }
            }
        }

        public void RegisterAdapter(Type type)
        {
            var adapter = Activator.CreateInstance(type) as IValueAdapter;
            if (adapter == null)
            {
                throw new Exception($"Critical error in logic. Type {type.Name} must derive from type IValueAdapter.");
            }

            if (!_adaptersByTargetType.TryGetValue(adapter.TargetType, out var list))
            {
                list                                      = new List<IValueAdapter>();
                _adaptersByTargetType[adapter.TargetType] = list;
            }

            list.Add(adapter);
        }

        public bool TryAdaptValue(Type targetType, object valueToAdapt, out object adaptedValue, out string errorMessage)
        {
            adaptedValue = null;
            errorMessage = null;
            if (_adaptersByTargetType.TryGetValue(targetType, out var adapters))
            {
                for (var i = 0; i < adapters.Count; i++)
                {
                    if (adapters[i].TryAdaptValue(valueToAdapt, out adaptedValue, out errorMessage)) return true;
                }
            }

            return false;
        }
    }
}