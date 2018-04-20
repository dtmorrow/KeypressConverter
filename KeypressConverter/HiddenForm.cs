using System;
using System.Windows.Forms;

namespace KeypressConverter
{
    public partial class HiddenForm : Form
    {
        public HiddenForm()
        {
            InitializeComponent();
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            KeyboardHook.SetKeyboardHook();
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeyboardHook.UnsetKeyboardHook();
        }
    }
}
