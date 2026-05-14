# OpenSpec 规范

DigYourWindows 采用 **Spec-Driven Development (SDD)** 开发模式，所有功能在编码前先定义规范。

## 什么是 OpenSpec？

OpenSpec 是一种规范驱动开发方法，核心原则：

1. **先规范，后代码** - 每个功能必须先有明确的规范定义
2. **验收驱动** - 使用 Given/When/Then 格式定义验收标准
3. **单一职责** - 每个规范只描述一个领域

## 规范结构

```
openspec/specs/
├── architecture/   # 架构设计决策
├── data/           # 数据模型和验证
├── export/         # 报告导出 API
├── features/       # 产品功能验收标准
├── hardware/       # 硬件监控阈值和异常
├── testing/        # 测试策略
└── workflow/       # 开发流程规范
```

## 领域规范

### Architecture 规范

定义系统架构设计决策：

- 分层架构设计
- 服务边界定义
- 依赖注入配置

### Data 规范

定义数据模型和验证规则：

- `DiagnosticData` 模型结构
- 数据验证规则
- JSON 序列化配置

### Features 规范

使用 Gherkin 格式定义功能验收标准：

```gherkin
Feature: 系统健康评分
  As a 用户
  I want to 获取系统健康评分
  So that I can 了解系统整体状态

  Scenario: 正常系统评分
    Given 系统运行正常
    When 用户请求健康评分
    Then 应返回 70-100 分的评分
    And 应显示详细的分项得分
```

### Hardware 规范

定义硬件监控阈值：

| 指标 | 正常范围 | 警告阈值 | 危险阈值 |
|------|----------|----------|----------|
| CPU 温度 | < 70°C | 70-85°C | > 85°C |
| GPU 温度 | < 75°C | 75-90°C | > 90°C |
| 内存使用 | < 70% | 70-85% | > 85% |
| 磁盘使用 | < 80% | 80-90% | > 90% |

### Testing 规范

定义测试策略：

- 单元测试覆盖率要求：≥ 80%
- 属性测试使用 FsCheck
- CI/CD 集成测试

## 工作流程

```mermaid
graph LR
    A[需求] --> B[编写 OpenSpec]
    B --> C[评审规范]
    C --> D[实现代码]
    D --> E[编写测试]
    E --> F[验收测试]
    F --> G[归档]
```

## 规范价值

1. **沟通工具** - 团队成员对需求有统一理解
2. **文档即代码** - 规范与代码同步更新
3. **可追溯性** - 每个功能都有明确的来源和目标
