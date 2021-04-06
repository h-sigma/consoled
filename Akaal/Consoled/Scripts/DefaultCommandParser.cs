using System;
using System.Collections.Generic;

namespace Akaal.Consoled
{
    public interface ICommandParser
    {
        bool Parse(string rawCommand, out string errorMessage, out string commandName, out object[] parameters,
            out VariableReference target, out VariableReference assignment);
    }

    public class VariableReference
    {
        public VariableReference(string variableName)
        {
            VariableName = variableName;
        }

        public string VariableName { get; }
    }

    public class DefaultValue
    {
        public static readonly DefaultValue Default = new DefaultValue();
    }

    public class DefaultCommandParser : ICommandParser
    {
        #region Implementation of ICommandParser

        public bool Parse(string rawCommand, out string errorMessage, out string commandName, out object[] parameters,
            out VariableReference target, out VariableReference assignment)
        {
            errorMessage = null;
            commandName  = null;
            parameters   = Array.Empty<object>();
            assignment   = null;
            target       = null;

            bool hasAssignment            = rawCommand.Contains("=");
            bool hasTarget                = rawCommand.Contains("->");
            int  lastItemStart            = 0;
            bool isItem                   = false;
            bool whitespaceIncludedInItem = false; //turned to false when parsing strings

            List<object> parsed = new List<object>();
            for (var i = 0; i < rawCommand.Length; i++)
            {
                char ch = rawCommand[i];
                if (ch == ' ' || ch == '\t' || ch == '=' || ch == '\n')
                {
                    if (whitespaceIncludedInItem) continue;
                    if (isItem)
                    {
                        bool didParse = TryParseItem(rawCommand, lastItemStart, i - lastItemStart, out object item,
                            out errorMessage);
                        if (!didParse) return false;

                        if (item is VariableReference varRef)
                        {
                            if (hasAssignment && assignment == null) assignment = varRef;
                            else if (commandName == null) commandName           = varRef.VariableName;
                            else parsed.Add(item);
                        }
                        else parsed.Add(item);

                        isItem = false;
                    }

                    continue;
                }

                if (ch == '-' && rawCommand.Length > i + 1 && rawCommand[i + 1] == '>')
                {
                    bool didParse = TryParseItem(rawCommand, lastItemStart, i - lastItemStart, out object item,
                        out errorMessage);
                    if (!didParse) return false;
                    target = (VariableReference) item;
                    isItem = false;
                    i++;
                    continue;
                }
                else if (ch == '"')
                {
                    whitespaceIncludedInItem ^= true; //toggle white space inclusion
                }

                if (!isItem)
                {
                    isItem        = true;
                    lastItemStart = i;
                }
            }

            parameters = parsed.ToArray();

            return true;
        }

        private bool TryParseItem(string rawCommand, int start, int count, out object result, out string error)
        {
            result = null;
            error  = null;
            if (count == 0) return false;
            char firstChar = rawCommand[start];
            if (firstChar == '"')
            {
                if (rawCommand[start + count - 1] != '"')
                {
                    error = "String argument does not end with a quote.";
                    return false;
                }

                result = rawCommand.Substring(start + 1, count - 2);
                return true;
            }
            else if (Char.IsDigit(firstChar) || firstChar == '.')
            {
                string itemString = rawCommand.Substring(start, count);
                //int or float
                if (rawCommand.IndexOf('.', start, count) != -1)
                {
                    bool success = float.TryParse(itemString, out float floatValue);
                    if (success)
                    {
                        result = floatValue;
                        return true;
                    }
                    else
                    {
                        error = $"'{itemString}' tried as float, but could not be parsed.";
                        return false;
                    }
                }
                else
                {
                    bool success = int.TryParse(itemString, out int intValue);
                    if (success)
                    {
                        result = intValue;
                        return true;
                    }
                    else
                    {
                        error = $"'{itemString}' tried as int, but could not be parsed.";
                        return false;
                    }
                }
            }
            else
            {
                bool isBoolean = count == 1 &&
                                 (firstChar == 'y' || firstChar == 'n' || firstChar == 't' || firstChar == 'f');
                isBoolean |= CheckSubstringExactMatch("true") || CheckSubstringExactMatch("false") ||
                             CheckSubstringExactMatch("yes")  || CheckSubstringExactMatch("no");
                if (isBoolean)
                {
                    result = firstChar == 'y' || firstChar == 't';
                }
                else if (count == 1 && firstChar == '_')
                {
                    result = DefaultValue.Default;
                }
                else
                {
                    result = new VariableReference(rawCommand.Substring(start, count));
                }

                return true;
            }

/*
            error = $"'{rawCommand.Substring(start, count)}' could not be matched to any known types.";
            return false;
*/

            bool CheckSubstringExactMatch(string match)
            {
                if (count != match.Length) return false;
                for (var i = 0; i < match.Length; i++)
                {
                    if (rawCommand[start + i] != match[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion
    }
}