# Windows 自訂訊息防熄屏鎖屏器 (Custom Screen Locker)

<div align="center">

<img src="app.ico" alt="Custom Screen Locker Icon" width="100"/>

# Windows 自訂訊息防熄屏鎖屏器 (Custom Screen Locker)

**自訂離開告示看板 • Windows 原生防熄屏 API • 固定密碼安全防護 • 系統托盤常駐與快速模板**

<p align="center">
  <a href="README.md"><b>English</b></a> | 
  <a href="README.zh-TW.md"><b>繁體中文</b></a> | 
  <a href="README.zh-CN.md"><b>简体中文</b></a>
</p>

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-blue.svg)](https://microsoft.com/windows)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/arch-x64-brightgreen.svg)]()
[![Memory Usage](https://img.shields.io/badge/%E8%A8%98%E6%86%B6%E9%AB%94%E4%BD%94%E7%94%A8-~19%20MB-success.svg)]()
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

</div>

---

## 🌟 專案概述

**Windows 自訂訊息防熄屏鎖屏器** 是一款專為開發人員、辦公室職員與實驗室研究人員打造的現代化全螢幕鎖定與防休眠工具。

當您需要暫時離開座位，但電腦仍在進行高負載運算、AI 模型訓練、大型檔案渲染或長時間下載時，本工具能**透過 Windows 原生底層電源 API 阻止螢幕熄滅與系統睡眠**；同時在所有顯示器上覆蓋高對比度且具沉浸感的全螢幕鎖屏畫面，展示您的自訂離開告示、緊急聯絡資訊與實時時鐘，並透過底層全域鍵盤鉤子攔截快捷鍵，保障電腦資安與工作不被中斷。

---

## ✨ 核心特色

### 1. 🛡️ 原生防熄屏常亮 (Stay-Awake API)
* **核心級電源調用**：直接調用 Windows 底層 API `SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED)`。
* **非滑鼠模擬**：絕不採用模擬滑鼠移動（Mouse Jiggler）等容易與工作焦點衝突或觸發資安稽核警報的手段。
* **解鎖自動還原**：解鎖退出後自動恢復 Windows 系統原本的省電休眠計畫，安全省電。

### 2. 🔕 全螢幕置頂與全域快捷鍵攔截
* **全螢幕置頂防護**：無邊框全螢幕最上層（Topmost）顯示，副螢幕同步覆蓋純黑防窺遮罩。
* **底層鍵盤鉤子 (`WH_KEYBOARD_LL`)**：精準攔截 `Win 鍵`、`Alt+Tab`、`Alt+F4`、`Alt+Esc` 與 `Ctrl+Esc`，防止路過人員隨意切換程式或中斷後台運作中的程序。

### 3. 🚀 系統托盤（System Tray）常駐與零閃爍架構
* **開機/啟動最小化至托盤**：支援啟動時直接靜默常駐於右下角通知區域，不彈出視窗干擾桌面工作。
* **零閃爍（Zero-Flickering）設計**：徹底重構 WPF 啟動生命週期，視窗建立至托盤過程 0 毫秒視覺閃爍。
* **托盤右鍵快速選單**：
  * **🔒 立即鎖定 (當前配置)**：以目前設定瞬間鎖定螢幕。
  * **📋 快速鎖定模板子選單**：直接在托盤右鍵選取模板一鍵進入鎖定，完全無需開啟主視窗！
  * **🚀 開機自動啟動**：點擊切換開機自啟動，實時連動註冊表與氣球提示。
  * **⚙️ 開啟設定面板**：快速叫出主控制台。
  * **🌐 介面語言**：右鍵動態無縫切換多國語言。
  * **❌ 結束程式**：安全釋放鍵盤鉤子並優雅退出。

### 4. 📋 多組可自定義鎖定模板 (Custom Lock Presets)
* **自由建立與儲存**：可自訂公告標題、詳細說明、聯絡方式與專屬主題色彩，點擊「➕ 儲存為模板」永久保存（如：`☕ 外出開會`、`🍱 午餐暫離`、`⚡ 跑模型/程式中`、`🏃 運動健身`）。
* **即時雙向連動**：儲存後立即更新至托盤右鍵選單，不再使用的模板亦可一鍵刪除。

### 5. 🔑 固定密碼安全防護 (Fixed Password Protection)
* **可選密碼保護**：設定解鎖密碼防止未授權操作；密碼留空則為直覺的點擊即解鎖模式。
* **固定密碼記憶**：勾選「固定此密碼」，密碼自動安全保存於本機，每次打開程式或從托盤快速鎖定時自動載入套用。
* **密碼明文切換**：提供 `👁 顯示 / 🔒 隱藏` 按鈕，隨時確認輸入內容。

### 6. 🎨 6 大高對比沉浸感主題
* 🌲 **翠綠幽靜森林 (Deep Forest)**：松柏墨綠背景搭配翡翠綠光暈與薄荷綠標籤。
* 🌾 **春日田園麥浪 (Pastoral Meadow)**：青翠金綠背景搭配金黃麥浪微光與春日青墨卡片。
* 🌅 **暮色落日餘暉 (Sunset Glow)**：晚霞酒紅背景搭配琥珀金橙微光與珊瑚橙標籤。
* 🌊 **浩瀚深海蔚藍 (Deep Ocean)**：深邃大洋藍背景搭配螢光天青水色與碧藍標籤。
* 🔮 **夢幻極光夜紫 (Aurora Purple)**：神秘深魅紫背景搭配璀璨霓虹與玫粉光暈。
* 🖤 **曜石極致酷黑 (Classic Black)**：純黑曜石背景搭配科技冷藍邊框微光。
* *所有主題全套連動（背景漸層、環境光暈、告示牌發光邊框、文字標籤、按鈕配色），並支援設定介面即時色條預覽。*

### 7. ⚡ 極致輕量化架構 (~19 MB 實體記憶體佔用)
* **主動工作集修剪 (Working Set Trimming)**：封裝調用 Windows 核心 API `EmptyWorkingSet`，在啟動、鎖定就緒與解鎖縮小時主動釋放 JIT 與繪圖快取，記憶體佔用降幅達 **87%**！
* **輕量執行期配置**：啟用輕量化桌面 Workstation GC（`ServerGarbageCollection=false`）與分層編譯。
* **CPU 佔用趨近 0.0%**：鎖屏時鐘精準對齊整秒更新，大幅降低喚醒頻率。

### 8. 🌐 國際化多語言 (i18n) 與自訂語言包
* **預設英文開局**：初次開啟一律預設為英文（English en-US）。
* **內建 4 國語言**：英文 (English)、繁體中文、簡體中文、日本語。
* **支援外置 JSON 語言檔**：點擊主介面「📂」按鈕即可打開語言包目錄，放入任意自訂 JSON 語言檔即可即時識別（內建 `custom_template.json` 範本與 `fr-FR.json` 法語範例）。

---

## 📥 執行檔下載

| 版本類型 | 檔案大小 | 系統需求 | 推薦使用情境 |
| :--- | :--- | :--- | :--- |
| **[Setup 完整版 / 獨立免安裝版](publish/setup/CustomScreenLocker_Setup.exe)** | ~170 MB | Windows 10/11 (64-bit) | **推薦大多數使用者。** 內建完整 .NET 9 執行階段，開箱即用，無需安裝任何額外環境依賴。 |
| **[Portable 可攜式極致輕量版](publish/portable/CustomScreenLocker_Portable.exe)** | ~359 KB | [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) | **極致輕巧。** 適合本機已安裝 .NET 9 或追求極致輕量與極低磁碟佔用的使用者。 |

---

## 🖥️ 快速操作指引

1. **執行程式**：雙擊 `CustomScreenLocker_Setup.exe`（或 `CustomScreenLocker_Portable.exe`）。
2. **首次引導**：首次打開會彈出語言設定視窗（預設英文），點擊 `Get Started / Continue` 即可。
3. **設定告示看板**：
   * 輸入主要公告文字（如：`正在進行深度學習模型訓練，請勿中斷！`）。
   * （選填）填寫緊急聯絡人分機或電話。
   * （選填）輸入解鎖密碼並勾選「固定此密碼」。
4. **挑選主題**：透過色彩預覽條挑選您喜愛的主題配色。
5. **進入鎖屏**：點擊 **`🔒 立即進入鎖屏`**（或選擇 3 秒 / 5 秒倒數鎖定）。
6. **解除鎖定**：輸入密碼後按 `Enter` 鍵或點擊 `解鎖` 按鈕即可復原桌面。

---

## 🔒 隱私與持久化安全

* **100% 儲存於使用者本機**：所有設定、自定義模板與個人語言包均儲存於使用者的本機目錄 `%LocalAppData%\CustomScreenLocker\config.json`，絕不寫入程式目錄或上傳外部。
* **零網路請求**：無任何遙測數據、無聯網通訊、無第三方廣告與追蹤代碼。
* **純淨開機註冊表**：開機自啟採用 `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`，免管理員權限，可隨時乾淨關閉。

---

## ⌨️ 本地開發與建置

### 環境需求
* Windows 10 / 11 (x64)
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 建置指令
```bash
# 複製專案庫
git clone https://github.com/your-username/custom-screen-locker.git
cd custom-screen-locker

# 建置 Debug 版本
dotnet build

# 發佈 Portable 可攜式單檔 (目標電腦需有 .NET 9 Desktop Runtime)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o ./publish/portable

# 發佈 Standalone / Setup 獨立單檔 (自帶 Runtime，免裝依賴)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o ./publish/setup
```

---

## 📜 開源授權

本專案採用 [MIT License](LICENSE) 授權開源。
