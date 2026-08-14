# Day 37 - Docker、部署配置和健康检查

## 今日目标

完成容器化和部署配置，让服务可在测试环境部署。

## 今天学习的 .NET 点

- ASP.NET Core Dockerfile。
- multi-stage build。
- 环境变量注入。
- Health Checks。

## 实现 Todo

- [ ] 编写 Dockerfile。
- [ ] 编写 docker-compose 或测试环境部署说明。
- [ ] 配置 ASPNETCORE_ENVIRONMENT。
- [ ] 配置 Postgres、Redis、Git、DingTalk、OSS、InnerServer 环境变量。
- [ ] 添加健康检查：应用自身、数据库、Redis。
- [ ] 添加启动 migration 策略说明，不一定自动执行生产 migration。
- [ ] 构建镜像。
- [ ] 本地用容器启动服务。
- [ ] 容器内跑 `/health`。
- [ ] 记录部署步骤。

## 验收标准

- Docker 镜像可构建。
- 容器可启动。
- 健康检查能反映 DB/Redis 状态。
- 部署配置不包含明文密钥。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
