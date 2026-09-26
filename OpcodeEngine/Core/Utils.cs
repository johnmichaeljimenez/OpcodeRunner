using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpcodeEngine.Core;

public static class Utils
{
    public static bool IsMatch(string candidate, string query)
    {
        if (candidate == null || query == null)
            return false;

        if (query.Contains("*"))
            return IsMatchWildcard(candidate, query);

        if (candidate.Contains("*"))
            return IsMatchWildcard(query, candidate);

        return string.Equals(candidate, query, StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasPrefix(string prefix, ref string input)
    {
        if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(input) || !input.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            return false;

        input = input.Substring(prefix.Length);
        return true;
    }

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

            if (c == '"')   //YAGNI: no double quotes needed yet
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

    public static bool IsMatchWildcard(string input, string wildcardPattern)
    {
        string regexPattern = Regex.Escape(wildcardPattern)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".");

        regexPattern = "^" + regexPattern + "$";

        return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
    }
}
