# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.9.0] - 2026-04-14

### Added

- 基础功能，具体参考README文档

## [1.0.0] - 2026-04-14

### Removed

- 移除LogicNodeTreeSystem，改为使用DagLogicManager  

## [1.0.1] - 2026-04-15

### Fixed

- 修复DagLogicManager的DagNode的Vector2在序列化错误

## [1.0.2] - 2026-04-16

### Added

- 新增中文字体的示例，之后所有示例使用tmp中文字体时都引用此字体即可

## [1.0.3] - 2026-04-16

### Added

- 公共资产中新增urp设置

### Changed

- 项目备忘录编辑器工具从示例移动至包本体中

### Removed

- 移出冗余的示例（ServiceTemplates、Timer）

## [1.0.4] - 2026-04-20

### Added

- Array4新增Int4调用

### Changed

- 工具类整理

### Fixed

- 修复DagLogicManager返回上一级方法的问题，暂时使用第一个父节点
