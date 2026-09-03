# DigYourWindows

[![CI](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml/badge.svg)](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/build-workbench/dig-your-windows)](https://github.com/build-workbench/dig-your-windows/releases)

> 文档站: [https://build-workbench.github.io/dig-your-windows/](https://build-workbench.github.io/dig-your-windows/)

**DigYourWindows** 是一款开源、轻量且完全离线的 Windows 深度诊断工具。一键采集硬件信息、实时监测性能指标、分析系统错误与可靠性记录，并给出直观的健康评分与优化建议。

---

## 快速上手

### 1. 下载

前往 [Releases](https://github.com/build-workbench/dig-your-windows/releases) 页面下载对应版本：

- **安装版** (`DigYourWindows_Setup.exe`)：推荐，自动检测并安装必要依赖。
- **独立免安装版** (`SCD` / 便携版)：解压即用，无需预先安装 .NET 运行时。
- **轻量便携版** (`FDD`)：体积较小，需系统已安装 .NET 10 运行时。

*注：建议右键「以管理员身份运行」，以完整读取 GPU 传感器与磁盘 SMART 等底层数据。*

### 2. 使用步骤

1. **启动应用**：以管理员身份运行 `DigYourWindows.UI.exe`。
2. **开始诊断**：点击「刷新数据」或「开始诊断」，自动采集硬件规格及系统日志。
3. **实时监控**：在仪表盘查看 CPU / GPU 负载、温度、显存占用与实时网络流量。
4. **健康评估**：查看 0~100 分系统健康评分，检查事件日志中的警告与错误，获取针对性优化建议。
5. **导出报告**：点击「导出报告」，保存自包含的 HTML 离线报告（可在任意浏览器查看）或结构化 JSON 数据。

---

## 主要功能

- **硬件深度检测**：读取 CPU、GPU、内存规格、磁盘 SMART 状态、网络适配器及 USB 设备。
- **实时性能监控**：低资源占用采集 CPU/GPU 温度、频率、负荷走势及网络带宽。
- **稳定性分析**：解析 Windows 事件日志（System/Application）及 Reliability Monitor 可靠性历史。
- **健康评分体系**：综合稳定性、性能、内存、磁盘四个维度进行量化评分并提供优化指引。
- **离线报告导出**：支持导出独立的单文件 HTML 报告与 JSON 数据，方便留存与归档。
- **纯离线安全**：全部计算与分析均在本地完成，无任何网络数据上传。

---

## 许可证

本项目采用 [MIT License](LICENSE) 开源协议。
