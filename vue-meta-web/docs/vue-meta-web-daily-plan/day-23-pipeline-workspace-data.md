# Day 23 - 流水线工作区数据层

## 今日目标

建立运行态流水线的数据层和上下文。

## Vue 差异点

- 用 composable 管理 active/history/swimLane。
- 用 `provide/inject` 支撑 CreatePipeline 和 PipelineList 共享上下文。

## Todo

- 实现 `usePipelineWorkspace(iterationId)`。
- 拉取 active pipeline。
- 拉取 history pipeline。
- 实现 `refreshActive`、`refreshHistory`、`refreshPipelines`。
- 实现泳道状态和 setter。
- 在迭代详情中 provide workspace。

## 验收

- 迭代详情能拉到 active/history 数据。
- 子组件无需 prop drilling 即可刷新流水线。
