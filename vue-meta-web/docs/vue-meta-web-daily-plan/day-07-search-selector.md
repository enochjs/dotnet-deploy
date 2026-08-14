# Day 07 - 远程搜索选择器

## 今日目标

实现 Vue 版 `SearchSelector`，支撑后续所有人员、应用、模板搜索。

## Vue 差异点

- 用 `v-model:value` 承接受控组件。
- 用 `watch` 处理详情回显。
- 用 `defineProps` / `defineEmits` 暴露契约。

## Todo

- 支持远程 `api(keyword)` 搜索和 debounce。
- 支持 `autoSearch`、`searchOnce`、`openSearch`。
- 支持 `mode=multiple`。
- 支持 `formatter`、`labelKey`、`valueKey`。
- 支持详情页通过 value 触发 `fatalApi` 回显。
- 用用户搜索和应用搜索各接一次验证。

## 验收

- 搜索、选择、清空、多选、初始值回显都正常。
- 不会因异步回显覆盖用户刚选择的值。
