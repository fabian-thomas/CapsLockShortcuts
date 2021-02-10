using CapsLockMacros.UI;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CapsLockMacros
{
    class Program
    {
        private static Config Config;
        private static MyNotifyIcon NotifyIcon;
        private static LowLevelKeyboard LowLevelKeyboard;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Config = new Config();

            using (NotifyIcon = new MyNotifyIcon())
            using (LowLevelKeyboard = new LowLevelKeyboard())
            {
                NotifyIcon.PropertyChanged += NotifyIcon_PropertyChanged;
                LowLevelKeyboard.KeyDown += LowLevelKeyboard_KeyDown;
                LowLevelKeyboard.KeyUp += LowLevelKeyboard_KeyUp;

                NotifyIcon.Active = true;
                Application.Run();
            }
        }


        #region shortcuts
        private static void LowLevelKeyboard_KeyDown(LowLevelKeyboard.KeyEventArgs e)
        {
            if (Config.BaseKeys.TrueForAll(key => LowLevelKeyboard.DownKeys.Contains(key)))
            {
                var shortcut = Config.Shortcuts.FirstOrDefault(s => s.InputKey == e.Key);

                if (shortcut != null)
                {
                    LowLevelKeyboard.SendKeyDown(shortcut.OutputKey);
                    e.Cancel = true;
                }
            }
        }

        private static void LowLevelKeyboard_KeyUp(LowLevelKeyboard.KeyEventArgs e)
        {
            if (Config.BaseKeys.TrueForAll(key => LowLevelKeyboard.DownKeys.Contains(key)))
            {
                var shortcut = Config.Shortcuts.FirstOrDefault(s => s.InputKey == e.Key);

                if (shortcut != null)
                {

                    LowLevelKeyboard.SendKeyUp(shortcut.OutputKey);
                    e.Cancel = true;
                }
            }

            // disable CapsLock toggle functionality
            if (Config.DisableCapsLockToggle && e.Key == Keys.CapsLock && Control.IsKeyLocked(Keys.CapsLock))
            {
                var timer = new System.Timers.Timer(1);
                timer.Elapsed += (sender, e) =>
                {
                    if (Control.IsKeyLocked(Keys.CapsLock))
                        LowLevelKeyboard.SendKeyPress(Keys.CapsLock);

                    timer.Dispose();
                };

                timer.Start();
            }

        }
        #endregion

        private static void NotifyIcon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NotifyIcon.Active))
            {
                if (NotifyIcon.Active)
                    LowLevelKeyboard.Start();
                else
                    LowLevelKeyboard.Stop();
            }
        }

        // Show a message box when unhandled exception occurs
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), $"{Constants.APPNAME} - Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
