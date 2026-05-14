# 架构设计

DigYourWindows 采用双层架构设计，实现业务逻辑与 UI 展示的清晰分离。

## 分层架构

```mermaid
graph TB
    subgraph Presentation["表现层 (UI)"]
        WPF["WPF Views"]
        VM["ViewModels"]
        Commands["RelayCommands"]
    end

    subgraph Business["业务逻辑层 (Core)"]
        Collector["DiagnosticCollectorService"]
        Performance["PerformanceService"]
        Report["ReportService"]
        Hardware["HardwareMonitorProvider"]
        Export["ExportService"]
    end

    subgraph Data["数据层"]
        Models["Models"]
        Events["Events"]
        Exceptions["Exceptions"]
    end

    WPF --> VM
    VM --> Commands
    Commands --> Collector
    Commands --> Performance
    Commands --> Report
    Collector --> Hardware
    Collector --> Models
    Performance --> Models
    Report --> Export
    Export --> Models
```

## 核心服务

| 服务 | 职责 |
|------|------|
| `DiagnosticCollectorService` | 协调所有数据采集 |
| `HardwareMonitorProvider` | 硬件监控单例封装 |
| `PerformanceService` | 健康评分算法 |
| `ReportService` | HTML/JSON 报告生成 |
| `ExportService` | 文件导出功能 |

## 数据流

```mermaid
sequenceDiagram
    participant User
    participant ViewModel
    participant Collector
    participant Hardware
    participant Performance
    participant Report

    User->>ViewModel: 点击"诊断"按钮
    ViewModel->>Collector: CollectAllAsync()
    Collector->>Hardware: GetCpuInfo()
    Hardware-->>Collector: CpuInfo
    Collector->>Hardware: GetGpuInfo()
    Hardware-->>Collector: GpuInfo
    Collector-->>ViewModel: DiagnosticData
    ViewModel->>Performance: CalculateScore(DiagnosticData)
    Performance-->>ViewModel: HealthScore
    ViewModel->>Report: GenerateReport(DiagnosticData, HealthScore)
    Report-->>ViewModel: ReportPath
    ViewModel-->>User: 显示结果
```

## 设计决策

### 为什么选择双层架构？

1. **关注点分离** - UI 只负责展示，Core 只负责业务逻辑
2. **可测试性** - Core 层可以独立进行单元测试
3. **可维护性** - 修改 UI 不影响业务逻辑

### 为什么选择 CommunityToolkit.Mvvm？

1. **源生成器** - 使用 `[ObservableProperty]` 和 `[RelayCommand]` 减少样板代码
2. **性能优化** - 编译时生成代码，无运行时反射开销
3. **微软官方** - 属于 .NET Community Toolkit 的一部分

### 为什么选择 LibreHardwareMonitor？

1. **全面覆盖** - 支持 CPU、GPU、主板、磁盘、网络等
2. **开源免费** - MIT 协议，可商业使用
3. **活跃维护** - 持续更新支持最新硬件
