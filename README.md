# DigYourWindows

[![CI](https://github.com/LessUp/dig-your-windows/actions/workflows/ci.yml/badge.svg)](https://github.com/LessUp/dig-your-windows/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/LessUp/dig-your-windows)](https://github.com/LessUp/dig-your-windows/releases)

> 📖 **文档站**: [https://lessup.github.io/dig-your-windows/](https://lessup.github.io/dig-your-windows/)

**Windows 深度诊断工具** — 一键采集硬件信息、分析事件日志、查看可靠性记录、计算系统健康评分并给出优化建议。

## 功能

| 功能 | 说明 |
|------|------|
| 🔍 硬件检测 | CPU、GPU、内存、磁盘（含 SMART）、网卡、USB 设备 |
| 📊 实时监控 | CPU 温度/负载/频率、GPU 温度/负载/显存、网络流量 |
| 📋 事件日志分析 | 自动提取 System/Application 错误与警告 |
| 📈 可靠性记录 | Windows 可靠性监视器数据与趋势可视化 |
| ✅ 健康评分 | 综合稳定性、性能、内存、磁盘四维度 0-100 评分 |
| 💡 智能建议 | 基于分析结果的针对性优化建议 |
| 📄 报告导出 | HTML / JSON 格式，HTML 报告自包含可离线查看 |
| 🗃️ 历史记录 | SQLite 存储诊断历史，支持趋势对比 |
| 🎨 主题切换 | 深色/浅色主题即时切换 |

## 快速开始

### 系统要求

| 组件 | 最低 | 推荐 |
|------|------|------|
| 操作系统 | Windows 10 (Build 19041+) | Windows 11 |
| 内存 | 4 GB | 8 GB+ |
| 磁盘 | 200 MB 可用空间 | 500 MB+ |
| 权限 | 标准用户 | 管理员（完整硬件访问） |

### 安装

**方式一：下载发布版（推荐）**

从 [Releases](https://github.com/LessUp/dig-your-windows/releases) 下载安装包：

| 版本 | 大小 | 说明 |
|------|------|------|
| `DigYourWindows_Setup.exe` | ~5MB | 需要时自动下载 .NET |
| FDD 便携版 | ~60MB | 需预装 .NET 10 运行时 |
| SCD 独立版 | ~180MB | 无任何依赖 |

**方式二：源码构建**

```powershell
git clone https://github.com/LessUp/dig-your-windows.git
cd dig-your-windows
dotnet restore
dotnet run --project src/DigYourWindows.UI/DigYourWindows.UI.csproj
```

> ⚠️ GPU 监控、SMART 数据读取等功能需要管理员权限。

### 首次使用

1. 启动 `DigYourWindows.UI.exe`
2. 点击"刷新数据"采集系统信息
3. 在仪表盘查看实时硬件监控
4. 导出 JSON 或 HTML 报告

## 技术栈

| 组件 | 技术 | 版本 | 用途 |
|------|------|------|------|
| 运行时 | .NET + WPF | 10.0 | 桌面应用框架 |
| UI 库 | WPF-UI | 4.0 | Fluent Design 组件 |
| MVVM | CommunityToolkit.Mvvm | 8.4 | 数据绑定与命令 |
| 图表 | ScottPlot | 5.1 | 性能趋势可视化 |
| 硬件监控 | LibreHardwareMonitor | 0.9 | CPU/GPU 温度、负载、频率 |
| 测试 | xUnit + FsCheck | 2.9 / 2.16 | 单元测试 + 属性测试 |

## 项目结构

```
dig-your-windows/
├── src/
│   ├── DigYourWindows.Core/     # 核心业务逻辑（服务、模型、异常）
│   └── DigYourWindows.UI/       # WPF 用户界面（MVVM、转换器、XAML）
├── tests/
│   └── DigYourWindows.Tests/    # 单元测试 + 属性测试 + 集成测试
├── docs/                        # VitePress 文档站（中文）
├── installer/                   # Inno Setup 安装脚本
└── scripts/                     # 构建与发布脚本
```

## 测试

```powershell
dotnet test DigYourWindows.slnx
dotnet test --collect:"XPlat Code Coverage"
dotnet test --filter "FullyQualifiedName~ReportServiceTests"
```

## 发布

推送 `v*` 标签触发自动发布：

```powershell
git tag v1.2.0
git push origin v1.2.0
```

GitHub Actions 自动构建并发布 FDD 便携版和 SCD 独立版。

## 安全

- ✅ 完全离线运行，不上传任何数据
- ✅ 所有操作仅使用本地文件系统
- ✅ 开源可审计

## 许可证

[MIT License](LICENSE) - Copyright © 2025-2026 LessUp
