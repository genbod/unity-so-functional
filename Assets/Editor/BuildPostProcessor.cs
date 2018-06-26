using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using static F;

public class BuildPostProcessor : MonoBehaviour {

	[PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var buildVersionPath = pathToBuiltProject + "/StreamingAssets/version.xml";
        var sourceVersionPath = Application.streamingAssetsPath + "/version.xml";
        NestableCoroutine<string> coroutineObject = new NestableCoroutine<string>(VersionHelper.GetVersion(sourceVersionPath));
        foreach(var x in coroutineObject.Routine) {}

        var build = coroutineObject.Value.Map(text => text.Split('.'))
            .Bind(GetBuildVersion)
            .ForEach( b =>
            {
                NestableCoroutine<object> setBuildVersionRoutine = new NestableCoroutine<object>(VersionHelper.SetVersion(buildVersionPath, b));
                foreach (var x in setBuildVersionRoutine.Routine) { }

                NestableCoroutine<object> setSourceVersionRoutine = new NestableCoroutine<object>(VersionHelper.SetVersion(sourceVersionPath, b));
                foreach (var x in setSourceVersionRoutine.Routine) { }
            });
    }

    private static Exceptional<string> GetBuildVersion(string[] versionParts)
    {
        if (versionParts.Length == 4)
        {
            var major = versionParts[0];
            var minor = versionParts[1];
            var buildDate = versionParts[2];
            var buildNumber = int.Parse(versionParts[3]);

            DateTime now = DateTime.Now;
            var curTimeStamp = now.Year.ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");
            buildNumber = curTimeStamp == buildDate ? buildNumber + 1 : 1;

            var build = major + "." + minor + "." + curTimeStamp + "." + buildNumber.ToString();
            return build;
        }
        return new ArgumentOutOfRangeException();
    }
}
