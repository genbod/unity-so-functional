using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextHelper {
    public static IEnumerator GetText(string filePath)
    {
        var opt = Url.Of(filePath);
        if (opt.IsSome())
        {
            var routine = opt.Map(WebRequest.GetWWWText);
            NestableCoroutine<string> coroutine = new NestableCoroutine<string>(routine);
            foreach (var x in coroutine.Routine) { yield return x; }
            yield return coroutine.Value;
        }
        else
        {
            yield return File.ReadAllText(filePath);
        }
    }
}
