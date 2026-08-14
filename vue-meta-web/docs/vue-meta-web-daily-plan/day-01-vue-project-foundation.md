# Day 01 - Vue 工程定型

## 今日目标

从空目录建立可运行的 Vue 3 工程，并冻结迁移约定。

## Vue 差异点

- 用 `<script setup lang="ts">` 作为默认组件写法。
- 用 `composables` 承接 React custom hooks 的职责。
- 用 Pinia 承接全局 store。

## Todo

- 初始化 Vite + Vue 3 + TypeScript 项目。
- 安装 Ant Design Vue、Vue Router、Pinia、axios、dayjs、oidc-client-ts。
- 配置 `@` alias、环境变量、基础样式入口、中文 locale。
- 建目录：`api`、`stores`、`views`、`components`、`composables`、`constants`、`types`、`utils`。
- 写一份迁移约定：组件命名、API 文件命名、store 命名、composable 命名、表单写法。

## 验收

- 本地 dev server 能启动。
- 首页能显示一个占位布局。
- 目录结构和迁移约定已经确定。
