# Day 08 - 列表页范式

## 今日目标

建立所有列表页的统一写法。

## Vue 差异点

- 用 composable 管理列表状态比封装大组件更灵活。
- 查询参数可用 `reactive`，分页结果用 `ref`。

## Todo

- 设计 `usePaginationList` 或明确页面内写法。
- 固定查询表单提交逻辑：重置 `pageIndex=1`。
- 固定分页变化逻辑：更新 `pageIndex/pageSize` 后刷新。
- 固定 loading、empty、table scroll、移动端表单布局。
- 做一个 demo 列表页验证模式。

## 验收

- 后续列表页能按同一模式复制。
- 搜索、分页、loading、移动端布局规则明确。
