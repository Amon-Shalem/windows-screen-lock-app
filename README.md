# Windows Custom Screen Locker & Stay-Awake

<div align="center">

<img src="app.ico" alt="Custom Screen Locker Icon" width="100"/>

# Windows Custom Screen Locker & Stay-Awake

**Display Custom Away Notices • Native Stay-Awake API • Fixed Password Security • Silent System Tray with Quick Presets**

<p align="center">
  <a href="README.md"><b>English</b></a> | 
  <a href="README.zh-TW.md"><b>繁體中文</b></a> | 
  <a href="README.zh-CN.md"><b>简体中文</b></a>
</p>

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-blue.svg)](https://microsoft.com/windows)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/arch-x64-brightgreen.svg)]()
[![Memory Usage](https://img.shields.io/badge/RAM%20Usage-~19%20MB-success.svg)]()
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

</div>

---

## 🌟 Overview

**Windows Custom Screen Locker & Stay-Awake** is a modern, ultra-lightweight Windows utility designed for developers, office professionals, and lab researchers. 

When you need to step away from your desk while keeping long-running simulations, model training, or data downloads running, this tool prevents Windows from sleeping or turning off the screen. At the same time, it covers all monitors with an aesthetic lock screen displaying your customized away message, contact info, and clock, while securing your desktop with low-level shortcut interception and optional password protection.

---

## ✨ Key Features

### 1. 🛡️ Native Stay-Awake (No Sleep, No Screen Off)
* **Kernel-Level Power Management**: Calls Windows Core API `SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED)`.
* **Zero Mouse Jiggling**: Does NOT simulate synthetic mouse/keyboard events that interfere with your workflows or trigger corporate audit alarms.
* **Auto-Restoration**: Automatically restores Windows standard power-saving schemes upon unlock.

### 2. 🔕 Comprehensive Desktop Lock & Shortcut Interception
* **Full-Screen Topmost Overlay**: Borderless topmost display across all active monitors. Secondary screens are blanked out with a clean blackout shield.
* **Low-Level Keyboard Hook (`WH_KEYBOARD_LL`)**: Intercepts `Win Key`, `Alt+Tab`, `Alt+F4`, `Alt+Esc`, and `Ctrl+Esc`, preventing passersby from switching windows or interrupting background processes.

### 3. 🚀 Silent System Tray & Instant Presets
* **Start Minimized to Tray**: Option to silently boot into the system notification area without opening any windows.
* **Zero-Flickering Architecture**: Pure background initialization with 0ms visual flash on startup.
* **Tray Right-Click Menu**:
  * **🔒 Lock Now**: Immediate lock with current settings.
  * **📋 Quick Presets Submenu**: One-click lock with custom presets directly from the tray—no need to open the main window!
  * **🚀 Launch on Startup**: Toggle Windows auto-start on boot with real-time registry sync.
  * **⚙️ Open Settings**: Quick access to the control center.
  * **🌐 Language Selector**: Switch interface language dynamically on the fly.
  * **❌ Exit Application**: Safely unhooks keyboard hooks and releases resources.

### 4. 📋 Customizable Lock Presets
* **Create & Save Presets**: Customize message, contact note, and color theme, then save as a named preset (e.g., `☕ In Meeting`, `🍱 Lunch Break`, `⚡ Model Training`, `🏃 Gym/Workout`).
* **Manage Presets**: Switch between presets instantly or delete unused ones with a single click. Presets are immediately updated in the tray menu.

### 5. 🔑 Fixed Password Protection
* **Optional Lock Password**: Set a password to prevent unauthorized unlock. Leave blank for click-to-unlock mode.
* **Fixed Password Storage**: Check "Fix this password" to save locally; it auto-applies on every launch and preset lock.
* **Password Visibility**: Toggle `👁 Show / 🔒 Hide` at any time to verify password accuracy.

### 6. 🎨 6 High-Contrast Immersive Themes
* 🌲 **Emerald Forest**: Deep pine green backdrop with soft mint emerald glow.
* 🌾 **Pastoral Meadow**: Golden meadow greenery with spring wheat radiance.
* 🌅 **Sunset Glow**: Warm twilight burgundy with amber orange highlights.
* 🌊 **Deep Ocean Blue**: Abyss deep-sea navy with vibrant cyan luminescence.
* 🔮 **Aurora Purple**: Mystical nighttime violet with radiant magenta accents.
* 🖤 **Obsidian Black**: Minimalist obsidian darkness with tech electric blue halo.
* *All themes include full UI synchronization (background gradients, glow aura, badge borders, and button palettes).*

### 7. ⚡ Ultra-Lightweight (~19 MB Working Set)
* **Active Working Set Trimming**: Integrates `EmptyWorkingSet` Windows API to reclaim JIT compilation and render caches upon startup, lock activation, and minimize.
* **Lightweight Runtime**: Utilizes Workstation GC (`ServerGarbageCollection=false`) and tiered compilation.
* **Near 0.0% CPU**: 1-second synchronized clock dispatching ensures virtually zero CPU consumption.

### 8. 🌐 Internationalization (i18n) & Custom Locales
* **Default English**: Starts in English by default across all packages.
* **Built-in Languages**: English (US), Traditional Chinese (繁體中文), Simplified Chinese (简体中文), Japanese (日本語).
* **Extensible JSON Locales**: Click the `📂` button to open the user locales directory and drop in any custom language JSON file (includes `custom_template.json` and `fr-FR.json` sample).

---

## 📥 Downloads

Grab the latest compiled binaries from the [GitHub Releases](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest) page:

| Edition | Size | Requirements | Download Link |
| :--- | :--- | :--- | :--- |
| **Setup / Standalone Edition** | ~170 MB | Windows 10/11 (64-bit) | [⬇️ **Download CustomScreenLocker_Setup.exe**](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest/download/CustomScreenLocker_Setup.exe) |
| **Portable Lightweight Edition** | ~359 KB | [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) | [⬇️ **Download CustomScreenLocker_Portable.exe**](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest/download/CustomScreenLocker_Portable.exe) |
| **Portable Bundle (.zip with locales)** | ~365 KB | [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) | [📦 **Download CustomScreenLocker_v1.0.0_Portable.zip**](https://github.com/Amon-Shalem/windows-screen-lock-app/releases/latest/download/CustomScreenLocker_v1.0.0_Portable.zip) |

---

## 🖥️ Usage Guide

1. **Launch**: Run `CustomScreenLocker_Setup.exe` (or `CustomScreenLocker_Portable.exe`).
2. **Initial Setup**: Select your language (defaults to English) and click `Get Started / Continue`.
3. **Configure Away Note**:
   * Type your headline message (e.g., `Running machine learning tasks, please do not disturb`).
   * (Optional) Enter emergency contact information or phone number.
   * (Optional) Enter an unlock password and check "Fix this password".
4. **Choose a Theme**: Select your preferred color palette with the real-time preview strip.
5. **Lock**: Click **`🔒 Lock Screen Now`** (or select a delay of 3s / 5s).
6. **Unlock**: Type your password and press `Enter` (or click `Unlock`).

---

## 🔒 Privacy & Persistence

* **100% Local Storage**: All user settings, custom presets, and language files are strictly stored in `%LocalAppData%\CustomScreenLocker\config.json`.
* **Zero Telemetry**: No network requests, no analytics, no external API calls.
* **Clean Uninstallation**: Auto-start registry key lives under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, which can be cleanly toggled off anytime without admin rights.

---

## ⌨️ Development & Build

### Prerequisites
* Windows 10 / 11 (x64)
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build Commands
```bash
# Clone the repository
git clone https://github.com/your-username/custom-screen-locker.git
cd custom-screen-locker

# Build debug binary
dotnet build

# Publish Portable Single-File (Requires .NET 9 Desktop Runtime on target PC)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o ./publish/portable

# Publish Standalone Setup Single-File (Self-contained, no runtime needed)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o ./publish/setup
```

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).
