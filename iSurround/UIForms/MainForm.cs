using iSurround.Services;
using iSurroundShared;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace iSurround.UIForms
{
    public partial class MainForm : Form
    {
        private bool allowVisible;
        private bool allowClose;

        // 控件声明
        private Button btn_setup_display_profiles;
        private Button btn_exit;
        private Label lbl_version;
        private CheckBox cb_minimise_notification_area;
        private CheckBox cb_auto_start;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip mainContextMenuStrip;
        private ToolStripMenuItem toolStripMenuItemHeading;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem openApplicationWindowToolStripMenuItem;
        private ToolStripMenuItem profileToolStripMenuItem;
        private ToolStripMenuItem profilesToolStripMenuItemHeading;
        private ToolStripSeparator profileToolStripSeparator;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;

        public MainForm()
        {
            InitializeComponent();
            InitializeLogic();
        }

        private void InitializeComponent()
        {
            // 窗体设置
            this.Text = "iSurround";
            this.Icon = Properties.Resources.iSurround;   // 如果您的资源名是 AppIcon 
            this.Size = new Size(420, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.Black;
            this.ShowIcon = true;

            // 按钮：Display Profiles
            btn_setup_display_profiles = new Button
            {
                BackColor = Color.Brown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                Location = new Point(30, 25),
                Size = new Size(150, 35),
                Text = "&Display Profiles",
                UseVisualStyleBackColor = false
            };
            btn_setup_display_profiles.Click += BtnSetupDisplayProfiles_Click;

            // 按钮：Exit
            btn_exit = new Button
            {
                BackColor = Color.Brown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                Location = new Point(230, 25),
                Size = new Size(100, 35),
                Text = "&Exit",
                UseVisualStyleBackColor = false
            };
            btn_exit.Click += BtnExit_Click;

            // 复选框：Minimise to system tray
            cb_minimise_notification_area = new CheckBox
            {
                ForeColor = Color.White,
                Location = new Point(30, 80),
                Size = new Size(180, 20),
                Text = "Minimise to system tray",
                UseVisualStyleBackColor = true
            };
            cb_minimise_notification_area.CheckedChanged += CbMinimise_CheckedChanged;

            // 复选框：Auto Start
            cb_auto_start = new CheckBox
            {
                ForeColor = Color.White,
                Location = new Point(30, 105),
                Size = new Size(120, 20),
                Text = "Auto Start",
                UseVisualStyleBackColor = true
            };
            cb_auto_start.CheckedChanged += CbAutoStart_CheckedChanged;

            // 版本标签
            lbl_version = new Label
            {
                ForeColor = Color.White,
                Location = new Point(30, 135),
                Size = new Size(200, 20),
                Text = "Version: {0}",
                AutoSize = false
            };

            // 托盘图标
            notifyIcon = new NotifyIcon();
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            // 上下文菜单
            mainContextMenuStrip = new ContextMenuStrip();
            toolStripMenuItemHeading = new ToolStripMenuItem("iSurround") { Enabled = false };
            toolStripSeparator = new ToolStripSeparator();
            openApplicationWindowToolStripMenuItem = new ToolStripMenuItem("&Open");
            openApplicationWindowToolStripMenuItem.Click += OpenApplicationWindowToolStripMenuItem_Click;
            profileToolStripMenuItem = new ToolStripMenuItem("&Profiles");
            profilesToolStripMenuItemHeading = new ToolStripMenuItem("Display Profiles") { Enabled = false, Font = new Font(mainContextMenuStrip.Font, FontStyle.Italic) };
            profileToolStripSeparator = new ToolStripSeparator();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem("E&xit");
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;

            profileToolStripMenuItem.DropDownItems.Add(profilesToolStripMenuItemHeading);
            profileToolStripMenuItem.DropDownItems.Add(profileToolStripSeparator);
            mainContextMenuStrip.Items.Add(toolStripMenuItemHeading);
            mainContextMenuStrip.Items.Add(toolStripSeparator);
            mainContextMenuStrip.Items.Add(openApplicationWindowToolStripMenuItem);
            mainContextMenuStrip.Items.Add(profileToolStripMenuItem);
            mainContextMenuStrip.Items.Add(toolStripSeparator1);
            mainContextMenuStrip.Items.Add(exitToolStripMenuItem);

            notifyIcon.ContextMenuStrip = mainContextMenuStrip;

            // 添加控件到窗体
            this.Controls.Add(btn_setup_display_profiles);
            this.Controls.Add(btn_exit);
            this.Controls.Add(cb_minimise_notification_area);
            this.Controls.Add(cb_auto_start);
            this.Controls.Add(lbl_version);
        }

        private void InitializeLogic()
        {
            // 版本号显示
            lbl_version.Text = string.Format(lbl_version.Text, Program.AppVersion);

            // 从设置加载状态
            allowVisible = !SettingsService.MinimiseOnStart;
            allowClose = allowVisible;

            cb_minimise_notification_area.Checked = SettingsService.MinimiseOnStart;
            cb_auto_start.Checked = SettingsService.StartOnBootUp;

            btn_exit.Text = SettingsService.MinimiseOnStart ? "&Close" : "&Exit";

            // 托盘图标
            notifyIcon.Icon = Properties.Resources.iSurround;
            notifyIcon.Visible = true;

            // 加载配置文件菜单
            RefreshNotifyIconMenus();

            // 如果需要显示主窗口，激活窗体
            if (allowVisible)
            {
                this.Load += (s, e) =>
                {
                    this.Activate();
                    this.BringToFront();
                };
            }
        }

        // 重写 SetVisibleCore 以实现最小化到托盘
        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        // 重写 OnFormClosing 以实现点关闭时的托盘行为
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                this.Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        // 刷新托盘配置文件菜单
        public void RefreshNotifyIconMenus()
        {
            while (profileToolStripMenuItem.DropDownItems.Count > 2)
                profileToolStripMenuItem.DropDownItems.RemoveAt(2);

            foreach (var profile in ProfileRepository.AllProfiles.OrderBy(p => p.Name))
            {
                var item = new ToolStripMenuItem(profile.Name, profile.ProfileBitmap, (s, e) =>
                {
                    Program.ApplyProfileTask(profile);
                    RefreshNotifyIconMenus();
                });
                profileToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        // 打开 Display Profiles 窗口
        private void BtnSetupDisplayProfiles_Click(object sender, EventArgs e)
        {
            var existing = Application.OpenForms.OfType<DisplayProfileForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                existing.BringToFront();
            }
            else
            {
                var form = new DisplayProfileForm();
                form.ShowDialog(this);
            }
        }

        // 退出按钮点击
        private void BtnExit_Click(object sender, EventArgs e)
        {
            if (cb_minimise_notification_area.Checked)
            {
                this.Hide();
            }
            else
            {
                allowClose = true;
                Application.Exit();
            }
        }

        // 最小化复选框
        private void CbMinimise_CheckedChanged(object sender, EventArgs e)
        {
            bool minimise = cb_minimise_notification_area.Checked;
            allowVisible = !minimise;
            allowClose = !minimise;
            btn_exit.Text = minimise ? "&Close" : "&Exit";

            SettingsService.MinimiseOnStart = minimise;
            SettingsService.Save();
        }

        // 自启动复选框
        private void CbAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            bool enable = cb_auto_start.Checked;
            StartupService.SetAutoStart(enable);

            SettingsService.StartOnBootUp = enable;
            SettingsService.Save();
        }

        // 托盘菜单：打开主窗口
        private void OpenApplicationWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowVisible = true;
            this.Show();
            this.Activate();
            this.BringToFront();
        }

        // 托盘菜单：退出
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowClose = true;
            Application.Exit();
        }

        // 托盘图标双击
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenApplicationWindowToolStripMenuItem_Click(sender, e);
        }

        // 外部调用更新托盘文字（如切换配置文件后）
        public void UpdateNotifyIconText(string text)
        {
            if (notifyIcon != null)
            {
                string shortText = text.Length >= 64 ? text.Substring(0, 45) : text;
                notifyIcon.Text = shortText;
            }
        }
    }
}