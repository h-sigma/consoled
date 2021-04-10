using System.Text;
using Akaal.Consoled.Attributes;
using UnityEngine;

namespace Akaal.Consoled.Commands
{
    [CommandModule("Module Commands", "module")]
    internal static class ConsoledModuleCommands
    {
        [Command("commands", "lists all commands registered in the module")]
        public static void ListAllCommands(Context ctx, string moduleName,
            bool showDescription = false)
        {
            var commands = ctx.CommandLibrary.GetCommandsByModule();
            var sb       = new StringBuilder();
            moduleName = moduleName.ToLower();
            foreach (var module in commands)
            {
                if (string.IsNullOrEmpty(moduleName) ||
                    !module.Key.ModuleName.ToLower().Contains(moduleName)) continue;

                ConsoledShared.PrintModule(showDescription, sb, module);
            }

            ctx.Console.WriteOut(sb.ToString());
        }
    }
}