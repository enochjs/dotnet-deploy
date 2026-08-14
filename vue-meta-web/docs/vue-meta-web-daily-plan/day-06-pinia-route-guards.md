# Day 06 - Pinia 和路由守卫

## 今日目标

完成全局用户状态和登录态初始化。

## Vue 差异点

- Pinia action 类似 store method，不需要 reducer。
- 路由守卫替代 Umi 层面的访问控制。

## Todo

- 建 `useGlobalStore`。
- 实现 `userInfo`、`getUserInfo`、`clearUserInfo`。
- 在主布局挂载时获取当前用户。
- 在路由守卫里判断 token 缺失和登录页例外。
- 顶部展示用户名称，退出调用 auth logout。

## 验收

- 登录后刷新页面仍能获取用户信息。
- 无 token 访问业务页会进入登录流程。
- 顶部用户名称和退出按钮可用。
