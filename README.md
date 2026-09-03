# DigYourWindows

[![CI](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml/badge.svg)](https://github.com/build-workbench/dig-your-windows/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/build-workbench/dig-your-windows)](https://github.com/build-workbench/dig-your-windows/releases)

> 官方文档: [https://build-workbench.github.io/dig-your-windows/](https://build-workbench.github.io/dig-your-windows/)

**DigYourWindows** 是一款开源、轻量且完全离线的 Windows 深度诊断工具。一键采集硬件状态、实时监测性能负载、排查系统错误日志与可靠性记录，并输出量化健康评分与针对性优化建议。

---

## 快速上手

### 版本选择

| 版本 | 文件名 | 体积 | 依赖要求 | 适用场景 |
| :--- | :--- | :--- | :--- | :--- |
| **安装包（推荐）** | `DigYourWindows_Setup.exe` | ~5 MB | 需要时自动下载 .NET | 日常使用，支持桌面快捷方式与自动卸载 |
| **独立免安装版** | `DigYourWindows-SCD.zip` | ~180 MB | 无（已打包完整运行时） | 离线环境、即开即用、U盘便携随身运行 |
| **框架依赖版** | `DigYourWindows-FDD.zip` | ~60 MB | 系统需已安装 .NET 10 | 体积敏感、开发者或已装 .NET 环境的用户 |

> **提示**：建议右键「以管理员身份运行」，以完整读取 GPU 传感器与磁盘 SMART 等底层底层硬件数据。

### 使用流程

```text
[ 1. 启动程序 ]  ──>  以管理员身份运行 DigYourWindows.UI.exe
       │
[ 2. 一键采集 ]  ──>  点击「刷新数据」，自动提取硬件规格、传感器指标与系统日志
       │
[ 3. 监控排查 ]  ──>  在仪表盘查看 CPU/GPU 实时波形、检查事件日志告警与稳定性趋势
       │
[ 4. 评估优化 ]  ──>  查看 0~100 系统健康综合评分，并参考下发的系统优化方案
       │
[ 5. 导出留存 ]  ──>  一键导出独立单文件 HTML 报告（支持离线浏览）或 JSON 数据
```

---

## 功能对照

| 模块 | 监测与诊断能力 | 输出与分析价值 |
| :--- | :--- | :--- |
| **硬件规格** | CPU、GPU、内存插槽/频率、网络适配器、USB 拓扑 | 清晰罗列整机详细硬件清单与总线连接情况 |
| **实时监控** | CPU/GPU 实时温度、工作负载、运行频率、显存、网络吞吐 | 图表可视化动态走势，快速捕捉高负载、降频或异常升温 |
| **磁盘健康** | NVMe / SATA 硬盘属性、SMART 状态、健康度百分比 | 预测硬盘潜在故障，提前防范数据丢失风险 |
| **系统日志** | 筛选提取 System 与 Application 级别 Error、Warning 事件 | 定位应用崩溃、系统死锁、驱动故障等根因 |
| **稳定性监视** | 接入 Windows Reliability Monitor 可靠性历史索引 | 生成每日稳定性趋势图，客观反映近期系统运行状态 |
| **健康评分** | 稳定性 (40%) + 性能 (30%) + 内存 (15%) + 磁盘 (15%) | 0~100 分综合打分，定位系统短板并给出修复建议 |
| **报告导出** | 导出自包含的单文件 HTML 报告，或导出结构化 JSON | 报告离线可查、易于分享传阅，也便于二次数据分析 |
| **离线安全** | 100% 本地运算与本地存储（SQLite 历史记录） | 零网络上传、零隐私外泄，安全可审计 |

---

## 许可证

本项目基于 [MIT License](LICENSE) 开源。
