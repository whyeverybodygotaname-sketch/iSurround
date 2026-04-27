using iSurround.Services;
using iSurroundShared;
using iSurroundShared.UserControls;
using iSurroundShared.Windows;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace iSurround.UIForms
{
    public partial class DisplayProfileForm : Form
    {
        private ListBox lstProfiles;
        private TextBox txtName;
        private TextBox txtHotkey;
        private Button btnAdd, btnRename, btnUpdate, btnDelete, btnApply;
        private Button btnSetHotkey, btnClearHotkey;
        private Label lblInstructions;
        private DisplayView displayView;
        private ProfileItem selectedProfile;

        public DisplayProfileForm()
        {
            InitializeComponent();
            SetupUI();
            RefreshProfileList();
        }

        private void InitializeComponent()
        {
            this.Text = "Display Profiles";
            this.Size = new Size(800, 750);   // 高度增加到 800
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources.iSurround;
        }

        private void SetupUI()
        {
            // 预览图 (顶部，横跨窗体)
            displayView = new DisplayView
            {
                Location = new Point(20, 20),
                //Size = new Size(760, 280),    // 宽度 580，高度 280改用anchor
                Height = 280,
                BackColor = Color.FromArgb(20, 20, 20),
                PaddingX = 50,                // 适当增加内边距，让布局更宽松
                PaddingY = 50,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            this.Load += (s, e) => {
                displayView.Width = this.ClientSize.Width - 40;
            };

            // 左侧列表 (位于预览图下方)
            lstProfiles = new ListBox
            {
                Location = new Point(20, 320),    // Y = 预览图底部 20+280=300，再加 20 间距
                Size = new Size(220, 380),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstProfiles.SelectedIndexChanged += LstProfiles_SelectedIndexChanged;
            lstProfiles.ScrollAlwaysVisible = true;

            // Name 标签及文本框 (右侧)
            var lblName = new Label
            {
                Text = "Name:",
                Location = new Point(260, 320),
                Size = new Size(50, 20),
                ForeColor = Color.White
            };

            txtName = new TextBox
            {
                Location = new Point(260, 345),
                Size = new Size(280, 25),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            // 第一行按钮：Add, Rename
            btnAdd = new Button
            {
                Text = "Add",
                Location = new Point(260, 390),
                Size = new Size(130, 35),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;

            btnRename = new Button
            {
                Text = "Rename",
                Location = new Point(410, 390),
                Size = new Size(130, 35),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRename.Click += BtnRename_Click;

            // 第二行按钮：Update, Delete
            btnUpdate = new Button
            {
                Text = "Update",
                Location = new Point(260, 435),
                Size = new Size(130, 35),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(410, 435),
                Size = new Size(130, 35),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;

            // 第三行按钮：Apply
            btnApply = new Button
            {
                Text = "Apply",
                Location = new Point(260, 480),
                Size = new Size(280, 35),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.Click += BtnApply_Click;

            // 热键区域
            var lblHotkeyLabel = new Label
            {
                Text = "Hotkey:",
                Location = new Point(260, 530),
                Size = new Size(60, 20),
                ForeColor = Color.White
            };

            txtHotkey = new TextBox
            {
                Location = new Point(260, 555),
                Size = new Size(155, 25),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                ReadOnly = true,
                Text = "None"
            };

            btnSetHotkey = new Button
            {
                Text = "Set",
                Location = new Point(435, 555),
                Size = new Size(50, 27),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSetHotkey.Click += BtnSetHotkey_Click;

            btnClearHotkey = new Button
            {
                Text = "Clear",
                Location = new Point(490, 555),
                Size = new Size(50, 27),
                BackColor = Color.Brown,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClearHotkey.Click += BtnClearHotkey_Click;

            // 说明标签
            lblInstructions = new Label
            {
                Text = "Select a profile.\nAdd      = save current display as new profile.\nRename  = change selected profile's name.\nUpdate  = overwrite selected profile with current display.\nDelete   = remove selected profile.\nApply    = switch to selected profile immediately.\nHotkey   = assign a global hotkey to switch to this profile.",
                Location = new Point(260, 595),
                Size = new Size(300, 130),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };

            this.Controls.Add(displayView);
            this.Controls.Add(lstProfiles);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnRename);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnApply);
            this.Controls.Add(lblHotkeyLabel);
            this.Controls.Add(txtHotkey);
            this.Controls.Add(btnSetHotkey);
            this.Controls.Add(btnClearHotkey);
            this.Controls.Add(lblInstructions);
        }

        private void RefreshProfileList()
        {
            lstProfiles.Items.Clear();
            foreach (var profile in ProfileRepository.AllProfiles.OrderBy(p => p.Name))
            {
                lstProfiles.Items.Add(profile.Name);
            }
            selectedProfile = null;
            txtName.Text = "";
            txtHotkey.Text = "None";
            displayView.Profile = null;
            btnRename.Enabled = false;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnApply.Enabled = false;
            btnSetHotkey.Enabled = false;
            btnClearHotkey.Enabled = false;
        }

        private void LstProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstProfiles.SelectedIndex >= 0)
            {
                string name = lstProfiles.SelectedItem.ToString();
                selectedProfile = ProfileRepository.AllProfiles.FirstOrDefault(p => p.Name == name);
                if (selectedProfile != null)
                {
                    txtName.Text = selectedProfile.Name;
                    txtHotkey.Text = selectedProfile.Hotkey == Keys.None ? "None" : new KeysConverter().ConvertToString(selectedProfile.Hotkey);
                    displayView.Profile = selectedProfile;
                    btnRename.Enabled = true;
                    btnUpdate.Enabled = true;
                    btnDelete.Enabled = true;
                    btnApply.Enabled = true;
                    btnSetHotkey.Enabled = true;
                    btnClearHotkey.Enabled = true;
                }
            }
            else
            {
                selectedProfile = null;
                txtName.Text = "";
                txtHotkey.Text = "None";
                displayView.Profile = null;
                btnRename.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                btnApply.Enabled = false;
                btnSetHotkey.Enabled = false;
                btnClearHotkey.Enabled = false;
            }
        }

        // 以下所有按钮事件方法与原版完全相同（省略，实际使用时保留）
        // 注意：如果您的原文件中有这些方法的完整实现，请将它们保留在下面的位置。
        // 由于篇幅，此处只列出方法签名，您需要将原有的实现粘贴过来。
        // 为了避免遗漏，请在替换后手动将您原有的 BtnAdd_Click, BtnRename_Click, BtnUpdate_Click,
        // BtnDelete_Click, BtnApply_Click, BtnSetHotkey_Click, BtnClearHotkey_Click 方法体复制到对应位置。


        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string desiredName = txtName.Text.Trim();
            string finalName;

            if (string.IsNullOrWhiteSpace(desiredName))
            {
                finalName = ProfileService.MakeUniqueName(NamingService.GenerateAutoName());
            }
            else
            {
                if (ProfileRepository.AllProfiles.Any(p => p.Name.Equals(desiredName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A profile with that name already exists. Please choose a different name or leave blank for auto‑naming.",
                        "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                finalName = desiredName;
            }

            var newProfile = new ProfileItem();
            newProfile.CreateProfileFromCurrentDisplaySettings(false);
            newProfile.Name = finalName;

            if (!DisplayValidationService.IsValidForSave(newProfile, out bool isSurroundOrEyefinity))
            {
                MessageBox.Show("The current display configuration cannot be saved as a profile.\nEnsure that all displays are properly connected and configured.",
                    "Invalid Display Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ProfileService.TryAddProfile(newProfile, out string errorMsg))
            {
                MessageBox.Show($"Profile '{finalName}' saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshProfileList();
                int idx = lstProfiles.Items.IndexOf(finalName);
                if (idx >= 0) lstProfiles.SelectedIndex = idx;
                txtName.Text = finalName;
                Program.RefreshHotkeys();
            }
            else
            {
                MessageBox.Show(errorMsg, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null) return;
            string newName = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Please enter a name for the profile.", "Name required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (ProfileRepository.AllProfiles.Any(p => p != selectedProfile && p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Another profile already has that name. Please choose a different name.",
                    "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            selectedProfile.Name = newName;
            ProfileRepository.SaveProfiles();
            MessageBox.Show($"Profile renamed to '{newName}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshProfileList();
            int idx = lstProfiles.Items.IndexOf(newName);
            if (idx >= 0) lstProfiles.SelectedIndex = idx;
            Program.RefreshHotkeys();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null) return;
            string originalName = selectedProfile.Name;
            ProfileRepository.CopyCurrentLayoutToProfile(selectedProfile);
            ProfileRepository.SaveProfiles();
            MessageBox.Show($"Profile '{originalName}' updated with current display settings.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshProfileList();
            int idx = lstProfiles.Items.IndexOf(originalName);
            if (idx >= 0) lstProfiles.SelectedIndex = idx;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null) return;
            if (MessageBox.Show($"Delete profile '{selectedProfile.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (ProfileRepository.RemoveProfile(selectedProfile))
                {
                    RefreshProfileList();
                    Program.RefreshHotkeys();
                }
                else
                {
                    MessageBox.Show("Failed to delete profile.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null)
            {
                MessageBox.Show("Please select a profile to apply.", "No profile selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = Program.ApplyProfileTask(selectedProfile);
            if (result == ApplyProfileResult.Successful)
            {
                MessageBox.Show($"Profile '{selectedProfile.Name}' applied successfully.", "Apply Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (result == ApplyProfileResult.Cancelled)
            {
                MessageBox.Show("Application of profile was cancelled.", "Apply Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show($"Failed to apply profile '{selectedProfile.Name}'.", "Apply Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSetHotkey_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null) return;

            Program.IsCapturingHotkey = true;

            try
            {
                using (var dialog = new HotkeyCaptureDialog())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Keys newHotkey = dialog.CapturedHotkey;
                        if (newHotkey != Keys.None)
                        {
                            if (HotkeyService.IsHotkeyConflict(selectedProfile, newHotkey))
                            {
                                return;
                            }

                            ProfileService.UpdateProfileHotkey(selectedProfile.UUID, newHotkey);
                            txtHotkey.Text = new KeysConverter().ConvertToString(newHotkey);
                            Program.RefreshHotkeys();
                            MessageBox.Show($"热键已设置为 {txtHotkey.Text}", "热键", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            selectedProfile = ProfileRepository.AllProfiles.FirstOrDefault(p => p.UUID == selectedProfile.UUID);
                        }
                    }
                }
            }
            finally
            {
                Program.IsCapturingHotkey = false;
            }
        }

        private void BtnClearHotkey_Click(object sender, EventArgs e)
        {
            if (selectedProfile == null) return;
            selectedProfile.Hotkey = Keys.None;
            ProfileRepository.SaveProfiles();
            txtHotkey.Text = "None";
            Program.RefreshHotkeys();
            MessageBox.Show("Hotkey cleared.", "Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

