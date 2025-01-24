using System;
using System.Collections.Generic;
using System.IO;
using NonsensicalKit.Tools;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Updater.Editor
{
    public class UpdaterEditor : EditorWindow
    {
        [MenuItem("NonsensicalKit/Updater")]
        private static void ShowWindow()
        {
            GetWindow(typeof(UpdaterEditor));
        }

        private readonly string _path = Path.Combine(Application.streamingAssetsPath, "UpdaterInfo.json");

        private List<PatchInfo> _patchInfos;

        private string _newVersion;
        private string _newUrl;
        private Vector2 _scrollPos;

        private void CreateGUI()
        {
            _patchInfos = new List<PatchInfo>();

            if (File.Exists(_path))
            {
                _patchInfos = JsonTool.DeserializeObject<List<PatchInfo>>(File.ReadAllText(_path));
            }
        }

        private void OnGUI()
        {
            ShowInfos(_patchInfos);
            _newVersion = EditorGUILayout.TextField("版本", _newVersion);
            _newUrl = EditorGUILayout.TextField("链接", _newUrl);
            if (GUILayout.Button("添加新版本"))
            {
                if (Version.TryParse(_newVersion, out Version newVersion) == false)
                {
                    EditorUtility.DisplayDialog("格式错误", "版本格式错误", "确认");
                }
                else if (_patchInfos.Count > 0 && Version.Parse(_patchInfos[^1].Version) >= newVersion)
                {
                    EditorUtility.DisplayDialog("版本错误", "新版本需高于最后一个版本", "确认");
                }
                else if (Uri.TryCreate(_newUrl, UriKind.Absolute, out _) == false)
                {
                    EditorUtility.DisplayDialog("格式错误", "链接格式错误(注意要添加协议头)", "确认");
                }
                else
                {
                    _patchInfos.Add(new PatchInfo() { Version = _newVersion, PatchUrl = _newUrl });
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("保存"))
            {
                string error = string.Empty;
                Version lastVersion = null;
                foreach (PatchInfo patchInfo in _patchInfos)
                {
                    if (Version.TryParse(patchInfo.Version, out Version newVersion) == false)
                    {
                        error = $"版本{patchInfo.Version}格式错误";
                        break;
                    }

                    if (Uri.TryCreate(patchInfo.PatchUrl, UriKind.Absolute, out _) == false)
                    {
                        error = $"链接{patchInfo.PatchUrl}格式错误(注意要添加协议头)";
                        break;
                    }

                    if (lastVersion != null && lastVersion >= newVersion)
                    {
                        error = $"版本{newVersion}处有版本号顺序错误，";
                        break;
                    }

                    lastVersion = newVersion;
                }

                if (string.IsNullOrEmpty(error))
                {
                    var infosJson = JsonTool.SerializeObject(_patchInfos);
                    File.WriteAllText(_path, infosJson);
                }
                else
                {
                    EditorUtility.DisplayDialog("无法保存", error, "确认");
                }
            }
        }


        private void ShowInfos(List<PatchInfo> patchInfos)
        {
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            PatchInfo deleteTarget = null;
            foreach (var info in patchInfos)
            {
                info.Version = EditorGUILayout.TextField("版本", info.Version);
                info.PatchUrl = EditorGUILayout.TextField("链接", info.PatchUrl);
                if (GUILayout.Button("删除此版本"))
                {
                    deleteTarget = info;
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (deleteTarget != null)
            {
                _patchInfos.Remove(deleteTarget);
            }
        }
    }
}
