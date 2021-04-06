using System;

namespace Akaal.Consoled
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPrefixAttribute : Attribute
    {
        public CommandPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }
    }
}