using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static F;

public class WebRequest {
	public static IEnumerator GetJsonString(Url url, IEnumerable<Tuple<string, string>> headers)
    {
        UnityWebRequest www = UnityWebRequest.Get(url.ToString());
        headers.ForEach(t => www.SetRequestHeader(t.Item1, t.Item2));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return None;
        }
        else
        {
            yield return Some(www.downloadHandler.text);
        }
    }
}
