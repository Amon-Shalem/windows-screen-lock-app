using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CustomScreenLocker
{
    public static class NativeMethods
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private static bool _isAwakeEnabled = false;

        public static void EnableKeepAwake()
        {
            try
            {
                // 保持螢幕常亮及系統喚醒狀態
                EXECUTION_STATE state = SetThreadExecutionState(
                    EXECUTION_STATE.ES_CONTINUOUS |
                    EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                    EXECUTION_STATE.ES_SYSTEM_REQUIRED
                );
                _isAwakeEnabled = (state != 0);
            }
            catch
            {
                _isAwakeEnabled = false;
            }
        }

        public static void DisableKeepAwake()
        {
            try
            {
                // 恢復正常省電與休眠狀態
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                _isAwakeEnabled = false;
            }
            catch
            {
                // 忽略例外
            }
        }

        public static bool IsAwakeEnabled => _isAwakeEnabled;

        #region 記憶體極致輕量化修剪 (Working Set Trimming)

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public static void TrimMemory()
        {
            try
            {
                // 強制觸發第 2 代完整垃圾回收，並等待終結器完成
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true, true);

                // 通知 Windows 核心收縮進程的工作集，將不常用的記憶體頁面立即釋放
                using var currentProcess = Process.GetCurrentProcess();
                EmptyWorkingSet(currentProcess.Handle);
            }
            catch
            {
                // 忽略修剪例外
            }
        }

        #endregion

        #region 低階鍵盤鉤子 (Low-Level Keyboard Hook)

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int VK_TAB = 0x09;
        private const int VK_ESCAPE = 0x1B;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_F4 = 0x73;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc? _proc;
        private static IntPtr _hookId = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void InstallKeyboardHook()
        {
            if (_hookId == IntPtr.Zero)
            {
                _proc = HookCallback;
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                if (curModule != null)
                {
                    _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName!), 0);
                }
            }
        }

        public static void UninstallKeyboardHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
                _proc = null;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                bool isAlt = (kb.flags & 0x20) != 0;

                // 攔截 Win 鍵
                if (kb.vkCode == VK_LWIN || kb.vkCode == VK_RWIN)
                {
                    return (IntPtr)1;
                }

                // 攔截 Alt + Tab
                if (isAlt && kb.vkCode == VK_TAB)
                {
                    return (IntPtr)1;
                }

                // 攔截 Alt + Esc
                if (isAlt && kb.vkCode == VK_ESCAPE)
                {
                    return (IntPtr)1;
                }

                // 攔截 Ctrl + Esc
                bool isCtrl = (GetKeyState(0x11) & 0x8000) != 0;
                if (isCtrl && kb.vkCode == VK_ESCAPE)
                {
                    return (IntPtr)1;
                }

                // 攔截 Alt + F4 防止被快捷關閉
                if (isAlt && kb.vkCode == VK_F4)
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        #endregion

        #region Windows 開機自啟動管理 (Auto Start on Boot)

        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryName = "CustomScreenLocker";

        public static bool IsAutoStartEnabled()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                if (key == null) return false;

                var value = key.GetValue(AppRegistryName) as string;
                if (string.IsNullOrEmpty(value)) return false;

                string currentExe = Environment.ProcessPath ?? string.Empty;
                if (string.IsNullOrEmpty(currentExe)) return false;

                return value.Contains(currentExe, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool SetAutoStart(bool enable)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null) return false;

                if (enable)
                {
                    string currentExe = Environment.ProcessPath ?? string.Empty;
                    if (string.IsNullOrEmpty(currentExe)) return false;

                    string command = $"\"{currentExe}\"";
                    key.SetValue(AppRegistryName, command);
                }
                else
                {
                    key.DeleteValue(AppRegistryName, false);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
