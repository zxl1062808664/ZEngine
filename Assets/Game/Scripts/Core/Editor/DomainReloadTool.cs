using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace DragonSoul.Editor
{
    public class DomainReloadTool
    {
        private const string logColor = "<color=grey>{0}</color>";
        private const string ReloadKey = "DomainReloadTool.Reload";
        private const string ReloadDomainTimerKey = "DomainReloadTool.ReloadDomainTimer";
        private const string CompileDomainTimerKey = "DomainReloadTool.CompileDomainTimer";

        [InitializeOnLoadMethod]
        static void Init()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void OnCompilationStarted(object obj)
        {
            Debug.LogFormat(logColor, "Begin Compile Domain...");
            //记录时间
            SessionState.SetInt(CompileDomainTimerKey, (int)(EditorApplication.timeSinceStartup * 1000));
        }

        private static void OnCompilationFinished(object obj)
        {
            var timeMS = (int)(EditorApplication.timeSinceStartup * 1000) -
                         SessionState.GetInt(CompileDomainTimerKey, 0);
            Debug.LogFormat(logColor, $"End Compile Domain : {timeMS} ms");
        }


        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (!SessionState.GetBool(ReloadKey, false))
                        ForceReloadDomain();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    SessionState.SetBool(ReloadKey, false);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
            }
        }

        //开始reload domain
        private static void OnBeforeAssemblyReload()
        {
            Debug.LogFormat(logColor, "Begin Reload Domain...");
            //记录时间
            SessionState.SetInt(ReloadDomainTimerKey, (int)(EditorApplication.timeSinceStartup * 1000));
        }

        //结束reload domain
        private static void OnAfterAssemblyReload()
        {
            var timeMS = (int)(EditorApplication.timeSinceStartup * 1000) -
                         SessionState.GetInt(ReloadDomainTimerKey, 0);
            Debug.LogFormat(logColor, $"End Reload Domain : {timeMS} ms");
            SessionState.SetBool(ReloadKey, true);
        }

        private static void ForceReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }
    }
}