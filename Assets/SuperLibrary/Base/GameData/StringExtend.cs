using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtend
{
    public static T ToEnum<T>(this string value, T defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        try
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        catch
        {
            Debug.LogError($"Cannot Parse '{value}' to enum '{typeof(T).Name}'");
            return defaultValue;
        }
    }
}
