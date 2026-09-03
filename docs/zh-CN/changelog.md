---
outline: deep
---

# 变更日志

## 未发布

### 修复
- 🐛 **历史库记录工具版本写死**：`tool_version` 改为从程序集版本读取，发版后自动更新
- 🐛 **VS Code 调试配置过期**：`launch.json` 的目标框架目录更新为 `net10.0-windows10.0.19041.0`
- 🐛 **应用无法启动**：6 个无效图标名（`Refresh24` 等）导致 XAML 解析崩溃；`<Run>` 与 `ProgressBar.Value` 默认 TwoWay 绑定只读属性导致崩溃
- 🐛 **事件日志扫描中断**：`FormatDescription` 抛出的 `EventLogException` 未被捕获，单条坏消息记录会截断整个日志读取（30 天窗口少读约 60 条事件）
- 🐛 **历史列表点击无响应**：`SelectEntryCommand` 未接线，选中条目现可加载该次诊断数据
- 🔒 **SQLitePCLRaw 升级至 2.1.13**：修复 GHSA-2m69-gcr7-jv3q 高危漏洞（捆绑 SQLite ≥ 3.50.2）

### 优化
- 🎨 **界面**：暗色主题硬编码颜色改用主题画刷；NaN/0°C 传感器值显示为"—"；内存单位统一 GB；评分卡层级优化；ScottPlot 中文字体与日期轴修复；空数据图表占位提示
- 🏗️ **架构**：新增 UI Services 层（绘图/主题/文件对话框），MainViewModel 职责拆分；可靠性趋势计算下沉 Core（`ReliabilityTrendBuilder`）
- 🗃️ **历史库**：新增保留策略（默认保留最近 50 条快照），防止无限增长
- 🧪 **测试**：LogService 支持注入日志目录（测试与真实日志隔离）；修复采集进度测试的偶发失败；启用 `TreatWarningsAsErrors`
- 📝 **文档**：架构/测试文档与代码现状同步；删除死代码 `ConfigurationService`

### 变更
- **归档轻量化重构**：彻底精简项目结构，降低维护负担
  - 删除 `openspec/` 整个规范体系（19 文件）—— 归档项目不再需要 spec-driven 流程
  - 删除 `CLAUDE.md`、`.github/copilot-instructions.md` —— AI 指令统一收敛至 `AGENTS.md`
  - 删除 `CONTRIBUTING.md`、`CHANGELOG.md`、`README.zh-CN.md` —— 根目录文档精简
  - 删除 `.github/workflows/pages.yml`、`scripts/pre-commit.sh`、`scripts/setup-hooks.sh` —— CI/脚本精简
  - 删除 `omnisharp.json` —— 过时配置
  - 删除 `docs/zh-CN/openspec/` 及英文文档入口 —— 文档站纯中文化
  - 修复架构文档死链接（services/data-flow 页面不存在）

## 最新版本

### [1.2.0] - 2026-04-27

#### 修复
- 🔧 **HardwareMonitorProvider** - 修复 `Open()` 失败时的资源泄漏问题
- 📝 README 路径引用修正

#### 新增
- 🧪 PerformanceService 评分算法属性测试
- 🧪 DiagnosticData 序列化往返属性测试
- 🧪 HTML 报告生成属性测试
- 📄 标准 `CLAUDE.md` 文件

#### 变更
- 📁 BMAD 目录规范化至 `docs/methodology/`
- 📋 Roadmap 澄清：便携模式已被 FDD 版本替代
- 📚 CONTRIBUTING.md 添加文档站引用

---

### [1.1.0] - 2026-04-16

#### 新增
- 🌍 **完整文档国际化** - 中英文双语文档全面上线
- 📚 **中文文档重构** - 优化结构，内容大幅扩充
- 📚 **英文文档新增** - 完整的英文版文档

#### 变更
- 优化 README.md 结构和内容
- 改进 VitePress 配置，添加国际化支持

---

## 历史版本

### [1.0.0] - 2026-04-16
- 📚 完整文档站重构
- 🔧 多处 Bug 修复

### [0.5.0] - 2026-03-22
- 🧪 测试套件扩展
- 🏗️ 代码重构优化

### [0.4.0] - 2026-03-13
- 🔧 LogService API 修复

### [0.3.0] - 2026-03-10
- 🔍 SEO 优化
- 📋 变更日志页面

### [0.2.0] - 2025-12-14
- 🏗️ WPF 依赖注入架构
- 📤 JSON 导出/导入
- 🌓 主题切换
- 📊 实时监控功能

### [0.1.0] - 2025-02-27
- 🚀 初始发布

---

## 完整变更日志

- [中文完整版 →](/zh-CN/changelog)
- [GitHub Releases →](https://github.com/LessUp/dig-your-windows/releases)

## 版本规范

本项目遵循 [语义化版本](https://semver.org/lang/zh-CN/)：

- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能新增
- **修订号**：向下兼容的问题修正
