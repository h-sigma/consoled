using System;

namespace Akaal.Consoled.Adapters
{
    public interface IValueAdapter
    {
        Type TargetType { get; }
        bool TryAdaptValue(object value, out object result, out string errorMessage);
    }
}