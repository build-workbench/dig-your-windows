---
outline: deep
---

# 变更日志

## 未发布

### 变更
- **仓库收敛**：移除与项目核心无关的方法论资产，降低维护负担
  - 删除 `.claude/skills/`（42 个 BMad skills，约 2.3MB）—— 与 Windows 诊断工具无关
  - 删除 `docs/methodology/bmad-framework/` 与 `docs/superpowers/` —— 无引用
- **文档站收敛为单语（zh-CN）**：移除 `docs/en-US/` 及 VitePress 中所有英文 locale 配置
- **AI 指令合并**：`CLAUDE.md` 与 `.github/copilot-instructions.md` 改为薄壳，统一指向 `AGENTS.md` 作为唯一权威

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
