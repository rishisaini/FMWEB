using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public static class DateExtensions
{
    public static string ToDateString(this DateTime? date)
    {
        if (!date.HasValue)
            return "";

        return date.Value.ToString("dd/MM/yyyy");
    }
}