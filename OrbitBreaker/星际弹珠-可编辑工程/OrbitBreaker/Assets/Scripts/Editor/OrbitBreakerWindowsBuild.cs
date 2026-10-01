using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerWindowsBuild
    {
        public static string OutputRoot => Path.GetFullPath(Path.Combine(Application.dataPath,"../../../"));
        [MenuItem("Tools/Orbit Breaker/Build Windows Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before building.");
            AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            string directory=Path.Combine(OutputRoot,"星际弹珠-WindowsDemo");
            Directory.CreateDirectory(directory);
            var oldMode=PlayerSettings.fullScreenMode;int oldW=PlayerSettings.defaultScreenWidth,oldH=PlayerSettings.defaultScreenHeight;
            try
            {
                PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
                PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=new[]{"Assets/Scenes/OrbitBreakerBattle.unity"},locationPathName=Path.Combine(directory,"OrbitBreaker.exe"),
                    target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                var r=new Result{result=report.summary.result.ToString(),unity=Application.unityVersion,timeUtc=DateTime.UtcNow.ToString("O"),
                    output=report.summary.outputPath,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,
                    seconds=report.summary.totalTime.TotalSeconds,
                    messages=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content).ToArray()};
                File.WriteAllText(Path.Combine(Application.dataPath,"../Docs/EVIDENCE/UPLOAD01-build.json"),JsonUtility.ToJson(r,true));
                Debug.Log("Orbit Breaker Windows build: "+r.result);
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(Application.dataPath,"../Docs/EVIDENCE/UPLOAD01-build-exception.txt"),e.ToString());throw;
            }
            finally{PlayerSettings.fullScreenMode=oldMode;PlayerSettings.defaultScreenWidth=oldW;PlayerSettings.defaultScreenHeight=oldH;AssetDatabase.SaveAssets();}
        }
        [Serializable] private class Result {public string result,unity,timeUtc,output;public int errors,warnings;public ulong bytes;public double seconds;public string[] messages;}
    }
}

