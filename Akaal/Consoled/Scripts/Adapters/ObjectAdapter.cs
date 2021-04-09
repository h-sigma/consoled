using System;

namespace Akaal.Consoled.Adapters
{
    public class ObjectAdapter : IValueAdapter
    {
        #region Implementation of IValueAdapter

        public bool CanAdapt(Type targetType) => targetType == typeof(object);

        public bool TryAdaptValue(object value, Type targetType, out object result, out string errorMessage)
        {
            errorMessage = null;
            result       = value;
            return true;
        }

        #endregion
    }
}