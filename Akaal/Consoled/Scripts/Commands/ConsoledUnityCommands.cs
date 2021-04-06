using System;

namespace Akaal.Consoled.Commands
{
    [CommandPrefix("unity-")]
    public static class ConsoledUnityCommands
    {
        [Command("find", "find an object by type.")]
        public static object Find(Context ctx, Type type, bool inactive = false)
        {
            if (type == null)
            {
                ctx.Console.WriteErrorLine($"Null type provided.");
            }
            else if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                ctx.Console.WriteErrorLine(
                    $"Can't find an object of that type as it does not derive from UnityEngine.Object .");
            }
            else
            {
                return UnityEngine.Object.FindObjectOfType(type);
            }

            return null;
        }
    }
}