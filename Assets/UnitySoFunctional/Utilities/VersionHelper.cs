using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;
using static F;

public static class VersionHelper
{
    static Exceptional<XmlNode> GetVersionNode(XmlDocument doc)
    {
        var list = doc.GetElementsByTagName("version");
        if (list.Count == 1)
        {
            return list.Item(0);
        }
        else return new InvalidDataException();
    }

    public static IEnumerator GetVersion(string filePath)
    {
        NestableCoroutine<string> getTextR = new NestableCoroutine<string>(TextHelper.GetText(filePath));
        foreach(var x in getTextR.Routine) { yield return x; }

        yield return getTextR.Value.Map(XmlHelper.LoadXml)
            .Bind(GetVersionNode)
            .Map(n => n.InnerText);
    }

    public static IEnumerator SetVersion(string filePath, string newVersion)
    {
        var getTextR = new NestableCoroutine<string>(TextHelper.GetText(filePath));
        foreach(var x in getTextR.Routine) { yield return x; }

        yield return getTextR.Value.Map(XmlHelper.LoadXml)
            .Bind(GetVersionNode)
            .ForEach(n =>
            {
                n.InnerText = newVersion;
                n.OwnerDocument.Save(filePath);
            });
    }
}
