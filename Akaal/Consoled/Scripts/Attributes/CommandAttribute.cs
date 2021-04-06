using System;

namespace Akaal.Consoled.Attributes
{
    #region Attribute

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class CommandAttribute : Attribute
    {
        public string   CommandName { get; }
        public string   Description { get; }
        public string[] Aliases     { get; }

        public CommandAttribute(string commandName, string description, params string[] aliases)
        {
            CommandName = commandName;
            Description = description;
            Aliases     = aliases;
        }
    }

    #endregion
}