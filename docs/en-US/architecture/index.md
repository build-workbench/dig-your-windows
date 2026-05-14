# Architecture Design

DigYourWindows adopts a dual-layer architecture design, achieving clear separation between business logic and UI presentation.

## Layered Architecture

```mermaid
graph TB
    subgraph Presentation["Presentation Layer (UI)"]
        WPF["WPF Views"]
        VM["ViewModels"]
        Commands["RelayCommands"]
    end

    subgraph Business["Business Logic Layer (Core)"]
        Collector["DiagnosticCollectorService"]
        Performance["PerformanceService"]
        Report["ReportService"]
        Hardware["HardwareMonitorProvider"]
        Export["ExportService"]
    end

    subgraph Data["Data Layer"]
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

## Core Services

| Service | Responsibility |
|---------|---------------|
| `DiagnosticCollectorService` | Coordinates all data collection |
| `HardwareMonitorProvider` | Singleton wrapper for hardware monitoring |
| `PerformanceService` | Health scoring algorithm |
| `ReportService` | HTML/JSON report generation |
| `ExportService` | File export functionality |

## Data Flow

```mermaid
sequenceDiagram
    participant User
    participant ViewModel
    participant Collector
    participant Hardware
    participant Performance
    participant Report

    User->>ViewModel: Click "Diagnose" button
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
    ViewModel-->>User: Display results
```

## Design Decisions

### Why Dual-Layer Architecture?

1. **Separation of Concerns** - UI handles display only, Core handles business logic only
2. **Testability** - Core layer can be unit tested independently
3. **Maintainability** - UI changes don't affect business logic

### Why CommunityToolkit.Mvvm?

1. **Source Generators** - `[ObservableProperty]` and `[RelayCommand]` reduce boilerplate
2. **Performance Optimized** - Compile-time code generation, no runtime reflection
3. **Microsoft Official** - Part of .NET Community Toolkit

### Why LibreHardwareMonitor?

1. **Comprehensive Coverage** - Supports CPU, GPU, motherboard, disk, network, etc.
2. **Open Source & Free** - MIT license, commercial use allowed
3. **Active Maintenance** - Continuous updates for latest hardware
