using System.Xml;

public class XmlHelper {
    public static XmlDocument LoadXml(string text)
    {
        XmlDocument userXml1 = new XmlDocument();
        userXml1.LoadXml(text);
        return userXml1;
    }
}
