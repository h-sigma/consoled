using System;
using UnityEngine;

namespace Akaal.Consoled
{
    public partial class ConsoledInstance : IConsole
    {
        #region (Private) Fields

        private ConsoleActions _consoleActions;

        #endregion

        #region Public

        //todo -- buffer write outs inside a command
        public void SubmitCommand(string rawCommand)
        {
            ExecuteRawCommand(rawCommand);
        }

        #region Implementation of IConsole

        public void WriteOut(string text, Color? color)
        {
            string finalText = _consoleActions.ColorTranslator == null
                ? text
                : _consoleActions.ColorTranslator(text, color);
            _outputHistory.PushBack(finalText);
            _consoleActions.WriteHandler?.Invoke(finalText);
        }

        public void WriteErrorLine(string text)
        {
            WriteOut('\n' + text, Theme.Error);
        }

        public void WriteWarningLine(string text)
        {
            WriteOut('\n' + text, Theme.Warning);
        }

        public void WriteInfoLine(string text)
        {
            WriteOut('\n' + text, Theme.Information);
        }

        public void Clear()
        {
            while (!_outputHistory.IsEmpty)
            {
                _outputHistory.PopBack();
            }

            _consoleActions.ClearHandler.Invoke();
        }

        #endregion

        #endregion

        #region Console Actions

        public void SetClearHandler(Action handler)
        {
            _consoleActions.ClearHandler = handler;
        }

        public void SetOutputHandler(Action<string> handler)
        {
            _consoleActions.WriteHandler = handler;
        }

        public void SetColorTranslator(Func<string, Color?, string> translator)
        {
            _consoleActions.ColorTranslator = translator;
        }

        private struct ConsoleActions
        {
            private Action<string>               _writeHandler;
            private Func<string, Color?, string> _colorTranslator;
            private Action                       _clearHandler;

            public Action<string> WriteHandler
            {
                get => _writeHandler;
                set => _writeHandler = value;
            }

            public Func<string, Color?, string> ColorTranslator
            {
                get => _colorTranslator;
                set => _colorTranslator = value;
            }

            public Action ClearHandler
            {
                get => _clearHandler;
                set => _clearHandler = value;
            }
        }

        #endregion
    }
}