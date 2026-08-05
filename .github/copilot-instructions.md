# DigYourWindows — Copilot Instructions

Windows deep diagnostics desktop app. .NET 10 + WPF. **Maintenance mode**: bug fixes only, no new features.

> **Full reference**: [`AGENTS.md`](../AGENTS.md) — architecture, services, build/test, OpenSpec workflow, conventions.

## Project Structure

```
src/DigYourWindows.Core/    # Business logic — services, models, exceptions
src/DigYourWindows.UI/      # WPF app — MVVM, XAML, converters
tests/DigYourWindows.Tests/ # xUnit + FsCheck (Unit/ PropertyTests/ Integration/)
openspec/specs/             # Source of truth — read before touching code
```

## Key Rules

1. **Check spec first**: Before any change, read `openspec/specs/<domain>/spec.md`
2. **No new features**: Only bug fixes and test improvements for v1.2.0
3. **Zero warnings**: `TreatWarningsAsErrors=true` — all builds must be clean
4. **No Newtonsoft.Json**: Use `System.Text.Json` exclusively
5. **MVVM via source generators**: `[ObservableProperty]` / `[RelayCommand]` only
6. **Nullable**: Handle all nullables properly; no `!` without comment

## Build & Test

```powershell
dotnet build DigYourWindows.slnx -c Release
dotnet test DigYourWindows.slnx -c Release
```

## Commit Format

`<type>(<scope>): <description>` — types: feat fix docs refactor test chore specs
