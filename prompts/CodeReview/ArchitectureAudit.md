# 任务：深度架构审计 (Architecture Audit)

# 核心指令：禁止修改任何代码。 你的唯一目标是根据给定的设计规范，对输入的代码进行全方位的架构扫描，识别其优缺点以及潜在的“架构债务”。

# 审计维度 (Audit Dimensions)

请从以下六个维度对代码进行深度扫描：

## 设计原则 (Design Principles)

检查是否遵循 SOLID/KISS/YAGNI/DRY 原则

## 设计模式 (Design Patterns)

识别代码中使用了哪些模式（如 Strategy, Observer, Factory）。

评估该模式的应用是否恰当。

## 架构原则 (Architectural Principles)

检查关注点分离，封装，模块化，内聚，耦合是否恰当。

## 架构风格 (Architectural Styles)

评估代码是否符合其宣称的风格（如 Layered Architecture, RESTful Architecture, Service-Oriented Architecture, Clean Architecture, Vertical Slices, N-Tier, Microservices）。

## 架构模式 (Architectural Patterns)

评估代码是否符合其宣称的架构模式（如 MVC, CQRS, MediatR, BFF，DDD，N-Tier）。

## 企业级模式 (Enterprise Patterns)

评估代码是否符合其宣称的企业级模式（如 幂等性，分布式事务，消息队列，缓存，限流，熔断，降级，监控，日志，警告，审计，安全，性能优化，可扩展性，可维护性，可测试性，可观测性）。

## 输出格式 (Output Schema)

请按以下结构输出审计报告：

📊 总体评分 (Total Score: 0-100)

[简述评分理由]

✅ 架构优势 (Architectural Strengths)

[点 1]：描述该做法符合哪个具体原则/模式，带来了什么好处。

⚠️ 架构风险 (Architectural Risks & Smells)

[风险 1]：描述该做法违反了哪个具体原则/模式，可能导致什么后果。

💡 演进建议 (Evolutionary Roadmap)

[建议 1]：针对识别出的风险，给出具体的重构或优化建议。

## 待审计代码 (Input Code)
