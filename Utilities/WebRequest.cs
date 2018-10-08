using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static F;

public class WebRequest {
    public static IEnumerator GetWWWText(Url path)
    {
        WWW www = new WWW(path.ToString());
        yield return www;
        yield return Some(www.text);
    }

    public static IEnumerator GetWebText(Url url)
    {
        return GetWebText(url, Enumerable.Empty<Tuple<string, string>>());
    }

	public static IEnumerator GetWebText(Url url, IEnumerable<Tuple<string, string>> headers, bool treatEmptyStringAsError = false)
    {
        UnityWebRequest www = UnityWebRequest.Get(url.ToString());
        www.chunkedTransfer = false;
        headers.ForEach(t => www.SetRequestHeader(t.Item1, t.Item2));
        yield return www.SendWebRequest();
        while (!www.isDone)
        {
            yield return null;
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return Exceptional.Of<string>(new WebRequestException(www.error));
        }
        else if (treatEmptyStringAsError && String.IsNullOrEmpty(www.downloadHandler.text))
        {
            yield return Exceptional.Of<string>(new WebRequestException("Result was empty"));
        }
        else yield return www.downloadHandler.text;
    }

    public static IEnumerator GetWebTexture(Url url)
    {
        return GetWebTexture(url, Enumerable.Empty<Tuple<string, string>>());
    }

    public static IEnumerator GetWebTexture(Url url, IEnumerable<Tuple<string, string>> headers)
    {
        UnityWebRequest www = UnityWebRequest.Get(url.ToString());
        www.chunkedTransfer = false;
        headers.ForEach(t => www.SetRequestHeader(t.Item1, t.Item2));
        yield return www.SendWebRequest();
        while (!www.isDone)
        {
            yield return null;
        }
        
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return Exceptional.Of<Texture2D>(new WebRequestException(www.error));
        }
        else
        {
            yield return www.downloadHandler.data;
        }
    }
}

public class WebRequestException : Exception
{
    public WebRequestException(string message) : base(message) { }
}
