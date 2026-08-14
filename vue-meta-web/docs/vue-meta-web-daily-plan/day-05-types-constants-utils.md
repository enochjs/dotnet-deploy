# Day 05 - 类型、常量、工具

## 今日目标

迁移基础类型和公共工具，保证后续页面有稳定依赖。

## Vue 差异点

- 类型不依赖框架，直接作为项目契约。
- Vue template 对枚举 map 的使用更频繁，常量结构要清晰。

## Todo

- 迁移 API namespace 类型或拆成模块化类型。
- 迁移分页类型 `Pagination<T>`。
- 迁移应用类型、环境、泳道、状态、流水线 job、用户角色、发布类型枚举。
- 实现 `formatTime`、`confirmOperate`、localStorage 工具。
- 实现复制文本工具，供分支、版本、镜像地址使用。

## 验收

- 常量能在组件中正常 import。
- 分页、状态、泳道、环境字典可直接用于 Select/Table。
