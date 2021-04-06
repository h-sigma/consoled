using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Akaal.Consoled
{
    public static class ConsoledUtils
    {
        public static object Default(this Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            if (!defaultPrimitives.TryGetValue(type, out var defaultPrimitive))
            {
                defaultPrimitive = Activator.CreateInstance(type);
            }

            return defaultPrimitive;
        }

        public static object PseudoDefault(this Type type)
        {
            if (type == typeof(Color))
            {
                return Color.white;
            }
            else if (type == typeof(string))
            {
                return string.Empty;
            }
            else if (type == typeof(AnimationCurve))
            {
                return AnimationCurve.Linear(0, 0, 1, 1);
            }
            else if (type == typeof(Gradient))
            {
                var gradient = new Gradient();

                gradient.SetKeys
                (
                    new[]
                    {
                        new GradientColorKey
                        {
                            color = Color.red,
                            time  = 0
                        },

                        new GradientColorKey
                        {
                            color = Color.blue,
                            time  = 1
                        },
                    },
                    new[]
                    {
                        new GradientAlphaKey
                        {
                            alpha = 1,
                            time  = 0,
                        },
                        new GradientAlphaKey
                        {
                            alpha = 1,
                            time  = 1,
                        },
                    }
                );

                return gradient;
            }
            else if (type.IsEnum)
            {
                // Support the [DefaultValue] attribute, fallback to zero-value
                // https://stackoverflow.com/questions/529929

                var values = Enum.GetValues(type);

                if (values.Length == 0)
                {
                    Debug.LogWarning($"Empty enum: {type}\nThis may cause problems with serialization.");
                    return Activator.CreateInstance(type);
                }

                return values.GetValue(0);
            }

            return type.Default();
        }

        private static readonly Dictionary<Type, object> defaultPrimitives = new Dictionary<Type, object>()
        {
            {typeof(int), default(int)},
            {typeof(uint), default(uint)},
            {typeof(long), default(long)},
            {typeof(ulong), default(ulong)},
            {typeof(short), default(short)},
            {typeof(ushort), default(ushort)},
            {typeof(byte), default(byte)},
            {typeof(sbyte), default(sbyte)},
            {typeof(float), default(float)},
            {typeof(double), default(double)},
            {typeof(decimal), default(decimal)},
            {typeof(Vector2), default(Vector2)},
            {typeof(Vector3), default(Vector3)},
            {typeof(Vector4), default(Vector4)},
            {typeof(Vector2Int), default(Vector2Int)},
            {typeof(Vector3Int), default(Vector3Int)},
        };

        public static object PseudoDefaultValue(this ParameterInfo parameterInfo)
        {
            if (parameterInfo.HasDefaultValue)
            {
                var defaultValue = parameterInfo.DefaultValue;

                // https://stackoverflow.com/questions/45393580
                if (defaultValue == null && parameterInfo.ParameterType.IsValueType)
                {
                    defaultValue = parameterInfo.ParameterType.PseudoDefault();
                }

                return defaultValue;
            }
            else
            {
                return parameterInfo.UnderlyingParameterType().PseudoDefault();
            }
        }

        public static Type UnderlyingParameterType(this ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType.IsByRef)
            {
                return parameterInfo.ParameterType.GetElementType();
            }
            else
            {
                return parameterInfo.ParameterType;
            }
        }

        // https://stackoverflow.com/questions/9977530/
        // https://stackoverflow.com/questions/16186694
        public static bool HasDefaultValue(this ParameterInfo parameterInfo)
        {
            return (parameterInfo.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;
        }
    }
}