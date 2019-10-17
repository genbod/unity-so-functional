using System.Globalization;
using UnityEngine;
using static F;

[CreateAssetMenu]
public class StringHelper : ScriptableObject
{
    public static string Append(string @this, string appendage)
        => @this + appendage;

    public static string GetPrettyNumber(long totalCount)
        => GetPrettyNumber((double)totalCount);

    public static string GetPrettyNumber(int totalCount)
        => GetPrettyNumber((double) totalCount);

    public static string GetPrettyNumber(float totalCount)
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

    public static Option<string> GetFormattedIntAsOptional(System.Object obj)
    {
        if (obj is int)
        {
            return Some(GetFormattedInt((int)obj));
        }
        else return None;
    }

    public static string GetFormattedInt(System.Object obj)
    {
        if (obj is int)
        {
            return GetFormattedInt((int)obj);
        }
        else return "NOT AN INT";
    }

    public static string GetFormattedInt(int totalCount)
        => totalCount.ToString("#,#", CultureInfo.InvariantCulture);
}
