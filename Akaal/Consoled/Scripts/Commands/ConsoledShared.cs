using System.Collections.Generic;
using System.Text;
using Akaal.Consoled.Attributes;

namespace Akaal.Consoled.Commands
{
    internal static class ConsoledShared
    {
        public static void PrintModule(bool showDescription, StringBuilder sb,
            KeyValuePair<CommandModule, IReadOnlyList<Command>> module)
        {
            sb.Append("\n  ").Append(module.Key.ModuleName);
            foreach (Command command in module.Value)
            {
                PrintCommand(showDescription, sb, command);
            }
        }

        public static void PrintCommand(bool showDescription, StringBuilder sb, Command command)
        {
            sb.Append("\n      ").Append(command.CommandName);
            for (int index = 0; index < command.Parameters.Length; index++)
            {
                bool first = index == 0;
                bool last  = index == command.Parameters.Length - 1;

                Parameter par = command.Parameters[index];
                if (first)
                {
                    if (par.ParameterType == typeof(Context))
                    {
                        //if (command.Parameters.Length > 1) sb.Append('(');
                        continue;
                    }
                }

                //sb.Append(par.ParameterType.Name).Append(' ');
                char left  = par.IsRequired ? '<' : '_';
                char right = par.IsRequired ? '>' : '_';
                sb.Append(' ').Append(left).Append(par.Name).Append(right);

                //if (!last) sb.Append(", ");
                //else sb.Append(')');
            }

            if (showDescription)
            {
                sb.Append(":\t").Append(command.Attr.Description).Append('\t');
            }
        }
    }
}