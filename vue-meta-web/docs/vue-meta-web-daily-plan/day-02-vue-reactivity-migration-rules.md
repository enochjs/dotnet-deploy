# Day 02 - Vue 响应式迁移规则

## 今日目标

把 React 心智映射到 Vue 心智，形成项目内可复用写法。

## Vue 差异点

- `useState` -> `ref` / `reactive`。
- `useMemo` -> `computed`。
- `useEffect` -> `watch` / `watchEffect` / 生命周期。
- Context -> `provide/inject`。

## Todo

- 写 `ref`、`reactive`、`computed`、`watch` 的项目级示例。
- 验证对象表单数据使用 `reactive` 还是 `ref` 更顺手。
- 验证异步列表 loading、pagination、searchParams 的组织方式。
- 设计 composable 返回值规范：状态、动作、刷新函数分开。
- 记录哪些场景用 `shallowRef`，例如表格实例、ag-grid api、WebSocket。

## 验收

- 形成一页 React -> Vue 映射笔记。
- 后续页面的状态组织方式已经固定。
