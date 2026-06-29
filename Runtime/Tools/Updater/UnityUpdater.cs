using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.NetworkTool;
using UnityEngine;
using UnityEngine.Networking;

namespace NonsensicalKit.Core.Updater
{
    public class UnityUpdater
    {
        private readonly string _getLatestUrl = "https://api.github.com/repos/NonsensicalHub/NonsensicalPatch/releases/latest";
        private readonly string _updaterFolderPath = Path.Combine(Application.temporaryCachePath, $"NonsensicalKit/Updater");

        private string _updaterPath;

        public IEnumerator GetLatestUpdater(Action<string> callback)
        {
            if (PlatformInfo.IsWindow == false)
            {
                callback?.Invoke(null);
                yield break;
            }

            UnityWebRequest getLatestRelease = new UnityWebRequest();
            yield return getLatestRelease.Get(_getLatestUrl);
            if (getLatestRelease.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(null);
                yield break;
            }

            string exeUrl = null;
            JObject jo = JObject.Parse(getLatestRelease.downloadHandler.text);

            try
            {
                var id = jo["id"].ToString();
                _updaterPath = Path.Combine(_updaterFolderPath, $"NonsensicalPatchUpdater_{id}.exe");
                if (File.Exists(_updaterPath))
                {
                    callback?.Invoke(_updaterPath);
                    yield break;
                }

                JArray assets = jo["assets"] as JArray;
                foreach (var asset in assets)
                {
                    if (asset["name"].ToString() == "NonsensicalPatchUpdater.zip")
                    {
                        exeUrl = asset["browser_download_url"].ToString();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                callback?.Invoke(null);
                yield break;
            }

            if (string.IsNullOrEmpty(exeUrl))
            {
                callback?.Invoke(null);
                yield break;
            }

            UnityWebRequest getZip = new UnityWebRequest();
            yield return getZip.Get(exeUrl);
            if (getZip.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(null);
                yield break;
            }

            FileTool.EnsureDir(_updaterFolderPath);

            ZipTool.UnzipFile(getZip.downloadHandler.data, _updaterFolderPath);

            callback?.Invoke(_updaterPath);
        }

        public IEnumerator GetTargetUpdater(string url, Action<string> callback, bool forceUpdate = false)
        {
            if (PlatformInfo.IsWindow == false)
            {
                callback?.Invoke(null);
                yield break;
            }

            _updaterPath = Path.Combine(_updaterFolderPath, "NonsensicalPatchUpdater.exe");
            if (File.Exists(_updaterPath))
            {
                if (forceUpdate)
                {
                    File.Delete(_updaterPath);
                }
                else
                {
                    callback?.Invoke(_updaterPath);
                    yield break;
                }
            }

            UnityWebRequest getZip = new UnityWebRequest();
            yield return getZip.Get(url);
            if (getZip.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(null);
                yield break;
            }

            FileTool.EnsureDir(_updaterFolderPath);

            ZipTool.UnzipFile(getZip.downloadHandler.data, _updaterFolderPath);

            callback?.Invoke(_updaterPath);
        }

        public IEnumerator Check(string url, Action<bool, List<PatchInfo>> callback)
        {
            UnityWebRequest getJson = new UnityWebRequest();
            yield return getJson.Get(url);
            string json = getJson.downloadHandler.text;
            List<PatchInfo> infos = null;
            try
            {
                infos = JsonConvert.DeserializeObject<List<PatchInfo>>(json);
            }
            catch
            {
            }

            if (infos != null)
            {
                callback?.Invoke(Check(infos), infos);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infos"></param>
        /// <returns>是否有新版本</returns>
        public bool Check(List<PatchInfo> infos)
        {
            if (PlatformInfo.IsWindow == false)
            {
                return false;
            }

            if (infos.Count == 0)
            {
                return false;
            }

            Version newestVersion = Version.Parse(infos[^1].Version);
            Version crtVersion = Version.Parse(Application.version);

            return newestVersion > crtVersion;
        }

        public bool Update(List<PatchInfo> infos, string exeName = null)
        {
            if (PlatformInfo.IsWindow == false)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_updaterPath))
            {
                return false;
            }

            if (File.Exists(_updaterPath) == false)
            {
                return false;
            }

            Version crtVersion = Version.Parse(Application.version);
            List<string> urls = new List<string>();
            foreach (var info in infos)
            {
                Version version = Version.Parse(info.Version);
                if (version > crtVersion)
                {
                    urls.Add(info.PatchUrl);
                }
            }

            UpdateConfig update = new UpdateConfig
            {
                TargetRootPath = Application.dataPath + "\\..",
                AutoStartPath = Application.dataPath + $"\\..\\{exeName ?? Application.productName}.exe",
                PatchUrls = urls
            };

            var rawString = JsonConvert.SerializeObject(update);
            var replaceString = rawString.Replace("\"", "\\\"");
            Process.Start(_updaterPath, $"\"{Process.GetCurrentProcess().Id}\" \"{replaceString}\"");
            Application.Quit();
            return true;
        }

        public bool Update(List<string> patchUrls, string exeName = null)
        {
            if (PlatformInfo.IsWindow == false)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_updaterPath))
            {
                return false;
            }

            if (File.Exists(_updaterPath) == false)
            {
                return false;
            }

            UpdateConfig update = new UpdateConfig
            {
                TargetRootPath = Application.dataPath + "\\..",
                AutoStartPath = Application.dataPath + $"\\..\\{exeName ?? Application.productName}.exe",
                PatchUrls = patchUrls
            };

            var rawString = JsonConvert.SerializeObject(update);
            var replaceString = rawString.Replace("\"", "\\\"");
            Process.Start(_updaterPath, $"\"{Process.GetCurrentProcess().Id}\" \"{replaceString}\"");
            Application.Quit();
            return true;
        }

        private class UpdateConfig
        {
            /// <summary>
            /// 所有需要安装的补丁的配置信息
            /// </summary>
            public List<string> PatchUrls { get; set; }

            /// <summary>
            /// 补丁目标根目录
            /// </summary>
            public string TargetRootPath { get; set; }

            /// <summary>
            /// 补丁打完后自动执行的可执行文件路径,为空时不自动执行
            /// </summary>
            public string AutoStartPath { get; set; }
        }
    }
}
