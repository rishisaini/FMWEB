using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public static class StringExtensions
{
    public static string ToStringHandleEmpty(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return "-";
        return str;
    }
    public static string RemoveLeadingTilde(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.StartsWith("~", StringComparison.Ordinal)
            ? input.Substring(1)
            : input;
    }
}