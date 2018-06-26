using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class StringHelper : MonoBehaviour {
    public static string Append(string @this, string appendage)
        => @this + appendage;

    public static string GetPrettyNumber(long totalCount)
        => GetPrettyNumber((double)totalCount);

    public static string GetPrettyNumber(int totalCount)
        => GetPrettyNumber((double) totalCount);

    public static string GetPrettyNumber(double totalCount)
    {
        if (totalCount > 100000)
        {
            var millions = totalCount / 1000000;
            return millions.ToString("0.00") + "M";
        }
        return (totalCount / 1000).ToString("0.00") + "K";
    }

    public static string GetFormattedInt(int totalCount)
        => totalCount.ToString("#,#", CultureInfo.InvariantCulture);
}
