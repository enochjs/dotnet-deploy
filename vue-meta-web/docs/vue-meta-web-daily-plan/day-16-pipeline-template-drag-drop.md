# Day 16 - 模板编辑器拖拽

## 今日目标

实现 job 拖入 stage 的交互。

## Todo

- 选择 Vue 拖拽方案，优先用轻量且稳定的库。
- 左侧展示全部 `PIPELINE_JOB_LIST`。
- stage 区域支持 drop。
- 拖入后向对应 stage.jobs 追加 job。
- 支持删除 stage 和 job。
- 验证拖拽后 form model 不丢字段。

## 验收

- 每种 job 都能拖入 stage。
- 拖入后保存 payload 与 Day 15 一致。
