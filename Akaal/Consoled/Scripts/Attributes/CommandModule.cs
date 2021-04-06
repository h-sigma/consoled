using System;

namespace Akaal.Consoled.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [CommandModule("Uncategorized")]
    public class CommandModule : Attribute
    {
        public CommandModule(string moduleName, string prefix = null)
        {
            ModuleName = moduleName;
            Prefix     = prefix;
        }

        public string ModuleName { get; }
        public string Prefix     { get; }

        public static CommandModule DefaultModule { get; private set; }

        static CommandModule()
        {
            DefaultModule = typeof(CommandModule).GetCustomAttributes(typeof(CommandModule), false)[0] as CommandModule;
        }
    }
}