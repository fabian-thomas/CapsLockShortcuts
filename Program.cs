using CapsLockMacros.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Forms;

namespace CapsLockMacros
{
    class Program
    {
        #region block CAPSLOCK
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        private const int WM_KEYUP = 0x101;
        private const int WM_KEYDOWN = 0x0100;

        private const uint KEYEVENTF_EXTENDEDKEY = 1;
        private const int WH_KEYBOARD_LL = 13;

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static bool CapsLockPressed = false;

        private static void SimulateKeyPress(IntPtr wParam, byte key)
        {
            if (wParam == (IntPtr)WM_KEYDOWN)
                keybd_event(key, 0x0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
            else if (wParam == (IntPtr)WM_KEYUP)
                keybd_event(key, 0x0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                if (key == Keys.CapsLock)
                {
                    if (wParam == (IntPtr)WM_KEYUP)
                    {
                        CapsLockPressed = false;
                        if (Control.IsKeyLocked(Keys.CapsLock))
                            Timer.Start();
                    }
                    else CapsLockPressed = true;
                }
                else if (CapsLockPressed)
                    foreach (var macro in Config)
                    {
                        Keys macroKey;
                        if (TryParseKey(macro.InputKey, out macroKey))
                        {
                            if (key == macroKey)
                            {
                                Keys keyToPress;
                                if (TryParseKey(macro.OutputKey, out keyToPress))
                                {
                                    var U = (byte)keyToPress;
                                    SimulateKeyPress(wParam, (byte)keyToPress);
                                    return new IntPtr(1);
                                }
                            }
                        }
                    }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool TryParseKey(string keyLabel, out Keys key)
        {
            if (!Enum.TryParse(keyLabel, out key))
            {
                if (keyLabel == "Ö" || keyLabel == "ö")
                    key = Keys.Oemtilde;
                else if (keyLabel == "Ä" || keyLabel == "ä")
                    key = Keys.Oem7;
                else if (keyLabel == "Ü" || keyLabel == "ü")
                    key = Keys.Oem1;
                else if (keyLabel == "Backspace")
                    key = Keys.Back;
                else if (keyLabel == "Pos1")
                    key = Keys.Home;
                else if (keyLabel == "Del")
                    key = Keys.Delete;
                else return false;
            }
            return true;
        }

        private static Timer Timer = new Timer
        {
            Interval = 1,
        };

        private static void Timer_Tick(object sender, EventArgs e)
        {
            keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
            keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

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

        #region config
        private const string CONFIG_PATH = "config.json";

        private readonly static List<Macro> DefaultConfig = new List<Macro>() {
                new Macro() { InputKey = "J", OutputKey = "Left" },
                new Macro() { InputKey = "I", OutputKey = "Up" },
                new Macro() { InputKey = "L", OutputKey = "Right" },
                new Macro() { InputKey = "K", OutputKey = "Down" },
                new Macro() { InputKey = "U", OutputKey = "Backspace" },
                new Macro() { InputKey="H", OutputKey="Pos1" },
                new Macro() { InputKey="Ö", OutputKey="End" },
                new Macro() { InputKey="O", OutputKey="Delete" }};
        private static List<Macro> Config;

        private static void WriteDefaultConfig()
        {
            File.WriteAllText(CONFIG_PATH, JsonSerializer.Serialize(
                DefaultConfig,
                new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                }));
        }
        #endregion

        static void Main(string[] args)
        {
            if (!File.Exists(CONFIG_PATH))
                WriteDefaultConfig();

            Config = JsonSerializer.Deserialize<List<Macro>>(File.ReadAllText(CONFIG_PATH));

            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
                keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }

            Timer.Tick += Timer_Tick;

            #region NotifyIcon

            NI = new NotifyIcon
            {
                Icon = AppIconEnabled,
                Text = "CapsLockMacros"
            };

            var cm = new ContextMenuStrip();
            NI.ContextMenuStrip = cm;
            var item = NI.ContextMenuStrip.Items.Add(DeactivateTitle);
            item.Click += new EventHandler(NI_ContextMenu_Deactivate_Click);
            item = NI.ContextMenuStrip.Items.Add(ExitTitle);
            item.Click += new EventHandler(NI_ContextMenu_Exit_Click);

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
        private const string DeactivateTitle = "Deactivate";
        private const string ActivateTitle = "Activate";
        private const string ExitTitle = "Exit";
        private static readonly Icon AppIconEnabled = Icon.ExtractAssociatedIcon("./Resources/AppIconEnabled.ico");
        private static readonly Icon AppIconDisabled = Icon.ExtractAssociatedIcon("./Resources/AppIconDisabled.ico");

        private static void NI_ContextMenu_Deactivate_Click(object sender, EventArgs e)
        {
            NI.ContextMenuStrip.Items.Clear();
            var item = NI.ContextMenuStrip.Items.Add(ActivateTitle);
            item.Click += new EventHandler(NI_ContextMenu_Activate_Click);
            item = NI.ContextMenuStrip.Items.Add(ExitTitle);
            item.Click += new EventHandler(NI_ContextMenu_Exit_Click);
            NI.Icon = AppIconDisabled;
            UnhookWindowsHookEx(_hookID);
        }

        private static void NI_ContextMenu_Activate_Click(object sender, EventArgs e)
        {
            NI.ContextMenuStrip.Items.Clear();
            var item = NI.ContextMenuStrip.Items.Add(DeactivateTitle);
            item.Click += new EventHandler(NI_ContextMenu_Deactivate_Click);
            item = NI.ContextMenuStrip.Items.Add(ExitTitle);
            item.Click += new EventHandler(NI_ContextMenu_Exit_Click);
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
