using System;
using System.Collections.Generic;
using UnityEngine;

namespace Akaal.Consoled.Adapters
{
    public sealed class TypeValueAdapter : IValueAdapter
    {
        #region MbCache

        private static readonly Dictionary<string, Type> _customMbs = new Dictionary<string, Type>();

        static TypeValueAdapter()
        {
            foreach (Type type in TypesCache.GetClassesDerivedFrom<MonoBehaviour>())
            {
                _customMbs[type.FullName] = type;
            }
        }

        #endregion

        #region Implementation of IValueAdapter

        public Type TargetType => typeof(Type);

        public bool CanAdapt(object value)
        {
            Type t = value.GetType();
            return t == typeof(string) || t == typeof(Type);
        }

        public bool TryAdaptValue(object value, out object result, out string errorMessage)
        {
            errorMessage = null;
            result       = null;
            if (value is string typeName)
            {
                result = Type.GetType(typeName);
                if (result == null)
                {
                    if (_customMbs.TryGetValue(typeName, out Type foundType))
                    {
                        result = foundType;
                        return true;
                    }

                    errorMessage = $"Type '{value}' not found.";
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}