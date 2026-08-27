# Scene Toolbar Handoff

> 根目录：`Assets/Editor/SceneToolbar/`  
> 运行环境：Unity Editor Only（Unity 6 / `6000.0.x`）  
> Overlay 显示名：**项目工具**（`ToolbarOverlay`）

本文档分两块：**框架（Core）** 与 **具体工具（Tools）**。接手时先确认改的是哪一层。

---

## 目录结构

```
Assets/Editor/SceneToolbar/
├── HANDOFF.md                 ← 本文档
├── Core/                      ← 工具栏框架（与具体业务无关）
│   ├── ISceneToolbarTool.cs
│   ├── SceneToolbarRegistry.cs
│   ├── SceneToolbarDropdownElement.cs
│   ├── SceneToolbarOverlay.cs
│   └── SceneToolbarBootstrap.cs
└── Tools/                     ← 具体工具（每个工具一个子文件夹）
    ├── SceneSwitcher/         ← 场景切换
    └── Example/               ← 扩展示例（默认未启用）
```


| 层       | 命名空间                                     | 何时改                     |
| ------- | ---------------------------------------- | ----------------------- |
| Core    | `Frame.Editor.SceneToolbar`              | 改注册机制、按钮基类、Overlay 挂载方式 |
| Tools.* | `Frame.Editor.SceneToolbar.Tools.<Name>` | 加/改某个工具的业务逻辑与 UI        |


---



# 一、框架（Core）



## 1.1 目标

提供可停靠的 Scene 工具条基础设施：注册表、Toolbar 按钮基类、Overlay 容器、启动注册入口。  
**不包含**任何具体业务（场景列表等）。

## 1.2 文件职责


| 文件                               | 职责                                            |
| -------------------------------- | --------------------------------------------- |
| `ISceneToolbarTool.cs`           | 工具逻辑契约（Id / Tooltip / Icon / Order / OnClick） |
| `SceneToolbarRegistry.cs`        | Register / Unregister / TryGet / ToolsChanged |
| `SceneToolbarDropdownElement.cs` | `EditorToolbarDropdown` 基类，按 ToolId 绑定逻辑      |
| `SceneToolbarOverlay.cs`         | `ToolbarOverlay`，构造函数里**静态列出**各 Element.Id    |
| `SceneToolbarBootstrap.cs`       | `[InitializeOnLoad]` 里 `Register` 各工具逻辑实例     |




## 1.3 数据流

```
Core/Bootstrap.Register(tool logic)
        │
Core/Overlay( Element.Id, Element.Id, ... )
        │
Tools/<X>/<X>Element : SceneToolbarDropdownElement
        └─ clicked → Registry.TryGet(ToolId) → tool.OnClick(worldBound)
```

- **逻辑层**：`ISceneToolbarTool`（可测、可替换）
- **UI 层**：`[EditorToolbarElement]`（Unity 要求 Id 编译期声明）



## 1.4 扩展新工具（框架侧固定三步）

Unity 限制：无法仅靠 Registry 运行时往 ToolbarOverlay 加按钮。

1. 在 `Tools/<MyTool>/` 实现 `ISceneToolbarTool` + `SceneToolbarDropdownElement` 子类
2. `Core/SceneToolbarBootstrap` → `Register(new MyTool())`
3. `Core/SceneToolbarOverlay` → `base(..., MyToolElement.Id)`

模板见 `Tools/Example/`。

## 1.5 框架 API 速查


| API                                        | 说明                                            |
| ------------------------------------------ | --------------------------------------------- |
| `SceneToolbarRegistry.Register/Unregister` | 注册/注销逻辑                                       |
| `SceneToolbarRegistry.TryGet`              | Element 点击时取逻辑                                |
| `SceneToolbarDropdownElement`              | 子类只需实现 `ToolId`                               |
| `ISceneToolbarTool.OnClick(Rect)`          | 优先 `menu.DropDown(rect)`，否则 `ShowAsContext()` |




## 1.6 框架约束

1. 仅 Editor
2. 新按钮必须改 Overlay 的 `base(...)`（Unity API）
3. 停靠时建议只设 `icon`、清空 `text`
4. Bootstrap 用 `delayCall` 注册



## 1.7 框架改动对照


| 需求                    | 改哪里                                |
| --------------------- | ---------------------------------- |
| 改注册机制 / 事件            | `Core/SceneToolbarRegistry`        |
| 改按钮绑定/外观默认行为          | `Core/SceneToolbarDropdownElement` |
| 挂新工具 Id / 改 Overlay 名 | `Core/SceneToolbarOverlay`         |
| 启动时注册哪些工具             | `Core/SceneToolbarBootstrap`       |


---



# 二、具体工具（Tools）

每个工具独占一个子目录，包含：逻辑、Toolbar Element、（可选）设置与窗口。

---



## 2.A SceneSwitcher（场景切换）

> 路径：`Tools/SceneSwitcher/`  
> 命名空间：`Frame.Editor.SceneToolbar.Tools.SceneSwitcher`



### 功能

点击工具栏图标 → 二级菜单列出过滤后的场景 → 点击打开。  
菜单末尾：**设置…**

### 文件


| 文件                               | 职责                                                               |
| -------------------------------- | ---------------------------------------------------------------- |
| `SceneSwitcherTool.cs`           | 菜单构建、过滤、打开场景                                                     |
| `SceneSwitcherElement.cs`        | `[EditorToolbarElement]`，Id = `Frame.SceneToolbar/SceneSwitcher` |
| `SceneSwitcherSettings.cs`       | 过滤配置读写                                                           |
| `SceneSwitcherSettingsWindow.cs` | 设置窗口 UI                                                          |




### 行为细节

- 搜索：`FindAssets("t:Scene", ["Assets"])` → `SceneSwitcherSettings.IsSceneVisible`
- **默认**：仅 `Assets/Scenes`（避免插件 Demo 刷屏）
- 设置：显示全部 / 包含目录 / 单场景隐藏
- 配置：`ProjectSettings/SceneSwitcherSettings.json`（建议进版本库）
- 菜单路径：`/` → `\`（避免 GenericMenu 子菜单）
- 打开：`SaveCurrentModifiedScenesIfUserWantsTo` + `OpenSceneMode.Single`



### 本工具改动对照


| 需求              | 改哪里                           |
| --------------- | ----------------------------- |
| 菜单项 / 打开逻辑      | `SceneSwitcherTool`           |
| 过滤规则 / 默认目录     | `SceneSwitcherSettings`       |
| 设置界面            | `SceneSwitcherSettingsWindow` |
| Toolbar 图标元素 Id | `SceneSwitcherElement`        |




### 使用

1. Overlay 菜单勾选「项目工具」，拖到顶栏
2. 直接点场景图标 → 选场景
3. 菜单底部「设置…」调整过滤

---



## 2.B Example（扩展示例）

> 路径：`Tools/Example/`  
> 命名空间：`Frame.Editor.SceneToolbar.Tools.Example`  
> **默认未启用**，可删可作模板。

启用三步：

1. 取消文件内 Registrar 注释，或在 Bootstrap `Register`
2. 取消 `SceneToolbarExampleElement` 注释
3. Overlay `base(...)` 加上 `SceneToolbarExampleElement.Id`

---



## 2.C 新增工具检查清单

在 `Tools/<Name>/` 下新建后：

- [ ] `*Tool.cs` 实现 `ISceneToolbarTool`
- [ ] `*Element.cs` 继承 `SceneToolbarDropdownElement` + `[EditorToolbarElement]`
- [ ] Bootstrap `Register`
- [ ] Overlay `base` 加入 Element.Id

- [ ]（可选）本工具专属 Settings / Window 放同目录

- [ ] 在本文档「二、具体工具」下追加一小节

---

*若代码与本文冲突，以源码为准并回写本文。*