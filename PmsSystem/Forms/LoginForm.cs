using System.Drawing.Drawing2D;
using PmsSystem.Helpers;

namespace PmsSystem.Forms
{
    public class LoginForm : Form
    {
        private Panel pnlMain;
        private Panel pnlCard;
        private Label lblTitle, lblSubtitle, lblStatus, lblRegisterLink, lblForgotPass;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnShowPass;
        private ComboBox cmbRole;
        private bool passVisible = false;

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

            // Kart
            pnlCard = new Panel
            {
                Size = new Size(420, 520),
                BackColor = Color.FromArgb(20, 32, 68),
                Location = new Point((1000 - 420) / 2, (650 - 520) / 2)
            };
            pnlCard.Paint += DrawCardBorder;
            pnlMain.Controls.Add(pnlCard);

            // Başlık
            lblTitle = new Label
            {
                Text = "🏨 PANSİYON YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = false,
                Size = new Size(380, 40),
                Location = new Point(20, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Pansiyon Yönetim Paneli",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = false,
                Size = new Size(380, 22),
                Location = new Point(20, 78),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblSubtitle);

            // Kullanıcı adı
            AddLabel(pnlCard, "👤  Kullanıcı Adı", 125);
            txtUsername = AddTextBox(pnlCard, "Kullanıcı adınızı giriniz", 150, false);

            // Şifre
            AddLabel(pnlCard, "🔒  Şifre", 215);
            txtPassword = AddTextBox(pnlCard, "Şifrenizi giriniz", 240, true);

            // Şifre göster butonu
            btnShowPass = new Button
            {
                Text = "👁",
                Size = new Size(36, 36),
                Location = new Point(345, 238),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12)
            };
            btnShowPass.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.Add(btnShowPass);

            // Rol seçimi
            AddLabel(pnlCard, "🎭  Rol", 300);
            cmbRole = new ComboBox
            {
                Size = new Size(380, 40),
                Location = new Point(20, 323),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(28, 44, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11)
            };
            cmbRole.Items.AddRange(new[] { "-- Rol Seçiniz --", "Admin", "Kasiyer" });
            cmbRole.SelectedIndex = 0;
            pnlCard.Controls.Add(cmbRole);

            // Giriş butonu
            btnLogin = new Button
            {
                Text = "GİRİŞ YAP",
                Size = new Size(380, 46),
                Location = new Point(20, 382),
                BackColor = Color.FromArgb(218, 165, 32),
                ForeColor = Color.FromArgb(10, 18, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.Add(btnLogin);

            // Durum etiketi
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize = false,
                Size = new Size(380, 22),
                Location = new Point(20, 435),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblStatus);

            // Alt bağlantılar
            lblForgotPass = new Label
            {
                Text = "Şifremi Unuttum",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                ForeColor = Color.FromArgb(120, 140, 180),
                AutoSize = true,
                Location = new Point(20, 465),
                Cursor = Cursors.Hand
            };
            pnlCard.Controls.Add(lblForgotPass);

            lblRegisterLink = new Label
            {
                Text = "Hesap Oluştur →",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = true,
                Location = new Point(270, 465),
                Cursor = Cursors.Hand
            };
            pnlCard.Controls.Add(lblRegisterLink);
        }

        private Label AddLabel(Panel parent, string text, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = true,
                Location = new Point(22, y)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox AddTextBox(Panel parent, string placeholder, int y, bool isPass)
        {
            var txt = new TextBox
            {
                Size = new Size(isPass ? 310 : 380, 36),
                Location = new Point(20, y),
                BackColor = Color.FromArgb(28, 44, 85),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                PasswordChar = isPass ? '●' : '\0'
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private void DrawCardBorder(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
            using var pen = new Pen(Color.FromArgb(50, 75, 130), 2);
            g.DrawRectangle(pen, rect);
            using var gold = new Pen(Color.FromArgb(218, 165, 32), 2);
            g.DrawLine(gold, 0, 0, pnlCard.Width, 0);
        }

        private void SetupEvents()
        {
            // Load
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
            };

            // Giriş
            btnLogin.Click += BtnLogin_Click;
            this.AcceptButton = btnLogin;

            // Hover
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(195, 145, 25);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(218, 165, 32);

            // Şifre göster
            btnShowPass.Click += (s, e) =>
            {
                passVisible = !passVisible;
                txtPassword.PasswordChar = passVisible ? '\0' : '●';
                btnShowPass.Text = passVisible ? "🙈" : "👁";
            };

            // Kayıt
            lblRegisterLink.Click += (s, e) =>
            {
                new RegisterForm().ShowDialog(this);
            };
            lblRegisterLink.MouseEnter += (s, e) => lblRegisterLink.ForeColor = Color.White;
            lblRegisterLink.MouseLeave += (s, e) => lblRegisterLink.ForeColor = Color.FromArgb(218, 165, 32);

            // Şifremi unuttum
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
           //metodu çağrılarak veritabanı sorgusu yapılır.
            if (AuthHelper.Login(user, pass, out string msg))
            {
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                lblStatus.Text = "✅ " + msg;
                Application.DoEvents();
                Thread.Sleep(800);

                this.Hide();
                var dashboard = new DashboardForm();
                dashboard.Show();
                dashboard.FormClosed += (s, ev) => this.Close(); // Close app when dashboard is closed
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
