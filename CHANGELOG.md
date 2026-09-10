# 更新日志

DigYourWindows 是一款开源、完全离线的 Windows 深度诊断工具：一键采集整机硬件规格，实时
监控 CPU / GPU / 网络负载，读取磁盘 SMART 与系统事件日志、可靠性记录，并给出 0~100 分
系统健康评分与针对性优化建议。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### 新增

- 以 WPF（.NET 10）重写 Windows 诊断工具，提供硬件信息、事件日志与可靠性记录查询；
  数据模型拆分为 Compute / Device / Disk / Event / Hardware / Performance 等领域模型。
- 实时硬件监控：CPU 温度 / 负载 / 频率，GPU 温度 / 负载 / 显存 / 功耗，以及网卡吞吐采样
  与 60 秒历史曲线，基于 LibreHardwareMonitor 采集、ScottPlot 绘制。
- 磁盘 SMART 健康：NVMe / SATA 硬盘属性、SMART 状态与健康度百分比，无数据时自动隐藏。
- 系统健康评分与优化建议：稳定性 40% + 性能 30% + 内存 15% + 磁盘 15%，由纯计算服务
  `PerformanceService` 产出。
- 可靠性趋势：接入 Windows Reliability Monitor 记录并按天聚合，趋势计算下沉为纯函数
  `ReliabilityTrendBuilder`。
- 报告导出：自包含单文件 HTML 与结构化 JSON，序列化与生成统一下沉 Core 的 `ReportService`。
- SQLite 诊断历史存储：快照记录、摘要与保留策略，UI 支持选择历史条目回放诊断数据。
- UI 导航化改造：Dashboard / Hardware / History / Logs / Monitoring 分页，新增设置对话框
  （字体、UI 缩放）与品牌图标；抽取绘图、主题、文件对话框、设置、分页等 UI 服务层。
- 打包发布链：自包含 / 框架依赖 zip（附 SHA-256）与 Inno Setup 安装包，配套 `build` /
  `publish` / `build-installer` PowerShell 脚本，以及构建 / 测试 / 发布 CI 流水线。
- 测试体系：xUnit + FsCheck 属性测试，覆盖评分算法、序列化往返、HTML 生成、SQLite 历史等。
- VitePress 文档站（GitHub Pages）：中英文双语、技术白皮书、数据 Schema 与使用指南。

### 变更

- 项目从早期 Rust 原型迁移至 WPF / .NET 10 双工程（Core 类库 + UI），采用 MVVM、依赖注入
  与 CommunityToolkit.Mvvm 源生成器。
- 引入诊断采集编排器 `DiagnosticCollectorService`（进度回传、可取消、部分失败降级并返回
  警告），并配套可轮转的 `LogService` 落盘日志。
- 统一工程配置：集中包版本管理（CPM）、`Directory.Build.props` 共享属性，启用
  `TreatWarningsAsErrors` 零容忍告警。
- 采纳 OpenSpec 规范管理工作流；规范与设计文档在归档阶段进一步收敛。
- README 与文档站多轮重构，最终定位为归档稳定版并精简为单页说明。

### 修复

- 修复应用启动崩溃：6 个无效图标名，以及只读属性的默认 TwoWay 绑定导致 XAML 解析失败。
- 修复事件日志扫描中断：单条坏消息记录抛出的异常会截断整个 30 天窗口的读取。
- 修复历史列表点击无响应：`SelectEntryCommand` 未接线，无法加载所选快照。
- 加固采集与监控：采集取消与意外异常降级，并完善传感器锁定、历史保留与日志轮转。
- 修复界面问题：暗色主题硬编码颜色、NaN / 0°C 传感器显示、内存单位与评分卡层级。
- 清理代码分析告警：CA1305（CultureInfo）、CA1707（测试方法命名）及 CI 构建错误。

### 移除

- 移除早期 Rust 实现目录与旧版标准化模型，统一到领域模型。
- 删除死代码：`ConfigurationService`、`ServiceException`、`WmiException` 等。
- 归档轻量化：移除 OpenSpec / BMad skills、`CLAUDE.md`、`CONTRIBUTING.md`、旧 `CHANGELOG.md`
  与英文文档入口，AI 指令统一收敛至 `AGENTS.md`。
