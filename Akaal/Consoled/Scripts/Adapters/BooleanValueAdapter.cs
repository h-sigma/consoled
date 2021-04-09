using System;

namespace Akaal.Consoled.Adapters
{
    public sealed class BooleanValueAdapter : IValueAdapter
    {
        #region Implementation of IValueAdapter

        public bool CanAdapt(Type targetType) => targetType == typeof(bool);

        public bool TryAdaptValue(object value, Type targetType, out object result, out string errorMessage)
        {
            result       = null;
            errorMessage = null;
            if (value is int intValue)
            {
                result = intValue != 0;
                return true;
            }

            errorMessage = "Unable to convert value to boolean.";
            return false;
        }

        #endregion
    }
}