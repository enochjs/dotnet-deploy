# Day 12 - 应用环境变量

## 今日目标

完成环境变量编辑器。

## Vue 差异点

- ag-grid api 用 `shallowRef` 保存。
- 隐藏 tab 下的数据要维护本地副本，不能只依赖 grid node。

## Todo

- 接入 `ag-grid-vue3`。
- 实现 KEY/VALUE 两列可编辑。
- 实现添加行、删除行、删除选中。
- 实现复制全部、复制选中。
- 实现粘贴 `KEY=VALUE` 文本并追加行。
- 保存前弹确认，提交 `subAppId/envKey/swimLaneKey/variables`。

## 验收

- 隐藏 tab 切回来数据不丢。
- 保存变量成功后有提示。
- 复制粘贴格式与 React 版一致。
