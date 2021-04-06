using System;
using System.Reflection;
using Akaal.Consoled.Attributes;

namespace Akaal.Consoled.Commands
{
    public struct Parameter
    {
        public Type   ParameterType { get; }
        public bool   IsRequired    { get; }
        public string Name          { get; }
        public object DefaultValue  { get; }

        public Parameter(Type parameterType, bool isRequired, string name, object defaultValue)
        {
            ParameterType = parameterType;
            IsRequired    = isRequired;
            Name          = name;
            DefaultValue  = defaultValue;
        }
    }

    public class Command
    {
        #region Props

        public CommandAttribute Attr           { get; }
        public string           CommandName    { get; }
        public MemberInfo       memberInfo     { get; }
        public Type             InstanceType   { get; }
        public bool             HasReturnValue { get; }
        public bool             RequiresTarget { get; }
        public Parameter[]      Parameters     { get; }
        public bool             ExpectsContext { get; }

        #endregion

        #region Ctor

        public Command(MethodInfo methodInfo, CommandAttribute attr, string commandName)
        {
            memberInfo     = methodInfo;
            Attr           = attr;
            CommandName    = commandName;
            HasReturnValue = methodInfo.ReturnType != typeof(void);
            RequiresTarget = !methodInfo.IsStatic;
            if (RequiresTarget)
            {
                InstanceType = methodInfo.DeclaringType;
            }

            var methodParams = methodInfo.GetParameters();
            ExpectsContext = methodParams.Length > 0 && methodParams[0].ParameterType == typeof(Context);
            Parameters = Array.ConvertAll(methodParams,
                info => new Parameter(info.ParameterType, !info.HasDefaultValue, info.Name, info.PseudoDefaultValue()));
        }

        public Command(FieldInfo fieldInfo, CommandAttribute attr, string commandName)
        {
            this.memberInfo = fieldInfo;
            Attr            = attr;
            CommandName     = commandName;
            HasReturnValue  = true;
            RequiresTarget  = !fieldInfo.IsStatic;
            if (RequiresTarget)
            {
                InstanceType = fieldInfo.DeclaringType;
            }

            ExpectsContext = false;
            Parameters = new[]
                {new Parameter(fieldInfo.FieldType, true, fieldInfo.Name, fieldInfo.FieldType.PseudoDefault())};
        }

        public Command(PropertyInfo propertyInfo, CommandAttribute attr, string commandName)
        {
            this.memberInfo = propertyInfo;
            Attr = attr;
            CommandName = commandName;
            HasReturnValue = propertyInfo.CanRead;
            RequiresTarget = (propertyInfo.CanRead ? propertyInfo.GetMethod.IsStatic : propertyInfo.SetMethod.IsStatic);
            ExpectsContext = false;
            if (RequiresTarget)
            {
                InstanceType = propertyInfo.DeclaringType;
            }

            Parameters = propertyInfo.CanWrite
                ? new[]
                {
                    new Parameter(propertyInfo.PropertyType, false, propertyInfo.Name,
                        propertyInfo.PropertyType.PseudoDefault())
                }
                : Array.Empty<Parameter>();
        }

        public static Command CreateCommand<T>(T memberInfo, CommandAttribute attr, string commandName)
            where T : MemberInfo
        {
            if (memberInfo is MethodInfo methodInfo) return new Command(methodInfo,       attr, commandName);
            if (memberInfo is PropertyInfo propertyInfo) return new Command(propertyInfo, attr, commandName);
            if (memberInfo is FieldInfo fieldInfo) return new Command(fieldInfo,          attr, commandName);
            throw new Exception("Only fields, properties, and methods are supported.");
        }

        #endregion

        public object Execute(Context context, object target, object[] parameters)
        {
            if (RequiresTarget)
            {
                if (InstanceType != null && !InstanceType.IsInstanceOfType(target))
                {
                    throw new Exception($"Command {Attr.CommandName} requires a target of type '{InstanceType.Name}'.");
                }
            }

            if (ExpectsContext)
            {
                //prepend context to parameter array
                object[] moddedParams = new object[parameters.Length + 1];
                moddedParams[0] = context;
                Array.Copy(parameters, 0, moddedParams, 1, parameters.Length);
                parameters = moddedParams;
            }

            if (memberInfo is MethodInfo methodInfo)
            {
                return ExecuteMethod(target, methodInfo, parameters);
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return ExecuteProperty(target, propertyInfo, parameters);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                return ExecuteField(target, fieldInfo, parameters);
            }
            else
            {
                throw new Exception(
                    $"MemberInfo for command {Attr.CommandName} is not of any specified type!? Type: {memberInfo?.GetType()}");
            }
        }

        #region Private Methods

        private object ExecuteField(object target, FieldInfo fieldInfo, object[] parameters)
        {
            if (parameters.Length > 0)
            {
                fieldInfo.SetValue(target, parameters[0]);
            }

            return fieldInfo.GetValue(target);
        }

        private object ExecuteProperty(object target, PropertyInfo propertyInfo, object[] parameters)
        {
            if (parameters.Length > 0 && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(target, parameters[0]);
            }

            return propertyInfo.CanRead ? propertyInfo.GetValue(target) : null;
        }

        private object ExecuteMethod(object target, MethodInfo methodInfo, object[] parameters)
        {
            return methodInfo.Invoke(target, parameters);
        }

        #endregion
    }
}