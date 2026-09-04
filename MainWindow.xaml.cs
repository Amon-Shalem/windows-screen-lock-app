using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace CustomScreenLocker
{
    public partial class MainWindow : Window
    {
        private class UserConfig
        {
            public string CustomMessage { get; set; } = string.Empty;
            public string ContactInfo { get; set; } = string.Empty;
            public bool KeepAwake { get; set; } = true;
            public int ThemeIndex { get; set; } = 0;
            public bool IsFixedPassword { get; set; } = false;
            public string FixedPassword { get; set; } = string.Empty;
            public string LanguageCode { get; set; } = "en-US";
            public bool IsFirstRunDone { get; set; } = false;
            public bool StartMinimizedToTray { get; set; } = true;
            public List<LockPreset> Presets { get; set; } = new();
        }

        private readonly string _configFilePath;
        private DispatcherTimer? _countdownTimer;
        private int _countdownSeconds = 0;
        private bool _isPasswordVisible = false;
        private bool _isSyncingPassword = false;
        private bool _isUpdatingLanguageUi = false;
        private bool _isRealExit = false;

        private Forms.NotifyIcon? _notifyIcon;
        private Forms.ContextMenuStrip? _trayMenu;

        private List<LockPreset> _presets = new();

        public MainWindow()
        {
            InitializeComponent();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "CustomScreenLocker");
            Directory.CreateDirectory(appFolder);
            _configFilePath = Path.Combine(appFolder, "config.json");

            InitTrayIcon();
        }

        private void InitTrayIcon()
        {
            try
            {
                _notifyIcon = new Forms.NotifyIcon();
                string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(icoPath))
                {
                    _notifyIcon.Icon = new Icon(icoPath);
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Shield;
                }

                _notifyIcon.Text = "Custom Screen Locker";
                _notifyIcon.Visible = true;
                _notifyIcon.DoubleClick += (s, e) => ShowAndActivate();

                RebuildTrayMenu();
            }
            catch { }
        }

        private void RebuildTrayMenu()
        {
            if (_notifyIcon == null) return;

            _trayMenu = new Forms.ContextMenuStrip();

            // 1. 立即鎖定 (當前配置)
            var lockNowItem = new Forms.ToolStripMenuItem(I18nManager.T("TrayLockNow"));
            lockNowItem.Font = new System.Drawing.Font(lockNowItem.Font, System.Drawing.FontStyle.Bold);
            lockNowItem.Click += (s, e) => Dispatcher.Invoke(() => LaunchLockScreen());
            _trayMenu.Items.Add(lockNowItem);

            // 2. 快速鎖定模板子選單
            var presetsSubMenu = new Forms.ToolStripMenuItem(I18nManager.T("TrayPresets"));
            foreach (var preset in _presets)
            {
                var p = preset;
                var item = new Forms.ToolStripMenuItem(p.Title);
                item.Click += (s, e) => Dispatcher.Invoke(() => LaunchLockScreenWithPreset(p));
                presetsSubMenu.DropDownItems.Add(item);
            }
            _trayMenu.Items.Add(presetsSubMenu);

            _trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // 3. 開啟設定面板
            var settingsItem = new Forms.ToolStripMenuItem(I18nManager.T("TrayOpenSettings"));
            settingsItem.Click += (s, e) => Dispatcher.Invoke(() => ShowAndActivate());
            _trayMenu.Items.Add(settingsItem);

            // 4. 開機自動啟動切換
            var autoStartItem = new Forms.ToolStripMenuItem(I18nManager.T("TrayAutoStart"));
            autoStartItem.Checked = NativeMethods.IsAutoStartEnabled();
            autoStartItem.Click += (s, e) => Dispatcher.Invoke(() =>
            {
                bool newState = !NativeMethods.IsAutoStartEnabled();
                NativeMethods.SetAutoStart(newState);
                autoStartItem.Checked = newState;
                if (ChkAutoStart != null)
                {
                    ChkAutoStart.IsChecked = newState;
                }
                string msg = newState ? I18nManager.T("AutoStartEnabledHint") : I18nManager.T("AutoStartDisabledHint");
                _notifyIcon?.ShowBalloonTip(2000, "Custom Screen Locker", msg, Forms.ToolTipIcon.Info);
            });
            _trayMenu.Items.Add(autoStartItem);

            // 5. 語言快速切換
            var langSubMenu = new Forms.ToolStripMenuItem(I18nManager.T("LanguageSelector"));
            foreach (var lang in I18nManager.GetAvailableLanguages())
            {
                var l = lang;
                var langItem = new Forms.ToolStripMenuItem(l.DisplayName);
                langItem.Checked = (l.Code == I18nManager.CurrentLanguageCode);
                langItem.Click += (s, e) => Dispatcher.Invoke(() =>
                {
                    I18nManager.SetLanguage(l.Code);
                    InitLanguageDropdown();
                    ApplyLocalization();
                    SaveConfig();
                });
                langSubMenu.DropDownItems.Add(langItem);
            }
            _trayMenu.Items.Add(langSubMenu);

            _trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // 5. 結束程式
            var exitItem = new Forms.ToolStripMenuItem(I18nManager.T("TrayExit"));
            exitItem.Click += (s, e) => Dispatcher.Invoke(() => ExitApplication());
            _trayMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = _trayMenu;
        }

        public void InitializeAndStart(bool isFirstRun)
        {
            if (isFirstRun)
            {
                TxtCustomMessage.Text = I18nManager.T("DefaultMessage");
                TxtContactInfo.Text = I18nManager.T("DefaultContact");
            }

            // 載入設定
            LoadConfig();

            // 初始化預設模板 (若無)
            EnsureDefaultPresets();

            // 初始化語言切換下拉選單
            InitLanguageDropdown();

            // 套用全介面國際化文字
            ApplyLocalization();

            // 渲染模板按鈕列表
            RenderPresetsUi();

            // 檢查並同步開機自啟狀態
            ChkAutoStart.IsChecked = NativeMethods.IsAutoStartEnabled();

            // 重構托盤選單
            RebuildTrayMenu();

            // 預覽條顏色刷新
            if (CmbTheme.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                UpdateThemePreview(tag);
            }

            // 首次啟動：直接展現主視窗讓使用者認識設定與功能
            if (isFirstRun)
            {
                ShowAndActivate();
                SaveConfig();
            }
            // 非首次啟動：若勾選「啟動時最小化至托盤」，則完全不顯示主視窗，安靜常駐托盤
            else if (ChkStartMinimized.IsChecked ?? true)
            {
                this.Visibility = Visibility.Hidden;
                this.ShowInTaskbar = false;
                _notifyIcon?.ShowBalloonTip(3000, "Custom Screen Locker", I18nManager.T("TrayReadyNotification"), Forms.ToolTipIcon.Info);
            }
            else
            {
                ShowAndActivate();
            }

            // 延遲 800ms 自動收縮工作集
            var trimTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            trimTimer.Tick += (s, ev) =>
            {
                trimTimer.Stop();
                NativeMethods.TrimMemory();
            };
            trimTimer.Start();
        }

        private void EnsureDefaultPresets()
        {
            if (_presets.Count == 0)
            {
                _presets.Add(new LockPreset
                {
                    Title = I18nManager.T("PresetMeeting"),
                    Message = I18nManager.T("PresetMeetingText"),
                    Contact = I18nManager.T("DefaultContact"),
                    Theme = "DeepOcean"
                });
                _presets.Add(new LockPreset
                {
                    Title = I18nManager.T("PresetLunch"),
                    Message = I18nManager.T("PresetLunchText"),
                    Contact = I18nManager.T("DefaultContact"),
                    Theme = "Pastoral"
                });
                _presets.Add(new LockPreset
                {
                    Title = I18nManager.T("PresetComputing"),
                    Message = I18nManager.T("PresetComputingText"),
                    Contact = I18nManager.T("DefaultContact"),
                    Theme = "Forest"
                });
                _presets.Add(new LockPreset
                {
                    Title = I18nManager.T("PresetAway"),
                    Message = I18nManager.T("PresetAwayText"),
                    Contact = "",
                    Theme = "ClassicBlack"
                });
            }
        }

        private void RenderPresetsUi()
        {
            WrapPanelPresets.Children.Clear();

            foreach (var preset in _presets)
            {
                var p = preset;

                var btnBorder = new Border
                {
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 42, 54)),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0, 0, 8, 8),
                    Padding = new Thickness(10, 5, 8, 5)
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };

                // 模板按鈕
                var tb = new TextBlock
                {
                    Text = p.Title,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center
                };
                tb.MouseLeftButtonDown += (s, e) => ApplyPresetToForm(p);
                sp.Children.Add(tb);

                // 刪除小按鈕
                var delBtn = new TextBlock
                {
                    Text = " ✕",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 163, 175)),
                    FontSize = 11,
                    Margin = new Thickness(6, 0, 0, 0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = I18nManager.T("BtnDeletePreset")
                };
                delBtn.MouseEnter += (s, e) => delBtn.Foreground = System.Windows.Media.Brushes.Red;
                delBtn.MouseLeave += (s, e) => delBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 163, 175));
                delBtn.MouseLeftButtonDown += (s, e) =>
                {
                    e.Handled = true;
                    DeletePreset(p);
                };
                sp.Children.Add(delBtn);

                btnBorder.Child = sp;
                WrapPanelPresets.Children.Add(btnBorder);
            }
        }

        private void ApplyPresetToForm(LockPreset preset)
        {
            TxtCustomMessage.Text = preset.Message;
            if (!string.IsNullOrEmpty(preset.Contact))
            {
                TxtContactInfo.Text = preset.Contact;
            }

            for (int i = 0; i < CmbTheme.Items.Count; i++)
            {
                if (CmbTheme.Items[i] is ComboBoxItem item && item.Tag is string tag && tag == preset.Theme)
                {
                    CmbTheme.SelectedIndex = i;
                    break;
                }
            }
        }

        private void DeletePreset(LockPreset preset)
        {
            _presets.Remove(preset);
            RenderPresetsUi();
            RebuildTrayMenu();
            SaveConfig();
        }

        private void BtnSaveAsPreset_Click(object sender, RoutedEventArgs e)
        {
            string prompt = I18nManager.T("PresetPromptName");
            var dlg = new PromptDialog(prompt, "📌 我的模板");
            dlg.Owner = this;
            if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.InputText))
            {
                string theme = "ClassicBlack";
                if (CmbTheme.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                {
                    theme = tag;
                }

                var newPreset = new LockPreset
                {
                    Title = dlg.InputText,
                    Message = TxtCustomMessage.Text,
                    Contact = TxtContactInfo.Text,
                    Theme = theme
                };

                _presets.Add(newPreset);
                RenderPresetsUi();
                RebuildTrayMenu();
                SaveConfig();
            }
        }

        private void ShowAndActivate()
        {
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ExitApplication()
        {
            _isRealExit = true;
            SaveConfig();
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isRealExit)
            {
                e.Cancel = true;
                SaveConfig();
                this.Hide();
                this.ShowInTaskbar = false;
                _notifyIcon?.ShowBalloonTip(2000, "Custom Screen Locker", I18nManager.T("MinimizeHint"), Forms.ToolTipIcon.Info);
                NativeMethods.TrimMemory();
            }
        }

        private string CurrentPassword
        {
            get => _isPasswordVisible ? TxtUnlockPlain.Text : PwdUnlock.Password;
            set
            {
                _isSyncingPassword = true;
                PwdUnlock.Password = value;
                TxtUnlockPlain.Text = value;
                _isSyncingPassword = false;
            }
        }

        private void InitLanguageDropdown()
        {
            _isUpdatingLanguageUi = true;
            CmbLanguage.Items.Clear();

            I18nManager.ScanCustomLanguages();
            int selectIndex = 0;
            int idx = 0;

            foreach (var lang in I18nManager.GetAvailableLanguages())
            {
                var item = new ComboBoxItem
                {
                    Content = lang.DisplayName,
                    Tag = lang.Code
                };
                CmbLanguage.Items.Add(item);

                if (lang.Code == I18nManager.CurrentLanguageCode)
                {
                    selectIndex = idx;
                }
                idx++;
            }

            CmbLanguage.SelectedIndex = selectIndex;
            _isUpdatingLanguageUi = false;
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingLanguageUi) return;

            if (CmbLanguage.SelectedItem is ComboBoxItem item && item.Tag is string code)
            {
                I18nManager.SetLanguage(code);
                ApplyLocalization();
                RebuildTrayMenu();
                SaveConfig();
            }
        }

        private void ApplyLocalization()
        {
            this.Title = I18nManager.T("AppTitle");
            TxtAppTitle.Text = I18nManager.T("Title");
            TxtAppSubtitle.Text = I18nManager.T("Subtitle");

            TxtPresetHeader.Text = I18nManager.T("PresetHeader");
            BtnSaveAsPreset.Content = I18nManager.T("BtnSaveAsPreset");

            TxtMsgHeader.Text = I18nManager.T("CustomMessageHeader");
            TxtContactHeader.Text = I18nManager.T("ContactInfoHeader");

            TxtPwdHeader.Text = I18nManager.T("PasswordCardHeader");
            BtnClearPassword.Content = I18nManager.T("BtnClearPassword");
            BtnToggleShowPwd.Content = _isPasswordVisible ? I18nManager.T("BtnHidePassword") : I18nManager.T("BtnShowPassword");
            TxtFixedPwdCheckTitle.Text = I18nManager.T("FixedPasswordCheck");
            TxtFixedPwdCheckDesc.Text = I18nManager.T("FixedPasswordHint");

            TxtThemeHeader.Text = I18nManager.T("ThemeHeader");
            ItemThemeForest.Content = I18nManager.T("ThemeForest");
            ItemThemePastoral.Content = I18nManager.T("ThemePastoral");
            ItemThemeSunset.Content = I18nManager.T("ThemeSunset");
            ItemThemeDeepOcean.Content = I18nManager.T("ThemeDeepOcean");
            ItemThemeAurora.Content = I18nManager.T("ThemeAurora");
            ItemThemeClassic.Content = I18nManager.T("ThemeClassic");

            TxtKeepAwakeTitle.Text = I18nManager.T("KeepAwakeTitle");
            TxtKeepAwakeDesc.Text = I18nManager.T("KeepAwakeDesc");

            TxtStartMinimizedTitle.Text = I18nManager.T("StartMinimizedTitle");
            TxtStartMinimizedDesc.Text = I18nManager.T("StartMinimizedDesc");

            TxtAutoStartTitle.Text = I18nManager.T("AutoStartTitle");
            TxtAutoStartDesc.Text = I18nManager.T("AutoStartDesc");

            TxtDelayTitle.Text = I18nManager.T("DelayTitle");
            RbDelay0.Content = I18nManager.T("DelayNow");
            RbDelay3.Content = I18nManager.T("Delay3");
            RbDelay5.Content = I18nManager.T("Delay5");

            BtnStartLock.Content = I18nManager.T("BtnStartLock");

            UpdatePasswordBadge();

            if (CmbTheme.SelectedItem is ComboBoxItem themeItem && themeItem.Tag is string tag)
            {
                UpdateThemePreview(tag);
            }
        }

        private void BtnOpenLocales_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", I18nManager.LocalesDirectory)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法開啟資料夾: {ex.Message}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<UserConfig>(json);
                    if (config != null)
                    {
                        if (!string.IsNullOrEmpty(config.LanguageCode))
                        {
                            I18nManager.SetLanguage(config.LanguageCode);
                        }

                        if (!string.IsNullOrEmpty(config.CustomMessage))
                        {
                            TxtCustomMessage.Text = config.CustomMessage;
                        }
                        if (!string.IsNullOrEmpty(config.ContactInfo))
                        {
                            TxtContactInfo.Text = config.ContactInfo;
                        }

                        ChkKeepAwake.IsChecked = config.KeepAwake;
                        ChkStartMinimized.IsChecked = config.StartMinimizedToTray;

                        if (config.ThemeIndex >= 0 && config.ThemeIndex < CmbTheme.Items.Count)
                        {
                            CmbTheme.SelectedIndex = config.ThemeIndex;
                        }

                        // 載入固定密碼
                        ChkFixedPassword.IsChecked = config.IsFixedPassword;
                        if (config.IsFixedPassword && !string.IsNullOrEmpty(config.FixedPassword))
                        {
                            CurrentPassword = config.FixedPassword;
                        }

                        // 載入自定義模板列表
                        if (config.Presets != null && config.Presets.Count > 0)
                        {
                            _presets = config.Presets;
                        }
                    }
                }
            }
            catch
            {
                // 忽略配置讀取失敗
            }

            UpdatePasswordBadge();
        }

        private void SaveConfig()
        {
            try
            {
                bool isFixed = ChkFixedPassword.IsChecked ?? false;
                var config = new UserConfig
                {
                    LanguageCode = I18nManager.CurrentLanguageCode,
                    IsFirstRunDone = true,
                    CustomMessage = TxtCustomMessage.Text,
                    ContactInfo = TxtContactInfo.Text,
                    KeepAwake = ChkKeepAwake.IsChecked ?? true,
                    StartMinimizedToTray = ChkStartMinimized.IsChecked ?? true,
                    ThemeIndex = CmbTheme.SelectedIndex,
                    IsFixedPassword = isFixed,
                    FixedPassword = isFixed ? CurrentPassword : string.Empty,
                    Presets = _presets
                };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }
            catch
            {
                // 忽略保存失敗
            }
        }

        private void UpdatePasswordBadge()
        {
            if (TxtPasswordBadge == null) return;

            bool isFixed = ChkFixedPassword.IsChecked ?? false;
            if (isFixed && !string.IsNullOrEmpty(CurrentPassword))
            {
                TxtPasswordBadge.Visibility = Visibility.Visible;
                TxtPasswordBadge.Text = I18nManager.T("FixedPasswordBadge");
            }
            else
            {
                TxtPasswordBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void PwdUnlock_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isSyncingPassword && !_isPasswordVisible)
            {
                _isSyncingPassword = true;
                TxtUnlockPlain.Text = PwdUnlock.Password;
                _isSyncingPassword = false;
                UpdatePasswordBadge();
            }
        }

        private void TxtUnlockPlain_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isSyncingPassword && _isPasswordVisible)
            {
                _isSyncingPassword = true;
                PwdUnlock.Password = TxtUnlockPlain.Text;
                _isSyncingPassword = false;
                UpdatePasswordBadge();
            }
        }

        private void BtnToggleShowPwd_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                TxtUnlockPlain.Text = PwdUnlock.Password;
                PwdUnlock.Visibility = Visibility.Collapsed;
                TxtUnlockPlain.Visibility = Visibility.Visible;
                TxtUnlockPlain.Focus();
                TxtUnlockPlain.CaretIndex = TxtUnlockPlain.Text.Length;
                BtnToggleShowPwd.Content = I18nManager.T("BtnHidePassword");
            }
            else
            {
                PwdUnlock.Password = TxtUnlockPlain.Text;
                TxtUnlockPlain.Visibility = Visibility.Collapsed;
                PwdUnlock.Visibility = Visibility.Visible;
                PwdUnlock.Focus();
                BtnToggleShowPwd.Content = I18nManager.T("BtnShowPassword");
            }
        }

        private void ChkFixedPassword_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePasswordBadge();
            SaveConfig();
        }

        private void BtnClearPassword_Click(object sender, RoutedEventArgs e)
        {
            CurrentPassword = string.Empty;
            UpdatePasswordBadge();
            SaveConfig();
        }

        private void BtnStartLock_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();

            int delaySeconds = 0;
            if (RbDelay3.IsChecked == true) delaySeconds = 3;
            else if (RbDelay5.IsChecked == true) delaySeconds = 5;

            if (delaySeconds == 0)
            {
                LaunchLockScreen();
            }
            else
            {
                StartCountdown(delaySeconds);
            }
        }

        private void StartCountdown(int seconds)
        {
            _countdownSeconds = seconds;
            BtnStartLock.IsEnabled = false;
            string template = I18nManager.T("BtnStartLockCountdown");
            BtnStartLock.Content = string.Format(template, _countdownSeconds);

            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += (s, ev) =>
            {
                _countdownSeconds--;
                if (_countdownSeconds <= 0)
                {
                    _countdownTimer.Stop();
                    BtnStartLock.IsEnabled = true;
                    BtnStartLock.Content = I18nManager.T("BtnStartLock");
                    LaunchLockScreen();
                }
                else
                {
                    BtnStartLock.Content = string.Format(template, _countdownSeconds);
                }
            };
            _countdownTimer.Start();
        }

        private void LaunchLockScreen()
        {
            string message = TxtCustomMessage.Text;
            string contact = TxtContactInfo.Text;
            string password = CurrentPassword;
            bool keepAwake = ChkKeepAwake.IsChecked ?? true;

            string theme = "ClassicBlack";
            if (CmbTheme.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                theme = tag;
            }

            DoLaunchLock(message, contact, password, keepAwake, theme);
        }

        private void LaunchLockScreenWithPreset(LockPreset preset)
        {
            string message = preset.Message;
            string contact = preset.Contact;
            string password = CurrentPassword; // 使用當前設定的固定密碼
            bool keepAwake = ChkKeepAwake.IsChecked ?? true;
            string theme = preset.Theme;

            DoLaunchLock(message, contact, password, keepAwake, theme);
        }

        private void DoLaunchLock(string message, string contact, string password, bool keepAwake, string theme)
        {
            this.Hide();

            var lockWindow = new LockOverlayWindow(message, contact, password, keepAwake, theme);
            lockWindow.Closed += (s, e) =>
            {
                // 解鎖後，若原本是最小化至托盤狀態，則繼續保持在托盤，使用者亦可隨時雙擊圖示開啟
                if (!(ChkStartMinimized.IsChecked ?? false))
                {
                    ShowAndActivate();
                }
                NativeMethods.TrimMemory();
            };

            lockWindow.Show();
        }

        private void CmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTheme.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                UpdateThemePreview(tag);
            }
        }

        private void UpdateThemePreview(string tag)
        {
            if (ThemePreviewBorder == null || TxtThemeDesc == null || ThemePreviewLabel == null) return;

            switch (tag)
            {
                case "Forest":
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(6, 35, 25), System.Windows.Media.Color.FromRgb(16, 185, 129), 0);
                    TxtThemeDesc.Text = "松柏墨綠 • 翡翠光暈";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 211, 153));
                    ThemePreviewLabel.Text = I18nManager.T("ThemeForest");
                    break;

                case "Pastoral":
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(31, 58, 16), System.Windows.Media.Color.FromRgb(132, 204, 22), 0);
                    TxtThemeDesc.Text = "金綠麥浪 • 春日青田";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(163, 230, 53));
                    ThemePreviewLabel.Text = I18nManager.T("ThemePastoral");
                    break;

                case "Sunset":
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(59, 18, 11), System.Windows.Media.Color.FromRgb(249, 115, 22), 0);
                    TxtThemeDesc.Text = "暮色晚霞 • 琥珀暖橘";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 146, 60));
                    ThemePreviewLabel.Text = I18nManager.T("ThemeSunset");
                    break;

                case "DeepOcean":
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(11, 37, 70), System.Windows.Media.Color.FromRgb(2, 132, 199), 0);
                    TxtThemeDesc.Text = "深邃大洋 • 螢光碧海";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(56, 189, 248));
                    ThemePreviewLabel.Text = I18nManager.T("ThemeDeepOcean");
                    break;

                case "AuroraPurple":
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(44, 14, 67), System.Windows.Media.Color.FromRgb(168, 85, 247), 0);
                    TxtThemeDesc.Text = "夢幻夜空 • 極光紫霓";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(192, 132, 252));
                    ThemePreviewLabel.Text = I18nManager.T("ThemeAurora");
                    break;

                case "ClassicBlack":
                default:
                    ThemePreviewBorder.Background = new LinearGradientBrush(
                        System.Windows.Media.Color.FromRgb(17, 18, 21), System.Windows.Media.Color.FromRgb(37, 99, 235), 0);
                    TxtThemeDesc.Text = "純粹曜石 • 科技冷藍";
                    TxtThemeDesc.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(96, 165, 250));
                    ThemePreviewLabel.Text = I18nManager.T("ThemeClassic");
                    break;
            }
        }

        private void ChkAutoStart_Click(object sender, RoutedEventArgs e)
        {
            bool enable = ChkAutoStart.IsChecked ?? false;
            NativeMethods.SetAutoStart(enable);
            RebuildTrayMenu();
            string msg = enable ? I18nManager.T("AutoStartEnabledHint") : I18nManager.T("AutoStartDisabledHint");
            _notifyIcon?.ShowBalloonTip(2000, "Custom Screen Locker", msg, Forms.ToolTipIcon.Info);
        }
    }
}
