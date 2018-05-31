using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using static MonoBehaviorExt;
using System.Linq;

public static class VersionHelper
{

    static string GetPath(string rootPath)
    {
        return rootPath + "/version.xml";
    }

    static IEnumerator GetVersionNode(string rootPath)
    {
        XmlNode versionNode = null;
        var filePath = GetPath(rootPath);
        string result = "";
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            result = www.text;
        }
        else
        {
            result = File.ReadAllText(filePath);
        }

        if (!string.IsNullOrEmpty(result))
        {
            XmlDocument xml = new XmlDocument();

            xml.LoadXml(result);

            var list = xml.GetElementsByTagName("version");
            if (list.Count == 1)
            {
                versionNode = list.Item(0);
            }
        }
        yield return versionNode;
    }

    public static IEnumerator GetVersion(string rootPath)
    {
        Coroutine<XmlNode> coroutineObject = new Coroutine<XmlNode>(GetVersionNode(rootPath));
        foreach(var x in coroutineObject.enumerable) { yield return x; }

        var node = coroutineObject.Value;
        if (node != null)
        {
            yield return node.InnerText;
        }
        yield return null;
    }

    public static IEnumerator SetVersion(string rootPath, string newVersion)
    {
        Coroutine<XmlNode> coroutineObject = new Coroutine<XmlNode>(GetVersionNode(rootPath));
        foreach(var x in coroutineObject.enumerable) { yield return x; }

        var node = coroutineObject.Value;
        if (node != null)
        {
            node.InnerText = newVersion;
            node.OwnerDocument.Save(GetPath(rootPath));
        }
    }
}
