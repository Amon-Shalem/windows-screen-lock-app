using System;
using System.IO;
using System.Windows;

namespace CustomScreenLocker
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "CustomScreenLocker");
            string configFilePath = Path.Combine(appFolder, "config.json");
            bool isFirstRun = !File.Exists(configFilePath);

            // 1. 如果是初次啟動，優先獨立彈出語言設定對話框，絕不預先繪製主視窗邊框
            if (isFirstRun)
            {
                var langDialog = new FirstRunLanguageWindow();
                langDialog.ShowDialog();
            }

            // 2. 實例化主視窗 (預設為隱藏狀態，不在工作列顯示)
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;

            // 3. 執行業務邏輯初始化與啟動判定
            mainWindow.InitializeAndStart(isFirstRun);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 確保應用程式結束時解除鎖定狀態與防熄屏設定
            NativeMethods.DisableKeepAwake();
            NativeMethods.UninstallKeyboardHook();
            base.OnExit(e);
        }
    }
}
