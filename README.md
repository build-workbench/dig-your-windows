# DigYourWindows

[![CI](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml/badge.svg)](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/build-workbench/dig-your-windows)](https://github.com/build-workbench/dig-your-windows/releases)

> 📖 **官方文档**: [https://build-workbench.github.io/dig-your-windows/](https://build-workbench.github.io/dig-your-windows/)

**DigYourWindows** 是一款开源、轻量且完全离线的 Windows 深度诊断工具。一键采集系统硬件、实时监测性能、分析系统错误日志与可靠性记录，并提供直观的健康评分与优化建议。

---

## ⚡ 快速开始

### 1. 下载与安装

前往 [Releases](https://github.com/build-workbench/dig-your-windows/releases) 页面下载最新版本：

- **安装版** (`DigYourWindows_Setup.exe`)：推荐，自动处理环境依赖。
- **独立免安装版** (`SCD` / 单文件便携版)：解压即用，无需预装 .NET 运行时。
- **便携版** (`FDD`)：体积小，需系统预装 .NET 10 运行时。

> 💡 **提示**：建议使用**管理员权限**运行以获取完整的 GPU 监控及磁盘 SMART 传感器数据。

### 2. 使用步骤

1. **启动应用**：以管理员身份运行 `DigYourWindows.UI.exe`。
2. **一键诊断**：点击主界面「刷新数据」或「开始诊断」，自动采集硬件与系统日志。
3. **查看监控**：在实时仪表盘中查看 CPU / GPU 负载、温度、显存占用以及网络流量走势。
4. **分析与建议**：查看健康评分（0~100 分）、事件日志告警以及系统针对性优化建议。
5. **导出报告**：点击「导出报告」，保存自包含的 HTML 离线报告或结构化 JSON 文件。

---

## ✨ 核心特性

- 🔍 **硬件深度检测**：CPU、GPU、内存规格、磁盘 SMART 健康状态、网络适配器与 USB 外设。
- 📊 **实时性能监视**：低开销实时监测 CPU/GPU 温度、频率、负荷及网速曲线。
- 📋 **系统稳定性分析**：解析 Windows 事件日志（System/Application）及 Reliability Monitor 历史趋势。
- 🩺 **健康评分体系**：综合稳定性、性能、内存、磁盘 4 大维度输出量化评分与改善建议。
- 📑 **全格式离线报告**：支持导出可直接在浏览器打开的单文件 HTML 报告与 JSON 数据。
- 🔒 **纯离线安全**：100% 本地采集与计算，零网络上传，保护个人与系统隐私。

---

## 🛠️ 开发者使用

### 源码运行

```powershell
git clone https://github.com/build-workbench/dig-your-windows.git
cd dig-your-windows
dotnet restore
dotnet run --project src/DigYourWindows.UI/DigYourWindows.UI.csproj
```

### 运行测试

```powershell
dotnet test DigYourWindows.slnx
```

---

## 📄 许可证

本项目基于 [MIT License](LICENSE) 开源。
