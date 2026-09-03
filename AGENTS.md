# DigYourWindows — AI Agent Reference

**Project**: Windows 深度诊断工具 | .NET 10 WPF | **Status**: v1.2.0 归档稳定版
**Repo**: `LessUp/dig-your-windows` | **Docs**: `https://lessup.github.io/dig-your-windows/`

---

## Architecture: Two-Layer

```
DigYourWindows.UI          →  WPF (MVVM, CommunityToolkit.Mvvm 8.4, ScottPlot 5.1, WPF-UI 4.0)
DigYourWindows.Core        →  Business logic (services, models, exceptions) — class library
DigYourWindows.Tests       →  xUnit 2.9 + FsCheck 2.16 (Unit/ PropertyTests/ Integration/)
```

## UI Services (DigYourWindows.UI/Services/)

Framework concerns wrapped as injectable services so ViewModels stay testable and UI-framework-agnostic (pattern borrowed from the WPF-UI Gallery reference app):

| Service | Responsibility |
|---------|----------------|
| `MonitorPlotService` | Owns the two ScottPlot `WpfPlot` controls and all rendering (theme, series, date axes, CJK fonts) |
| `ApplicationThemeService` | Wraps WPF-UI `ApplicationThemeManager` for dark/light toggling |
| `FileDialogService` | File pickers and shell reveal (`Process.Start`) — never used directly in ViewModels |

## Core Services (DigYourWindows.Core/Services/)

| Service | Responsibility | Key Dependency |
|---------|---------------|----------------|
| `HardwareService` | CPU/GPU/RAM/Disk/Network/USB via WMI | `System.Management` |
| `CpuMonitorService` | Real-time CPU temp/load/freq | `HardwareMonitorProvider` |
| `GpuMonitorService` | Real-time GPU temp/load/VRAM | `HardwareMonitorProvider` |
| `NetworkMonitorService` | Adapter info + bandwidth tracking | WMI + PerformanceCounter |
| `DiskSmartService` | SMART health data (NVMe + SATA) | `LibreHardwareMonitorLib` |
| `EventLogService` | System/Application errors via XML query | `EventLogReader` |
| `ReliabilityService` | Windows Reliability Monitor data | WMI `Win32_ReliabilityRecords` |
| `PerformanceService` | Health scoring: stability 40%+perf 30%+mem 15%+disk 15% | Pure computation |
| `ReportService` | HTML + JSON export | `System.Text.Json` |
| `DiagnosticCollectorService` | Orchestrates all collection with progress+cancellation | All above |
| `HardwareMonitorProvider` | Thread-safe singleton wrapping `LibreHardwareMonitor.Computer` | `LibreHardwareMonitorLib 0.9.4` |
| `SqliteHistoryStoreService` | Diagnostic history persistence | `Microsoft.Data.Sqlite` |
| `LogService` | Buffered file logging with rotation | `StreamWriter` |
| `ReliabilityTrendBuilder` | Pure per-day reliability trend aggregation (testable) | — |

## Data Models (DigYourWindows.Core/Models/)

| File | Key Types |
|------|-----------|
| `DiagnosticData.cs` | `DiagnosticData` (root), `SystemInfo` |
| `HardwareData.cs` | `HardwareData`, `MemoryData`, `NetworkAdapterInfo` |
| `DiskModels.cs` | `DiskInfoData`, `DiskSmartData`, `SmartAttribute` |
| `ComputeModels.cs` | `CpuData`, `GpuInfoData` |
| `EventModels.cs` | `LogEvent`, `ReliabilityRecord` |
| `DeviceModels.cs` | Network/USB device info |
| `PerformanceAnalysisData.cs` | `PerformanceAnalysisData`, `Recommendation` |
| `CollectionModels.cs` | `DiagnosticCollectionResult`, `DiagnosticCollectionProgress` |
| `DiagnosticHistoryRecord.cs` / `DiagnosticHistorySummary.cs` | History store records |

## Custom Exceptions (DigYourWindows.Core/Exceptions/)

| Exception | Factory Methods | Key Properties |
|-----------|----------------|----------------|
| `ReportException` | `ReportException.Serialization(...)`, `.InvalidData(...)` | `ErrorType` (ReportErrorType enum) |

`ReportErrorType`: `Unknown`, `Serialization`, `InvalidData`.

## Build Commands

```powershell
dotnet restore DigYourWindows.slnx
dotnet build DigYourWindows.slnx -c Release --no-restore
dotnet test DigYourWindows.slnx -c Release --no-restore
dotnet test --filter "Category=Unit"
dotnet test --collect:"XPlat Code Coverage"
```

## Code Conventions

```csharp
// ✅ Correct
public sealed class HardwareMonitorProvider : IHardwareMonitorProvider, IDisposable
{
    private readonly object _lock = new();
    private ILogService _logService;  // _camelCase fields
    public Computer Computer { get; }  // PascalCase props/methods
}

// ❌ Wrong: manual INotifyPropertyChanged, Newtonsoft.Json, non-nullable without justification
```

- **MVVM**: `[ObservableProperty]` + `[RelayCommand]` source generators — never manual implementation
- **Async**: All I/O ops take `CancellationToken cancellationToken = default`
- **Nullable**: All `?` must be handled — no `!` suppressions without comment
- **Warnings = Errors**: Zero tolerance; `TreatWarningsAsErrors` is on

## Commit Convention

```
<type>(<scope>): <description>
types: feat fix docs refactor test chore
scopes: core ui tests docs build
```

## Archive Status

**v1.2.0** 是稳定归档版本。项目不再积极开发，仅保留 bug 修复。
