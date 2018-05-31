using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class BuildPostProcessor : MonoBehaviour {

	[PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var buildVersionPath = pathToBuiltProject + "/StreamingAssets";
        var sourceVersionPath = Application.streamingAssetsPath;
        Coroutine<string> coroutineObject = new Coroutine<string>(VersionHelper.GetVersion(sourceVersionPath));
        foreach(var x in coroutineObject.enumerable) {}
        
        var versionText = coroutineObject.Value;
        
        var versionParts = versionText.Split('.');
        var major = versionParts[0];
        var minor = versionParts[1];
        var buildDate = versionParts[2];
        var buildNumber = int.Parse(versionParts[3]);

        DateTime now = DateTime.Now;
        var curTimeStamp = now.Year.ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");
        buildNumber = curTimeStamp == buildDate ? buildNumber + 1 : 1;

        var build = major +"." + minor + "." + curTimeStamp + "." + buildNumber.ToString();

        Coroutine<object> setBuildVersionRoutine = new Coroutine<object>(VersionHelper.SetVersion(buildVersionPath, build));
        foreach(var x in setBuildVersionRoutine.enumerable) {}

        Coroutine<object> setSourceVersionRoutine = new Coroutine<object>(VersionHelper.SetVersion(sourceVersionPath, build));
        foreach (var x in setSourceVersionRoutine.enumerable) {}
    }
}
