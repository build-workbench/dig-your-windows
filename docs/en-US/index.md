---
layout: home
---

<!-- Home Header -->
<div class="home-header">
  <div class="home-header-left">
    <div class="home-logo">DYW</div>
    <div>
      <span class="home-title">DigYourWindows</span>
      <span class="home-subtitle">Windows Deep Diagnostics Tool</span>
    </div>
  </div>
  <div class="home-nav">
    <a href="./whitepaper/">Whitepaper</a>
    <a href="./architecture/">Architecture</a>
    <a href="./openspec/">OpenSpec</a>
    <a href="https://github.com/AICL-Lab/dig-your-windows">GitHub</a>
    <a href="../zh-CN/">中文</a>
  </div>
</div>

<!-- Intro + Stats -->
<div class="home-intro-row">
  <div class="home-intro">
    DigYourWindows is a Windows deep diagnostics tool built with .NET 10 + WPF, following OpenSpec specification-driven development, providing hardware monitoring, event log analysis, and system health scoring.
  </div>
  <div class="home-stats">
    <span><strong>.NET 10</strong></span>
    <span><strong>WPF</strong></span>
    <span><strong>OpenSpec</strong></span>
  </div>
</div>

## Core Capabilities

<div class="feature-map">
  <div class="feature-card">
    <div class="feature-card-title">🏗️ Architecture Design</div>
    <div class="feature-card-desc">
      Dual-layer architecture: Core business logic + UI presentation, 16 core services, clear responsibility boundaries.
    </div>
    <div class="feature-tags">
      <a href="./architecture/" class="feature-tag">Overview</a>
      <a href="./architecture/services" class="feature-tag">Services</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">📋 OpenSpec</div>
    <div class="feature-card-desc">
      Spec-Driven Development, specification first, acceptance-driven development, ensuring code quality.
    </div>
    <div class="feature-tags">
      <a href="./openspec/" class="feature-tag">Overview</a>
      <a href="https://github.com/AICL-Lab/dig-your-windows/tree/master/openspec/specs" class="feature-tag">Source</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">📊 Health Scoring</div>
    <div class="feature-card-desc">
      Comprehensive scoring across stability, performance, memory, and disk dimensions, generating 0-100 system health score.
    </div>
    <div class="feature-tags">
      <a href="./scoring/" class="feature-tag">Algorithm</a>
      <a href="./scoring/#threshold-definitions" class="feature-tag">Thresholds</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">🔧 Hardware Monitoring</div>
    <div class="feature-card-desc">
      Based on LibreHardwareMonitor, supports real-time CPU/GPU temperature, load, and frequency monitoring.
    </div>
    <div class="feature-tags">
      <a href="./architecture/" class="feature-tag">Tech Choice</a>
      <a href="https://github.com/AICL-Lab/dig-your-windows/releases/latest" class="feature-tag">Download</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">🧪 Test Coverage</div>
    <div class="feature-card-desc">
      xUnit unit tests + FsCheck property-based tests, 80%+ code coverage requirement.
    </div>
    <div class="feature-tags">
      <a href="./guide/testing" class="feature-tag">Testing Guide</a>
      <a href="https://github.com/AICL-Lab/dig-your-windows/actions" class="feature-tag">CI/CD</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">📄 Report Export</div>
    <div class="feature-card-desc">
      Supports HTML/JSON format export, HTML reports are self-contained for offline viewing.
    </div>
    <div class="feature-tags">
      <a href="./reference/data-schema" class="feature-tag">Data Schema</a>
      <a href="https://aicl-lab.github.io/dig-your-windows/data-schema.json" class="feature-tag">JSON Schema</a>
    </div>
  </div>
</div>

<!-- Quick Start -->
<div class="quick-start">
  <div class="quick-start-title">Quick Start</div>
  <div class="quick-start-content">
    <div class="command-block">
      <code>git clone https://github.com/AICL-Lab/dig-your-windows.git</code>
    </div>
    After cloning, run <code>dotnet run --project src/DigYourWindows.UI</code> to start the application.
  </div>
</div>

<!-- Tech Stack -->
<div class="tech-stack-section vp-doc">

## Tech Stack

| Component | Technology | Description |
|-----------|------------|-------------|
| Runtime | **.NET 10 + WPF** | Desktop application framework |
| UI Library | **WPF-UI 4.0** | Fluent Design style components |
| MVVM Framework | **CommunityToolkit.Mvvm 8.4** | Data binding and commands |
| Charting | **ScottPlot 5.1** | Real-time performance visualization |
| Hardware Monitoring | **LibreHardwareMonitorLib 0.9** | CPU/GPU temperature and load |
| Testing Framework | **xUnit 2.9 + FsCheck 2.16** | Unit tests + Property-based tests |

</div>

<!-- Documentation Links -->
<div class="docs-section vp-doc">

## Documentation

<div class="docs-grid">
  <a href="./guide/getting-started" class="doc-card">
    <div class="doc-icon">🚀</div>
    <h4>Getting Started</h4>
    <p>Environment setup, build, run, first-use guide</p>
  </a>
  <a href="./whitepaper/" class="doc-card">
    <div class="doc-icon">📕</div>
    <h4>Whitepaper</h4>
    <p>Project overview, tech positioning, architecture highlights</p>
  </a>
  <a href="./architecture/" class="doc-card">
    <div class="doc-icon">🏗️</div>
    <h4>Architecture</h4>
    <p>Dual-layer architecture, core services, data flow design</p>
  </a>
  <a href="./guide/faq" class="doc-card">
    <div class="doc-icon">❓</div>
    <h4>FAQ</h4>
    <p>Common questions and troubleshooting</p>
  </a>
</div>

</div>

<!-- License -->
<div class="license-section vp-doc">

## Open Source License

This project is licensed under the <a href="https://github.com/AICL-Lab/dig-your-windows/blob/master/LICENSE" target="_blank" rel="noopener"><strong>MIT License</strong></a>

Copyright © 2025-2026 <a href="https://github.com/AICL-Lab" target="_blank" rel="noopener">AICL-Lab</a>

</div>

<style scoped>
/* Home Header */
.home-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  margin-bottom: 2rem;
  border-bottom: 1px solid var(--vp-c-divider);
}

.home-header-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.home-logo {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--vp-c-brand-1) 0%, var(--dyw-accent-purple, #8764b8) 100%);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 700;
  font-size: 14px;
}

.home-title {
  font-weight: 600;
  font-size: 1.1rem;
  color: var(--vp-c-text-1);
}

.home-subtitle {
  color: var(--vp-c-text-2);
  font-size: 0.85rem;
  margin-left: 0.5rem;
}

.home-nav {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
}

.home-nav a {
  color: var(--vp-c-text-2);
  text-decoration: none;
  transition: color 0.15s ease;
}

.home-nav a:hover {
  color: var(--vp-c-brand-1);
}

/* Intro + Stats */
.home-intro-row {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.home-intro {
  flex: 1;
  color: var(--vp-c-text-2);
  font-size: 0.95rem;
  line-height: 1.7;
}

.home-stats {
  display: flex;
  gap: 1rem;
  font-size: 0.85rem;
  color: var(--vp-c-text-2);
  flex-shrink: 0;
}

.home-stats strong {
  color: var(--vp-c-brand-1);
}

/* Feature Map */
.feature-map {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 2rem;
}

.feature-card {
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  border-radius: 12px;
  padding: 1.25rem;
  transition: all 0.2s ease;
}

.feature-card:hover {
  border-color: var(--vp-c-brand-1);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 120, 212, 0.1);
}

.feature-card-title {
  font-weight: 600;
  font-size: 1rem;
  color: var(--vp-c-text-1);
  margin-bottom: 0.5rem;
}

.feature-card-desc {
  font-size: 0.85rem;
  color: var(--vp-c-text-2);
  line-height: 1.6;
  margin-bottom: 0.75rem;
}

.feature-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.feature-tag {
  font-size: 0.75rem;
  color: var(--vp-c-brand-1);
  text-decoration: none;
  padding: 0.25rem 0.5rem;
  background: var(--vp-c-brand-soft);
  border-radius: 4px;
  transition: all 0.15s ease;
}

.feature-tag:hover {
  background: var(--vp-c-brand-1);
  color: white;
}

/* Quick Start */
.quick-start {
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  border-radius: 12px;
  padding: 1.25rem;
  margin-bottom: 2rem;
}

.quick-start-title {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--vp-c-text-1);
  margin-bottom: 0.5rem;
}

.quick-start-content {
  color: var(--vp-c-text-2);
  font-size: 0.9rem;
  line-height: 1.6;
}

.quick-start-content code {
  background: var(--vp-c-bg);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.85rem;
  color: var(--vp-c-brand-1);
}

.command-block {
  background: var(--vp-c-bg);
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  margin: 0.75rem 0;
}

.command-block code {
  background: transparent;
  padding: 0;
  font-family: ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, monospace;
  font-size: 0.85rem;
  color: var(--vp-c-text-1);
}

/* Tech Stack */
.tech-stack-section {
  margin: 2rem 0;
}

.tech-stack-section table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid var(--vp-c-divider);
}

.tech-stack-section th {
  background: var(--vp-c-bg-soft);
  font-weight: 600;
  text-align: left;
  padding: 0.75rem 1rem;
  font-size: 0.85rem;
}

.tech-stack-section td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--vp-c-divider);
  font-size: 0.9rem;
}

.tech-stack-section tr:last-child td {
  border-bottom: none;
}

.tech-stack-section tr:hover td {
  background: var(--vp-c-bg-soft);
}

/* Documentation */
.docs-section {
  margin: 2rem 0;
}

.docs-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
  margin-top: 1rem;
}

.doc-card {
  display: block;
  padding: 1.25rem;
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  border-radius: 12px;
  text-decoration: none;
  transition: all 0.2s ease;
}

.doc-card:hover {
  background: var(--vp-c-bg);
  border-color: var(--vp-c-brand-1);
  transform: translateY(-2px);
}

.doc-icon {
  font-size: 1.75rem;
  margin-bottom: 0.5rem;
}

.doc-card h4 {
  margin: 0 0 0.25rem;
  color: var(--vp-c-brand-1);
  font-size: 0.95rem;
}

.doc-card p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--vp-c-text-2);
}

/* License */
.license-section {
  text-align: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--vp-c-divider);
}

/* Responsive */
@media (max-width: 959px) {
  .feature-map {
    grid-template-columns: repeat(2, 1fr);
  }

  .docs-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .home-intro-row {
    flex-direction: column;
    gap: 1rem;
  }
}

@media (max-width: 639px) {
  .feature-map {
    grid-template-columns: 1fr;
  }

  .docs-grid {
    grid-template-columns: 1fr;
  }

  .home-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .home-nav {
    flex-wrap: wrap;
  }
}
</style>
