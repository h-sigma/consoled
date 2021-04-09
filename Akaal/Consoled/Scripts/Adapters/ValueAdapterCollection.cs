using System;
using System.Collections.Generic;
using System.Reflection;

namespace Akaal.Consoled.Adapters
{
    public class ValueAdapterCollection
    {
        /*private readonly Dictionary<Type, List<IValueAdapter>> _adaptersByTargetType =
            new Dictionary<Type, List<IValueAdapter>>();*/

        private static readonly List<IValueAdapter> _adapters = new List<IValueAdapter>();

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

            /*
            if (!_adaptersByTargetType.TryGetValue(adapter.TargetType, out var list))
            {
                list                                      = new List<IValueAdapter>();
                _adaptersByTargetType[adapter.TargetType] = list;
            }*/

            _adapters.Add(adapter);
        }

        public bool TryAdaptValue(Type targetType, object valueToAdapt, out object adaptedValue,
            out string errorMessage)
        {
            adaptedValue = null;
            errorMessage = null;
            if (typeof(Enum).IsAssignableFrom(targetType))
            {
                return TryAdaptEnum(targetType, valueToAdapt, out adaptedValue, out errorMessage);
            }
            else //if (_adaptersByTargetType.TryGetValue(targetType, out var adapters))
            {
                foreach (IValueAdapter adapter in /*adapters*/ _adapters)
                {
                    if (adapter.CanAdapt(targetType) && adapter.TryAdaptValue(valueToAdapt, targetType, out adaptedValue, out errorMessage))
                        return true;
                }
            }

            return false;
        }

        private bool TryAdaptEnum(Type targetType, object valueToAdapt, out object adaptedValue,
            out string errorMessage)
        {
            adaptedValue = null;
            errorMessage = null;
            if (valueToAdapt is string enumName)
            {
                try
                {
                    adaptedValue = Enum.Parse(targetType, enumName, true);
                    if (adaptedValue != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    //
                }

                errorMessage = $"Name '{enumName}' could not be parsed into enum of type {targetType.Name}";
            }
            else
            {
                errorMessage =
                    $"Value of type {adaptedValue?.GetType().Name} could not be adapted to target of type {targetType.Name}.";
            }

            return false;
        }
    }
}