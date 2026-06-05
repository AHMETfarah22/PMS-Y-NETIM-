using System;
using System.Drawing;
using System.Windows.Forms;
using PmsSystem.Helpers;

namespace PmsSystem.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtUsername, txtFullName, txtEmail, txtPhone, txtPassword, txtConfirmPass;
        private Button btnRegister;
        private Label lblStatus;

        public RegisterForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "PMS - Yeni Hesap Oluştur";
            this.Size = new Size(480, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 32, 68);

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 15,
                Padding = new Padding(30, 20, 30, 20)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.Controls.Add(tlpMain);

            var lblTitle = new Label
            {
                Text = "🏨 Yeni Hesap Oluştur",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 20)
            };
            tlpMain.Controls.Add(lblTitle, 0, 0);

            tlpMain.Controls.Add(CreateLabel("👤  Ad Soyad *"), 0, 1);
            txtFullName = CreateTextBox(false);
            tlpMain.Controls.Add(txtFullName, 0, 2);

            tlpMain.Controls.Add(CreateLabel("🔑  Kullanıcı Adı *"), 0, 3);
            txtUsername = CreateTextBox(false);
            tlpMain.Controls.Add(txtUsername, 0, 4);

            tlpMain.Controls.Add(CreateLabel("📧  E-Posta"), 0, 5);
            txtEmail = CreateTextBox(false);
            tlpMain.Controls.Add(txtEmail, 0, 6);

            tlpMain.Controls.Add(CreateLabel("📱  Telefon"), 0, 7);
            txtPhone = CreateTextBox(false);
            tlpMain.Controls.Add(txtPhone, 0, 8);

            tlpMain.Controls.Add(CreateLabel("🔒  Şifre *"), 0, 9);
            txtPassword = CreateTextBox(true);
            tlpMain.Controls.Add(txtPassword, 0, 10);

            tlpMain.Controls.Add(CreateLabel("🔒  Şifre Tekrar *"), 0, 11);
            txtConfirmPass = CreateTextBox(true);
            tlpMain.Controls.Add(txtConfirmPass, 0, 12);

            btnRegister = new Button
            {
                Text = "HESAP OLUŞTUR",
                Dock = DockStyle.Fill,
                Height = 45,
                BackColor = Color.FromArgb(218, 165, 32),
                ForeColor = Color.FromArgb(10, 18, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 0, 10)
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            tlpMain.Controls.Add(btnRegister, 0, 13);

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tlpMain.Controls.Add(lblStatus, 0, 14);

            btnRegister.Click += BtnRegister_Click;
            btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = Color.FromArgb(195, 145, 25);
            btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = Color.FromArgb(218, 165, 32);
            this.AcceptButton = btnRegister;
        }

        private Label CreateLabel(string labelText)
        {
            return new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
        }

        private TextBox CreateTextBox(bool isPass)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Height = 36,
                BackColor = Color.FromArgb(28, 44, 85),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                PasswordChar = isPass ? '●' : '\0',
                Margin = new Padding(0, 0, 0, 10)
            };
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            string name = txtFullName.Text.Trim();
            string user = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string pass = txtPassword.Text;
            string confirm = txtConfirmPass.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ * işaretli alanlar zorunludur!";
                return;
            }

            if (pass != confirm)
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ Şifreler eşleşmiyor!";
                return;
            }

            btnRegister.Enabled = false;
            if (AuthHelper.Register(user, name, email, pass, phone, out string msg))
            {
                MessageBox.Show(msg, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ " + msg;
                btnRegister.Enabled = true;
            }
        }
    }
}
