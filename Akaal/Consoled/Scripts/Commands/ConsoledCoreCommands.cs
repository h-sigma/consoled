using System.Text;
using UnityEngine;

namespace Akaal.Consoled.Commands
{
    public static class ConsoledCoreCommands
    {
        [Command("reload", "reloads the command library. (profiler reasons)")]
        public static void Reload(Context ctx)
        {
            ctx.CommandLibrary.Load();
        }

        [Command("help", "lists all commands registered.")]
        public static void ListAllCommands(Context ctx, bool details = false)
        {
            var commands = ctx.CommandLibrary.GetAllCommands();
            var sb       = new StringBuilder();
            foreach (Command command in commands)
            {
                sb.Append("\n  ").Append(command.CommandName);
                if (details)
                {
                    for (int index = 0; index < command.Parameters.Length; index++)
                    {
                        bool first = index == 0;
                        bool last  = index == command.Parameters.Length - 1;

                        Parameter par = command.Parameters[index];
                        if (first)
                        {
                            if (par.ParameterType == typeof(Context))
                            {
                                if (command.Parameters.Length > 1) sb.Append('(');
                                continue;
                            }
                        }

                        sb.Append(par.ParameterType.Name).Append(' ');
                        sb.Append(par.Name);

                        if (!par.IsRequired)
                        {
                            sb.Append(" = ");
                            sb.Append(par.DefaultValue);
                        }

                        if (!last) sb.Append(", ");
                        else sb.Append(')');
                    }
                }

                sb.Append(":\t").Append(command.Attr.Description).Append('\t');
            }

            ctx.Console.WriteOut(sb.ToString(), Color.HSVToRGB(0.83f, 0.01f, 0.76f));
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