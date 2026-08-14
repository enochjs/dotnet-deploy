# Day 11 - DingTalk Client 与钉钉建需求

## 今日目标

实现 DingTalk Client 基础能力，并完成 `/api/demand/open/create-from-dingtalk`。

## 今天学习的 .NET 点

- Typed HttpClient 的请求/响应 DTO。
- JSON 反序列化和空值保护。
- 正则解析字符串。
- 外部数据幂等写入。

## 实现 Todo

- [ ] 实现 DingTalk options：appKey、appSecret、approveCode、机器人 webhook。
- [ ] 实现 getToken。
- [ ] 实现 getUserByMobile、getUser、getUserIdByUnionId。
- [ ] 实现钉钉姓名解析：`花名(实名)`、flower_name 优先。
- [ ] 实现 resolveDingtalkUserProfile。
- [ ] 实现机器人 sendMessage，支持 text 和 markdown。
- [ ] 实现 CreateRequirementFromDingtalk DTO。
- [ ] 实现钉钉建需求：补齐人员、解析 creator、匹配本地 User。
- [ ] 实现已有需求按 name 幂等更新。
- [ ] 实现 remark 拼接：禅道号、前端、后端、测试、备注。
- [ ] 测试 unionId、userid、姓名匹配、已有需求更新、人员匹配失败。

## 验收标准

- 钉钉建需求接口开放且不要求登录。
- 重复推送不会重复创建需求。
- 人员匹配失败有日志但不编造用户。
- 钉钉 Client 可 Mock。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
