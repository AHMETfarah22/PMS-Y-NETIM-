using System.Drawing.Drawing2D;
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

            int y = 20;

            var lblTitle = new Label
            {
                Text = "🏨 Yeni Hesap Oluştur",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 165, 32),
                AutoSize = false,
                Size = new Size(440, 36),
                Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            y += 55;
           //ref y parametresi kullanılarak her alan eklendiğinde bir sonraki alanın konumu otomatik olarak hesaplanır.
            txtFullName = AddField("👤  Ad Soyad *", ref y, false);
            txtUsername = AddField("🔑  Kullanıcı Adı *", ref y, false);
            txtEmail = AddField("📧  E-Posta", ref y, false);
            txtPhone = AddField("📱  Telefon", ref y, false);
            txtPassword = AddField("🔒  Şifre *", ref y, true);
            txtConfirmPass = AddField("🔒  Şifre Tekrar *", ref y, true);

            // Kayıt ol butonu
            btnRegister = new Button
            {
                Text = "HESAP OLUŞTUR",
                Size = new Size(440, 46),
                Location = new Point(20, y + 10),
                BackColor = Color.FromArgb(218, 165, 32),
                ForeColor = Color.FromArgb(10, 18, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnRegister);
            y += 65;

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize = false,
                Size = new Size(440, 22),
                Location = new Point(20, y + 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);

            btnRegister.Click += BtnRegister_Click;
            btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = Color.FromArgb(195, 145, 25);
            btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = Color.FromArgb(218, 165, 32);
            this.AcceptButton = btnRegister;
        }

        private TextBox AddField(string labelText, ref int y, bool isPass)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 160, 200),
                AutoSize = true,
                Location = new Point(22, y)
            };
            this.Controls.Add(lbl);

            var txt = new TextBox
            {
                Size = new Size(440, 36),
                Location = new Point(20, y + 20),
                BackColor = Color.FromArgb(28, 44, 85),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                PasswordChar = isPass ? '●' : '\0'
            };
            this.Controls.Add(txt);
            y += 70;
            return txt;
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
            //metodu üzerinden veritabanına INSERT komutu gönderilir.AUTHelper
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
