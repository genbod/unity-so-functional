using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextUtils
{
    public static string GetPrettyNumber(double totalCount)
    {
        if (totalCount > 100000)
        {
            var millions = totalCount / 1000000;
            return millions.ToString("0.00") + "M";
        }
        return (totalCount / 1000).ToString("0.00") + "K";
    }
}
