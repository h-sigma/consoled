using UnityEngine;

namespace Akaal.Consoled
{
    public interface IConsole
    {
        void WriteOut(string text, Color? color = null);
        void WriteErrorLine(string text);
        void WriteWarningLine(string text);
        void WriteInfoLine(string text);
        void SubmitCommand(string rawCommand);
        void Clear();
    }
}