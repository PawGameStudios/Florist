using System;
using System.Collections.Generic;
using Config;

public static class Extensions
{
    public static List<int> AllIndexesOf(this string str, string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("The string to find may not be empty", "value");

        List<int> indexes = new();
        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index);
            if (index == -1)
                return indexes;
            if (int.TryParse(str.Substring(index - 1, 1), out int indexValue))
                indexes.Add(indexValue);
        }
    }

    public static string RemoveIndexIndicators(this string str, string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("The string to find may not be empty", "value");

        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index);
            if (index == -1)
                return str;
            str = str.Remove(index - 1, 2);
        }
    }
}
