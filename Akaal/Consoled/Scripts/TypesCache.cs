using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Akaal.Consoled
{
    public static class TypesCache
    {
        private static List<Type>                     _types;
        private static List<(Type, MemberInfo[])>     _members;
        private static Dictionary<Type, MemberInfo[]> _membersByType;

        static TypesCache()
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes());
            _types         = new List<Type>(types);
            _members       = new List<(Type, MemberInfo[])>();
            _membersByType = new Dictionary<Type, MemberInfo[]>();
            foreach (Type type in _types)
            {
                MemberInfo[] memberInfos = type.GetMembers();
                _members.Add((type, memberInfos));
                _membersByType[type] = memberInfos;
            }
        }

        #region Public

        public static IReadOnlyList<Type> GetClassesDerivedFrom<T>()
        {
            return DerivedCache<T>.derivedTypes;
        }

        public static IReadOnlyList<FieldInfo> GetFieldsWithAttribute<T>() where T : Attribute
        {
            return MemberAttributeCache<T>.fieldInfos;
        }

        public static IReadOnlyList<PropertyInfo> GetPropertiesWithAttribute<T>() where T : Attribute
        {
            return MemberAttributeCache<T>.propertyInfos;
        }

        public static IReadOnlyList<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
        {
            return MemberAttributeCache<T>.methodInfos;
        }

        public static IReadOnlyList<Type> GetClassesWithAttribute<T>() where T : Attribute
        {
            return ClassAttributeCache<T>.classes;
        }

        #endregion

        private static class DerivedCache<T>
        {
            public static readonly List<Type> derivedTypes;

            static DerivedCache()
            {
                derivedTypes = new List<Type>();
                var baseType = typeof(T);
                foreach (Type type in _types)
                {
                    if (baseType.IsAssignableFrom(type)) derivedTypes.Add(type);
                }
            }
        }

        private static class ClassAttributeCache<T> where T : Attribute
        {
            public static readonly List<Type> classes = new List<Type>();

            static ClassAttributeCache()
            {
                Type attrType = typeof(T);
                foreach (var kvp in _members)
                {
                    if (Attribute.IsDefined(kvp.Item1, attrType))
                    {
                        classes.Add(kvp.Item1);
                    }
                }
            }
        }

        private static class MemberAttributeCache<T> where T : Attribute
        {
            public static readonly List<FieldInfo>    fieldInfos    = new List<FieldInfo>();
            public static readonly List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            public static readonly List<MethodInfo>   methodInfos   = new List<MethodInfo>();

            static MemberAttributeCache()
            {
                Type attrType = typeof(T);
                foreach (var kvp in _members)
                {
                    foreach (MemberInfo memberInfo in kvp.Item2)
                    {
                        if (Attribute.IsDefined(memberInfo, attrType))
                        {
                            if (memberInfo is FieldInfo fieldInfo) fieldInfos.Add(fieldInfo);
                            else if (memberInfo is PropertyInfo propertyInfo) propertyInfos.Add(propertyInfo);
                            else if (memberInfo is MethodInfo methodInfo) methodInfos.Add(methodInfo);
                        }
                    }
                }
            }
        }
    }
}