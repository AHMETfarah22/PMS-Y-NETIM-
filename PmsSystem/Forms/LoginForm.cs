using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using PmsSystem.Helpers;
using PmsSystem.Components;

namespace PmsSystem.Forms
{
    public class LoginForm : Form
    {
        private Panel pnlMain;
        private RoundedPanel pnlCard;
        private Label lblTitle, lblSubtitle, lblStatus, lblRegisterLink, lblForgotPass;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnShowPass;
        private ComboBox cmbRole;
        private CheckBox chkRemember;
        private bool passVisible = false;
        private readonly string rememberPath = Path.Combine(Application.StartupPath, "remember.json");

        public LoginForm()
        {
            InitUI();
            SetupEvents();
        }

        private void InitUI()
        {
            this.Text = "PMS - Pansiyon Yönetim Sistemi | Giriş";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(10, 18, 42);

            // Arka plan gradient panel
            pnlMain = new Panel { Dock = DockStyle.Fill };
            pnlMain.Paint += (s, e) =>
            {
                using var br = new LinearGradientBrush(pnlMain.ClientRectangle,
                    Color.FromArgb(8, 15, 40), Color.FromArgb(20, 35, 80), 135f);
                e.Graphics.FillRectangle(br, pnlMain.ClientRectangle);
            };
            this.Controls.Add(pnlMain);

            // Kart - Responsive Center
            pnlCard = new RoundedPanel
            {
                Size = new Size(420, 520),
                BackColor = Color.FromArgb(20, 32, 68),
                BorderRadius = 24
            };
            
            // Auto center card
            pnlMain.Resize += (s, e) => {
                pnlCard.Location = new Point((pnlMain.Width - pnlCard.Width) / 2, (pnlMain.Height - pnlCard.Height) / 2);
            };
            pnlCard.Location = new Point((this.ClientSize.Width - pnlCard.Width) / 2, (this.ClientSize.Height - pnlCard.Height) / 2);
            pnlMain.Controls.Add(pnlCard);

            TableLayoutPanel tlpCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                Padding = new Padding(30, 20, 30, 20)
            };
            tlpCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pnlCard.Controls.Add(tlpCard);

            // Başlık
            lblTitle = new Label
            {
                Text = "🏨 PANSİYON YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 5)
            };
            tlpCard.Controls.Add(lblTitle, 0, 0);

            lblSubtitle = new Label
            {
                Text = "Pansiyon Yönetim Paneli",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 20)
            };
            tlpCard.Controls.Add(lblSubtitle, 0, 1);

            // Kullanıcı adı
            tlpCard.Controls.Add(CreateLabel("👤  Kullanıcı Adı"), 0, 2);
            txtUsername = CreateTextBox("Kullanıcı adınızı giriniz", false);
            tlpCard.Controls.Add(txtUsername, 0, 3);

            // Şifre
            tlpCard.Controls.Add(CreateLabel("🔒  Şifre"), 0, 4);
            
            Panel pnlPass = new Panel { Dock = DockStyle.Fill, Height = 40, Margin = new Padding(0, 0, 0, 10) };
            txtPassword = CreateTextBox("Şifrenizi giriniz", true);
            txtPassword.Width = 300;
            pnlPass.Controls.Add(txtPassword);

            btnShowPass = new Button
            {
                Text = "👁",
                Size = new Size(36, 36),
                Location = new Point(310, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12)
            };
            btnShowPass.FlatAppearance.BorderSize = 0;
            pnlPass.Controls.Add(btnShowPass);
            tlpCard.Controls.Add(pnlPass, 0, 5);

            // Rol seçimi
            tlpCard.Controls.Add(CreateLabel("🎭  Rol"), 0, 6);
            cmbRole = new ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 40,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(28, 44, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(0, 0, 0, 10)
            };
            cmbRole.Items.AddRange(new[] { "-- Rol Seçiniz --", "Admin", "Kasiyer" });
            cmbRole.SelectedIndex = 0;
            tlpCard.Controls.Add(cmbRole, 0, 7);

            // Beni hatırla
            chkRemember = new CheckBox
            {
                Text = "Beni Hatırla",
                ForeColor = Color.FromArgb(140, 160, 200),
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 15)
            };
            tlpCard.Controls.Add(chkRemember, 0, 8);

            // Giriş butonu
            btnLogin = new Button
            {
                Text = "GİRİŞ YAP",
                Dock = DockStyle.Fill,
                Height = 45,
                BackColor = Color.FromArgb(218, 165, 32),
                ForeColor = Color.FromArgb(10, 18, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 10)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            tlpCard.Controls.Add(btnLogin, 0, 9);

            // Durum etiketi
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10)
            };
            tlpCard.Controls.Add(lblStatus, 0, 10);

            // Alt bağlantılar
            TableLayoutPanel tlpLinks = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpLinks.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlpLinks.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            lblForgotPass = new Label
            {
                Text = "Şifremi Unuttum",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                ForeColor = Color.FromArgb(120, 140, 180),
                AutoSize = true,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            tlpLinks.Controls.Add(lblForgotPass, 0, 0);

            lblRegisterLink = new Label
            {
                Text = "Hesap Oluştur →",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = true,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Cursor = Cursors.Hand
            };
            tlpLinks.Controls.Add(lblRegisterLink, 1, 0);

            tlpCard.Controls.Add(tlpLinks, 0, 11);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
        }

        private TextBox CreateTextBox(string placeholder, bool isPass)
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
                Margin = new Padding(0, 0, 0, 15)
            };
        }

        private void SetupEvents()
        {
            this.Load += (s, e) =>
            {
                try
                {
                    Database.DatabaseHelper.InitializeDatabase();
                    lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                    lblStatus.Text = "✅ Veritabanı hazır!";
                    Task.Delay(2000).ContinueWith(_ => Invoke(() => lblStatus.Text = ""));
                }
                catch (Exception ex)
                {
                    lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                    lblStatus.Text = "❌ DB Hata: " + ex.Message;
                }
                txtUsername.Focus();

                if (File.Exists(rememberPath))
                {
                    try {
                        var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(rememberPath));
                        if (data != null) {
                            txtUsername.Text = data.GetValueOrDefault("u", "");
                            string r = data.GetValueOrDefault("r", "");
                            if (r == "Admin") cmbRole.SelectedIndex = 1;
                            else if (r == "Kasiyer") cmbRole.SelectedIndex = 2;
                            chkRemember.Checked = true;
                            if (!string.IsNullOrEmpty(txtUsername.Text)) txtPassword.Focus();
                        }
                    } catch { }
                }
            };

            btnLogin.Click += BtnLogin_Click;
            this.AcceptButton = btnLogin;

            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(195, 145, 25);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(218, 165, 32);

            btnShowPass.Click += (s, e) =>
            {
                passVisible = !passVisible;
                txtPassword.PasswordChar = passVisible ? '\0' : '●';
                btnShowPass.Text = passVisible ? "🙈" : "👁";
            };

            lblRegisterLink.Click += (s, e) => new RegisterForm().ShowDialog(this);
            lblRegisterLink.MouseEnter += (s, e) => lblRegisterLink.ForeColor = Color.White;
            lblRegisterLink.MouseLeave += (s, e) => lblRegisterLink.ForeColor = Color.FromArgb(218, 165, 32);

            lblForgotPass.Click += (s, e) =>
                MessageBox.Show("Şifre sıfırlama için sistem yöneticinize başvurunuz.",
                    "Şifremi Unuttum", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;

            if (cmbRole.SelectedIndex == 0)
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ Lütfen bir rol seçiniz!";
                return;
            }

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ Kullanıcı adı ve şifre boş olamaz!";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "⏳ GİRİŞ...";
            Application.DoEvents();
             if (AuthHelper.Login(user, pass, out string msg))
            {
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                lblStatus.Text = "✅ " + msg;
                Application.DoEvents();

                if (chkRemember.Checked) {
                    var data = new Dictionary<string, string> { { "u", user }, { "r", cmbRole.SelectedItem?.ToString() ?? "" } };
                    File.WriteAllText(rememberPath, System.Text.Json.JsonSerializer.Serialize(data));
                } else if (File.Exists(rememberPath)) {
                    File.Delete(rememberPath);
                }

                Thread.Sleep(800);

                this.Hide();
                var dashboard = new DashboardForm();
                dashboard.Show();
                dashboard.FormClosed += (s, ev) => this.Close(); 
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "❌ " + msg;
                btnLogin.Enabled = true;
                btnLogin.Text = "GİRİŞ YAP";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
