using System.Windows.Forms;

namespace iSurround.UIForms
{
    public class HotkeyCaptureDialog : Form
    {
        private Label lblPrompt;
        private Label lblHotkey;
        private Button btnCancel;
        private Keys capturedKey = Keys.None;
        private bool capturing = true;

        public Keys CapturedHotkey => capturedKey;

        public HotkeyCaptureDialog()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += HotkeyCaptureDialog_KeyDown;
            this.FormClosing += (s, e) => { if (capturing) capturedKey = Keys.None; };
            this.Load += (s, e) => this.Activate();
        }

        private void InitializeComponent()
        {
            this.lblPrompt = new Label();
            this.lblHotkey = new Label();
            this.btnCancel = new Button();
            this.SuspendLayout();

            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblPrompt.Text = "Press a key combination (e.g., Ctrl+Alt+F1):";
            this.lblPrompt.ForeColor = System.Drawing.Color.White;

            this.lblHotkey.AutoSize = true;
            this.lblHotkey.Location = new System.Drawing.Point(12, 40);
            this.lblHotkey.Size = new System.Drawing.Size(200, 20);
            this.lblHotkey.Text = "(none)";
            this.lblHotkey.ForeColor = System.Drawing.Color.Yellow;

            this.btnCancel.Location = new System.Drawing.Point(12, 70);
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += (s, e) => { capturedKey = Keys.None; this.Close(); };

            this.ClientSize = new System.Drawing.Size(300, 110);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.lblHotkey);
            this.Controls.Add(this.btnCancel);
            this.Text = "Set Hotkey";
            this.BackColor = System.Drawing.Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void HotkeyCaptureDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (!capturing) return;

            // 忽略单独的修饰键
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu)
                return;

            Keys modifiers = Keys.None;
            if (e.Control) modifiers |= Keys.Control;
            if (e.Shift) modifiers |= Keys.Shift;
            if (e.Alt) modifiers |= Keys.Alt;

            capturedKey = modifiers | e.KeyCode;
            lblHotkey.Text = new KeysConverter().ConvertToString(capturedKey);
            capturing = false;
            e.Handled = true;
            e.SuppressKeyPress = true; // 阻止系统发出叮声
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}