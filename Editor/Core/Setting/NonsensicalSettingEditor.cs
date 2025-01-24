using System.IO;
using NonsensicalKit.Core.Setting;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Setting
{
    public class NonsensicalSettingEditor
    {
        [MenuItem("NonsensicalKit/Init NonsensicalSetting|初始化配置文件")]
        public static void InitSetting()
        {
            if (Directory.Exists(Application.dataPath + "/Resources") == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (File.Exists(Application.dataPath + "/Resources/NonsensicalSetting.asset"))
            {
                Debug.Log("已存在配置文件");
                return;
            }

            NonsensicalSetting asset = Object.Instantiate<NonsensicalSetting>(NonsensicalSetting.DefaultSetting);
            AssetDatabase.CreateAsset(asset, "Assets/Resources/NonsensicalSetting.asset");
            Debug.Log("配置文件创建完成，路径：\"/Resources/NonsensicalSetting.asset\"");
            Selection.activeObject = asset;
        }
    }
}
