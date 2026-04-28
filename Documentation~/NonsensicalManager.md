# 服务初始化与获取

## 简介
`NonsensicalKit.Core` 当前使用 `ServiceCore` 作为统一服务入口，在运行时自动创建并初始化 `IClassService` / `IMonoService`。

## 解决的问题
- 统一服务创建与初始化时序，减少脚本间手工依赖。
- 对异步初始化服务提供安全获取方式，避免“服务未就绪就使用”。
- 通过配置控制运行中的服务集合，降低场景初始化耦合。

## 如何使用
1. 创建服务类并实现 `IClassService` 或 `IMonoService`。
2. 在 `Resources/NonsensicalSetting` 的 `RunningServices` 中加入服务类型名。
3. 业务侧通过 `ServiceCore.Get<T>()` 或 `ServiceCore.TryGet<T>(out var service)` 获取已创建服务。
4. 若服务存在初始化耗时，使用 `ServiceCore.SafeGet<T>(callback)` 在服务就绪后回调。
