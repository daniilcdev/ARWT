#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ARWT.Core
{
    public class PostBuildApplyHtmlPatches : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            if (target != BuildTarget.WebGL)
            {
                return;
            }

#if UNITY_2021_1_OR_NEWER
            var template = PlayerSettings.WebGL.template.Replace("PROJECT:", string.Empty);
            // delete modified template
            var indexHtmlPath = Path.Combine($"{Application.dataPath}/WebGLTemplates/{template}", "index.html");
            File.Delete(indexHtmlPath);

            // revert template to initial state
            File.Move(indexHtmlPath.Replace("index.html", "index.tmp.html"), indexHtmlPath);

            // delete copied temp file from output
            File.Delete(Path.Combine(targetPath, "index.tmp.html"));
#endif
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            var template = PlayerSettings.WebGL.template.Replace("PROJECT:", string.Empty);
            var indexHtmlPath = Path.Combine($"{Application.dataPath}/WebGLTemplates/{template}", "index.html");
            // template will be patched before build, need to back up it in unmodified state
            File.Copy(indexHtmlPath, indexHtmlPath.Replace("index.html", "index.tmp.html"));

            var indexHtmlContent = File.ReadAllText(indexHtmlPath);
            var patchFiles = Directory.GetFiles(Application.dataPath + "/Editor/HtmlPatches", "*.html");

            foreach (var patchFile in patchFiles)
            {
                var anchor = Path.GetFileNameWithoutExtension(patchFile);
#if UNITY_2021_1_OR_NEWER
                indexHtmlContent = indexHtmlContent.Replace(anchor, File.ReadAllText(patchFile));
#else
                indexHtmlContent = indexHtmlContent.Replace(anchor, string.Empty);
#endif

                Debug.Log($"HTML patch applied: {Path.GetFileName(patchFile)}");
            }

            File.WriteAllText(indexHtmlPath, indexHtmlContent);
        }
    }
}
#endif