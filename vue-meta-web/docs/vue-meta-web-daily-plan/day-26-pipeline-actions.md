# Day 26 - 流水线操作

## 今日目标

完成运行态流水线操作。

## Todo

- 实现 retry job。
- 实现 next stage。
- 实现 stop pipeline。
- 实现 history redeploy。
- 构建成功时支持复制镜像地址。
- KA 且 USE_OSS_ZIP 时支持复制 OSS zip 地址。
- 操作后调用工作区刷新。

## 验收

- 重试、继续、终止、再发一次均可触发接口。
- 操作后 active/history 状态刷新。
