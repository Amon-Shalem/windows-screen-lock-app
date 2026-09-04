using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace CustomScreenLocker
{
    public static class I18nManager
    {
        public class LanguageInfo
        {
            public string Code { get; set; } = "en-US";
            public string DisplayName { get; set; } = "English (US)";
            public string CultureCode { get; set; } = "en-US";
            public bool IsCustom { get; set; } = false;
        }

        public static string CurrentLanguageCode { get; private set; } = "en-US";
        public static CultureInfo CurrentCulture { get; private set; } = new CultureInfo("en-US");

        public static event Action? LanguageChanged;

        private static readonly Dictionary<string, LanguageInfo> _availableLanguages = new();
        private static readonly Dictionary<string, string> _strings = new();

        public static string LocalesDirectory { get; private set; } = string.Empty;
        public static string AppBaseLocalesDirectory { get; private set; } = string.Empty;

        static I18nManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            LocalesDirectory = Path.Combine(appData, "CustomScreenLocker", "locales");
            Directory.CreateDirectory(LocalesDirectory);

            AppBaseLocalesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locales");
            try { Directory.CreateDirectory(AppBaseLocalesDirectory); } catch { }

            InitDefaultLanguages();
            CreateCustomLocaleSample();
            ScanCustomLanguages();
        }

        public static IEnumerable<LanguageInfo> GetAvailableLanguages() => _availableLanguages.Values;

        private static void InitDefaultLanguages()
        {
            _availableLanguages["zh-TW"] = new LanguageInfo { Code = "zh-TW", DisplayName = "繁體中文 (Traditional Chinese)", CultureCode = "zh-TW" };
            _availableLanguages["zh-CN"] = new LanguageInfo { Code = "zh-CN", DisplayName = "简体中文 (Simplified Chinese)", CultureCode = "zh-CN" };
            _availableLanguages["en-US"] = new LanguageInfo { Code = "en-US", DisplayName = "English (US)", CultureCode = "en-US" };
            _availableLanguages["ja-JP"] = new LanguageInfo { Code = "ja-JP", DisplayName = "日本語 (Japanese)", CultureCode = "ja-JP" };
        }

        private static void CreateCustomLocaleSample()
        {
            try
            {
                string sampleFile = Path.Combine(LocalesDirectory, "custom_template.json");
                if (!File.Exists(sampleFile))
                {
                    var sample = new Dictionary<string, object>
                    {
                        ["_language_code"] = "custom-example",
                        ["_language_name"] = "自定義語言範本 (Custom Template)",
                        ["_culture_code"] = "en-US",
                        ["AppTitle"] = "Custom Screen Locker",
                        ["Title"] = "Screen Locker",
                        ["Subtitle"] = "My custom subtitle here...",
                        ["BtnStartLock"] = "🔒 Lock Screen",
                        ["NoticeTag"] = "📢 Notice",
                        ["UnlockPromptPassword"] = "Please enter your password",
                        ["BtnUnlock"] = "Unlock"
                    };
                    File.WriteAllText(sampleFile, JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch { }
        }

        public static void ScanCustomLanguages()
        {
            var dirs = new List<string> { LocalesDirectory };
            if (!string.IsNullOrEmpty(AppBaseLocalesDirectory) && Directory.Exists(AppBaseLocalesDirectory) && AppBaseLocalesDirectory != LocalesDirectory)
            {
                dirs.Add(AppBaseLocalesDirectory);
            }

            foreach (var dir in dirs)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;

                    foreach (var file in Directory.GetFiles(dir, "*.json"))
                    {
                        if (Path.GetFileName(file).Equals("custom_template.json", StringComparison.OrdinalIgnoreCase)) continue;

                        try
                        {
                            string json = File.ReadAllText(file);
                            using var doc = JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            string code = root.TryGetProperty("_language_code", out var c) ? c.GetString() ?? Path.GetFileNameWithoutExtension(file) : Path.GetFileNameWithoutExtension(file);
                            string name = root.TryGetProperty("_language_name", out var n) ? n.GetString() ?? code : code;
                            string culture = root.TryGetProperty("_culture_code", out var cu) ? cu.GetString() ?? "en-US" : "en-US";

                            _availableLanguages[code] = new LanguageInfo
                            {
                                Code = code,
                                DisplayName = $"✨ {name} (自訂檔)",
                                CultureCode = culture,
                                IsCustom = true
                            };
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        public static void SetLanguage(string languageCode)
        {
            ScanCustomLanguages();

            if (!_availableLanguages.ContainsKey(languageCode))
            {
                languageCode = "en-US";
            }

            CurrentLanguageCode = languageCode;
            var info = _availableLanguages[languageCode];
            try
            {
                CurrentCulture = new CultureInfo(info.CultureCode);
            }
            catch
            {
                CurrentCulture = new CultureInfo("en-US");
            }

            _strings.Clear();

            // 1. 載入內建文字字典
            LoadBuiltinStrings(languageCode);

            // 2. 若為自定義檔案，覆蓋外部 JSON 中的詞條
            if (info.IsCustom)
            {
                LoadCustomStrings(languageCode);
            }

            LanguageChanged?.Invoke();
        }

        private static void LoadCustomStrings(string code)
        {
            var dirs = new List<string> { LocalesDirectory };
            if (!string.IsNullOrEmpty(AppBaseLocalesDirectory) && Directory.Exists(AppBaseLocalesDirectory) && AppBaseLocalesDirectory != LocalesDirectory)
            {
                dirs.Add(AppBaseLocalesDirectory);
            }

            foreach (var dir in dirs)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (var file in Directory.GetFiles(dir, "*.json"))
                    {
                        string json = File.ReadAllText(file);
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        string c = root.TryGetProperty("_language_code", out var cp) ? cp.GetString() ?? "" : "";
                        if (c == code || Path.GetFileNameWithoutExtension(file) == code)
                        {
                            foreach (var prop in root.EnumerateObject())
                            {
                                if (!prop.Name.StartsWith("_"))
                                {
                                    _strings[prop.Name] = prop.Value.GetString() ?? "";
                                }
                            }
                            return;
                        }
                    }
                }
                catch { }
            }
        }

        public static string T(string key, string fallback = "")
        {
            if (_strings.TryGetValue(key, out var val))
            {
                return val;
            }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        private static void LoadBuiltinStrings(string code)
        {
            switch (code)
            {
                case "zh-CN":
                    LoadSimplifiedChineseStrings();
                    break;
                case "ja-JP":
                    LoadJapaneseStrings();
                    break;
                case "zh-TW":
                    LoadTraditionalChineseStrings();
                    break;
                case "en-US":
                default:
                    LoadEnglishStrings();
                    break;
            }
        }

        private static void LoadTraditionalChineseStrings()
        {
            _strings["AppTitle"] = "Windows 自訂訊息防熄屏鎖屏器";
            _strings["Title"] = "螢幕鎖定自訂器";
            _strings["Subtitle"] = "自訂離開告示、固定安全密碼，並透過 Windows API 原生阻止螢幕熄滅與休眠。";
            _strings["PresetHeader"] = "常用快速範本";
            _strings["PresetMeeting"] = "☕ 外出開會";
            _strings["PresetLunch"] = "🍱 午餐暫離";
            _strings["PresetComputing"] = "⚡ 跑模型/程式中";
            _strings["PresetAway"] = "🔕 暫離勿觸";
            _strings["PresetMeetingText"] = "外出開會中，預計 15:00 返回。如需緊急聯絡請撥打電話或發送訊息。";
            _strings["PresetLunchText"] = "午餐暫離，約 13:30 回到座位。請勿關閉電腦。";
            _strings["PresetComputingText"] = "⚡ 正在執行重要運算任務與檔案處理，請勿操作或中斷程序！";
            _strings["PresetAwayText"] = "暫時離開座位，稍後即回。";
            _strings["CustomMessageHeader"] = "主要公告訊息 (顯示於螢幕中央)";
            _strings["DefaultMessage"] = "外出開會中，預計 14:30 返回，請勿碰觸電腦。";
            _strings["ContactInfoHeader"] = "補充備註 / 聯絡方式 (選填)";
            _strings["DefaultContact"] = "分機 8888 / 緊急電話 0900-000-000";
            _strings["PasswordCardHeader"] = "🔑 解鎖密碼設定";
            _strings["FixedPasswordBadge"] = "(已固定密碼保護)";
            _strings["BtnClearPassword"] = "清除密碼";
            _strings["BtnShowPassword"] = "👁 顯示";
            _strings["BtnHidePassword"] = "🔒 隱藏";
            _strings["FixedPasswordCheck"] = "固定此密碼 (保存至本機，每次打開自動載入套用)";
            _strings["FixedPasswordHint"] = "免除每次鎖定重複輸入的麻煩；留空則為點擊即解鎖模式";
            _strings["ThemeHeader"] = "鎖屏視覺主題 (可預覽即時配色)";
            _strings["ThemeForest"] = "🌲 翠綠幽靜森林 (Deep Forest)";
            _strings["ThemePastoral"] = "🌾 春日田園麥浪 (Pastoral Meadow)";
            _strings["ThemeSunset"] = "🌅 暮色落日餘暉 (Sunset Glow)";
            _strings["ThemeDeepOcean"] = "🌊 浩瀚深海蔚藍 (Deep Ocean)";
            _strings["ThemeAurora"] = "🔮 夢幻極光夜紫 (Aurora Purple)";
            _strings["ThemeClassic"] = "🖤 曜石極致酷黑 (Classic Black)";
            _strings["KeepAwakeTitle"] = "保持螢幕常亮 (防止系統休眠與熄屏)";
            _strings["KeepAwakeDesc"] = "透過 SetThreadExecutionState 原生驅動，不會被電源逾時關閉螢幕";
            _strings["DelayTitle"] = "鎖定延遲：";
            _strings["DelayNow"] = "立即鎖定";
            _strings["Delay3"] = "3 秒倒數";
            _strings["Delay5"] = "5 秒倒數";
            _strings["BtnStartLock"] = "🔒 立即進入鎖屏";
            _strings["BtnStartLockCountdown"] = "⏳ {0} 秒後進入鎖屏...";
            _strings["LockAwakeStatus"] = "螢幕常亮保護中 (不熄屏)";
            _strings["LockSystemDefaultStatus"] = "系統預設電源模式";
            _strings["LockSystemStatus"] = "🔒 系統鎖定保護";
            _strings["NoticeTag"] = "📢 留言告示";
            _strings["ContactPrefix"] = "📞 聯絡資訊：";
            _strings["UnlockPromptPassword"] = "請輸入密碼以解除鎖定";
            _strings["UnlockPromptDirect"] = "目前處於免密碼鎖定模式";
            _strings["BtnUnlock"] = "解鎖";
            _strings["BtnDirectUnlock"] = "點擊解除鎖定";
            _strings["ErrorPassword"] = "密碼錯誤，請重新輸入！";
            _strings["LanguageSelector"] = "🌐 介面語言：";
            _strings["BtnOpenLocaleFolder"] = "📂 自訂語言檔";
            _strings["TrayLockNow"] = "🔒 立即鎖定 (當前配置)";
            _strings["TrayPresets"] = "📋 快速鎖定模板";
            _strings["TrayOpenSettings"] = "⚙️ 開啟設定面板";
            _strings["TrayExit"] = "❌ 結束程式";
            _strings["TrayReadyNotification"] = "鎖屏自訂器已在系統托盤待命，可隨時右鍵快速鎖屏。";
            _strings["BtnSaveAsPreset"] = "➕ 儲存為模板";
            _strings["BtnDeletePreset"] = "🗑️ 刪除";
            _strings["PresetPromptName"] = "請輸入新模板的標題 (例如: 🏃 運動健身):";
            _strings["StartMinimizedTitle"] = "啟動時最小化至系統托盤 (不直接顯示主視窗)";
            _strings["StartMinimizedDesc"] = "開啟時在右下角默默待命，雙擊圖示或右鍵選單即可快速操作";
            _strings["MinimizeHint"] = "應用程式已最小化至右下角系統托盤。";
            _strings["TrayAutoStart"] = "🚀 開機自動啟動";
            _strings["AutoStartTitle"] = "開機時自動啟動 (Windows 登入後自動執行)";
            _strings["AutoStartDesc"] = "開機後直接常駐托盤，隨時為您提供離座鎖屏保護";
            _strings["AutoStartEnabledHint"] = "已開啟開機自動啟動。下次開機將自動在托盤待命！";
            _strings["AutoStartDisabledHint"] = "已關閉開機自動啟動。";
        }

        private static void LoadSimplifiedChineseStrings()
        {
            _strings["AppTitle"] = "Windows 自定义信息防熄屏锁屏器";
            _strings["Title"] = "屏幕锁定自定义器";
            _strings["Subtitle"] = "自定义离开告示、固定安全密码，并调用 Windows API 原生阻止屏幕休眠熄灭。";
            _strings["PresetHeader"] = "常用快捷模版";
            _strings["PresetMeeting"] = "☕ 外出开会";
            _strings["PresetLunch"] = "🍱 午餐暂离";
            _strings["PresetComputing"] = "⚡ 跑模型/程序中";
            _strings["PresetAway"] = "🔕 暂离勿动";
            _strings["PresetMeetingText"] = "外出开会中，预计 15:00 返回。如需紧急联系请拨打电话或发送消息。";
            _strings["PresetLunchText"] = "午餐暂离，约 13:30 回到工位。请勿关闭电脑。";
            _strings["PresetComputingText"] = "⚡ 正在执行重要计算任务与数据处理，请勿操作或中断程序！";
            _strings["PresetAwayText"] = "暂时离开工位，稍后即回。";
            _strings["CustomMessageHeader"] = "主要告示信息 (居中显示于屏幕)";
            _strings["DefaultMessage"] = "外出开会中，预计 14:30 返回，请勿碰触电脑。";
            _strings["ContactInfoHeader"] = "补充备注 / 联系方式 (选填)";
            _strings["DefaultContact"] = "分机 8888 / 紧急电话 138-0000-0000";
            _strings["PasswordCardHeader"] = "🔑 解锁密码设置";
            _strings["FixedPasswordBadge"] = "(已固定密码保护)";
            _strings["BtnClearPassword"] = "清除密码";
            _strings["BtnShowPassword"] = "👁 显示";
            _strings["BtnHidePassword"] = "🔒 隐藏";
            _strings["FixedPasswordCheck"] = "固定此密码 (保存在本地，每次打开自动载入套用)";
            _strings["FixedPasswordHint"] = "免去每次锁定重复输入的麻烦；留空则为点击即解锁模式";
            _strings["ThemeHeader"] = "锁屏视觉主题 (即时预览配色)";
            _strings["ThemeForest"] = "🌲 翠绿幽静森林 (Deep Forest)";
            _strings["ThemePastoral"] = "🌾 春日田园麦浪 (Pastoral Meadow)";
            _strings["ThemeSunset"] = "🌅 暮色落日余晖 (Sunset Glow)";
            _strings["ThemeDeepOcean"] = "🌊 浩瀚深海蔚蓝 (Deep Ocean)";
            _strings["ThemeAurora"] = "🔮 梦幻极光夜紫 (Aurora Purple)";
            _strings["ThemeClassic"] = "🖤 曜石极致酷黑 (Classic Black)";
            _strings["KeepAwakeTitle"] = "保持屏幕常亮 (防止系统休眠与熄屏)";
            _strings["KeepAwakeDesc"] = "通过 SetThreadExecutionState 原生驱动，不会被电源超时关闭屏幕";
            _strings["DelayTitle"] = "锁定延迟：";
            _strings["DelayNow"] = "立即锁定";
            _strings["Delay3"] = "3 秒倒数";
            _strings["Delay5"] = "5 秒倒数";
            _strings["BtnStartLock"] = "🔒 立即进入锁屏";
            _strings["BtnStartLockCountdown"] = "⏳ {0} 秒后进入锁屏...";
            _strings["LockAwakeStatus"] = "屏幕常亮保护中 (不熄屏)";
            _strings["LockSystemDefaultStatus"] = "系统默认电源模式";
            _strings["LockSystemStatus"] = "🔒 系统锁定保护";
            _strings["NoticeTag"] = "📢 留言告示";
            _strings["ContactPrefix"] = "📞 联系信息：";
            _strings["UnlockPromptPassword"] = "请输入密码以解除锁定";
            _strings["UnlockPromptDirect"] = "当前处于免密码锁定模式";
            _strings["BtnUnlock"] = "解锁";
            _strings["BtnDirectUnlock"] = "点击解除锁定";
            _strings["ErrorPassword"] = "密码错误，请重新输入！";
            _strings["LanguageSelector"] = "🌐 界面语言：";
            _strings["BtnOpenLocaleFolder"] = "📂 自定义语言包";
            _strings["TrayLockNow"] = "🔒 立即锁定 (当前配置)";
            _strings["TrayPresets"] = "📋 快速锁定模版";
            _strings["TrayOpenSettings"] = "⚙️ 打开设置面板";
            _strings["TrayExit"] = "❌ 退出程序";
            _strings["TrayReadyNotification"] = "锁屏自定义器已在系统托盘待命，可随时右键快速锁屏。";
            _strings["BtnSaveAsPreset"] = "➕ 保存为模版";
            _strings["BtnDeletePreset"] = "🗑️ 删除";
            _strings["PresetPromptName"] = "请输入新模版的标题 (例如: 🏃 运动健身):";
            _strings["StartMinimizedTitle"] = "启动时最小化到系统托盘 (不直接显示主窗口)";
            _strings["StartMinimizedDesc"] = "开启时在右下角静默待命，双击图标或右键菜单即可快速操作";
            _strings["MinimizeHint"] = "应用程序已最小化至右下角系统托盘。";
            _strings["TrayAutoStart"] = "🚀 开机自动启动";
            _strings["AutoStartTitle"] = "开机时自动启动 (Windows 登录后自动运行)";
            _strings["AutoStartDesc"] = "开机后直接常驻托盘，随时为您提供离座锁屏保护";
            _strings["AutoStartEnabledHint"] = "已开启开机自动启动。下次开机将自动在托盘待命！";
            _strings["AutoStartDisabledHint"] = "已关闭开机自动启动。";
        }

        private static void LoadEnglishStrings()
        {
            _strings["AppTitle"] = "Windows Custom Screen Locker & Stay-Awake";
            _strings["Title"] = "Custom Screen Locker";
            _strings["Subtitle"] = "Display custom away notes, set fixed passwords, and keep display awake natively.";
            _strings["PresetHeader"] = "Quick Presets";
            _strings["PresetMeeting"] = "☕ In Meeting";
            _strings["PresetLunch"] = "🍱 Lunch Break";
            _strings["PresetComputing"] = "⚡ Computing Tasks";
            _strings["PresetAway"] = "🔕 Step Away";
            _strings["PresetMeetingText"] = "In a meeting, expected back at 15:00. Please call or message if urgent.";
            _strings["PresetLunchText"] = "Out for lunch, back around 13:30. Please do not power off PC.";
            _strings["PresetComputingText"] = "⚡ Running critical computations & tasks. Please do not interrupt!";
            _strings["PresetAwayText"] = "Stepped away from desk, will be back shortly.";
            _strings["CustomMessageHeader"] = "Main Away Message (Centered on screen)";
            _strings["DefaultMessage"] = "In a meeting, back around 14:30. Please do not disturb.";
            _strings["ContactInfoHeader"] = "Additional Note / Contact (Optional)";
            _strings["DefaultContact"] = "Ext. 8888 / Mobile: +1 (555) 019-2834";
            _strings["PasswordCardHeader"] = "🔑 Unlock Password Settings";
            _strings["FixedPasswordBadge"] = "(Fixed Password Active)";
            _strings["BtnClearPassword"] = "Clear";
            _strings["BtnShowPassword"] = "👁 Show";
            _strings["BtnHidePassword"] = "🔒 Hide";
            _strings["FixedPasswordCheck"] = "Fix this password (Saved locally & auto-applied on launch)";
            _strings["FixedPasswordHint"] = "Avoid typing each time; leave blank for click-to-unlock";
            _strings["ThemeHeader"] = "Visual Theme (Live Color Preview)";
            _strings["ThemeForest"] = "🌲 Emerald Forest (Deep Forest)";
            _strings["ThemePastoral"] = "🌾 Pastoral Meadow (Pastoral)";
            _strings["ThemeSunset"] = "🌅 Sunset Glow (Sunset)";
            _strings["ThemeDeepOcean"] = "🌊 Deep Ocean Blue (Deep Ocean)";
            _strings["ThemeAurora"] = "🔮 Aurora Purple (Aurora)";
            _strings["ThemeClassic"] = "🖤 Obsidian Black (Classic Black)";
            _strings["KeepAwakeTitle"] = "Keep Screen Awake (Prevent Sleep/Lock)";
            _strings["KeepAwakeDesc"] = "Native SetThreadExecutionState prevents display timeout natively";
            _strings["DelayTitle"] = "Lock Delay:";
            _strings["DelayNow"] = "Immediate";
            _strings["Delay3"] = "3s Countdown";
            _strings["Delay5"] = "5s Countdown";
            _strings["BtnStartLock"] = "🔒 Lock Screen Now";
            _strings["BtnStartLockCountdown"] = "⏳ Locking in {0}s...";
            _strings["LockAwakeStatus"] = "Screen Awake Active (No Sleep)";
            _strings["LockSystemDefaultStatus"] = "System Default Power Mode";
            _strings["LockSystemStatus"] = "🔒 System Locked & Protected";
            _strings["NoticeTag"] = "📢 AWAY NOTICE";
            _strings["ContactPrefix"] = "📞 Contact Info: ";
            _strings["UnlockPromptPassword"] = "Enter password to unlock";
            _strings["UnlockPromptDirect"] = "Direct unlock mode (No password)";
            _strings["BtnUnlock"] = "Unlock";
            _strings["BtnDirectUnlock"] = "Click to Unlock";
            _strings["ErrorPassword"] = "Incorrect password, please try again!";
            _strings["LanguageSelector"] = "🌐 Language:";
            _strings["BtnOpenLocaleFolder"] = "📂 Custom Locales";
            _strings["TrayLockNow"] = "🔒 Lock Now (Current Config)";
            _strings["TrayPresets"] = "📋 Quick Presets";
            _strings["TrayOpenSettings"] = "⚙️ Open Settings";
            _strings["TrayExit"] = "❌ Exit";
            _strings["TrayReadyNotification"] = "Screen Locker is active in the tray. Right-click to lock anytime.";
            _strings["BtnSaveAsPreset"] = "➕ Save as Preset";
            _strings["BtnDeletePreset"] = "🗑️ Delete";
            _strings["PresetPromptName"] = "Enter a title for this preset (e.g. 🏃 Gym/Workout):";
            _strings["StartMinimizedTitle"] = "Start minimized to system tray (Hide main window on launch)";
            _strings["StartMinimizedDesc"] = "Silently stays in system tray; right-click menu or double click to interact";
            _strings["MinimizeHint"] = "Application minimized to system tray.";
            _strings["TrayAutoStart"] = "🚀 Launch on Startup";
            _strings["AutoStartTitle"] = "Launch on Windows Startup (Run on login)";
            _strings["AutoStartDesc"] = "Silently stays in system tray after boot, ready to lock anytime";
            _strings["AutoStartEnabledHint"] = "Launch on Startup enabled! Will stay in tray on next boot.";
            _strings["AutoStartDisabledHint"] = "Launch on Startup disabled.";
        }

        private static void LoadJapaneseStrings()
        {
            _strings["AppTitle"] = "Windows カスタム画面ロック＆スリープ防止ツール";
            _strings["Title"] = "画面ロック・カスタマイザー";
            _strings["Subtitle"] = "離席メッセージを表示し、Windows API で画面の消灯・スリープを防止します。";
            _strings["PresetHeader"] = "クイックテンプレート";
            _strings["PresetMeeting"] = "☕ 会議中";
            _strings["PresetLunch"] = "🍱 昼食休憩";
            _strings["PresetComputing"] = "⚡ 計算処理中";
            _strings["PresetAway"] = "🔕 一時離席";
            _strings["PresetMeetingText"] = "会議中です。15:00 に戻る予定です。緊急時は連絡してください。";
            _strings["PresetLunchText"] = "昼食のため離席中。13:30 頃に戻ります。PC を切らないでください。";
            _strings["PresetComputingText"] = "⚡ 重要プログラム実行中のため、PC を操作しないでください！";
            _strings["PresetAwayText"] = "一時的に席を外しています。すぐに戻ります。";
            _strings["CustomMessageHeader"] = "表示メッセージ (画面中央)";
            _strings["DefaultMessage"] = "会議中につき 14:30 頃に戻ります。操作しないでください。";
            _strings["ContactInfoHeader"] = "連絡先 / 補足メモ (任意)";
            _strings["DefaultContact"] = "内線 8888 / 携帯 090-0000-0000";
            _strings["PasswordCardHeader"] = "🔑 解除パスワード設定";
            _strings["FixedPasswordBadge"] = "(固定パスワード有効)";
            _strings["BtnClearPassword"] = "クリア";
            _strings["BtnShowPassword"] = "👁 表示";
            _strings["BtnHidePassword"] = "🔒 非表示";
            _strings["FixedPasswordCheck"] = "このパスワードを固定 (保存して次回自動適用)";
            _strings["FixedPasswordHint"] = "入力の手間を省きます。空欄の場合はクリックで解除可能";
            _strings["ThemeHeader"] = "ビジュアルテーマ (プレビュー対応)";
            _strings["ThemeForest"] = "🌲 静寂の森林 (Deep Forest)";
            _strings["ThemePastoral"] = "🌾 春の田園 (Pastoral Meadow)";
            _strings["ThemeSunset"] = "🌅 夕焼け・夕暮れ (Sunset Glow)";
            _strings["ThemeDeepOcean"] = "🌊 深海ブルー (Deep Ocean)";
            _strings["ThemeAurora"] = "🔮 オーロラパープル (Aurora Purple)";
            _strings["ThemeClassic"] = "🖤 オブシディアンブラック (Classic Black)";
            _strings["KeepAwakeTitle"] = "画面常時オン (スリープ・消灯を防止)";
            _strings["KeepAwakeDesc"] = "SetThreadExecutionState により電源タイムアウトをネイティブに防止";
            _strings["DelayTitle"] = "遅延時間：";
            _strings["DelayNow"] = "今すぐロック";
            _strings["Delay3"] = "3秒後";
            _strings["Delay5"] = "5秒後";
            _strings["BtnStartLock"] = "🔒 画面をロックする";
            _strings["BtnStartLockCountdown"] = "⏳ {0} 秒後にロック...";
            _strings["LockAwakeStatus"] = "画面常時オン保護中 (スリープ無効)";
            _strings["LockSystemDefaultStatus"] = "通常電源モード";
            _strings["LockSystemStatus"] = "🔒 画面ロック中";
            _strings["NoticeTag"] = "📢 離席中のお知らせ";
            _strings["ContactPrefix"] = "📞 連絡先：";
            _strings["UnlockPromptPassword"] = "パスワードを入力して解除";
            _strings["UnlockPromptDirect"] = "パスワードなしモード";
            _strings["BtnUnlock"] = "解除";
            _strings["BtnDirectUnlock"] = "クリックして解除";
            _strings["ErrorPassword"] = "パスワードが間違っています。再入力してください！";
            _strings["LanguageSelector"] = "🌐 言語 (Language)：";
            _strings["BtnOpenLocaleFolder"] = "📂 言語ファイルを開く";
            _strings["TrayLockNow"] = "🔒 今すぐロック (現在の設定)";
            _strings["TrayPresets"] = "📋 クイックテンプレート";
            _strings["TrayOpenSettings"] = "⚙️ 設定を開く";
            _strings["TrayExit"] = "❌ 終了";
            _strings["TrayReadyNotification"] = "タスクトレイで待機中です。右クリックで素早く画面をロックできます。";
            _strings["BtnSaveAsPreset"] = "➕ テンプレートとして保存";
            _strings["BtnDeletePreset"] = "🗑️ 削除";
            _strings["PresetPromptName"] = "テンプレート名を入力してください (例: 🏃 運動中):";
            _strings["StartMinimizedTitle"] = "起動時にタスクトレイへ最小化 (メイン画面を非表示)";
            _strings["StartMinimizedDesc"] = "起動時にトレイへ静かに常駐し、ダブルクリックや右クリックで操作します";
            _strings["MinimizeHint"] = "タスクトレイに最小化しました。";
            _strings["TrayAutoStart"] = "🚀 PC起動時に自動実行";
            _strings["AutoStartTitle"] = "Windows 起動時に自動実行 (ログイン時に常駐)";
            _strings["AutoStartDesc"] = "PC 起動後にトレイへ常駐し、いつでも素早く画面を保護できます";
            _strings["AutoStartEnabledHint"] = "自動起動を有効にしました。次回起動時にトレイで待機します。";
            _strings["AutoStartDisabledHint"] = "自動起動を無効にしました。";
        }
    }
}
