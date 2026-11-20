using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public static class EnumerableExtensions
{
    public static IEnumerable<SelectListItem> ToSelectList<T, TTextProperty, TValueProperty>(this IEnumerable<T> instance, Func<T, TTextProperty> text, Func<T, TValueProperty> value, Func<T, bool> selectedItem = null)
    {
        return instance.Select(t => new SelectListItem
        {
            Text = Convert.ToString(text(t)),
            Value = Convert.ToString(value(t)),
            Selected = selectedItem != null ? selectedItem(t) : false
        });
    }

    public static IEnumerable<object> ToSelectList<T, TTextProperty, TValueProperty>(this IEnumerable<T> instance, Func<T, TTextProperty> text, Func<T, TValueProperty> value, Func<T, TValueProperty> fieldType)
    {
        return instance.Select(t => new
        {
            Text = Convert.ToString(text(t)),
            Value = Convert.ToString(value(t)),
            FieldType = Convert.ToString(fieldType(t))
        });
    }

    public static IEnumerable<object> ToSelectListExtended<T, TTextProperty, TValueProperty>(this IEnumerable<T> instance, Func<T, TTextProperty> text, Func<T, TValueProperty> value, Func<T, TTextProperty> description)
    {
        return instance.Select(t => new
        {
            Text = Convert.ToString(text(t)),
            Value = Convert.ToString(value(t)),
            Description = Convert.ToString(description(t))
        });
    }
    public static IEnumerable<object> ToSelectListExtended<T, TTextProperty, TValueProperty, TBoolProperty>(this IEnumerable<T> instance, Func<T, TTextProperty> text, Func<T, TValueProperty> value, Func<T, TTextProperty> description, Func<T, TBoolProperty> allowAllRegionSelect)
    {
        return instance.Select(t => new
        {
            Text = Convert.ToString(text(t)),
            Value = Convert.ToString(value(t)),
            Description = Convert.ToString(description(t)),
            AllowAllRegionSelect = Convert.ToString(allowAllRegionSelect(t)).ToLower()
        });
    }
}