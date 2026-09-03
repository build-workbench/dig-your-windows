---
layout: home
---

<!-- Home Header -->
<div class="home-header">
  <div class="home-header-left">
    <div class="home-logo">DYW</div>
    <div>
      <span class="home-title">DigYourWindows</span>
      <span class="home-subtitle">Windows 深度诊断工具</span>
    </div>
  </div>
  <div class="home-nav">
    <a href="./whitepaper/">技术白皮书</a>
    <a href="./architecture/">架构设计</a>
    <a href="./scoring/">健康评分</a>
    <a href="https://github.com/build-workbench/dig-your-windows">GitHub</a>
  </div>
</div>

<!-- Intro + Stats -->
<div class="home-intro-row">
  <div class="home-intro">
    DigYourWindows 是一款 Windows 深度诊断工具，采用 .NET 10 + WPF 构建，提供硬件监控、事件日志分析、系统健康评分等功能。
  </div>
  <div class="home-stats">
    <span><strong>.NET 10</strong></span>
    <span><strong>WPF</strong></span>
    <span><strong>开源</strong></span>
  </div>
</div>

## 核心能力

<div class="feature-map">
  <div class="feature-card">
    <div class="feature-card-title">🏗️ 架构设计</div>
    <div class="feature-card-desc">
      双层架构：Core 业务逻辑层 + UI 展示层，16 个核心服务，清晰的职责边界。
    </div>
    <div class="feature-tags">
      <a href="./architecture/" class="feature-tag">架构概览</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">📊 健康评分算法</div>
    <div class="feature-card-desc">
      综合稳定性、性能、内存、磁盘四维度，生成 0-100 系统健康评分。
    </div>
    <div class="feature-tags">
      <a href="./scoring/" class="feature-tag">算法详解</a>
      <a href="./scoring/#阈值定义" class="feature-tag">阈值设计</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">🔧 硬件监控</div>
    <div class="feature-card-desc">
      基于 LibreHardwareMonitor，支持 CPU/GPU 温度、负载、频率实时监控。
    </div>
    <div class="feature-tags">
      <a href="./architecture/" class="feature-tag">技术选型</a>
      <a href="https://github.com/build-workbench/dig-your-windows/releases/latest" class="feature-tag">下载</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">🧪 测试覆盖</div>
    <div class="feature-card-desc">
      xUnit 单元测试 + FsCheck 属性测试，80%+ 代码覆盖率要求。
    </div>
    <div class="feature-tags">
      <a href="./guide/testing" class="feature-tag">测试指南</a>
      <a href="https://github.com/build-workbench/dig-your-windows/actions" class="feature-tag">CI/CD</a>
    </div>
  </div>

  <div class="feature-card">
    <div class="feature-card-title">📄 报告导出</div>
    <div class="feature-card-desc">
      支持 HTML/JSON 格式导出，HTML 报告自包含，无需网络即可查看。
    </div>
    <div class="feature-tags">
      <a href="./reference/data-schema" class="feature-tag">数据 Schema</a>
      <a href="https://build-workbench.github.io/dig-your-windows/data-schema.json" class="feature-tag">JSON Schema</a>
    </div>
  </div>
</div>

<!-- Quick Start -->
<div class="quick-start">
  <div class="quick-start-title">快速开始</div>
  <div class="quick-start-content">
    <div class="command-block">
      <code>git clone https://github.com/build-workbench/dig-your-windows.git</code>
    </div>
    克隆仓库后，运行 <code>dotnet run --project src/DigYourWindows.UI</code> 即可启动应用。
  </div>
</div>

<!-- Tech Stack -->
<div class="tech-stack-section vp-doc">

## 技术栈

| 组件 | 技术选型 | 说明 |
|------|----------|------|
| 运行框架 | **.NET 10 + WPF** | 桌面应用开发框架 |
| UI 组件库 | **WPF-UI 4.0** | Fluent Design 风格组件 |
| MVVM 框架 | **CommunityToolkit.Mvvm 8.4** | 数据绑定与命令 |
| 图表库 | **ScottPlot 5.1** | 实时性能趋势可视化 |
| 硬件监控 | **LibreHardwareMonitorLib 0.9** | CPU/GPU 温度与负载 |
| 测试框架 | **xUnit 2.9 + FsCheck 2.16** | 单元测试 + 属性测试 |

</div>

<!-- Documentation Links -->
<div class="docs-section vp-doc">

## 使用指南

<div class="docs-grid">
  <a href="./guide/getting-started" class="doc-card">
    <div class="doc-icon">🚀</div>
    <h4>快速开始</h4>
    <p>环境搭建、构建运行、首次使用指南</p>
  </a>
  <a href="./whitepaper/" class="doc-card">
    <div class="doc-icon">📕</div>
    <h4>技术白皮书</h4>
    <p>项目概述、技术定位、架构亮点</p>
  </a>
  <a href="./architecture/" class="doc-card">
    <div class="doc-icon">🏗️</div>
    <h4>架构设计</h4>
    <p>双层架构、核心服务、数据流设计</p>
  </a>
  <a href="./guide/faq" class="doc-card">
    <div class="doc-icon">❓</div>
    <h4>常见问题</h4>
    <p>常见问题解答与故障排除</p>
  </a>
</div>

</div>

<!-- License -->
<div class="license-section vp-doc">

## 开源许可

本项目采用 <a href="https://github.com/build-workbench/dig-your-windows/blob/master/LICENSE" target="_blank" rel="noopener"><strong>MIT License</strong></a> 开源协议

Copyright © 2025-2026 <a href="https://github.com/build-workbench" target="_blank" rel="noopener">build-workbench</a>

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
