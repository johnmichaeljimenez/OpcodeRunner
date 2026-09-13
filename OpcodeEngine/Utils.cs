using System;
using System.Collections.Generic;
using System.Text;

namespace OpcodeEngine;

public static class Utils
{
    public static bool TryParseBool(string input, out bool result)
    {
        if (bool.TryParse(input, out result))
        {
            return true;
        }

        if (input == "1")
        {
            result = true;
            return true;
        }
        if (input == "0")
        {
            result = false;
            return true;
        }

        result = false;
        return false;
    }

    public static List<string> SplitArguments(this string input)
    {
        var result = new List<string>();
        var currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ' ' && !inQuotes)
            {
                if (currentToken.Length > 0)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                currentToken.Append(c);
            }
        }

        if (currentToken.Length > 0)
        {
            result.Add(currentToken.ToString());
        }

        return result;
    }
}
