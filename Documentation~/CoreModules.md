# 核心模块与 Service 子模块详解

本文档用于承载 `NonsensicalKit.Core` 的详细模块介绍。  
`README.md` 保持简洁索引，具体说明统一放在 `Documentation~` 目录中维护。

---

## 核心模块一览

### Service

- **模块定位**：统一管理全局服务的创建、初始化与获取。
- **核心入口**：`ServiceCore`
- **使用方法**：
  1. 新建服务类并实现 `IClassService` 或 `IMonoService`。
  2. 在 `Resources/NonsensicalSetting` 中注册该服务。
  3. 业务层通过 `ServiceCore.Get<T>()` / `TryGet<T>()` 获取实例。
  4. 若初始化时序不确定，统一改用 `SafeGet<T>(Action<T>)`。

### Save

- **模块定位**：统一管理模块化存档的采集、写入、读取与恢复流程。
- **核心入口**：`SaveService`、`ISaveProvider`、`SaveProviderBehaviour`
- **使用方法**：
  1. 在业务模块实现 `ISaveProvider`（或继承 `SaveProviderBehaviour`）并提供唯一 `SaveKey`。
  2. 在 `CaptureAsBytes()` 中输出模块数据，在 `RestoreFromBytes(byte[])` 中恢复数据。
  3. 通过 `ServiceCore.Get<SaveService>().Save(slotId)` 执行保存。
  4. 通过 `ServiceCore.Get<SaveService>().Load(slotId)` 执行加载。
- **最小示例**：
```csharp
using NonsensicalKit.Core.Save;
using UnityEngine;

public sealed class PlayerSaveProvider : SaveProviderBehaviour
{
    [SerializeField] private int m_hp = 100;

    public override byte[] CaptureAsBytes()
    {
        return System.BitConverter.GetBytes(m_hp);
    }

    public override void RestoreFromBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length < sizeof(int))
        {
            return;
        }

        m_hp = System.BitConverter.ToInt32(bytes, 0);
    }
}
```

### Log

- **模块定位**：统一日志打印、等级控制、来源分组。
- **核心入口**：`LogCore`、`NonsensicalLog`
- **使用方法**：
  1. 业务代码统一使用日志接口输出，避免直接散用 `Debug.Log`。
  2. 按系统或模块名增加前缀标签，便于筛选定位。
  3. 区分调试、警告、错误等级，保证线上日志可控。
  4. 在编辑器日志窗口按标签过滤，快速回溯问题链路。

### Timer

- **模块定位**：管理延时任务、循环任务和计时调度。
- **核心入口**：`TimerSystem`、`NonsensicalTimer`
- **使用方法**：
  1. 通过 Timer 系统注册一次性或循环任务。
  2. 为关键任务保存任务 ID/句柄，便于取消和追踪。
  3. 在 `OnDisable` / `OnDestroy` 中注销任务，防止悬挂回调。
  4. 对高频任务控制间隔，避免无意义高频触发。

### Aggregator

- **模块定位**：通过消息/方法/对象聚合实现模块解耦通信。
- **核心入口**：`MessageAggregator`、`MethodAggregator`、`ObjectAggregator`
- **使用方法**：
  1. 发布方只定义消息体和发布行为，不直接依赖接收方。
  2. 订阅方在生命周期开始时注册，在结束时取消订阅。
  3. 跨系统状态广播优先走 Aggregator（UI、网络、流程）。
  4. 消息体字段保持最小必要集合，降低耦合成本。

### Items

- **模块定位**：提供统一基础结构类型，减少重复造轮子。
- **核心入口**：`Int2/3/4`、`Float2/3/4`、`Array2/3/4` 等
- **使用方法**：
  1. 在通用数据模型中优先使用 Items 内类型。
  2. 跨模块传参统一结构定义，避免多套同义结构。
  3. 新增结构时保持命名和序列化行为一致。

### Utility

- **模块定位**：提供常见静态工具能力（数学、JSON、文件、反射等）。
- **核心入口**：`MathTool`、`JsonTool`、`FileTool`、`ReflectionTool` 等
- **使用方法**：
  1. 开发前先检索是否已有对应 Tool，优先复用。
  2. IO/反射类调用增加异常保护和失败回退。
  3. 新增工具按职责归类，避免“万能工具类”膨胀。

### NetworkTool

- **模块定位**：封装 HTTP/Socket/WebSocket 的连接与请求流程。
- **核心入口**：`HttpManager`、`HttpTaskTool`、`SocketHelper`、`WebSocketHelper`
- **使用方法**：
  1. 在 Helper 层统一封装请求参数、超时与错误码处理。
  2. 将重连、断线恢复、心跳等连接策略集中管理。
  3. 业务层只处理请求意图和响应数据，不直接操作底层连接。

### ObjectPool

- **模块定位**：复用高频对象，降低 GC 和实例化开销。
- **核心入口**：`GameObjectPool`、`ComponentPool`、`ListPool`、`DictionaryPool`
- **使用方法**：
  1. 将高频创建销毁对象迁移到对象池获取/回收。
  2. 回收前重置 Transform、状态字段与事件绑定。
  3. 按场景或业务域拆分池实例，避免池内容污染。

### CameraTool

- **模块定位**：提供可复用的多模式相机控制方案。
- **核心入口**：`NonsensicalCamera`、`FreedomCamera`、`FocusCamera`
- **使用方法**：
  1. 根据场景选择对应控制脚本并挂载到相机对象。
  2. 将速度、阻尼、边界等参数暴露给可调配置。
  3. 输入层统一输出控制量，相机层只消费输入结果。

### PlayerController

- **模块定位**：提供第一人称/第三人称控制模板。
- **核心入口**：`FirstPersonPlayerController`、`ThirdPersonPlayerController`
- **说明**：运行时代码目录为 `PlayerControler`（历史命名），对外文档统一使用 `PlayerController`。
- **使用方法**：
  1. 将控制器挂载到角色对象并绑定输入源。
  2. 按项目规则替换移动、旋转、跳跃逻辑。
  3. 与 Animator 参数联动，保持表现与状态同步。

### MeshTool

- **模块定位**：用于网格构建、编辑与辅助处理。
- **核心入口**：`MeshBuilder`、`MeshBuilderPlus`、`MeshHelper`、`ModelHelper`
- **使用方法**：
  1. 动态网格场景优先使用 Builder 进行批量构建。
  2. 通过脏标记更新局部网格，避免每帧全量重建。
  3. 重计算法线/切线等重操作尽量放初始化阶段。

### DagLogicNode

- **模块定位**：构建 DAG 流程节点和跳转控制关系。
- **核心入口**：`DagLogicManager`、`DagGraphConfig`、各类节点组件
- **使用方法**：
  1. 在编辑器中配置节点图与跳转条件。
  2. 运行时由 `DagLogicManager` 负责节点驱动与切换。
  3. 在节点组件中实现进入、执行、退出的业务行为。
  4. 为关键节点增加日志，便于排查流程分支问题。

### Updater

- **模块定位**：处理版本检测、补丁描述和更新流程接入。
- **核心入口**：`UnityUpdater`、`PatchInfo`
- **使用方法**：
  1. 定义版本号规则与补丁信息结构。
  2. 在启动流程早期接入更新检查逻辑。
  3. 下载完成后做校验并执行补丁应用，再进入主流程。

### GUITool / EasyTool

- **模块定位**：提供轻量调试和交互组件，便于快速验证功能。
- **核心入口**：FPS、Esc 面板、移动器、下载器等组件脚本
- **使用方法**：
  1. 原型期可直接挂载组件快速验证交互效果。
  2. 稳定后将通用能力二次封装成项目统一组件。
  3. 关闭线上不必要的调试组件，避免影响体验。

---

## Service 子模块说明

### ConfigService

- **用途**：统一管理项目配置资产（ScriptableObject/配置文件）的加载与访问
- **使用方法**：
  1. 将全局共享配置资产集中登记到 ConfigService。
  2. 启动阶段统一加载，业务层按类型读取配置。
  3. 配置变更后通过统一事件通知刷新依赖模块。

### SettingService

- **用途**：管理“可变设置项”（如画质、音量、输入偏好等）的读写与持久化
- **使用方法**：
  1. 按模块定义设置项数据结构（音频/画质/控制等）。
  2. 在设置变更时立即落盘或按策略批量持久化。
  3. 进入游戏时加载并应用设置到对应系统。

### AssetBundleService

- **用途**：统一管理 AssetBundle 的加载、依赖处理、缓存与释放
- **使用方法**：
  1. 通过统一加载入口请求目标资源（同步或异步）。
  2. 加载时处理依赖链和缓存命中策略。
  3. 场景结束或引用归零后释放无用 Bundle。
