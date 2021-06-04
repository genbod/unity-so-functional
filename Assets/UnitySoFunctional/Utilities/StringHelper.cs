using DragonDogStudios.UnitySoFunctional.Functional;
using static DragonDogStudios.UnitySoFunctional.Functional.F;
using System.Globalization;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
    [CreateAssetMenu]
    public class StringHelper : ScriptableObject
    {
        public static string Append(string @this, string appendage)
            => @this + appendage;

        public static string GetPrettyNumber(long totalCount)
            => GetPrettyNumber((double)totalCount);

        public static string GetPrettyNumber(int totalCount)
            => GetPrettyNumber((double)totalCount);

        public static string GetPrettyNumber(float totalCount)
            => GetPrettyNumber((double)totalCount);

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
            if (obj is int i)
            {
                return Some(GetFormattedInt(i));
            }
            else return None;
        }

        public static string GetFormattedInt(System.Object obj)
        {
            if (obj is int i)
            {
                return GetFormattedInt(i);
            }
            else return "NOT AN INT";
        }

        public static string GetFormattedInt(int totalCount)
            => totalCount.ToString("#,#", CultureInfo.InvariantCulture);
    }
}