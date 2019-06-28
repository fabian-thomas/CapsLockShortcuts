using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DisableCapsLock
{
    class Program
    {

        #region CAPSLOCK blockieren

        private const byte VK_CAPSLOCK = 0x14;
        private const uint KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;

        private const int WM_KEYUP = 0x101;

        private const int WH_KEYBOARD_LL = 13;

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {

                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                if (key == Keys.CapsLock && Control.IsKeyLocked(Keys.CapsLock))
                {
                    Timer.Start();
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam); ;
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            Timer.Stop();
        }

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


        private static Timer Timer = new Timer
        {
            Interval = 1,
        };

        static void Main(string[] args)
        {
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
                keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }

            Timer.Tick += Timer_Tick;

            #region NotifyIcon

            NI = new NotifyIcon
            {
                Icon = AppIconEnabled,
                Text = "DisableCapsLock"
            };

            var cm = new ContextMenu();
            NI.ContextMenu = cm;
            cm.MenuItems.Add(DeactivateMI);
            cm.MenuItems.Add(ExitMI);

            NI.Visible = true;

            #endregion

            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);

            NI.Visible = false;
            NI.Dispose();
        }

        #region NotifyIcon

        private static NotifyIcon NI;
        private static readonly MenuItem DeactivateMI = new MenuItem("Deaktivieren", new EventHandler(NI_ContextMenu_Deactivate_Click));
        private static readonly MenuItem ActivateMI = new MenuItem("Aktivieren", new EventHandler(NI_ContextMenu_Activate_Click));
        private static readonly MenuItem ExitMI = new MenuItem("Beenden", new EventHandler(NI_ContextMenu_Exit_Click));
        private static readonly Icon AppIconEnabled = Icon.ExtractAssociatedIcon("./Resources/AppIconEnabled.ico");
        private static readonly Icon AppIconDisabled = Icon.ExtractAssociatedIcon("./Resources/AppIconDisabled.ico");

        private static void NI_ContextMenu_Deactivate_Click(object sender, EventArgs e)
        {
            NI.ContextMenu.MenuItems.Clear();
            NI.ContextMenu.MenuItems.Add(ActivateMI);
            NI.ContextMenu.MenuItems.Add(ExitMI);
            NI.Icon = AppIconDisabled;
            UnhookWindowsHookEx(_hookID);
        }

        private static void NI_ContextMenu_Activate_Click(object sender, EventArgs e)
        {
            NI.ContextMenu.MenuItems.Clear();
            NI.ContextMenu.MenuItems.Add(DeactivateMI);
            NI.ContextMenu.MenuItems.Add(ExitMI);
            NI.Icon = AppIconEnabled;
            _hookID = SetHook(_proc);
        }

        private static void NI_ContextMenu_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}