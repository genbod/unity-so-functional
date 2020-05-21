using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
    public static class JsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            try
            {
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.Items;
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public static string ToJson<T>(List<T> array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(List<T> array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public List<T> Items;
        }
        public static string FixJson(string value)
        {
            value = "{\"Items\":" + value + "}";
            return value;
        }
    }
}
