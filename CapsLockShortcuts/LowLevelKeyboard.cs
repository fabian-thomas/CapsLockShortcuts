using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapsLockMacros
{
    class LowLevelKeyboard : IDisposable
    {
        #region constants
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        private const int WM_KEYUP = 0x101;
        private const int WM_KEYDOWN = 0x0100;

        private const uint KEYEVENTF_EXTENDEDKEY = 1;
        private const int WH_KEYBOARD_LL = 13;
        #endregion

        #region events
        public delegate void KeyEventHandler(KeyEventArgs e);
        public class KeyEventArgs
        {
            public readonly Keys Key;
            public bool Cancel { get; set; }

            public KeyEventArgs(Keys key)
            {
                Key = key;
            }
        }

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        #endregion

        #region Properties
        public readonly HashSet<Keys> DownKeys = new HashSet<Keys>();
        #endregion

        #region send key presses
        public void SendKeyDown(Keys key)
        {
            keybd_event((byte)key, 0x0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
        }

        public void SendKeyUp(Keys key)
        {
            keybd_event((byte)key, 0x0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public void SendKeyPress(Keys key)
        {
            SendKeyDown(key);
            SendKeyUp(key);
        }
        #endregion

        #region global keyboard hook
        private LowLevelKeyboardProc HookCallbackProc;
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                var eventArgs = new KeyEventArgs(key);

                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)260)
                {
                    DownKeys.Add(key);
                    KeyDown?.Invoke(eventArgs);
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    DownKeys.Remove(key);
                    KeyUp?.Invoke(eventArgs);
                }

                if (eventArgs.Cancel)
                    return new IntPtr(1);
            }

            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }

        private IntPtr HookID = IntPtr.Zero;
        private void SetHook()
        {
            if (HookID != IntPtr.Zero)
                return;

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                HookCallbackProc = HookCallback;
                HookID = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallbackProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void Unhook()
        {
            if (HookID == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(HookID);
            HookID = IntPtr.Zero;
        }
        #endregion

        #region disposal
        public void Start()
        {
            SetHook();
        }

        public void Stop()
        {
            Unhook();
        }

        public void Dispose()
        {
            Stop();
        }
        #endregion

        #region dll imports
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        #endregion
    }
}
