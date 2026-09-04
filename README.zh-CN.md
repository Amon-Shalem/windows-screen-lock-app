# Windows 自定义信息防熄屏锁屏器 (Custom Screen Locker)

<div align="center">

<img src="app.ico" alt="Custom Screen Locker Icon" width="100"/>

# Windows 自定义信息防熄屏锁屏器 (Custom Screen Locker)

**自定义离开告示看板 • Windows 原生防熄屏 API • 固定密码安全防护 • 系统托盘常驻与快速模版**

<p align="center">
  <a href="README.md"><b>English</b></a> | 
  <a href="README.zh-TW.md"><b>繁體中文</b></a> | 
  <a href="README.zh-CN.md"><b>简体中文</b></a>
</p>

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-blue.svg)](https://microsoft.com/windows)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/arch-x64-brightgreen.svg)]()
[![Memory Usage](https://img.shields.io/badge/%E5%86%85%E5%AD%98%E5%8D%A0%E7%94%A8-~19%20MB-success.svg)]()
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

</div>

---

## 🌟 项目概述

**Windows 自定义信息防熄屏锁屏器** 是一款专为开发者、科研人员与职场办公打造的现代化全屏锁定与防休眠工具。

当您需要暂时离开工位，但电脑仍在执行大模型推理、数据处理、模型训练、视频渲染或大文件下载时，本工具能**直接调用 Windows 原生底层电源 API 阻止屏幕熄灭与系统休眠**；同时在所有显示器上覆盖高对比度、沉浸式的全屏锁屏界面，展示您的自定义离开留言、紧急联系方式与实时时钟，并通过底层键盘钩子拦截系统快捷键，全方位保障设备安全与后台任务不被意外中断。

---

## ✨ 核心特色

### 1. 🛡️ 原生防熄屏常亮 (Stay-Awake API)
* **内核级电源调用**：直接调用 Windows 底层 API `SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED)`。
* **非模拟鼠标**：绝不使用模拟鼠标抖动（Mouse Jiggler）等容易与前台工作冲突或触发安全审计报警的方式。
* **解锁自动恢复**：退出锁屏后自动恢复 Windows 系统原本的节能与熄屏策略。

### 2. 🔕 全屏置顶与系统快捷键拦截
* **全屏置顶防护**：无边框全屏最前端（Topmost）显示，副屏同步覆盖纯黑防窥遮罩。
* **底层键盘钩子 (`WH_KEYBOARD_LL`)**：精准拦截 `Win 键`、`Alt+Tab`、`Alt+F4`、`Alt+Esc` 与 `Ctrl+Esc`，防止路过人员随意切换窗口或关闭后台任务。

### 3. 🚀 系统托盘（System Tray）常驻与零闪烁架构
* **开机/启动最小化至托盘**：支持启动时直接静默常驻在右下角通知区域，不弹出主窗口打扰正常工作。
* **零闪烁（Zero-Flickering）机制**：重构启动流程，窗口在托盘创建全过程实现 0 毫秒视觉闪烁。
* **托盘右键快捷菜单**：
  * **🔒 立即锁定 (当前配置)**：以当前设定即刻全屏锁定。
  * **📋 快速锁定模版子菜单**：直接在托盘右键挑选模版一键进入锁屏，无需打开主设置窗口！
  * **🚀 开机自动启动**：点击切换开机自启动，实时连动注册表与气球提示。
  * **⚙️ 打开设置面板**：快速唤出控制台。
  * **🌐 界面语言**：右键菜单即时无缝切换多国语言。
  * **❌ 退出程序**：安全卸载键盘钩子并退出。

### 4. 📋 多组可自定义锁屏模版 (Custom Lock Presets)
* **自由创建与保存**：自定义告示标题、详细内容、联系方式与专属主题，点击“➕ 保存为模版”永久保存（例如：`☕ 外出开会`、`🍱 午餐暂离`、`⚡ 跑模型/程序中`、`🏃 运动健身`）。
* **托盘实时联动**：保存后立即更新到托盘右键菜单，不再使用的模版支持一键删除。

### 5. 🔑 固定密码安全防护 (Fixed Password Protection)
* **可选密码保护**：输入解锁密码防止未经许可操作；密码留空则为便捷的点击即解锁模式。
* **固定密码记忆**：勾选“固定此密码”，密码安全保存于本地，每次打开软件或托盘快速锁屏自动载入。
* **密码明文切换**：提供 `👁 显示 / 🔒 隐藏` 按钮，随时核验输入内容。

### 6. 🎨 6 大高对比沉浸式主题
* 🌲 **翠绿幽静森林 (Deep Forest)**：松柏墨绿背景搭配翡翠绿光晕与薄荷绿标签。
* 🌾 **春日田园麦浪 (Pastoral Meadow)**：青翠金绿背景搭配金黄麦浪微光与春日青墨卡片。
* 🌅 **暮色落日余晖 (Sunset Glow)**：晚霞酒红背景搭配琥珀金橙微光与珊瑚橙标签。
* 🌊 **浩瀚深海蔚蓝 (Deep Ocean)**：深邃大洋蓝背景搭配荧光天青水色与碧蓝标签。
* 🔮 **梦幻极光夜紫 (Aurora Purple)**：神秘深魅紫背景搭配璀璨霓虹与玫粉光晕。
* 🖤 **曜石极致酷黑 (Classic Black)**：纯黑曜石背景搭配科技冷蓝微光。
* *主题全面联动（背景渐变、环境光晕、告示牌发光边框、文字标签、按钮配色），并支持设置界面实时色条预览。*

### 7. ⚡ 极致轻量化架构 (~19 MB 物理内存占用)
* **主动工作集修剪 (Working Set Trimming)**：封装调用 Windows 核心 API `EmptyWorkingSet`，在启动、锁屏就绪与解锁缩小至托盘时主动释放 JIT 缓存与绘图缓存，内存降幅达 **87%**！
* **轻量运行时配置**：启用轻量化桌面 Workstation GC（`ServerGarbageCollection=false`）与分层编译。
* **CPU 占用趋近 0.0%**：锁屏时钟精确对齐整秒刷新，大幅降低系统唤醒频次。

### 8. 🌐 国际化多语言 (i18n) 与自定义语言包
* **默认英文开局**：初次启动一律默认呈现英文（English en-US）。
* **内置 4 国语言**：英文 (English)、繁体中文、简体中文、日语 (日本語)。
* **支持外置 JSON 语言包**：点击主界面“📂”按钮即可打开语言包目录，放入任意自定义 JSON 语言包即可自动识别载入（内置 `custom_template.json` 模版与 `fr-FR.json` 法语示例）。

---

## 📥 程序下载

您可以前往 [GitHub Releases 最新发布页](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest) 下载预编译好的执行程序：

| 版本类型 | 文件大小 | 系统需求 | 下载链接 |
| :--- | :--- | :--- | :--- |
| **Setup 完整版 / 独立免安装版** | 162 MB | Windows 10/11 (64-bit) | [⬇️ **下载 CustomScreenLocker_Setup.exe**](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest/download/CustomScreenLocker_Setup.exe) |
| **Portable 便携式极轻量版** | 351 KB | [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) | [⬇️ **下载 CustomScreenLocker_Portable.exe**](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest/download/CustomScreenLocker_Portable.exe) |

---

## 🖥️ 快速使用指南

1. **运行程序**：双击 `CustomScreenLocker_Setup.exe`（或 `CustomScreenLocker_Portable.exe`）。
2. **首次引导**：初次启动弹出语言选择弹窗（默认英文），点击 `Get Started / Continue` 进入。
3. **设置告示看板**：
   * 输入主要留言告示（如：`正在进行深度学习模型训练，请勿关闭！`）。
   * （选填）填写紧急联系人分机或手机号。
   * （选填）输入解锁密码并勾选“固定此密码”。
4. **选择主题**：通过颜色预览条选择喜爱的主题配色。
5. **进入锁屏**：点击 **`🔒 立即进入锁屏`**（或选择 3 秒 / 5 秒倒数）。
6. **解除锁定**：输入密码后按 `Enter` 键或点击 `解锁` 按钮即可恢复桌面。

---

## 🔒 隐私与持久化存储安全

* **100% 存储于用户本地**：所有配置、自定义模版与语言包均储存于用户本地目录 `%LocalAppData%\CustomScreenLocker\config.json`，绝不写入程序目录或上传网络。
* **零网络请求**：无任何遥测收集、无网络通信、无第三方广告与统计代码。
* **纯净开机自启**：写入 `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`，无需管理员权限，随用随关。

---

## ⌨️ 本地开发与构建

### 环境需求
* Windows 10 / 11 (x64)
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 构建命令
```bash
# 克隆仓库
git clone https://github.com/your-username/custom-screen-locker.git
cd custom-screen-locker

# 构建 Debug 版本
dotnet build

# 发布 Portable 便携单文件 (目标电脑需安装 .NET 9 Desktop Runtime)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o ./publish/portable

# 发布 Standalone / Setup 独立单文件 (自带 Runtime，免装依赖)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o ./publish/setup
```

---

## 📜 开源协议

本项目基于 [MIT License](LICENSE) 协议开源。
