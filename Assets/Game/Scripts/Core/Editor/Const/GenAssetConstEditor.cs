using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HK.Editor
{
    public class GenAssetConstEditor : UnityEditor.Editor
    {
        static string csName = "AssetConst";
        static string folderPath = "Assets/Game/Scripts/Runtime/Generate";
        static string resPath = "Assets/Game/Res";
        static string template = "Assets/Resources/Template/AssetsConstTemplate.txt";

        [MenuItem("Tool/GenAssetsConst")]
        static void Gen()
        {
            string path = $"{folderPath}/{csName}.cs";

            StringBuilder sb = new StringBuilder();
            Bridging(ref sb, resPath);

            var readAllText = File.ReadAllText(template);
            var content = readAllText.Replace("#CONSTCONTENT#", sb.ToString()).Replace("\\", "/")
                .Replace("#CLASSNAME#", csName);
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            File.WriteAllText(path, content);

            Debug.Log("生成完成！");
        }

        static void Bridging(ref StringBuilder sb, string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            var fileInfos = info.GetFiles();
            var directoryInfos = info.GetDirectories();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Name.Contains(".meta"))
                    continue;

                string filePath = fileInfo.FullName.Substring(Application.dataPath.Length - 6);
                var name = filePath.Replace("\\", "_")
                    .Replace(".", "_")
                    .Replace("-", "_")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("（", "")
                    .Replace("）", "")
                    .Replace("{", "")
                    .Replace("}", "")
                    .Replace("【", "")
                    .Replace("】", "")
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("+", String.Empty);
                sb.AppendLine($"public const string {name} = @\"{filePath}\";");
            }

            foreach (var directoryInfo in directoryInfos)
            {
                Bridging(ref sb, directoryInfo.FullName);
            }
        }
    }
}