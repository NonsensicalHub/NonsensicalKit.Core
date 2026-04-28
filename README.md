# NonsensicalKit.Core

`com.nonsensicallab.nonsensicalkit.core` 是 NonsensicalKit 的核心能力包，也是其他 NonsensicalKit 扩展包的基础依赖。  
它提供了项目级框架能力（Service / Log / Timer / Aggregator）、运行时工具集以及编辑器效率工具。

---

## 核心模块一览

- `Service`：统一管理全局服务的创建、初始化与获取。  
- `Save`：统一管理模块化存档的采集、写入、读取与恢复流程。  
- `Log`：统一日志打印、等级控制与来源分组。  
- `Timer`：管理延时任务、循环任务和计时调度。  
- `Aggregator`：通过消息/方法/对象聚合实现模块解耦通信。  
- `Items`：提供统一基础结构类型，减少重复造轮子。  
- `Utility`：提供数学、JSON、文件、反射等静态工具能力。  
- `NetworkTool`：封装 HTTP/Socket/WebSocket 的连接与请求流程。  
- `ObjectPool`：复用高频对象，降低 GC 与实例化开销。  
- `CameraTool`：提供可复用的多模式相机控制方案。  
- `PlayerController`：提供第一/第三人称控制模板。  
- `MeshTool`：用于网格构建、编辑与辅助处理。  
- `DagLogicNode`：构建 DAG 流程节点和跳转控制关系。  
- `Updater`：处理版本检测、补丁描述和更新流程接入。  
- `GUITool / EasyTool`：提供轻量调试和交互组件。  

详细模块介绍（定位、入口、使用方式、示例）请查看：
- [核心模块与 Service 子模块详解](Documentation~/CoreModules.md)
- [服务初始化与获取](Documentation~/NonsensicalManager.md)

---

## 编辑器工具

| 工具 | 作用 | 菜单入口 |
| --- | --- | --- |
| 资源重名检测 | 扫描 `Resources` 下同名资源，提前规避加载冲突 | `NonsensicalKit/Items/检测资源重名` |
| 场景对象排序 | 按名称批量排序根节点或选中节点子物体 | `NonsensicalKit/Items/根据名称排序` |
| 丢失脚本检测 | 扫描当前场景并定位 Missing Script 对象 | `NonsensicalKit/Items/查找场景中的丢失脚本` |
| 批量重命名 | 对选中节点下所有子物体按规则重命名 | `NonsensicalKit/批量修改/修改子物体名称` |
| 组件挂载/批处理 | 批量添加组件、统一挂载规则与结构整理 | `NonsensicalKit/批量修改/*` |
| Collider 快速修复 | 自动补齐/修正碰撞体尺寸与挂载流程 | `NonsensicalKit/*Collider*` |
| AssetBundle 辅助工具 | 打包选项配置、平台切换、批量设置包名 | `NonsensicalKit/AssetBundle辅助工具` |
| Updater 编辑器 | 更新流程相关配置的可视化编辑与调试 | `UpdaterEditor` 窗口入口 |
| DAG 可视化编辑器 | 以图编辑方式配置 DAG 节点与连接关系 | `DagEditorWindow` |
| Console 跳转优化 | 双击日志时自动跳过包装层，直达业务代码 | `DebugJump`（自动生效） |
| 调试辅助窗口 | 日志 Tag 过滤、GUIStyle 查看、名称复制等 | `IgnoreLogTagWindow`、`GUIStyleViewer`、`NameCopier` |

---

## 第三方模块

### NaughtyAttributes

NaughtyAttributes 的编辑器集成与属性绘制支持

---

## 示例

- `PlayerController`：角色控制示例
- `NonsensicalCameraTemplate`：相机控制示例
- `NaughtyAttributes`：编辑器属性示例
- `GUISetting`：GUI 系统设置面板示例
- `CommonAssets`：通用字体与 URP 资源示例
