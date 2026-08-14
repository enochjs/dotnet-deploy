# Day 31 - Bugfix 合入

## 今日目标

完成需求详情中的 bugfix 合入能力。

## Todo

- 生成 `bug_fix_YYYY-MM-DD` 版本号。
- 合入前检查已选择迭代。
- 弹出确认框展示版本、泳道、迭代数量。
- 调用 bugfix-from-requirement 接口。
- 成功后刷新流水线。
- 触发 300ms、1000ms、2000ms 快速补刷。
- 失败时展示失败明细。

## 验收

- 选中多个迭代可触发 bugfix 合入。
- 成功和部分失败提示都清晰。
