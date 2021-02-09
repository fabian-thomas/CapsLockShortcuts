using System.Windows.Forms;

namespace CapsLockMacros.UI
{
    public partial class KeycodesHelper : Form
    {
        private readonly LowLevelKeyboard LowLevelKeyboard = new LowLevelKeyboard();

        public KeycodesHelper()
        {
            InitializeComponent();

            LowLevelKeyboard.KeyDown += LowLevelKeyboard_KeyDown;
            LowLevelKeyboard.Start();

            FormClosed += KeycodesHelper_FormClosed;
        }


        private void LowLevelKeyboard_KeyDown(LowLevelKeyboard.KeyEventArgs e)
        {
            // update text in textbox
            var text = string.Empty;
            bool firstItem = true;

            foreach (var key in LowLevelKeyboard.DownKeys)
            {
                if (!firstItem)
                    text += " + ";
                else
                    firstItem = false;

                text += key.ToString();
            }

            PressedKeyTB.Text = text;
        }

        private void KeycodesHelper_FormClosed(object sender, FormClosedEventArgs e)
        {
            LowLevelKeyboard.Dispose();
        }

        private void CopyButton_Click(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PressedKeyTB.Text))
                Clipboard.SetText(PressedKeyTB.Text);
        }
    }
}
