# Day 15 - 模板编辑器数据结构

## 今日目标

先完成流水线模板编辑器的数据结构和保存，不做拖拽。

## Vue 差异点

- 嵌套数组表单建议先用稳定数据模型，再绑定 UI。
- 表单结构必须对齐后端 `stages -> jobs -> extra`。

## Todo

- 实现模板名称、templateKey 表单。
- 加载编辑详情并回显。
- 加载 copyId 并生成 `_copy_timestamp` 名称和 key。
- 实现 stage 新增、删除、插入。
- 实现 job 新增、删除。
- 实现 create/update 保存。

## 验收

- 新增模板 payload 正确。
- 编辑模板 payload 正确。
- 复制新增不会覆盖原模板。
