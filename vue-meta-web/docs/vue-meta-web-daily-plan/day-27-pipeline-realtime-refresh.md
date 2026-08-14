# Day 27 - 流水线实时刷新

## 今日目标

实现 WebSocket 刷新和兜底轮询。

## Todo

- 根据 BASE_API 构造 `/api/pipeline/ws`。
- query 带 iterationId 和 token。
- 连接成功后发送 subscribe。
- 收到 `pipeline-changed` 后 800ms 防抖刷新。
- 断线 5 秒后重连。
- 每 60 秒兜底刷新，页面隐藏时不刷新。
- 组件卸载时清理 socket 和 timer。

## 验收

- 后端推送时流水线自动刷新。
- 断网恢复后能重连。
- 离开页面不再保留定时器。
