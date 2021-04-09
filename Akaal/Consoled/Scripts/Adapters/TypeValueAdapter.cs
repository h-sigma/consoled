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

        public bool CanAdapt(Type targetType) => targetType == typeof(string) || targetType == typeof(Type);

        public bool TryAdaptValue(object value, Type targetType, out object result, out string errorMessage)
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
            else if (value is Type)
            {
                result = value;
                return true;
            }

            return false;
        }

        #endregion
    }
}