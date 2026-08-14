# Day 29 - Monitor 模块

## 今日目标

实现监控模块：错误上报、stack 落盘、钉钉告警、详情 source map 映射、处理状态更新。

## 今天学习的 .NET 点

- 文件系统异步读写。
- 正则解析 stack trace。
- HTTP 下载 source map。
- 非核心副作用失败隔离。

## 实现 Todo

- [ ] 实现 Monitor DTO：create、query、update、detail。
- [ ] 实现 `/api/monitor/create`。
- [ ] 保存 stack 到配置目录下日期文件夹。
- [ ] 保存 Monitor 表记录。
- [ ] 发送钉钉 markdown 告警。
- [ ] 钉钉发送失败不影响监控记录保存。
- [ ] 实现 findAll、list、detail、update、remove。
- [ ] detail 读取 stack 文件。
- [ ] 根据 version 解析 source map 地址并映射原始位置。
- [ ] 测试 stack 为空、文件不存在、source map 下载失败、告警失败、resolveTime。

## 验收标准

- 错误上报能保存数据库和 stack 文件。
- 告警失败不会丢失监控记录。
- detail 能返回 detail 和 sourceStack。
- update 能记录 resolveTime。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
