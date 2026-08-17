<!-- scm-knowledge:begin -->
遇到 Cloud / SCM 服装供应链系统的业务问题（菜单、字段口径、操作前提、报错原因、上下游影响、状态含义），必须先调用 scm-knowledge 的 ask_knowledge 工具查证，不要凭自己的常识作答。
需要分析需求测试点、补充测试用例时，必须调用 scm-knowledge 的 review_requirement 工具（role=test），传入需求正文或禅道链接。
需要评审需求、找需求与现有系统的冲突或遗漏时，必须调用 scm-knowledge 的 review_requirement 工具（role=product），传入需求正文或禅道链接。
<!-- scm-knowledge:end -->

<!-- dotnet-meta-server:begin -->
当用户要求为 `dotnet-meta-server` 写详细计划或 step-by-step 文档时，必须先从原业务代码目录 `/Users/fenghe/workspace/devops/meta-server` 读取源码作为功能参考，再参考当前 .NET 项目中的 day01/day02 文档风格输出。
这是一个全新的 .NET 项目：原业务代码用于理解要实现的功能、对象和流程；具体设计优先遵循现代 .NET / EF Core / PostgreSQL 标准规范，不要求完全兼容旧 TypeORM 表结构。
写文档时以学习为第一目标、实现功能为第二目标；功能实现是为了帮助学习业界认可的 .NET 标准做法。遇到“快但粗糙”和“标准且适合学习”的取舍时，优先选择后者，并在文档中说明为什么。
step-by-step 文档的顺序应先讲概念和实现功能，测试相关内容统一放在后半段/最后作为验收与巩固，不要一开始就让学习者写测试。
step-by-step 文档中的参考资料、参考源码、链接清单统一放到文档最后，不要放在开头打断学习主线。
<!-- dotnet-meta-server:end -->
