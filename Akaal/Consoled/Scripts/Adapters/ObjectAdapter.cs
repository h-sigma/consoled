using System;

namespace Akaal.Consoled.Adapters
{
    public class ObjectAdapter : IValueAdapter
    {
        #region Implementation of IValueAdapter

        public Type TargetType => typeof(object);

        public bool TryAdaptValue(object value, out object result, out string errorMessage)
        {
            errorMessage = null;
            result       = value;
            return true;
        }

        #endregion
    }
}