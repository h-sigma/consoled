using System;

namespace Akaal.Consoled.Adapters
{
    public interface IValueAdapter
    {
        bool CanAdapt(Type targetType);
        bool TryAdaptValue(object value, Type targetType, out object result, out string errorMessage);
    }
}