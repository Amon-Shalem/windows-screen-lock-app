using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CustomScreenLocker
{
    public partial class LockOverlayWindow : Window
    {
        private readonly string _customMessage;
        private readonly string _contactInfo;
        private readonly string _password;
        private readonly bool _keepAwake;
        private readonly string _theme;

        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _trimTimer;
        private readonly List<SecondaryBlankWindow> _secondaryWindows = new();
        private bool _isUnlocked = false;

        public LockOverlayWindow(string customMessage, string contactInfo, string password, bool keepAwake, string theme)
        {
            InitializeComponent();

            _customMessage = string.IsNullOrWhiteSpace(customMessage) ? "螢幕已鎖定" : customMessage;
            _contactInfo = contactInfo?.Trim() ?? string.Empty;
            _password = password ?? string.Empty;
            _keepAwake = keepAwake;
            _theme = theme;

            // 時鐘計時器 (1 秒整步更新，大幅降低喚醒頻率)
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += ClockTimer_Tick;

            // 定期記憶體修剪計時器 (每 3 分鐘主動釋放不常用分頁，鎖屏期間記憶體恆定低水準)
            _trimTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(3)
            };
            _trimTimer.Tick += (s, e) => NativeMethods.TrimMemory();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 套用資料
            CustomMessageText.Text = _customMessage;

            if (string.IsNullOrEmpty(_contactInfo))
            {
                ContactBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                ContactBorder.Visibility = Visibility.Visible;
                ContactInfoText.Text = _contactInfo;
            }

            // 判斷是否需要輸入密碼
            if (string.IsNullOrEmpty(_password))
            {
                UnlockPromptText.Text = I18nManager.T("UnlockPromptDirect");
                PasswordInputArea.Visibility = Visibility.Collapsed;
                BtnDirectUnlock.Visibility = Visibility.Visible;
            }
            else
            {
                UnlockPromptText.Text = I18nManager.T("UnlockPromptPassword");
                PasswordInputArea.Visibility = Visibility.Visible;
                BtnDirectUnlock.Visibility = Visibility.Collapsed;
                UnlockPasswordBox.Focus();
            }

            // 國際化動態文字
            BtnUnlockSubmit.Content = I18nManager.T("BtnUnlock");
            BtnDirectUnlock.Content = I18nManager.T("BtnDirectUnlock");
            NoticeTagText.Text = I18nManager.T("NoticeTag");
            TxtContactPrefix.Text = I18nManager.T("ContactPrefix");
            TxtLockSystemStatus.Text = I18nManager.T("LockSystemStatus");

            // 套用主題顏色
            ApplyTheme(_theme);

            // 是否開啟防熄屏
            if (_keepAwake)
            {
                NativeMethods.EnableKeepAwake();
                AwakeStatusText.Text = I18nManager.T("LockAwakeStatus");
            }
            else
            {
                AwakeStatusText.Text = I18nManager.T("LockSystemDefaultStatus");
            }

            // 安裝鍵盤鉤子
            NativeMethods.InstallKeyboardHook();

            // 啟動時鐘
            UpdateClock();
            _clockTimer.Start();

            // 覆蓋副螢幕 (如果有多螢幕)
            CoverSecondaryScreens();

            // 獲取焦點
            this.Activate();
            this.Focus();

            // 啟動定期修剪並在進入鎖屏 600ms 後執行首次深度記憶體收縮
            _trimTimer.Start();
            var initialTrimTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            initialTrimTimer.Tick += (s, ev) =>
            {
                initialTrimTimer.Stop();
                NativeMethods.TrimMemory();
            };
            initialTrimTimer.Start();
        }

        private void ApplyTheme(string theme)
        {
            switch (theme)
            {
                case "Forest": // 🌲 翠綠幽靜森林
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(6, 35, 25), Color.FromRgb(2, 18, 12), 45);
                    GlowTopColor.Color = Color.FromRgb(16, 185, 129); // 翡翠綠
                    GlowBottomColor.Color = Color.FromRgb(52, 211, 153); // 薄荷綠
                    GlowEllipseTop.Opacity = 0.35;
                    GlowEllipseBottom.Opacity = 0.30;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(12, 43, 32));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(6, 26, 19));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(9, 33, 24));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(22, 101, 74));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    break;

                case "Pastoral": // 🌾 春日田園麥浪
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(31, 58, 16), Color.FromRgb(13, 28, 6), 45);
                    GlowTopColor.Color = Color.FromRgb(132, 204, 22); // 春芽嫩綠
                    GlowBottomColor.Color = Color.FromRgb(234, 179, 8); // 麥浪金黃
                    GlowEllipseTop.Opacity = 0.38;
                    GlowEllipseBottom.Opacity = 0.32;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(39, 71, 21));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(132, 204, 22));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(250, 204, 21));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(18, 35, 10));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(25, 47, 14));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(77, 124, 27));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(101, 163, 13));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(101, 163, 13));
                    break;

                case "Sunset": // 🌅 暮色落日餘暉
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(59, 18, 11), Color.FromRgb(28, 6, 4), 45);
                    GlowTopColor.Color = Color.FromRgb(239, 68, 68); // 夕陽赤紅
                    GlowBottomColor.Color = Color.FromRgb(249, 115, 22); // 琥珀金橙
                    GlowEllipseTop.Opacity = 0.35;
                    GlowEllipseBottom.Opacity = 0.32;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(71, 23, 14));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(249, 115, 22));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(32, 10, 6));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(48, 15, 10));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(154, 52, 18));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(234, 88, 12));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(234, 88, 12));
                    break;

                case "DeepOcean": // 🌊 浩瀚深海蔚藍
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(11, 37, 70), Color.FromRgb(4, 16, 32), 45);
                    GlowTopColor.Color = Color.FromRgb(2, 132, 199); // 蔚藍
                    GlowBottomColor.Color = Color.FromRgb(6, 182, 212); // 青碧
                    GlowEllipseTop.Opacity = 0.36;
                    GlowEllipseBottom.Opacity = 0.30;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(14, 51, 94));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(5, 24, 44));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(8, 34, 64));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 64, 175));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                    break;

                case "AuroraPurple": // 🔮 夢幻極光夜紫
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(44, 14, 67), Color.FromRgb(18, 4, 29), 45);
                    GlowTopColor.Color = Color.FromRgb(168, 85, 247); // 璀璨霓紫
                    GlowBottomColor.Color = Color.FromRgb(236, 72, 153); // 魅惑玫粉
                    GlowEllipseTop.Opacity = 0.38;
                    GlowEllipseBottom.Opacity = 0.32;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(58, 19, 89));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(168, 85, 247));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(192, 132, 252));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(25, 6, 38));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(37, 11, 58));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(126, 34, 206));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(147, 51, 234));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(147, 51, 234));
                    break;

                case "ClassicBlack": // 🖤 曜石極致酷黑
                default:
                    BackgroundGradientBorder.Background = new LinearGradientBrush(
                        Color.FromRgb(17, 18, 21), Color.FromRgb(7, 7, 9), 45);
                    GlowTopColor.Color = Color.FromRgb(59, 130, 246); // 冷藍
                    GlowBottomColor.Color = Color.FromRgb(99, 102, 241); // 紫藍
                    GlowEllipseTop.Opacity = 0.22;
                    GlowEllipseBottom.Opacity = 0.18;

                    NoticeCardBorder.Background = new SolidColorBrush(Color.FromRgb(28, 30, 38));
                    NoticeCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(55, 65, 81));
                    NoticeTagText.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    ContactBorder.Background = new SolidColorBrush(Color.FromRgb(13, 15, 19));

                    UnlockCardBorder.Background = new SolidColorBrush(Color.FromRgb(20, 22, 28));
                    UnlockCardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(43, 48, 60));
                    BtnUnlockSubmit.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    BtnDirectUnlock.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    break;
            }
        }

        private void CoverSecondaryScreens()
        {
            try
            {
                // 枚舉所有顯示器覆蓋副螢幕
                // 使用 Windows API 虛擬螢幕座標
                double vLeft = SystemParameters.VirtualScreenLeft;
                double vTop = SystemParameters.VirtualScreenTop;
                double vWidth = SystemParameters.VirtualScreenWidth;
                double vHeight = SystemParameters.VirtualScreenHeight;

                // 若虛擬寬度大於主螢幕寬度，代表存在副螢幕
                if (vWidth > SystemParameters.PrimaryScreenWidth || vHeight > SystemParameters.PrimaryScreenHeight)
                {
                    // 在副螢幕範圍開啟覆蓋層
                    var secWin = new SecondaryBlankWindow(vLeft, vTop, vWidth, vHeight);
                    // 確保在主視窗之下但在其他應用程式之上
                    secWin.Show();
                    _secondaryWindows.Add(secWin);

                    // 再次確保主解鎖視窗在最前
                    this.Topmost = true;
                    this.BringIntoView();
                }
            }
            catch
            {
                // 忽略多螢幕邊緣計算例外
            }
        }

        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            ClockText.Text = now.ToString("HH:mm:ss");

            // 依據當前選用語言的文化格式顯示日期
            DateText.Text = now.ToString("D", I18nManager.CurrentCulture);
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UnlockPasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryUnlock();
            }
        }

        private void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {
            TryUnlock();
        }

        private void TryUnlock()
        {
            if (string.IsNullOrEmpty(_password))
            {
                // 免密碼直接解鎖
                PerformUnlock();
                return;
            }

            if (UnlockPasswordBox.Password == _password)
            {
                PerformUnlock();
            }
            else
            {
                ErrorMsgText.Visibility = Visibility.Visible;
                ErrorMsgText.Text = I18nManager.T("ErrorPassword");
                UnlockPasswordBox.SelectAll();
                UnlockPasswordBox.Focus();
            }
        }

        private void PerformUnlock()
        {
            _isUnlocked = true;
            _clockTimer.Stop();
            _trimTimer.Stop();

            // 關閉副螢幕視窗
            foreach (var win in _secondaryWindows)
            {
                try
                {
                    win.Close();
                }
                catch { }
            }
            _secondaryWindows.Clear();

            // 卸載鍵盤鉤子
            NativeMethods.UninstallKeyboardHook();

            // 恢復電源待機設定
            NativeMethods.DisableKeepAwake();

            // 解鎖清理後收縮記憶體
            NativeMethods.TrimMemory();

            // 關閉鎖屏視窗
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_isUnlocked && !string.IsNullOrEmpty(_password))
            {
                if (!UnlockPasswordBox.IsFocused)
                {
                    UnlockPasswordBox.Focus();
                }
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!_isUnlocked)
            {
                // 若鎖定中失去焦點，立即搶回最上層焦點
                this.Topmost = true;
                this.Activate();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isUnlocked)
            {
                // 未解鎖前阻止正常關閉
                e.Cancel = true;
            }
        }
    }
}
