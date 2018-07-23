
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using static F;

public class LocalSettingsHelper
{
    public static IEnumerator GetSettings(string filePath)
    {
        NestableCoroutine<string> getTextR = new NestableCoroutine<string>(TextHelper.GetText(filePath));
        foreach(var x in getTextR.Routine) { yield return x; }

        yield return getTextR.Value.Map(XmlHelper.LoadXml)
            .Map(x => x.GetElementsByTagName("parameter"))
            .AsEither()
            .AsEnumerable()
            .Bind(l => l.Cast<XmlNode>())
            .Bind(GetNameValuePair);
    }

    static Option<Tuple<string, string>> GetNameValuePair(XmlNode node)
    {
        if (node.Attributes["name"] != null && node.Attributes["value"] != null)
        {
            return Some(new Tuple<string, string>(node.Attributes["name"].Value, node.Attributes["value"].Value));
        }
        return None;
    }

    public static Option<T> GetSetting<T>(Dictionary<string, string> settings, string name) where T : IComparable
    {
        IComparable value = null;

        Type settingType = typeof(T);
        if (settings.ContainsKey(name) && settings[name] != "" && !settings[name].StartsWith("#{"))
        {
            var valueString = settings[name];
            if (settingType == typeof(int))
            {
                value = int.Parse(valueString);
            }
            else if (settingType == typeof(float))
            {
                value = float.Parse(valueString);
            }
            else if (settingType == typeof(string))
            {
                value = valueString;
            }
        }
        else if (PlayerPrefs.HasKey(name))// Look for value in PlayerPrefs
        {
            if (settingType == typeof(int))
            {
                value = PlayerPrefs.GetInt(name);
            }
            else if (settingType == typeof(float))
            {
                value = PlayerPrefs.GetFloat(name);
            }
            else if (settingType == typeof(string))
            {
                value = PlayerPrefs.GetString(name);
            }
        }

        // return value
        return value == null ? None : Some((T)value);
    }
}

public static class SettingsExt
{
    public static Func<string, Option<T>> GetSetting<T>(this Dictionary<string, string> _settings) where T : IComparable
        => name
        => LocalSettingsHelper.GetSetting<T>(_settings, name);
}
