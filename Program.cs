using CapsLockMacros.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Forms;

namespace CapsLockMacros
{
    class Program
    {
        #region block Capslock
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
                    else
                        CapsLockPressed = true;
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
                if (keyLabel == "Ö")
                    key = Keys.Oemtilde;
                else if (keyLabel == "Ä")
                    key = Keys.Oem7;
                else if (keyLabel == "Ü")
                    key = Keys.Oem1;
                else if (keyLabel == "Backspace")
                    key = Keys.Back;
                else if (keyLabel == "Pos1")
                    key = Keys.Home;
                else if (keyLabel == "Del" || keyLabel == "Entf")
                    key = Keys.Delete;
                else
                    return false;
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
                new Macro() { InputKey = "O", OutputKey="Delete" },
                new Macro() { InputKey = "H", OutputKey="Pos1" },
                new Macro() { InputKey = "Ö", OutputKey="End" } };
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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
                Icon = AppIcon,
                Text = "CapsLockMacros"
            };

            var cm = new ContextMenuStrip();
            NI.ContextMenuStrip = cm;
            SetMenu_Activated();

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
        private const string ShowConfigTitle = "Show config file";
        private static readonly Icon AppIcon = Icon.ExtractAssociatedIcon("./Resources/AppIcon.ico");

        private static void Deactivate_Click(object sender, EventArgs e)
        {
            SetMenu_Deactivated();
            NI.Icon = AppIcon;
            UnhookWindowsHookEx(_hookID);
        }

        private static void Activate_Click(object sender, EventArgs e)
        {
            SetMenu_Activated();
            NI.Icon = AppIcon;
            _hookID = SetHook(_proc);
        }

        private static void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static void ShowConfigFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", CONFIG_PATH));
        }

        private static void SetMenu_Activated()
        {
            NI.ContextMenuStrip.Items.Clear();

            //Deactivate
            var item = NI.ContextMenuStrip.Items.Add(DeactivateTitle);
            item.Click += new EventHandler(Deactivate_Click);

            //Show Config file
            item = NI.ContextMenuStrip.Items.Add(ShowConfigTitle);
            item.Click += new EventHandler(ShowConfigFolder_Click);

            //Version
            item = NI.ContextMenuStrip.Items.Add($"Version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            item.Enabled = false;

            //Exit
            item = NI.ContextMenuStrip.Items.Add(ExitTitle);
            item.Click += new EventHandler(Exit_Click);
        }

        private static void SetMenu_Deactivated()
        {
            NI.ContextMenuStrip.Items.Clear();

            // Activate
            var item = NI.ContextMenuStrip.Items.Add(ActivateTitle);
            item.Click += new EventHandler(Activate_Click);

            //Show Config file
            item = NI.ContextMenuStrip.Items.Add(ShowConfigTitle);
            item.Click += new EventHandler(ShowConfigFolder_Click);

            //Version
            item = NI.ContextMenuStrip.Items.Add($"Version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            item.Enabled = false;

            //Exit
            item = NI.ContextMenuStrip.Items.Add(ExitTitle);
            item.Click += new EventHandler(Exit_Click);
        }
        #endregion

        // Show a message box when unhandled exception occurs
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), $"{Assembly.GetExecutingAssembly().GetName().Name} - Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
