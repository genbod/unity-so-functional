using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;
using static F;

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
        var coroutineObject = new NestableCoroutine<XmlNode>(GetVersionNode(rootPath));
        foreach(var x in coroutineObject.Routine) { yield return x; }
        
        yield return coroutineObject.Value.Match(
            () => None,
            (f) => Some(f.InnerText));

        //var node = coroutineObject.Value;
        //if (node != null)
        //{
        //    yield return node.InnerText;
        //}
        //yield return null;
    }

    public static IEnumerator SetVersion(string rootPath, string newVersion)
    {
        var coroutineObject = new NestableCoroutine<XmlNode>(GetVersionNode(rootPath));
        foreach(var x in coroutineObject.Routine) { yield return x; }

        coroutineObject.Value.ForEach(n =>
        {
            n.InnerText = newVersion;
            n.OwnerDocument.Save(GetPath(rootPath));
        });

        //var node = coroutineObject.Value;
        //if (node != null)
        //{
        //    node.InnerText = newVersion;
        //    node.OwnerDocument.Save(GetPath(rootPath));
        //}
    }
}
