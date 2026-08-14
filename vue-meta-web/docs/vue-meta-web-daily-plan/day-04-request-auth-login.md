# Day 04 - 请求层和登录

## 今日目标

完成请求封装和登录闭环。

## Vue 差异点

- 请求层和 React 无关，可直接迁移思想。
- 登录状态通过 Pinia 和路由守卫配合。

## Todo

- 封装 axios 实例：`baseURL`、timeout、headers、错误处理。
- 每个请求带 `Authorization`、`timezone: Asia/Shanghai`、`useOriginData: true`。
- 实现 token storage：读、写、清理。
- 迁移 OIDC 登录、登录回调、退出逻辑。
- 实现 401 或登录失效弹窗，确认后重新登录。

## 验收

- 登录回调能保存 token。
- 退出能清 token 并跳转。
- 手动模拟 401 时能弹窗并重新登录。
