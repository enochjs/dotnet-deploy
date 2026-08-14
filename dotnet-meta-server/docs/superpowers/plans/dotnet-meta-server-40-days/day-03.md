# Day 03 - EF Core 实体映射和 PostgreSQL 表结构

## 今日目标

把原系统核心表映射成 EF Core Entity 和配置类。你熟悉业务，因此今天重点是 TypeORM 到 EF Core 的表达方式转换。

## 今天学习的 .NET 点

- `DbContext`、`DbSet<TEntity>`。
- Fluent API：`ToTable`、`HasKey`、`Property`、`HasIndex`。
- nullable reference types 和数据库 nullable。
- PostgreSQL 类型映射：uuid、text、varchar、jsonb、timestamp。

## 实现 Todo

- [ ] 创建 `MetaServerDbContext`。
- [ ] 创建实体：Application、SubApplication、User、Requirement、Iteration。
- [ ] 创建实体：IntegrationRelease、IntegrationReleaseApp。
- [ ] 创建实体：PipelineTpl、PipelineTplStage、PipelineTplJob。
- [ ] 创建实体：Pipeline、PipelineJob、Deploy、Monitor。
- [ ] 为每个实体创建独立 `IEntityTypeConfiguration<T>`。
- [ ] 明确 snake_case 表名和列名策略。
- [ ] 映射 JSON 字段：ranchers、variables、extra。
- [ ] 处理原系统关闭外键约束的关系，避免 EF 默认强约束引入不兼容。
- [ ] 写 metadata 测试，验证表名、主键、关键列名、索引。

## 验收标准

- 14 张核心表都有 Entity 和配置类。
- 字段名、表名、主键类型与原系统兼容。
- JSON 字段有明确序列化策略。
- 测试能检查至少表名、主键、关键索引。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
