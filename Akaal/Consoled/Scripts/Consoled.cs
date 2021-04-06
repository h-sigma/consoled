using System;
using Akaal.Consoled.Adapters;
using Akaal.Consoled.Commands;
using UnityEngine;

namespace Akaal.Consoled
{
    public static class Consoled
    {
        private static ICommandParser         _commandParser   = new DefaultCommandParser();
        private static CircularBuffer<string> _outputHistory   = new CircularBuffer<string>(50);
        private static CircularBuffer<string> _commandsHistory = new CircularBuffer<string>(50);
        private static ConsoleImpl            _console         = new ConsoleImpl() {SubmitHandler = SubmitCommand};

        public static class Colors
        {
            public static readonly Color ParseError  = Color.red;
            public static readonly Color Error       = Color.red;
            public static readonly Color Warning     = Color.yellow;
            public static readonly Color Important   = Color.magenta;
            public static readonly Color Information = Color.white;
        }

        #region Public

        public static ValueAdapterCollection Adapters { get; } = new ValueAdapterCollection();

        public static Context Context { get; } =
            new Context(_console, new CommandLibrary());

        public static bool IsReady => Context.CommandLibrary.IsReady;

        public static CircularBuffer<string> GetOutputHistory()  => _outputHistory;
        public static CircularBuffer<string> GetCommandHistory() => _commandsHistory;

        public static void SubmitCommand(string rawCommand)
        {
            WriteOut("\n> " + rawCommand, Colors.Error);
            bool isValid = _commandParser.Parse(rawCommand + ' ', out string error, out string commandName,
                out object[] parameters, out VariableReference target, out VariableReference assignment);
            _commandsHistory.PushFront(rawCommand);

            if (!isValid)
            {
                WriteOut(error, Colors.ParseError);
            }
            else
            {
                ExecuteCommand(commandName, parameters, target, assignment);
            }
        }

        #region Console Facade

        public static void SetClearHandler(Action handler)
        {
            _console.ClearHandler = handler;
        }

        public static void SetOutputHandler(Action<string> handler)
        {
            _console.WriteHandler = handler;
        }

        public static void SetColorTranslator(Func<string, Color?, string> translator)
        {
            _console.ColorTranslator = translator;
        }

        public static void WriteOut(string text, Color? color)
        {
            _console.WriteOut(text, color);
        }

        #endregion

        #endregion

        #region Private

        private static void ExecuteCommand(string commandName, object[] arguments, VariableReference target,
            VariableReference assignment)
        {
            var command = Context.CommandLibrary.GetCommand(commandName);
            if (command == null)
            {
                if (assignment != null && arguments.Length > 0)
                {
                    Context.SetVariable(assignment, RealObject(arguments[0], null));
                }
                else
                {
                    WriteOut($"\nCommand '{commandName}' not found.", Colors.Error);
                }

                return;
            }

            int systemSuppliedArgsCount  = (command.ExpectsContext ? 1 : 0);
            int userSuppliedArgsExpected = command.Parameters.Length - systemSuppliedArgsCount;
            if (arguments.Length > userSuppliedArgsExpected)
            {
                WriteOut(
                    $"\nToo many arguments to command. Expected at most {userSuppliedArgsExpected}. Received {arguments.Length}.",
                    Colors.Error);
            }
            else if (arguments.Length < userSuppliedArgsExpected)
            {
                for (var i = arguments.Length + systemSuppliedArgsCount; i < command.Parameters.Length; i++)
                {
                    if (command.Parameters[i].IsRequired)
                    {
                        WriteOut(
                            $"\nToo few arguments to command. Expected at least {i + 1}. Received {arguments.Length}.",
                            Colors.Error);
                        return;
                    }
                }

                var tempArgs = new object[userSuppliedArgsExpected];
                Array.Copy(arguments, 0, tempArgs, 0, arguments.Length);
                for (var i = arguments.Length; i < userSuppliedArgsExpected; i++)
                {
                    tempArgs[i] = Type.Missing;
                }

                arguments = tempArgs;
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = RealObject(arguments[i], command.Parameters[i + systemSuppliedArgsCount].DefaultValue);
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == Type.Missing) continue;
                Parameter matchingParameter = command.Parameters[i + systemSuppliedArgsCount];
                if (!matchingParameter.ParameterType.IsInstanceOfType(arguments[i]))
                {
                    if (Adapters.TryAdaptValue(matchingParameter.ParameterType, arguments[i], out object adaptedValue,
                        out string errorMessage))
                    {
                        arguments[i] = adaptedValue;
                    }
                    else
                    {
                        errorMessage = string.IsNullOrEmpty(errorMessage)
                            ? $"Could not adapt value of type '{arguments[i].GetType().Name}' to expected parameter '{matchingParameter.Name}' of type '{matchingParameter.ParameterType.Name}'."
                            : errorMessage;
                        WriteOut('\n' + errorMessage, Colors.Error);
                        return;
                    }
                }
            }

            var targetObj = Context.GetVariable(target);
            try
            {
                var result = command.Execute(Context, targetObj, arguments);
                if (command.HasReturnValue && assignment != null)
                {
                    Context.SetVariable(assignment, result);
                }
            }
            catch (Exception ex)
            {
                WriteOut($"\nException caught during execution of command (will be re-thrown). Message: {ex.Message}",
                    Colors.Error);
                throw;
            }
        }

        private static object RealObject(object argument, object defaultValue)
        {
            if (argument is VariableReference vref)
            {
                return Context.GetVariable(vref);
            }

            if (argument is DefaultValue)
            {
                return defaultValue;
            }

            return argument;
        }

        #endregion

        #region ConsoleImp

        //todo -- buffer write outs inside a command
        private class ConsoleImpl : IConsole
        {
            private Action<string>               _writeHandler;
            private Func<string, Color?, string> _colorTranslator;
            private Action<string>               _submitHandler;
            private Action                       _clearHandler;

            public Action<string> WriteHandler
            {
                get => _writeHandler;
                set => _writeHandler = value;
            }

            public Action<string> SubmitHandler
            {
                get => _submitHandler;
                set => _submitHandler = value;
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

            #region Implementation of IConsole

            public void WriteOut(string text, Color? color)
            {
                string finalText = _colorTranslator == null ? text : _colorTranslator(text, color);
                _outputHistory.PushBack(finalText);
                WriteHandler?.Invoke(finalText);
            }

            public void WriteErrorLine(string text)
            {
                WriteOut('\n' + text, Colors.Error);
            }

            public void WriteWarningLine(string text)
            {
                WriteOut('\n' + text, Colors.Warning);
            }

            public void WriteInfoLine(string text)
            {
                WriteOut('\n' + text, Colors.Information);
            }

            public void SubmitCommand(string rawCommand)
            {
                SubmitHandler?.Invoke(rawCommand);
            }

            public void Clear()
            {
                while (!_outputHistory.IsEmpty)
                {
                    _outputHistory.PopBack();
                }

                ClearHandler.Invoke();
            }

            #endregion
        }

        #endregion
    }
}