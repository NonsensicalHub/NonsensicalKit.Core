#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

// 备忘录数据类
[Serializable]
public class MemoData
{
    public string title = "新备忘录";
    public string content = "";
    public string category = "未分类";
    public DateTime creationTime;
    public DateTime lastEditTime;
}

// 备忘录管理器
public class MemoManager
{
    private const string MEMO_FOLDER = "Assets/ProjectMemos";
    private const string MEMO_INDEX_FILE = "memo_index.json";
    internal List<MemoData> memos = new List<MemoData>();

    // 初始化备忘录系统
    public void Initialize()
    {
        if (!Directory.Exists(MEMO_FOLDER))
        {
            Directory.CreateDirectory(MEMO_FOLDER);
        }

        LoadIndex();
        LoadMemos();
    }

    // 加载备忘录索引
    private void LoadIndex()
    {
        string indexPath = Path.Combine(MEMO_FOLDER, MEMO_INDEX_FILE);
        if (File.Exists(indexPath))
        {
            try
            {
                string json = File.ReadAllText(indexPath);
                memos = JsonConvert.DeserializeObject<List<MemoData>>(json) ?? new List<MemoData>();
            }
            catch (Exception e)
            {
                Debug.LogError($"加载备忘录索引失败: {e.Message}");
                memos = new List<MemoData>();
            }
        }
    }

    // 保存备忘录索引
    private void SaveIndex()
    {
        string indexPath = Path.Combine(MEMO_FOLDER, MEMO_INDEX_FILE);
        try
        {
            string json = JsonConvert.SerializeObject(memos, Formatting.Indented);
            File.WriteAllText(indexPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存备忘录索引失败: {e.Message}");
        }
    }

    // 加载所有备忘录内容
    private void LoadMemos()
    {
        foreach (var memo in memos)
        {
            string contentPath = GetMemoContentPath(memo);
            if (File.Exists(contentPath))
            {
                try
                {
                    memo.content = File.ReadAllText(contentPath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载备忘录内容失败: {e.Message}");
                }
            }
        }
    }

    // 创建新备忘录
    public MemoData CreateNewMemo()
    {
        var newMemo = new MemoData
        {
            creationTime = DateTime.Now,
            lastEditTime = DateTime.Now
        };
        memos.Add(newMemo);
        SaveIndex();
        return newMemo;
    }

    // 保存备忘录
    public void SaveMemo(MemoData memo)
    {
        memo.lastEditTime = DateTime.Now;
        SaveIndex();

        string contentPath = GetMemoContentPath(memo);
        try
        {
            File.WriteAllText(contentPath, memo.content);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存备忘录内容失败: {e.Message}");
        }
    }

    // 删除备忘录 - 改进：同时删除.meta文件
    public void DeleteMemo(MemoData memo)
    {
        memos.Remove(memo);
        SaveIndex();

        string contentPath = GetMemoContentPath(memo);
        DeleteFileAndMeta(contentPath);
    }

    // 获取备忘录内容路径
    private string GetMemoContentPath(MemoData memo)
    {
        string safeTitle = Path.GetInvalidFileNameChars()
            .Aggregate(memo.title, (current, c) => current.Replace(c, '_'));
        return Path.Combine(MEMO_FOLDER, $"{safeTitle}.txt");
    }

    // 获取所有分类
    public List<string> GetCategories()
    {
        var categories = new List<string> { "未分类" };
        categories.AddRange(memos.Select(m => m.category).Distinct().Where(c => !string.IsNullOrEmpty(c)));
        return categories.Distinct().ToList();
    }

    // 刷新所有数据
    public void RefreshData()
    {
        LoadIndex();
        LoadMemos();
    }

    // 安全删除文件及其.meta文件
    private void DeleteFileAndMeta(string filePath)
    {
        try
        {
            // 删除文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 删除对应的.meta文件
            string metaFilePath = filePath + ".meta";
            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }

            // 刷新Unity资产数据库
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"删除文件失败: {e.Message}");
        }
    }
}

// 备忘录编辑窗口
public class ProjectMemoWindow : EditorWindow
{
    private MemoManager memoManager;
    [SerializeField] private MemoData currentMemo;
    private List<MemoData> filteredMemos;
    private string searchText = "";
    private string[] categories;
    private int selectedCategory = 0;
    private int lastSelectedCategory = 0;
    private Vector2 memoListScrollPos;
    private Vector2 memoContentScrollPos;
    private bool isEditing = false;
    private bool isRefreshing = false;
    private bool stylesInitialized = false;
    private bool forceContentRedraw = false;
    private GUIStyle titleStyle;
    private GUIStyle categoryStyle;
    private GUIStyle dateStyle;
    private GUIStyle searchFieldStyle;
    private Texture2D searchIcon;
    private Texture2D refreshIcon;
    private float lastRefreshTime = 0f;
    private const float REFRESH_COOLDOWN = 1f; // 刷新冷却时间（秒）


    [MenuItem("Tools/项目备忘录")]
    public static void ShowWindow()
    {
        GetWindow<ProjectMemoWindow>("项目备忘录");
    }

    private void OnEnable()
    {
        memoManager = new MemoManager();
        memoManager.Initialize();
        filteredMemos = new List<MemoData>(memoManager.memos);
        categories = memoManager.GetCategories().ToArray();

        // 加载Unity内置图标
        searchIcon = EditorGUIUtility.IconContent("Search").image as Texture2D;
        refreshIcon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;

        // 创建新备忘录（如果没有）
        if (memoManager.memos.Count == 0)
        {
            SetCurrentMemo(memoManager.CreateNewMemo());
        }
        else
        {
            SetCurrentMemo(memoManager.memos[0]);
        }
    }

    private void OnGUI()
    {
        // 安全检查，确保GUI系统已初始化
        if (EditorStyles.boldLabel == null)
            return;
        Undo.undoRedoPerformed += Repaint;
        
        // 延迟初始化样式
        if (!stylesInitialized)
        {
            InitializeStyles();
            stylesInitialized = true;
        }

        // 处理强制内容重绘
        if (forceContentRedraw)
        {
            EditorGUI.FocusTextInControl(null); // 清除所有焦点
            forceContentRedraw = false;
        }

        DrawMenu();
        DrawList();
        DrawMemo();
    }

    private void DrawMenu()
    {
        // 顶部工具栏
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("新建", EditorStyles.toolbarButton))
        {
            CreateNewMemo();
        }

        if (GUILayout.Button("保存", EditorStyles.toolbarButton))
        {
            SaveCurrentMemo();
        }

        if (GUILayout.Button("删除", EditorStyles.toolbarButton) && currentMemo != null)
        {
            DeleteCurrentMemo();
        }

        // 刷新按钮
        EditorGUI.BeginDisabledGroup(isRefreshing || (EditorApplication.timeSinceStartup - lastRefreshTime < REFRESH_COOLDOWN));
        if (GUILayout.Button(new GUIContent(refreshIcon, "刷新"), EditorStyles.toolbarButton,
                GUILayout.Width(25)))
        {
            RefreshData();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        // 显示刷新状态
        if (isRefreshing)
        {
            EditorGUILayout.HelpBox("正在刷新数据...", MessageType.Info);
        }

        // 搜索和分类筛选
        EditorGUILayout.BeginHorizontal();

        // 创建带图标的搜索框
        Rect searchRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20));

        // 绘制搜索图标
        if (searchIcon != null)
        {
            EditorGUI.DrawPreviewTexture(
                new Rect(searchRect.x + 5, searchRect.y + 2, 16, 16),
                searchIcon
            );
        }

        // 绘制搜索文本框（无背景）
        searchText = EditorGUI.TextField(
            new Rect(searchRect.x + 22, searchRect.y, searchRect.width - 25, searchRect.height),
            searchText, searchFieldStyle
        );

        if (GUILayout.Button("搜索", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            FilterMemos();
        }

        EditorGUILayout.EndHorizontal();

        selectedCategory = EditorGUILayout.Popup("分类", selectedCategory, categories);
        if (lastSelectedCategory != selectedCategory)
        {
            lastSelectedCategory = selectedCategory;
            FilterMemos();
        }
    }

    private void DrawList()
    {
        // 备忘录列表和内容区域
        EditorGUILayout.BeginHorizontal();

        // 备忘录列表
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        memoListScrollPos = EditorGUILayout.BeginScrollView(memoListScrollPos,
            GUILayout.ExpandHeight(true));

        for (int i = 0; i < filteredMemos.Count; i++)
        {
            var memo = filteredMemos[i];
            bool isSelected = memo == currentMemo;
            GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

            if (GUILayout.Button($"{memo.title} [{memo.category}]", style))
            {
                if (currentMemo != null && isEditing)
                {
                    // 询问是否保存当前编辑
                    if (EditorUtility.DisplayDialog("保存更改", "是否保存当前备忘录的更改？", "保存", "不保存"))
                    {
                        SaveCurrentMemo();
                    }
                }

                SetCurrentMemo(memo);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawMemo()
    {
        // 备忘录内容
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (currentMemo != null)
        {
            currentMemo.title = EditorGUILayout.TextField("标题", currentMemo.title, titleStyle);
            currentMemo.category = EditorGUILayout.TextField("分类", currentMemo.category, categoryStyle);

            EditorGUILayout.LabelField($"创建时间: {currentMemo.creationTime}", dateStyle);
            EditorGUILayout.LabelField($"最后编辑: {currentMemo.lastEditTime}", dateStyle);

            memoContentScrollPos = EditorGUILayout.BeginScrollView(memoContentScrollPos,
                GUILayout.ExpandHeight(true));

            // 使用带ID的TextArea，并在需要时强制重绘
            EditorGUI.BeginChangeCheck();
            var content = EditorGUILayout.TextArea(currentMemo.content,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                {
                    Undo.RecordObject(this, "edit memo");
                    EditorUtility.SetDirty(this);
                    isEditing = true;
                }

                currentMemo.content = content;
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("请创建新备忘录", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void InitializeStyles()
    {
        titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(0, 0, 10, 10)
        };

        categoryStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Italic,
            margin = new RectOffset(0, 0, 5, 5)
        };

        dateStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            margin = new RectOffset(0, 0, 5, 10)
        };

        // 搜索框样式 - 移除白色背景
        searchFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            margin = new RectOffset(0, 0, 5, 5),
            padding = new RectOffset(22, 5, 2, 2), // 为图标预留空间
            fixedHeight = 20,
            normal = { background = null },
            focused = { background = null }
        };
    }

    private void FilterMemos()
    {
        string category = categories[selectedCategory];
        filteredMemos = memoManager.memos
            .Where(m => (string.IsNullOrEmpty(searchText) || m.title.Contains(searchText) || m.content.Contains(searchText))
                        && (category == "未分类" || m.category == category))
            .ToList();
    }

    private void RefreshMemoList()
    {
        categories = memoManager.GetCategories().ToArray();
        if (selectedCategory >= categories.Length)
        {
            selectedCategory = categories.Length - 1;
        }

        FilterMemos();
    }

    // 刷新数据方法
    private async void RefreshData()
    {
        if (isRefreshing || (EditorApplication.timeSinceStartup - lastRefreshTime < REFRESH_COOLDOWN))
            return;

        try
        {
            isRefreshing = true;
            lastRefreshTime = (float)EditorApplication.timeSinceStartup;
            Repaint();

            // 在后台线程刷新数据
            await Task.Run(() =>
            {
                memoManager.RefreshData();
            });

            // 更新UI
            filteredMemos = new List<MemoData>(memoManager.memos);
            RefreshMemoList();

            // 保持当前选中的备忘录，如果已删除则选择第一个
            if (!filteredMemos.Contains(currentMemo) && filteredMemos.Count > 0)
            {
                SetCurrentMemo(filteredMemos[0]);
            }

            ShowNotification(new GUIContent("数据已刷新"));
        }
        catch (Exception e)
        {
            Debug.LogError($"刷新数据失败: {e.Message}");
            ShowNotification(new GUIContent($"刷新失败: {e.Message}"));
        }
        finally
        {
            isRefreshing = false;
            Repaint();
        }
    }

    // 保存当前备忘录
    private void SaveCurrentMemo()
    {
        if (currentMemo == null)
            return;

        memoManager.SaveMemo(currentMemo);
        isEditing = false;
        forceContentRedraw = true; // 保存后强制内容区重绘
        ShowNotification(new GUIContent("备忘录已保存"));

        // 更新列表中的数据
        RefreshMemoList();
    }

    // 创建新备忘录
    private void CreateNewMemo()
    {
        if (currentMemo != null && isEditing)
        {
            if (EditorUtility.DisplayDialog("保存更改", "是否保存当前备忘录的更改？", "保存", "不保存"))
            {
                SaveCurrentMemo();
            }
        }

        SetCurrentMemo(memoManager.CreateNewMemo());
        searchText = "";
        filteredMemos = new List<MemoData>(memoManager.memos);
    }

    // 删除当前备忘录
    private void DeleteCurrentMemo()
    {
        if (currentMemo == null)
            return;

        if (EditorUtility.DisplayDialog("确认删除", "确定要删除此备忘录吗？", "是", "否"))
        {
            memoManager.DeleteMemo(currentMemo);

            MemoData newCurrent = null;
            if (memoManager.memos.Count > 0)
            {
                newCurrent = memoManager.memos[0];
            }
            else
            {
                newCurrent = memoManager.CreateNewMemo();
            }

            SetCurrentMemo(newCurrent);
            RefreshMemoList();
        }
    }

    // 设置当前备忘录并强制刷新内容
    private void SetCurrentMemo(MemoData memo)
    {
        currentMemo = memo;
        isEditing = false;
        forceContentRedraw = true; // 切换备忘录时强制内容区重绘
        Repaint(); // 强制立即刷新UI
    }
}
#endif
