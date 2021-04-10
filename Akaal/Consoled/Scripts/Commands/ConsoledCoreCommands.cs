using System.Text;
using Akaal.Consoled.Attributes;
using UnityEngine;

namespace Akaal.Consoled.Commands
{
    internal static class ConsoledCoreCommands
    {
        [Command("reload", "reloads the command library. (profiler reasons)")]
        public static void Reload(Context ctx)
        {
            ctx.CommandLibrary.Load();
        }

        [Command("commands", "lists all commands registered, optionally only those matching a partial command.")]
        public static void ListAllCommands(Context ctx, string partialCommand = null,
            bool showDescription = false)
        {
            var  commands = ctx.CommandLibrary.GetCommandsByModule();
            var  sb       = new StringBuilder();
            bool doCheck  = !string.IsNullOrEmpty(partialCommand);
            partialCommand = partialCommand?.ToLower();
            bool didNotFind = doCheck;
            foreach (var module in commands)
            {
                if (!doCheck)
                {
                    ConsoledShared.PrintModule(showDescription, sb, module);
                }
                else
                {
                    foreach (Command command in module.Value)
                    {
                        if (command.CommandName.ToLower().Contains(partialCommand))
                        {
                            ConsoledShared.PrintCommand(showDescription, sb, command);
                            didNotFind = false;
                        }
                    }
                }
            }

            if (didNotFind)
            {
                sb.AppendLine($"No command like '{partialCommand}' found.");
            }

            ctx.Console.WriteOut(sb.ToString());
        }

        [Command("clear", "clears the console output.")]
        public static void ClearConsole(Context ctx)
        {
            ctx.Console.Clear();
        }

        [Command("print", "prints the string representation of an object")]
        public static void Print(Context ctx, object itemToPrint, bool useJson = false)
        {
            if (itemToPrint == null) ctx.Console.WriteWarningLine("Object is null.");
            else
            {
                string rep = !useJson ? itemToPrint.ToString() : JsonUtility.ToJson(itemToPrint, true);
                ctx.Console.WriteInfoLine(rep);
            }
        }
    }
}