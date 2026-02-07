using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class ScriptsToReviewExportWindow : EditorWindow
    {
        private const string DefaultSourceFolder = "Assets/Scripts";
        private const string DefaultOutputFile = "Assets/scripts_review.txt";
        private const string FileSeparator = "\n\n" + "==========\n\n";

        private string _sourceFolder = DefaultSourceFolder;
        private string _outputPath = DefaultOutputFile;
        private Vector2 _scrollPos;
        private string _lastLog = "";

        [MenuItem("Tools/Export Scripts to Review TXT")]
        public static void ShowWindow()
        {
            var w = GetWindow<ScriptsToReviewExportWindow>("Scripts → Review TXT");
            w.minSize = new Vector2(400, 200);
        }

        private void OnGUI()
        {
            GUILayout.Space(8);
            EditorGUILayout.LabelField("Сбор всех .cs из папки в один TXT для ревью (например, в GPT).", EditorStyles.wordWrappedLabel);
            GUILayout.Space(8);

            _sourceFolder = EditorGUILayout.TextField("Папка с скриптами", _sourceFolder);
            _outputPath = EditorGUILayout.TextField("Выходной TXT файл", _outputPath);

            GUILayout.Space(8);
            if (GUILayout.Button("Экспорт", GUILayout.Height(28)))
                Export();

            if (!string.IsNullOrEmpty(_lastLog))
            {
                GUILayout.Space(8);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(_lastLog, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
            }
        }

        private void Export()
        {
            string projectPath = Application.dataPath;
            if (projectPath.EndsWith("/Assets") || projectPath.EndsWith("\\Assets"))
                projectPath = Path.GetDirectoryName(projectPath);
            string sourceFull = Path.Combine(projectPath, _sourceFolder.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(sourceFull))
            {
                _lastLog = $"Ошибка: папка не найдена:\n{sourceFull}";
                return;
            }

            var files = new List<string>();
            CollectCsFiles(sourceFull, sourceFull, files);

            if (files.Count == 0)
            {
                _lastLog = $"В папке нет .cs файлов:\n{sourceFull}";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"# Export: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Source: {_sourceFolder}");
            sb.AppendLine($"# Files: {files.Count}");
            sb.AppendLine();
            sb.Append(FileSeparator);

            foreach (string fullPath in files)
            {
                string relativePath = fullPath.Substring(sourceFull.Length).TrimStart(Path.DirectorySeparatorChar, '/').Replace('\\', '/');
                sb.AppendLine($"# FILE: {relativePath}");
                sb.AppendLine();
                try
                {
                    string content = File.ReadAllText(fullPath);
                    sb.Append(content);
                }
                catch (System.Exception e)
                {
                    sb.AppendLine($"# Read error: {e.Message}");
                }
                sb.Append(FileSeparator);
            }

            string outFull = Path.Combine(projectPath, _outputPath.Replace('/', Path.DirectorySeparatorChar));
            string outDir = Path.GetDirectoryName(outFull);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            try
            {
                File.WriteAllText(outFull, sb.ToString());
                _lastLog = $"Готово.\nЗаписано файлов: {files.Count}\nПуть: {outFull}";
                EditorUtility.RevealInFinder(outFull);
            }
            catch (System.Exception e)
            {
                _lastLog = $"Ошибка записи:\n{e.Message}";
            }
        }

        private static void CollectCsFiles(string rootFull, string dirFull, List<string> result)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dirFull, "*.cs"))
                    result.Add(Path.GetFullPath(f));
                foreach (string d in Directory.GetDirectories(dirFull))
                    CollectCsFiles(rootFull, d, result);
            }
            catch { /* skip inaccessible */ }
        }
    }
}
