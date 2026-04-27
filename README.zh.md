[English](./README.md) | 简体中文

<div align="center"><img width="200" src="https://github.com/user-attachments/assets/d002c198-7bb3-4a46-896f-f85ad8090500"/>
  <h1> 7DaysToDie-ServerKit </h1>
  <p>开源的 7 Days to Die 专用服务器的MOD。提供 RESTful API 和强大的网页管理面板，帮助各服主和管理员管理七日杀专用服务器。同时支持 Linux 和 Windows (Ubuntu 22.04 和 Windows Server 2016+ 测试通过)。支持多国语言: English, Deutsch, Español, Français, Italiano, 日本語, 한국어, Polski, Português (Brasil), Русский, Türkçe, 简体中文, 繁體中文
  </p>
</div>

[![stars](https://img.shields.io/github/stars/IceCoffee1024/7DaysToDie-ServerKit?style=flat-square&logo=GitHub)](https://github.com/IceCoffee1024/7DaysToDie-ServerKit)
[![license](https://img.shields.io/github/license/IceCoffee1024/7DaysToDie-ServerKit?style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)
[![Crowdin](https://badges.crowdin.net/7daystodie-serverkit/localized.svg)](https://crowdin.com/project/7daystodie-serverkit)

[前端代码仓库](https://github.com/IceCoffee1024/7DaysToDie-ServerKit-webui)

## 🌐 兼容性
仅限专用服务端。所需游戏版本：V1.0+

## 🎉 特征
- 支持在线地图
- 支持查看和修改serverconfig.xml
- 您可以通过web面板打开和关闭服务器，并自动重新启动服务器
- 使用web控制台，您可以查看后台信息并实时执行命令
- 支持查看所有玩家的在线和离线信息，包括但不限于背包、腰带、装备或技能的所有细节
- 支持黑名单，封禁解封玩家。
- 支持传送功能，支持TP和sethome等自定义传送命令。
- 具有存储和积分系统
- 提供许多API供大家使用
- 支持自动备份归档
- 支持服务器物品分发和玩家物品实时移除
- 还有更多功能供您探索。

# 📌 入门

## **视频教程**
https://www.bilibili.com/video/BV1pw4m1k7K3

https://docs.qq.com/doc/DYlJlVmpaWmZKUHFt

## **安装指南**

### 1. **下载最新版本**
- 从 [此处](https://github.com/IceCoffee1024/7DaysToDie-ServerKit/releases) 获取最新版本。

### 2. **将版本放入您的 Mods 文件夹中**
- 将下载的 .zip 存档文件解压到您的 `7 Days to Die Dedicated Server/Mods` 文件夹中。
- 以下是所需的服务器文件：
- 0_TFP_Harmony
- TFP_CommandExtensions
- TFP_MapRendering
- TFP_WebServer

### 3. **启动服务器**
- 通常运行 startdedicated.bat
- 等待服务器进程完全启动。
- 打开浏览器访问 http://IP:8888
- 默认用户名：admin
- 默认密码：123456

## 🚀 配置
- 要配置您的登录用户名和密码，请编辑位于`7 Days to Die Dedicated Server/LSTY_Data`中的`appsettings.json`文件。
- 重新启动服务器以使其生效。

## 🍻 命令
您可以使用 help 命令查看所有可用命令。

| 命令 | 说明 |
| --- | --- |
| ty-GiveItem | 将物品直接放入玩家的库存中。如果库存已满，则掉落到地面。|
| ty-GlobalMessage | 向所有连接的客户端发送消息。|
| ty-SayToPlayer | 向单个玩家发送消息。|
| ty-RemovePlayerItems | 从玩家的库存中移除玩家的物品。|
| ty-RemovePlayerLandClaims | 移除玩家的土地主张。|
| ty-ResetPlayerProfile | 重置玩家的个人资料。|
| ty-RestartServer | 重启服务器，可选参数 -f/force。|

## 📦️ API 文档
[<img src="https://cdn.apifox.com/logo/apifox-logo-64.png" alt="Run In Apifox" style="width: 24px; height: 24px;"> Run In Apifox](https://apifox.com/apidoc/shared-085f4112-2ab3-4273-ae37-fee9bdef44d6)

## 🌱 更新日志
每个版本的详细变更都记录在 [发行说明](./CHANGELOG.md) 中

## ⚡️ 链接
[官方] [七日杀官方论坛](https://community.7daystodie.com/topic/37613-tianyiserverkit-a-server-panel-management-tool-for-v10)
[官方] [Nexus Mods](https://www.nexusmods.com/7daystodie/mods/5924)
[非官方] [7daystodiemods.com](https://7daystodiemods.com/serverkit)

## 👷 支持
欢迎加入服主交流QQ群：[470804744](https://qm.qq.com/cgi-bin/qm/qr?k=p3TKGDnBAxxyVsR79pF-WYHI3BjsYiHe&jump_from=webapi&authKey=wTpnGpOGOsAaNTD4TqL4kukLQnxT+TmDFQx803v+Q2zWU0E7LYuSkBQQI+WhrqFB)

## 🙈 贡献
我们欢迎并感谢贡献。贡献代码或翻译内容请参阅 [此处](./CONTRIBUTING.zh.md)

## 💚 捐赠
您的支持是对我的最大鼓励！如果您觉得这个项目有用，您可以给作者买杯咖啡 :coffee:

[!["Buy Me a Coffee at ko-fi.com"](https://storage.ko-fi.com/cdn/kofi1.png?v=3)](https://ko-fi.com/L3L012RJ8R)

![image](https://github.com/user-attachments/assets/615fb619-5f40-42da-86ad-e60de11cdef2)

## 🎨 截图
![image](https://github.com/user-attachments/assets/581cd03d-e271-4011-b547-b82ad16f64a3)
<table>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/fa9e18a5-65d1-46a1-bd3f-3d136bf4411c">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/748b33cb-bfbc-4585-848f-0ac07a192121">
    </td>
  </tr>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/84661343-8a20-414b-90be-a27705555259">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/92c87d6d-8406-415b-9d13-a07a18ddb087">
    </td>
  </tr>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/c068c9bf-0ebb-4b3d-a3c9-e01c957cfca4">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/dfdb1dfb-f801-463b-a8d1-dc64ab88736e">
    </td>
  </tr>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/52a5d65a-0c9e-4812-baad-f6fd0c84ef95">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/4d8317ab-388b-4191-9d31-df778d93f7a6">
    </td>
  </tr>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/193760ae-9b66-4c81-82e9-a31860130f4d">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/5b3d551b-8d77-4c0d-bd93-83b74316fff6">
    </td>
  </tr>
  <tr>
    <td>
      <img src="https://github.com/user-attachments/assets/3c5e6605-3c16-45ed-a0ee-a5327e7a3056">
    </td>
    <td>
      <img src="https://github.com/user-attachments/assets/0fb8610f-8a6e-4005-b2ed-5043aa066b99">
    </td>
  </tr>
</table>

## 📄 免责声明
本软件所使用的源代码公开透明可见，且本软件仅供学习研究使用，不参与任何盈利组织，因使用本软件引起的或与本软件有关的任何争议，各方应友好协商解决。
对本项目代码的任何私人修改均由进行这些修改的人独自负责，本软件著作团队对使用本软件过程中可能对用户自己或他人造成的任何形式的损失和伤害不承担任何责任。
如用户下载、安装和使用本软件，即表明用户信任本软件著作团队且同意相关协议和免责声明。

## 历史版本
天依七日杀服主工具系列从2017年着手开发，至今已经迭代了9个大版本
<table>
  <tr>
    <td style="width: 50%">
      <div>V1.0 采用易语言开发</div>
      <img src="https://github.com/user-attachments/assets/ba407055-0eee-4140-8371-a2e77a27f924">
    </td>
    <td style="width: 50%">
      <div>V2.0 采用 C++ 和 Qt 开发</div>
      <img src="https://github.com/user-attachments/assets/93d0204e-4dab-4c3c-9f59-8646b0056042">
    </td>
  </tr>
  <tr>
    <td style="width: 50%">
      <div>V3.0 采用 C++ 和 Qt 开发</div>
      <img src="https://github.com/user-attachments/assets/dfd2f607-2f65-4ff5-9d29-f7a433ea1469">
    </td>
    <td style="width: 50%">
      <div>V4.0 采用 C# 和 WPF 开发 <a href="https://github.com/IceCoffee1024/TianYiSdtdServerTools">代码仓库</a></div>
      <img src="https://github.com/user-attachments/assets/7a33428c-dad5-4c0c-9867-506ab626225a">
    </td>
  </tr>
  <tr>
    <td style="width: 50%">
      <div>V5.0 开始采用 Web 全栈架构，前端基于Vue</div>
      <img src="https://github.com/user-attachments/assets/63eec623-901a-488c-a108-4629a0c804ad">
    </td>
    <td style="width: 50%">
      <div>V6.0</div>
      <img src="https://github.com/user-attachments/assets/53b97c9d-596d-49d4-87db-1c182fa41810">
    </td>
  </tr>
  <tr>
    <td style="width: 50%">
      <div>V7.0</div>
      <img src="https://github.com/user-attachments/assets/f320be74-0390-495e-b998-6cd344a1534a">
    </td>
    <td style="width: 50%">
      <div>V8.0</div>
      <img src="https://github.com/user-attachments/assets/a2e38ae6-7840-4a46-ad8c-705a396a0a04">
    </td>
  </tr>
  <tr>
    <td style="width: 50%">
      <div>V9.0</div>
      <img src="https://github.com/user-attachments/assets/05a7728a-62f3-4a0f-919b-b1e118f1059b">
    </td>
    <td style="width: 50%">
      <div>V10.0</div>
      <img src="https://github.com/user-attachments/assets/581cd03d-e271-4011-b547-b82ad16f64a3">
    </td>
  </tr>
</table>
