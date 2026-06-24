using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace NonsensicalKit.Core.Editor
{
    /// <summary>
    /// Unity Editor 窗口，用于批量导入预定义的 Git 包，支持从 GitCode 或 GitHub 获取。
    /// </summary>
    public class NonsensicalImporter : EditorWindow
    {
        #region Constants and Fields

        /// <summary>
        /// EditorPrefs 键，用于存储是否不再自动弹出窗口的设置。
        /// </summary>
        private const string EditorPrefKeyDoNotAutoShow = "NonsensicalImporter_DoNotAutoShow";

        /// <summary>
        /// EditorPrefs 键，用于存储上次选择的数据源（GitCode 或 GitHub）。
        /// </summary>
        private const string EditorPrefKeyLastSource = "NonsensicalImporter_LastSource";

        /// <summary>
        /// 数据源枚举。
        /// </summary>
        private enum DataSource
        {
            GitCode,
            GitHub
        }

        /// <summary>
        /// 预定义的固定包列表 (名称, GitCode URL, GitHub URL)。
        /// </summary>
        private static readonly (string name, string gitCodeUrl, string gitHubUrl)[] FixedPackages =
            new (string, string, string)[]
            {
                ("Core", "https://gitcode.com/NonsensicalLab/NonsensicalKit.Core.git",
                    "https://github.com/NonsensicalHub/NonsensicalKit.Core.git"),
                ("UGUI", "https://gitcode.com/NonsensicalLab/NonsensicalKit.UGUI.git",
                    "https://github.com/NonsensicalHub/NonsensicalKit.UGUI.git"),
                ("WebGL", "https://gitcode.com/NonsensicalLab/NonsensicalKit.WebGL.git",
                    "https://github.com/NonsensicalHub/NonsensicalKit.WebGL.git"),
                ("DigitalTwin", "https://gitcode.com/NonsensicalLab/NonsensicalKit.DigitalTwin.git",
                    "https://github.com/NonsensicalHub/NonsensicalKit.DigitalTwin.git"),
                ("Simulation", "https://gitcode.com/NonsensicalLab/NonsensicalKit.Simulation.git",
                    "https://github.com/NonsensicalHub/NonsensicalKit.Simulation.git"),
            };

        /// <summary>
        /// 当前选中的数据源。
        /// </summary>
        private DataSource _currentSource = DataSource.GitCode;

        /// <summary>
        /// 跟踪每个包是否被选中导入的布尔数组。
        /// </summary>
        private bool[] _selected;

        /// <summary>
        /// 滚动视图的位置。
        /// </summary>
        private Vector2 _scrollPosition;

        /// <summary>
        /// 用户是否选择了“不再自动弹出”选项。
        /// </summary>
        private bool _dontShowAgain = false;

        // --- 导入队列和状态管理 ---

        /// <summary>
        /// 待导入包的索引队列。
        /// </summary>
        private readonly Queue<int> _importQueue = new Queue<int>();

        /// <summary>
        /// 当前正在进行的 Add 请求。
        /// </summary>
        private AddRequest _currentAddRequest = null;

        /// <summary>
        /// 当前正在导入的包在 FixedPackages 中的索引。
        /// </summary>
        private int _currentPackageIndex = -1;

        /// <summary>
        /// 标记是否正在进行导入流程。
        /// </summary>
        private bool _isImporting = false;

        /// <summary>
        /// 已成功导入的包数量。
        /// </summary>
        private int _importedCount = 0;

        /// <summary>
        /// 总共需要导入的包数量。
        /// </summary>
        private int _totalToImport = 0;

        #endregion

        #region Menu Item and Initialization

        /// <summary>
        /// 在 Unity Editor 菜单中添加 "NonsensicalKit/Importer" 项，用于手动打开窗口。
        /// </summary>
        [MenuItem("Tools/NonsensicalKit/Importer", false, 30)]
        public static void ShowWindowManual()
        {
            GetWindow<NonsensicalImporter>(true, "Nonsensical Importer", true).Show();
        }

        /// <summary>
        /// 在 Unity Editor 启动时运行一次。检查是否已安装核心包，如果没有且用户未禁用，则自动弹出窗口。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // 使用 delayCall 确保在 Editor 初始化完成后执行
            EditorApplication.delayCall += () =>
            {
                // 检查是否已存在任何 NonsensicalKit 包
                string[] packagePaths = new string[]
                {
                    Path.Combine("Packages", "com.nonsensicallab.nonsensicalkit.core"),
                    Path.Combine("Packages", "com.nonsensicallab.nonsensicalkit.ugui"),
                    Path.Combine("Packages", "com.nonsensicallab.nonsensicalkit.webgl"),
                    Path.Combine("Packages", "com.nonsensicallab.nonsensicalkit.digitaltwin"),
                    Path.Combine("Packages", "com.nonsensicallab.nonsensicalkit.simulation")
                };

                bool anyPackageExists = false;
                foreach (var relativePath in packagePaths)
                {
                    // Application.dataPath 是 Assets 文件夹的路径
                    string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
                    if (Directory.Exists(fullPath))
                    {
                        anyPackageExists = true;
                        break;
                    }
                }

                // 如果没有已安装的包，并且用户没有选择“不再显示”
                if (!anyPackageExists && !EditorPrefs.GetBool(EditorPrefKeyDoNotAutoShow, false))
                {
                    var window = GetWindow<NonsensicalImporter>(true, "Nonsensical Importer", true);
                    // 居中显示窗口
                    window.position = new Rect(
                        (Screen.currentResolution.width - 800) * 0.5f,
                        (Screen.currentResolution.height - 600) * 0.5f,
                        800, 600
                    );
                    window.Show();
                }
            };
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// 当窗口启用时调用。初始化选中状态和设置。
        /// </summary>
        private void OnEnable()
        {
            // 初始化选中数组，默认全部选中
            _selected = new bool[FixedPackages.Length];
            for (int i = 0; i < _selected.Length; i++)
            {
                _selected[i] = true;
            }

            // 从 EditorPrefs 加载“不再显示”设置
            _dontShowAgain = EditorPrefs.GetBool(EditorPrefKeyDoNotAutoShow, false);

            // 从 EditorPrefs 加载上次选择的数据源
            string lastSourceStr = EditorPrefs.GetString(EditorPrefKeyLastSource, DataSource.GitCode.ToString());
            if (Enum.TryParse(lastSourceStr, out DataSource lastSource))
            {
                _currentSource = lastSource;
            }
            else
            {
                _currentSource = DataSource.GitCode; // 默认
            }
        }

        /// <summary>
        /// 当窗口禁用或关闭时调用。清理正在进行的导入任务和进度条。
        /// </summary>
        private void OnDisable()
        {
            if (_isImporting)
            {
                EditorApplication.update -= PollRequests;
            }

            EditorUtility.ClearProgressBar();
        }

        #endregion

        #region GUI

        /// <summary>
        /// 绘制窗口的 GUI。
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Space(6);
            EditorGUILayout.LabelField("导入以下 Git 包", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("勾选要导入的包，然后点击 'Import Selected'。", MessageType.Info);

            GUILayout.Space(6);

            // 数据源选项卡
            EditorGUI.BeginChangeCheck();
            string[] sourceNames = Enum.GetNames(typeof(DataSource));
            _currentSource = (DataSource)GUILayout.Toolbar((int)_currentSource, sourceNames);
            if (EditorGUI.EndChangeCheck())
            {
                // 保存用户选择的数据源
                EditorPrefs.SetString(EditorPrefKeyLastSource, _currentSource.ToString());
            }

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选/反选", GUILayout.Width(100)))
            {
                ToggleAllSelections();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (FixedPackages.Length == 0)
            {
                EditorGUILayout.HelpBox("当前没有配置任何固定包。", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < FixedPackages.Length; i++)
                {
                    DrawPackageItem(i);
                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            _dontShowAgain = EditorGUILayout.ToggleLeft("不再自动弹出此窗口", _dontShowAgain, GUILayout.Width(200));

            EditorGUI.BeginDisabledGroup(_isImporting); // 导入时禁用按钮
            if (GUILayout.Button("Import Selected", GUILayout.Height(30)))
            {
                StartImportSequential();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                CancelImport();
            }

            EditorGUILayout.EndHorizontal();

            if (_isImporting)
            {
                GUILayout.Space(8);
                EditorGUILayout.HelpBox($"正在从 {_currentSource} 导入 ({_importedCount}/{_totalToImport})... 请等待当前包完成。",
                    MessageType.Info);
            }

            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// 绘制单个包的 GUI 项。
        /// </summary>
        private void DrawPackageItem(int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _selected[index] = EditorGUILayout.Toggle(_selected[index], GUILayout.Width(18));

            EditorGUILayout.LabelField(FixedPackages[index].name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            string currentUrl = _currentSource == DataSource.GitCode
                ? FixedPackages[index].gitCodeUrl
                : FixedPackages[index].gitHubUrl;
            if (GUILayout.Button("复制 URL", GUILayout.Width(80)))
            {
                EditorGUIUtility.systemCopyBuffer = currentUrl;
                ShowNotification(new GUIContent("已复制 URL 到剪贴板"), 1.5f); // 1.5秒后自动消失
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.SelectableLabel(currentUrl, EditorStyles.miniLabel, GUILayout.Height(18));
            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

        /// <summary>
        /// 切换所有包的选中状态。
        /// </summary>
        private void ToggleAllSelections()
        {
            bool allSelected = true;
            foreach (var isSelected in _selected)
            {
                if (!isSelected)
                {
                    allSelected = false;
                    break;
                }
            }

            bool newState = !allSelected; // 如果全选则反选，否则全选
            for (int i = 0; i < _selected.Length; i++)
            {
                _selected[i] = newState;
            }
        }

        #endregion

        #region Import Logic

        /// <summary>
        /// 启动顺序导入流程。
        /// </summary>
        private void StartImportSequential()
        {
            if (_isImporting)
            {
                EditorUtility.DisplayDialog("正在导入", "已有导入任务在进行中。", "OK");
                return;
            }

            // 保存设置
            EditorPrefs.SetBool(EditorPrefKeyDoNotAutoShow, _dontShowAgain);
            EditorPrefs.SetString(EditorPrefKeyLastSource, _currentSource.ToString());

            // 准备导入队列
            _importQueue.Clear();
            for (int i = 0; i < _selected.Length; i++)
            {
                if (_selected[i]) // 只要选中就加入队列，URL在实际导入时再取
                {
                    _importQueue.Enqueue(i);
                }
            }

            if (_importQueue.Count == 0)
            {
                EditorUtility.DisplayDialog("未选择", "请先勾选至少一个要导入的包。", "OK");
                return;
            }

            // 初始化导入状态
            _isImporting = true;
            _importedCount = 0;
            _totalToImport = _importQueue.Count;
            _currentAddRequest = null;
            _currentPackageIndex = -1;

            // 注册 Update 回调以轮询请求状态
            EditorApplication.update -= PollRequests; // 确保只注册一次
            EditorApplication.update += PollRequests;

            Debug.Log($"开始顺序导入 {_totalToImport} 个包 (来源: {_currentSource})...");
            StartNextAdd();
        }

        /// <summary>
        /// 开始导入队列中的下一个包。
        /// </summary>
        private void StartNextAdd()
        {
            if (_importQueue.Count == 0)
            {
                CompleteImport();
                return;
            }

            _currentPackageIndex = _importQueue.Dequeue();

            // 根据当前数据源选择 URL
            string url = _currentSource == DataSource.GitCode
                ? FixedPackages[_currentPackageIndex].gitCodeUrl
                : FixedPackages[_currentPackageIndex].gitHubUrl;

            url = url.Trim();

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError($"包 {FixedPackages[_currentPackageIndex].name} 的 {_currentSource} URL 为空。跳过此包。");
                _importedCount++; // 也算作已处理
                StartNextAdd(); // 继续下一个
                return;
            }

            try
            {
                _currentAddRequest = Client.Add(url);
                Debug.Log($"开始导入: {FixedPackages[_currentPackageIndex].name} ({url})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"请求添加包失败 ({FixedPackages[_currentPackageIndex].name}): {ex.Message}");
                // 遇到错误也继续下一个
                _currentAddRequest = null;
                _importedCount++; // 也算作已处理
                StartNextAdd();
            }
        }

        /// <summary>
        /// 在 EditorApplication.update 中调用，用于轮询当前请求的状态。
        /// </summary>
        private void PollRequests()
        {
            if (!_isImporting) return;

            if (_currentAddRequest == null)
            {
                // 如果没有当前请求，可能是在 StartNextAdd 中刚设置完或出错，等待下一帧
                return;
            }

            if (!_currentAddRequest.IsCompleted)
            {
                // 请求未完成，更新进度条
                float progress = Mathf.Clamp01((float)_importedCount / _totalToImport);
                EditorUtility.DisplayProgressBar(
                    "Importing Git Packages",
                    $"Importing {FixedPackages[_currentPackageIndex].name} from {_currentSource}...",
                    progress
                );
                return;
            }

            // 请求已完成
            string packageName = FixedPackages[_currentPackageIndex].name;
            string url = _currentSource == DataSource.GitCode
                ? FixedPackages[_currentPackageIndex].gitCodeUrl
                : FixedPackages[_currentPackageIndex].gitHubUrl;

            if (_currentAddRequest.Status == StatusCode.Success)
            {
                Debug.Log($"<color=green>导入成功:</color> {packageName} ({url})");
                _importedCount++;
            }
            else
            {
                Debug.LogError($"<color=red>导入失败:</color> {packageName} ({url}) - {_currentAddRequest.Error?.message}");
                // 即使失败也计入已处理，继续下一个
                _importedCount++;
            }

            // 清理当前请求，准备下一个
            _currentAddRequest = null;
            StartNextAdd(); // 这会检查队列是否为空并调用 CompleteImport
        }

        /// <summary>
        /// 完成所有导入任务。
        /// </summary>
        private void CompleteImport()
        {
            FinishImportProcess();
            AssetDatabase.Refresh(); // 刷新资源数据库
            Debug.Log($"导入流程完成 (来源: {_currentSource})。成功导入 {_importedCount} / {_totalToImport} 个包。");
            EditorUtility.DisplayDialog("导入完成", $"导入任务已完成 ({_importedCount}/{_totalToImport} 成功)。请查看控制台以获得详细信息。", "OK");
            Close(); // 完成后关闭窗口
        }

        /// <summary>
        /// 取消当前导入任务。
        /// </summary>
        private void CancelImport()
        {
            EditorPrefs.SetBool(EditorPrefKeyDoNotAutoShow, _dontShowAgain);
            EditorPrefs.SetString(EditorPrefKeyLastSource, _currentSource.ToString());
            if (_isImporting)
            {
                FinishImportProcess();
                Debug.Log("导入任务已被用户取消。");
                EditorUtility.DisplayDialog("导入已取消", "导入任务已被取消。", "OK");
            }

            Close();
        }

        /// <summary>
        /// 清理导入过程中的状态和回调。
        /// </summary>
        private void FinishImportProcess()
        {
            EditorApplication.update -= PollRequests;
            _isImporting = false;
            _currentAddRequest = null;
            _importQueue.Clear();
            EditorUtility.ClearProgressBar();
        }

        #endregion
    }
}
