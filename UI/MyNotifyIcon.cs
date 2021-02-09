using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CapsLockMacros.UI
{
    public class MyNotifyIcon : IDisposable, INotifyPropertyChanged
    {
        #region string resources

        private const string StatusText = "Active";
        private const string ShowConfigText = "Config";
        private const string KeycodesHelperText = "Keycodes";
        private const string ExitText = "Exit";

        #endregion

        #region observable properties

        private bool active;
        public bool Active
        {
            get => active;
            set
            {
                if (value != active)
                {
                    active = value;
                    ToggleActiveItem.Checked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        #endregion


        private readonly NotifyIcon NotifyIcon = new NotifyIcon { Text = Constants.APPNAME };
        private readonly ToolStripMenuItem ToggleActiveItem;

        public event PropertyChangedEventHandler PropertyChanged;


        public MyNotifyIcon()
        {
            NotifyIcon.Text = AppDomain.CurrentDomain.FriendlyName;
            NotifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            // add items
            var cms = new ContextMenuStrip();

            ToggleActiveItem = new ToolStripMenuItem(StatusText)
            {
                CheckOnClick = true,
                Checked = Active
            };
            ToggleActiveItem.CheckedChanged += ToggleActive_CheckedChanged;
            cms.Items.Add(ToggleActiveItem);

            cms.Items.Add(new ToolStripSeparator());

            var keycodesHelperItem = cms.Items.Add(KeycodesHelperText);
            keycodesHelperItem.Click += KeycodesHelperItem_Click;

            var showConfigItem = cms.Items.Add(ShowConfigText);
            showConfigItem.Click += ShowConfig_Click;

            cms.Items.Add(new ToolStripSeparator());

            var exitItem = cms.Items.Add(ExitText);
            exitItem.Click += Exit_Click;

            NotifyIcon.ContextMenuStrip = cms;

            NotifyIcon.Visible = true;
        }

        private void ToggleActive_CheckedChanged(object sender, EventArgs e)
        {
            Active = ToggleActiveItem.Checked;
        }

        private void KeycodesHelperItem_Click(object sender, EventArgs e)
        {
            new KeycodesHelper().ShowDialog();
        }

        private void ShowConfig_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", $"/select,\"{Constants.CONFIG_PATH}\"");
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void Dispose()
        {
            NotifyIcon.Dispose();
        }
    }
}
