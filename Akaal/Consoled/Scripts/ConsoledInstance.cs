using System;
using Akaal.Consoled.Adapters;
using Akaal.Consoled.Commands;

namespace Akaal.Consoled
{
    public partial class ConsoledInstance
    {
        #region (Private) Fields

        private readonly ICommandParser         _commandParser   = new DefaultCommandParser();
        private readonly CircularBuffer<string> _outputHistory   = new CircularBuffer<string>(50);
        private readonly CircularBuffer<string> _commandsHistory = new CircularBuffer<string>(50);

        #endregion

        #region Constructors

        public ConsoledInstance()
        {
            Context = new Context(this, new CommandLibrary());
        }

        private ConsoledInstance(ConsoledInstance other)
        {
            Theme = other.Theme;
        }

        #endregion

        #region Public

        public static ValueAdapterCollection Adapters { get; } = new ValueAdapterCollection();

        public Context Context { get; }

        public bool IsReady => Context.CommandLibrary.IsReady;

        public CircularBuffer<string> OutputHistory => _outputHistory;
        public CircularBuffer<string> CommandHistory => _commandsHistory;

        #endregion

        #region Private

        private void ExecuteRawCommand(string rawCommand)
        {
            WriteOut("\n> " + rawCommand, Theme.Error);
            bool isValid = _commandParser.Parse(rawCommand + ' ', out string error, out string commandName,
                out object[] parameters, out VariableReference target, out VariableReference assignment);
            _commandsHistory.PushFront(rawCommand);

            if (!isValid)
            {
                WriteOut(error, Theme.ParseError);
            }
            else
            {
                ExecuteCommand(commandName, parameters, target, assignment);
            }
        }

        private void ExecuteCommand(string commandName, object[] arguments, VariableReference target,
            VariableReference assignment)
        {
            var command = Context.CommandLibrary.GetCommand(commandName);
            if (command == null)
            {
                if (assignment != null && arguments.Length > 0)
                {
                    Context.SetVariable(assignment, RealObject(Context, arguments[0], null));
                }
                else
                {
                    WriteOut($"\nCommand '{commandName}' not found.", Theme.Error);
                }

                return;
            }

            int systemSuppliedArgsCount  = (command.ExpectsContext ? 1 : 0);
            int userSuppliedArgsExpected = command.Parameters.Length - systemSuppliedArgsCount;
            if (arguments.Length > userSuppliedArgsExpected)
            {
                WriteOut(
                    $"\nToo many arguments to command. Expected at most {userSuppliedArgsExpected}. Received {arguments.Length}.",
                    Theme.Error);
            }
            else if (arguments.Length < userSuppliedArgsExpected)
            {
                for (var i = arguments.Length + systemSuppliedArgsCount; i < command.Parameters.Length; i++)
                {
                    if (command.Parameters[i].IsRequired)
                    {
                        WriteOut(
                            $"\nToo few arguments to command. Expected at least {i}. Received {arguments.Length}.",
                            Theme.Error);
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
                arguments[i] = RealObject(Context, arguments[i],
                    command.Parameters[i + systemSuppliedArgsCount].DefaultValue);
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
                        WriteOut('\n' + errorMessage, Theme.Error);
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
                    Theme.Error);
                throw;
            }
        }

        private static object RealObject(Context ctx, object argument, object defaultValue)
        {
            if (argument is VariableReference vref)
            {
                return ctx.GetVariable(vref);
            }

            if (argument is DefaultValue)
            {
                return defaultValue;
            }

            return argument;
        }

        #endregion
    }
}