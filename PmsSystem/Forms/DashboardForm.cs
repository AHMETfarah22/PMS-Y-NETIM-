using System.Data;
using PmsSystem.Database;
using PmsSystem.Helpers;

namespace PmsSystem.Forms 
{
    public class DashboardForm : Form
    {
        private Panel pnlSidebar, pnlPageArea;
        private Label lblPageTitle;
        private Color accentBlue = Color.FromArgb(43, 87, 154);
        private Color sidebarDark = Color.FromArgb(30, 36, 50);
        private Color pageBg = Color.FromArgb(245, 247, 250);

        public DashboardForm()
        {
            this.Text = "PMS | Pansiyon Yonetim Sistemi";
            this.Size = new Size(1280, 820);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = pageBg;
            this.Font = new Font("Segoe UI", 10);

            BuildSidebar();
            BuildPageArea();
            ShowPage("Ana Sayfa");
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel { Width = 220, Dock = DockStyle.Left, BackColor = sidebarDark };
            this.Controls.Add(pnlSidebar);

            pnlSidebar.Controls.Add(new Label { Text = "SOM-PMS", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(100, 180, 255), AutoSize = true, Location = new Point(30, 22) });

            string[] items = { "Ana Sayfa", "Rezervasyon", "Odalar", "Müşteriler", "Depo", "Market", "Raporlar" };
            int y = 90;
            foreach (var item in items)
            {
                var b = new Button { Text = "    " + item, TextAlign = ContentAlignment.MiddleLeft, Size = new Size(220, 48), Location = new Point(0, y), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, ForeColor = Color.FromArgb(170, 180, 200), Font = new Font("Segoe UI", 10) };
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 52, 70);
                string page = item;
                b.Click += (s, e) => ShowPage(page);
                pnlSidebar.Controls.Add(b);
                y += 52;
            }
        }

        private void BuildPageArea()
        {
            pnlPageArea = new Panel { Dock = DockStyle.Fill, BackColor = pageBg };
            this.Controls.Add(pnlPageArea);
            pnlPageArea.BringToFront();
        }

        private void ShowPage(string page)
        {
            pnlPageArea.Controls.Clear();

            var pnlHead = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White };
            pnlHead.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 225, 230)), 0, 54, pnlHead.Width, 54);
            pnlPageArea.Controls.Add(pnlHead);

            lblPageTitle = new Label { Text = page, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(40, 50, 70), Location = new Point(20, 12), AutoSize = true };
            pnlHead.Controls.Add(lblPageTitle);

            string userText = AuthHelper.CurrentUser != null ? $"{AuthHelper.CurrentUser.FullName} ({AuthHelper.CurrentUser.Role})" : "";
            pnlHead.Controls.Add(new Label { Text = userText, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = accentBlue, Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true, Location = new Point(700, 18) });

            var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = pageBg, Padding = new Padding(20, 15, 20, 15), AutoScroll = true };
            pnlPageArea.Controls.Add(pnlBody);
            pnlBody.BringToFront();

            try
            {
                switch (page)
                {
                    case "Ana Sayfa": PageDashboard(pnlBody); break;
                    case "Rezervasyon": PageReservation(pnlBody); break;
                    case "Odalar": PageRooms(pnlBody); break;
                    case "Müşteriler": PageCustomers(pnlBody); break;
                    case "Depo": PageStorage(pnlBody); break;
                    case "Market": PageMarket(pnlBody); break;
                    default: pnlBody.Controls.Add(new Label { Text = page + " modulu yakin zamanda...", AutoSize = true, Location = new Point(30, 30) }); break;
                }
            }
            catch (Exception ex)
            {
                pnlBody.Controls.Add(new Label { Text = "Hata: " + ex.Message, ForeColor = Color.Red, AutoSize = true, Location = new Point(20, 20) });
            }
        }

        // ═══════════════════ DASHBOARD (ROOMS FROM DB) ═══════════════════
        private void PageDashboard(Panel body)
        {
            DataTable rooms = DataAccess.GetAllRooms();

            int totalRooms = rooms.Rows.Count;
            int occupiedBeds = 0;
            int availableRooms = 0;
            foreach (DataRow r in rooms.Rows)
            {
                occupiedBeds += Convert.ToInt32(r["OccupiedBeds"]);
                if (r["Status"].ToString() == "Available") availableRooms++;
            }

            int x = 0;
            AddStat(body, ref x, "TOPLAM ODA", totalRooms.ToString(), Color.FromArgb(72, 187, 120));
            AddStat(body, ref x, "DOLU YATAK", occupiedBeds.ToString(), Color.FromArgb(239, 68, 68));
            AddStat(body, ref x, "MUSAIT ODA", availableRooms.ToString(), Color.FromArgb(59, 130, 246));
            AddStat(body, ref x, "ODALAR", (totalRooms - availableRooms) + " Dolu", Color.FromArgb(245, 158, 11));

            var pnlRooms = new Panel { Location = new Point(0, 130), BackColor = Color.White, Padding = new Padding(15) };
            pnlRooms.Size = new Size(body.Width - 40, 450);
            pnlRooms.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            body.Controls.Add(pnlRooms);

            pnlRooms.Controls.Add(new Label { Text = "Oda Durum Paneli", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(15, 10), AutoSize = true });

            var flow = new FlowLayoutPanel { Location = new Point(15, 45), AutoScroll = true };
            flow.Size = new Size(pnlRooms.Width - 30, 390);
            flow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pnlRooms.Controls.Add(flow);

            foreach (DataRow r in rooms.Rows)
            {
                string num = r["RoomNumber"].ToString()!;
                int cap = Convert.ToInt32(r["Capacity"]);
                int occ = Convert.ToInt32(r["OccupiedBeds"]);
                string st = r["Status"].ToString()!;
                string tip = r["OdaTipi"].ToString()!;
                decimal fiyat = Convert.ToDecimal(r["Fiyat"]);
                AddRoom(flow, num, st, cap, occ, tip, fiyat);
            }
        }

        private void AddStat(Panel body, ref int x, string title, string val, Color color)
        {
            var p = new Panel { Location = new Point(x, 0), Size = new Size(230, 100), BackColor = Color.White };
            p.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(230, 230, 230)); e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); using var top = new Pen(color, 3); e.Graphics.DrawLine(top, 0, 0, p.Width, 0); };
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(18, 18), AutoSize = true });
            p.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = color, Location = new Point(18, 48), AutoSize = true });
            body.Controls.Add(p);
            x += 245;
        }

        private void AddRoom(FlowLayoutPanel flow, string num, string status, int cap, int occ, string tip, decimal fiyat)
        {
            // Renk: Dolu=kirmizi, Kismi=mavi, Musait => Deniz=sari/gold, Standart=yesil
            Color bg;
            if (status == "Occupied") bg = Color.FromArgb(239, 68, 68);
            else if (status == "Partial") bg = Color.FromArgb(59, 130, 246);
            else bg = (tip == "Deniz Manzarali") ? Color.FromArgb(234, 179, 8) : Color.FromArgb(34, 197, 94);

            var pnl = new Panel { Size = new Size(125, 115), BackColor = bg, Margin = new Padding(6), Cursor = Cursors.Hand };
            pnl.Controls.Add(new Label { Text = num, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, Size = new Size(125, 26), Location = new Point(0, 5), TextAlign = ContentAlignment.MiddleCenter });
            
            string tipKisa = tip == "Deniz Manzarali" ? "Deniz" : "Std";
            pnl.Controls.Add(new Label { Text = tipKisa, Font = new Font("Segoe UI", 7, FontStyle.Italic), ForeColor = Color.FromArgb(255, 255, 200), Size = new Size(125, 14), Location = new Point(0, 32), TextAlign = ContentAlignment.MiddleCenter });
            pnl.Controls.Add(new Label { Text = $"{cap} Yatak", Font = new Font("Segoe UI", 8), ForeColor = Color.White, Size = new Size(125, 16), Location = new Point(0, 48), TextAlign = ContentAlignment.MiddleCenter });
            pnl.Controls.Add(new Label { Text = $"{fiyat:N0} TL", Font = new Font("Segoe UI", 7), ForeColor = Color.White, Size = new Size(125, 14), Location = new Point(0, 65), TextAlign = ContentAlignment.MiddleCenter });

            string txt = occ > 0 && occ < cap ? $"{occ}/{cap} Dolu" : (occ >= cap ? "TAM DOLU" : "MUSAIT");
            pnl.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.White, Size = new Size(125, 20), Location = new Point(0, 85), TextAlign = ContentAlignment.MiddleCenter });

            EventHandler clickHandler = (s, e) => { OpenReservationPopup(num, cap, occ, status); };
            pnl.Click += clickHandler;
            foreach (Control c in pnl.Controls) c.Click += clickHandler;
            flow.Controls.Add(pnl);
        }

        private void OpenReservationPopup(string roomNum, int cap, int occ, string status)
        {
            if (status == "Occupied") { MessageBox.Show($"Oda {roomNum} tamamen doludur.", "Bilgi"); return; }

            var f = new Form { Text = $"Oda {roomNum} - Kayit", Size = new Size(420, 720), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 25;
            f.Controls.Add(new Label { Text = $"Oda {roomNum} ({cap} Yatak, {occ} Dolu)", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 40;

            f.Controls.Add(new Label { Text = "Yatak No:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var cmb = new ComboBox { Location = new Point(30, y), Size = new Size(340, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= cap; i++) cmb.Items.Add($"{i}. Yatak");
            cmb.SelectedIndex = 0; f.Controls.Add(cmb); y += 35;

            f.Controls.Add(new Label { Text = "Giris/Cikis Tarihleri:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var dtpGiris = new DateTimePicker { Location = new Point(30, y), Size = new Size(165, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            f.Controls.Add(dtpGiris);
            var dtpCikis = new DateTimePicker { Location = new Point(205, y), Size = new Size(165, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };
            f.Controls.Add(dtpCikis); y += 35;

            f.Controls.Add(new Label { Text = "TC Kimlik No:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = accentBlue, Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtTC = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "11 haneli TC", MaxLength = 11 }; f.Controls.Add(txtTC); y += 35;

            f.Controls.Add(new Label { Text = "Ad:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtAd = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "Adi" }; f.Controls.Add(txtAd); y += 35;

            f.Controls.Add(new Label { Text = "Soyad:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtSoyad = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "Soyadi" }; f.Controls.Add(txtSoyad); y += 35;

            f.Controls.Add(new Label { Text = "Telefon:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtTel = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "05xx..." }; f.Controls.Add(txtTel); y += 35;

            f.Controls.Add(new Label { Text = "E-Posta:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtMail = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "ornek@mail.com" }; f.Controls.Add(txtMail); y += 35;

            f.Controls.Add(new Label { Text = "Adres:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 20;
            var txtAdres = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = "Adres bilgisi" }; f.Controls.Add(txtAdres); y += 40;

            txtTC.TextChanged += (s, e) => {
                string input = txtTC.Text.Trim();
                if (input.Length >= 3) {
                    var dt = DataAccess.GetCustomersByIdentityPrefix(input);
                    if (dt.Rows.Count == 1) {
                        var row = dt.Rows[0];
                        string fullId = row["IdentityNumber"].ToString() ?? "";
                        txtAd.Text = row["FirstName"].ToString();
                        txtSoyad.Text = row["LastName"].ToString();
                        txtTel.Text = row["Phone"].ToString();
                        txtMail.Text = row["Email"].ToString();
                        txtAdres.Text = row["Address"].ToString();
                        // Auto-complete TC field without re-triggering event
                        if (txtTC.Text != fullId) {
                            txtTC.TextChanged -= null;
                            txtTC.Text = fullId;
                            txtTC.SelectionStart = fullId.Length;
                        }
                        if (DataAccess.IsCustomerStaying(fullId)) {
                            MessageBox.Show("DİKKAT: Bu müşteri şu anda otelde konaklamaktadır!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            };

            var btn = new Button { Text = "KAYDET", Location = new Point(30, y), Size = new Size(340, 45), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text))
                { MessageBox.Show("Ad ve Soyad zorunludur."); return; }
                if (dtpCikis.Value <= dtpGiris.Value)
                { MessageBox.Show("Cikis tarihi giris tarihinden sonra olmali."); return; }
                try
                {
                    int bedNo = cmb.SelectedIndex + 1;
                    if (DataAccess.IsBedOccupied(roomNum, bedNo, dtpGiris.Value, dtpCikis.Value))
                    {
                        MessageBox.Show("Bu yatak seçtiğiniz tarihler arasında zaten doludur. Lütfen başka bir yatak seçiniz.", "Yatak Dolu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int custId = DataAccess.AddCustomer(txtAd.Text.Trim(), txtSoyad.Text.Trim(), txtTel.Text.Trim(), txtMail.Text.Trim(), roomNum, bedNo, txtAdres.Text.Trim(), txtTC.Text.Trim());
                    DataAccess.AddReservation(custId, roomNum, bedNo, dtpGiris.Value, dtpCikis.Value);
                    MessageBox.Show($"Basarili! {txtAd.Text} {txtSoyad.Text} kaydedildi!", "Basarili");
                    f.Close(); ShowPage("Ana Sayfa");
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn);
            f.ShowDialog();
        }

        // ═══════════════════ RESERVATION PAGE ═══════════════════
        private void PageReservation(Panel body)
        {
            var pnlForm = new Panel { Location = new Point(0, 0), Size = new Size(450, 700), BackColor = Color.White, AutoScroll = true };
            pnlForm.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 220, 220)); e.Graphics.DrawRectangle(pen, 0, 0, pnlForm.Width - 1, pnlForm.Height - 1); };
            body.Controls.Add(pnlForm);

            pnlForm.Controls.Add(new Label { Text = "Yeni Rezervasyon Olustur", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(25, 20), AutoSize = true });

            int y = 55;
            TextBox MakeField(string label, string ph, string colorHex = "#505A64") {
                pnlForm.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml(colorHex), Location = new Point(25, y), AutoSize = true }); y += 20;
                var t = new TextBox { Location = new Point(25, y), Size = new Size(390, 28), PlaceholderText = ph, BorderStyle = BorderStyle.FixedSingle }; pnlForm.Controls.Add(t); y += 35; return t;
            }

            pnlForm.Controls.Add(new Label { Text = "Oda Numarasi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 90, 100), Location = new Point(25, y), AutoSize = true }); y += 20;
            var cmbOda = new ComboBox { Location = new Point(25, y), Size = new Size(260, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            var availRooms = DataAccess.GetAvailableRoomsWithPrice();
            cmbOda.DataSource = availRooms;
            cmbOda.DisplayMember = "RoomNumber";
            cmbOda.ValueMember = "RoomNumber";
            pnlForm.Controls.Add(cmbOda);

            var lblOdaBilgi = new Label { Location = new Point(295, y + 4), Size = new Size(120, 20), ForeColor = accentBlue, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "" };
            pnlForm.Controls.Add(lblOdaBilgi);
            y += 35;

            void RefreshOdaBilgi() {
                if (cmbOda.SelectedValue == null) return;
                var rInfo = DataAccess.GetRoomInfo(cmbOda.SelectedValue.ToString()!);
                if (rInfo != null)
                    lblOdaBilgi.Text = $"Kat {rInfo["FloorNumber"]} | {rInfo["CurrentPrice"]:N0} TL";
            }
            cmbOda.SelectedIndexChanged += (s, e) => RefreshOdaBilgi();
            RefreshOdaBilgi();

            pnlForm.Controls.Add(new Label { Text = "Yatak No:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 90, 100), Location = new Point(25, y), AutoSize = true }); y += 20;
            var cmbYatak = new ComboBox { Location = new Point(25, y), Size = new Size(390, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYatak.Items.AddRange(new[] { "1. Yatak", "2. Yatak", "3. Yatak", "4. Yatak" });
            cmbYatak.SelectedIndex = 0; pnlForm.Controls.Add(cmbYatak); y += 35;

            pnlForm.Controls.Add(new Label { Text = "Giris Tarihi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 90, 100), Location = new Point(25, y), AutoSize = true }); y += 20;
            var dtpGiris = new DateTimePicker { Location = new Point(25, y), Size = new Size(390, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            pnlForm.Controls.Add(dtpGiris); y += 35;

            pnlForm.Controls.Add(new Label { Text = "Cikis Tarihi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 90, 100), Location = new Point(25, y), AutoSize = true }); y += 20;
            var dtpCikis = new DateTimePicker { Location = new Point(25, y), Size = new Size(390, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };
            pnlForm.Controls.Add(dtpCikis); y += 35;

            var txtTC = MakeField("TC Kimlik No:", "11 haneli TC", "#2b579a");
            txtTC.MaxLength = 11;
            var txtAd = MakeField("Ad:", "Musteri adi");
            var txtSoyad = MakeField("Soyad:", "Musteri soyadi");
            var txtTel = MakeField("Telefon:", "05xx...");
            var txtMail = MakeField("E-Posta:", "mail@ornek.com");
            var txtAdres = MakeField("Adres:", "Adres bilgisi");

            txtTC.TextChanged += (s, e) => {
                string input = txtTC.Text.Trim();
                if (input.Length >= 3) {
                    var dt = DataAccess.GetCustomersByIdentityPrefix(input);
                    if (dt.Rows.Count == 1) {
                        var row = dt.Rows[0];
                        string fullId = row["IdentityNumber"].ToString() ?? "";
                        txtAd.Text = row["FirstName"].ToString();
                        txtSoyad.Text = row["LastName"].ToString();
                        txtTel.Text = row["Phone"].ToString();
                        txtMail.Text = row["Email"].ToString();
                        txtAdres.Text = row["Address"].ToString();
                        if (txtTC.Text != fullId) {
                            txtTC.TextChanged -= null;
                            txtTC.Text = fullId;
                            txtTC.SelectionStart = fullId.Length;
                        }
                        if (DataAccess.IsCustomerStaying(fullId)) {
                            MessageBox.Show("DİKKAT: Bu müşteri şu anda otelde konaklamaktadır!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            };

            var btnKaydet = new Button { Text = "KAYDET", Location = new Point(25, y + 5), Size = new Size(390, 45), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text) || cmbOda.SelectedValue == null)
                { MessageBox.Show("Oda, Ad ve Soyad zorunludur."); return; }
                if (dtpCikis.Value <= dtpGiris.Value)
                { MessageBox.Show("Cikis tarihi giris tarihinden sonra olmali."); return; }
                try
                {
                    string roomNum = cmbOda.SelectedValue.ToString()!;
                    int bedNo = cmbYatak.SelectedIndex + 1;
                    if (DataAccess.IsBedOccupied(roomNum, bedNo, dtpGiris.Value, dtpCikis.Value))
                    {
                        MessageBox.Show("Bu yatak seçtiğiniz tarihler arasında zaten doludur. Lütfen başka bir yatak seçiniz.", "Yatak Dolu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int custId = DataAccess.AddCustomer(txtAd.Text.Trim(), txtSoyad.Text.Trim(), txtTel.Text.Trim(), txtMail.Text.Trim(), roomNum, bedNo, txtAdres.Text.Trim(), txtTC.Text.Trim());
                    DataAccess.AddReservation(custId, roomNum, bedNo, dtpGiris.Value, dtpCikis.Value);
                    MessageBox.Show($"Basarili! {txtAd.Text} {txtSoyad.Text} kaydedildi!", "Basarili");
                    txtTC.Clear(); txtAd.Clear(); txtSoyad.Clear(); txtTel.Clear(); txtMail.Clear(); txtAdres.Clear();
                    LoadReservationList(body);
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            pnlForm.Controls.Add(btnKaydet);
            LoadReservationList(body);
        }

        // ═══════════════════ ROOMS PAGE ═══════════════════
        private void PageRooms(Panel body)
        {
            body.Controls.Add(new Label { Text = "Oda Yönetimi", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true });

            DataGridView dgv;
            try
            {
                var dt = DataAccess.GetAllRoomsDetailed();
                dgv = new DataGridView
                {
                    Name = "dgvRooms",
                    Location = new Point(0, 45),
                    Size = new Size(body.Width - 40, 380),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    RowHeadersVisible = false,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    DataSource = dt,
                    Font = new Font("Segoe UI", 10)
                };
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 38;
                dgv.RowTemplate.Height = 32;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                body.Controls.Add(dgv);
            }
            catch (Exception ex)
            {
                body.Controls.Add(new Label { Text = "Oda yuklenemedi: " + ex.Message, ForeColor = Color.Red, Location = new Point(0, 50), AutoSize = true });
                return;
            }

            int bx = 0;
            // Add Room button
            var btnAdd = new Button { Text = "➕ ODA EKLE", Location = new Point(bx, 440), Size = new Size(150, 45), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => { ShowAddRoomForm(); ShowPage("Odalar"); };
            body.Controls.Add(btnAdd); bx += 160;

            // Delete Room button
            var btnDel = new Button { Text = "🗑 ODA SİL", Location = new Point(bx, 440), Size = new Size(150, 45), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) => {
                if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Silmek için bir oda seçin."); return; }
                string roomNum = dgv.SelectedRows[0].Cells["RoomNumber"].Value.ToString()!;
                if (MessageBox.Show($"Oda {roomNum} silinecek. Emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try { DataAccess.DeleteRoom(roomNum); ShowPage("Odalar"); }
                    catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
                }
            };
            body.Controls.Add(btnDel); bx += 160;

            // Set Price button
            var btnPrice = new Button { Text = "💰 FİYAT BELİRLE", Location = new Point(bx, 440), Size = new Size(160, 45), BackColor = accentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPrice.FlatAppearance.BorderSize = 0;
            btnPrice.Click += (s, e) => {
                if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Fiyat girmek için bir oda seçin."); return; }
                string roomNum = dgv.SelectedRows[0].Cells["RoomNumber"].Value.ToString()!;
                ShowSetPriceForm(roomNum);
                ShowPage("Odalar");
            };
            body.Controls.Add(btnPrice); bx += 170;

            // Price History button
            var btnHist = new Button { Text = "📋 FİYAT GEÇMİŞİ", Location = new Point(bx, 440), Size = new Size(160, 45), BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnHist.FlatAppearance.BorderSize = 0;
            btnHist.Click += (s, e) => {
                if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Geçmiş için bir oda seçin."); return; }
                string roomNum = dgv.SelectedRows[0].Cells["RoomNumber"].Value.ToString()!;
                ShowPriceHistoryForm(roomNum);
            };
            body.Controls.Add(btnHist);
        }

        private void ShowAddRoomForm()
        {
            var f = new Form { Text = "Yeni Oda Ekle", Size = new Size(400, 340), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20;
            f.Controls.Add(new Label { Text = "Yeni Oda Ekle", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 45;

            TextBox Field(string lbl, string ph) {
                f.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
                var t = new TextBox { Location = new Point(30, y), Size = new Size(320, 28), PlaceholderText = ph, BorderStyle = BorderStyle.FixedSingle }; f.Controls.Add(t); y += 38; return t;
            }

            var txtNo = Field("Oda Numarasi:", "Ornek: 401");
            var txtKat = Field("Kat Numarasi:", "Ornek: 4");
            var txtTip = Field("Oda Tipi:", "Standart veya Deniz Manzarali");
            var txtKap = Field("Kapasite (Yatak):", "Ornek: 2");

            var btn = new Button { Text = "KAYDET", Location = new Point(30, y + 5), Size = new Size(320, 42), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                try {
                    if (string.IsNullOrWhiteSpace(txtNo.Text)) { MessageBox.Show("Oda numarası zorunludur."); return; }
                    if (!int.TryParse(txtKat.Text, out int kat)) { MessageBox.Show("Geçerli bir kat numarası girin."); return; }
                    if (!int.TryParse(txtKap.Text, out int kap) || kap < 1) { MessageBox.Show("Geçerli bir kapasite girin."); return; }
                    DataAccess.AddRoom(txtNo.Text.Trim(), kat, txtTip.Text.Trim() == "" ? "Standart" : txtTip.Text.Trim(), kap);
                    MessageBox.Show($"Oda {txtNo.Text} eklendi!", "Başarılı");
                    f.Close();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn);
            f.ShowDialog();
        }

        private void ShowSetPriceForm(string roomNum)
        {
            var currentInfo = DataAccess.GetRoomInfo(roomNum);
            decimal currentPrice = currentInfo != null ? Convert.ToDecimal(currentInfo["CurrentPrice"]) : 0;

            var f = new Form { Text = $"Oda {roomNum} - Fiyat Belirle", Size = new Size(380, 220), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            f.Controls.Add(new Label { Text = $"Mevcut Fiyat: {currentPrice:N0} TL", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = accentBlue, Location = new Point(30, 20), AutoSize = true });
            f.Controls.Add(new Label { Text = "Yeni Fiyat (TL):", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, 60), AutoSize = true });
            var txtP = new TextBox { Location = new Point(30, 82), Size = new Size(300, 30), PlaceholderText = "Ornek: 1500", BorderStyle = BorderStyle.FixedSingle };
            f.Controls.Add(txtP);
            var btn = new Button { Text = "FİYAT KAYDET (eski fiyat korunur)", Location = new Point(30, 122), Size = new Size(300, 42), BackColor = accentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                if (!decimal.TryParse(txtP.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal newPrice))
                { MessageBox.Show("Geçerli bir fiyat girin."); return; }
                DataAccess.SetRoomPrice(roomNum, newPrice);
                MessageBox.Show($"Oda {roomNum} için yeni fiyat {newPrice:N0} TL olarak kaydedildi!\nEski fiyat ({currentPrice:N0} TL) geçmişte tutuldu.", "Başarılı");
                f.Close();
            };
            f.Controls.Add(btn);
            f.ShowDialog();
        }

        private void ShowPriceHistoryForm(string roomNum)
        {
            var f = new Form { Text = $"Oda {roomNum} - Fiyat Geçmişi", Size = new Size(400, 380), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            f.Controls.Add(new Label { Text = $"Oda {roomNum} Fiyat Geçmişi", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true });
            try {
                var dt = DataAccess.GetRoomPriceHistory(roomNum);
                var dgv = new DataGridView { Location = new Point(20, 50), Size = new Size(340, 280), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, DataSource = dt, Font = new Font("Segoe UI", 10) };
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                f.Controls.Add(dgv);
            }
            catch { f.Controls.Add(new Label { Text = "Fiyat geçmişi bulunamadı.", Location = new Point(20, 55), AutoSize = true }); }
            f.ShowDialog();
        }

        private void LoadReservationList(Panel body)
        {
            // Remove old list if exists
            var old = body.Controls.Find("pnlResList", false);
            foreach (var o in old) body.Controls.Remove(o);

            var pnlList = new Panel { Name = "pnlResList", Location = new Point(470, 0), Size = new Size(480, 520), BackColor = Color.White };
            pnlList.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 220, 220)); e.Graphics.DrawRectangle(pen, 0, 0, pnlList.Width - 1, pnlList.Height - 1); };
            body.Controls.Add(pnlList);

            pnlList.Controls.Add(new Label { Text = "Mevcut Rezervasyonlar (DB)", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true });

            try
            {
                var dt = DataAccess.GetReservations();
                var dgv = new DataGridView { Location = new Point(15, 50), Size = new Size(450, 450), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, DataSource = dt };
                pnlList.Controls.Add(dgv);
            }
            catch { pnlList.Controls.Add(new Label { Text = "Henuz rezervasyon yok.", Location = new Point(15, 50), AutoSize = true }); }
        }

        // ═══════════════════ CUSTOMERS PAGE ═══════════════════
        private void PageCustomers(Panel body)
        {
            body.Controls.Add(new Label { Text = "Kayitli Musteriler", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true });

            try
            {
                var dt = DataAccess.GetAllCustomers();

                body.Controls.Add(new Label { Text = $"Toplam Kayitli Musteri: {dt.Rows.Count}", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = accentBlue, Location = new Point(0, 35), AutoSize = true });

                var dgv = new DataGridView
                {
                    Location = new Point(0, 70),
                    Size = new Size(body.Width - 40, 480),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    RowHeadersVisible = false,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    DataSource = dt,
                    Font = new Font("Segoe UI", 10)
                };
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 38;
                dgv.RowTemplate.Height = 32;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                body.Controls.Add(dgv);

                var btnAddCust = new Button { Text = "➕ YENİ MÜŞTERİ EKLE", Location = new Point(0, 560), Size = new Size(200, 48), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                btnAddCust.FlatAppearance.BorderSize = 0;
                btnAddCust.Click += (s, e) => { ShowCustomerForm(); ShowPage("Müşteriler"); };
                body.Controls.Add(btnAddCust);
            }
            catch (Exception ex)
            {
                body.Controls.Add(new Label { Text = "Musteri bulunamadi. " + ex.Message, ForeColor = Color.Red, Location = new Point(0, 50), AutoSize = true });
            }
        }

        private void ShowCustomerForm()
        {
            var f = new Form { Text = "Musteri Kaydi", Size = new Size(420, 580), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 25;
            f.Controls.Add(new Label { Text = "Yeni Musteri Kaydi", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 45;

            TextBox Field(string label, string ph) {
                f.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
                var t = new TextBox { Location = new Point(30, y), Size = new Size(340, 30), PlaceholderText = ph, BorderStyle = BorderStyle.FixedSingle }; f.Controls.Add(t); y += 42; return t;
            }

            var txtTC = Field("TC Kimlik No:", "11 haneli");
            txtTC.MaxLength = 11;
            var txtAd = Field("Ad:", "Adi");
            var txtSoyad = Field("Soyad:", "Soyadi");
            var txtTel = Field("Telefon:", "05xx...");
            var txtMail = Field("E-Posta:", "mail@ornek.com");
            var txtAdres = Field("Adres:", "Adres bilgisi");

            var btn = new Button { Text = "KAYDET", Location = new Point(30, y + 5), Size = new Size(340, 48), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                try {
                    if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text) || txtTC.Text.Length != 11)
                    { MessageBox.Show("TC, Ad ve Soyad zorunludur."); return; }

                    DataAccess.AddCustomer(txtAd.Text.Trim(), txtSoyad.Text.Trim(), txtTel.Text.Trim(), txtMail.Text.Trim(), "", 0, txtAdres.Text.Trim(), txtTC.Text.Trim());
                    var res = MessageBox.Show($"Musteri kaydedildi! Simdi bu musteri icin rezervasyon yapmak ister misiniz?", "Basarili", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    f.Close();
                    if (res == DialogResult.Yes) {
                        ShowPage("Rezervasyon");
                        // Find txtTC in newer page and fill it
                        foreach (Control c in pnlPageArea.Controls) {
                            if (c is Panel pBody) {
                                foreach (Control c2 in pBody.Controls) {
                                    if (c2 is Panel pForm) {
                                        foreach (Control c3 in pForm.Controls) {
                                            if (c3 is TextBox t && t.PlaceholderText == "11 haneli TC") {
                                                t.Text = txtTC.Text;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn);
            f.ShowDialog();
        }

        // ═══════════════════ DEPO PAGE ═══════════════════
        private void PageStorage(Panel body)
        {
            body.Controls.Add(new Label { Text = "📦 Depo Yönetimi", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true });
            body.Controls.Add(new Label { Text = "Depo Stokları: Ürün tanımı ve depodaki mevcut stok", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(0, 32), AutoSize = true });

            DataGridView dgv;
            try {
                var dt = DataAccess.GetAllStorageStocks();
                if (dt.Columns.Contains("ProductID"))   dt.Columns["ProductID"].ColumnName   = "ID";
                if (dt.Columns.Contains("Barcode"))     dt.Columns["Barcode"].ColumnName     = "Barkod";
                if (dt.Columns.Contains("ItemName"))    dt.Columns["ItemName"].ColumnName    = "Ürün Adı";
                if (dt.Columns.Contains("Price"))       dt.Columns["Price"].ColumnName       = "Fiyat (TL)";
                if (dt.Columns.Contains("Location"))    dt.Columns["Location"].ColumnName    = "Konum";
                if (dt.Columns.Contains("StorageQuantity")) dt.Columns["StorageQuantity"].ColumnName = "Depo Stok";

                dgv = new DataGridView {
                    Name = "dgvStorage", Location = new Point(0, 60), Size = new Size(body.Width - 40, 380), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, DataSource = dt, Font = new Font("Segoe UI", 10)
                };
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); dgv.ColumnHeadersHeight = 38; dgv.RowTemplate.Height = 32; dgv.EnableHeadersVisualStyles = false; dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                body.Controls.Add(dgv);
                body.Controls.Add(new Label { Text = $"Toplam Çeşit: {dt.Rows.Count}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(0, 450), AutoSize = true });
            } catch (Exception ex) { body.Controls.Add(new Label { Text = "Depo verisi yüklenemedi: " + ex.Message, ForeColor = Color.Red, Location = new Point(0, 65), AutoSize = true }); return; }

            int bx = 0;
            var btnAdd = new Button { Text = "📦 ÜRÜN GİRİŞİ", Location = new Point(bx, 480), Size = new Size(160, 48), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => { ShowAddStorageForm(); ShowPage("Depo"); };
            body.Controls.Add(btnAdd); bx += 170;

            var btnSend = new Button { Text = "🛒 MARKETE GÖNDER", Location = new Point(bx, 480), Size = new Size(200, 48), BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0) { ShowTransferForm(dgv.SelectedRows[0]); ShowPage("Depo"); }
                else { MessageBox.Show("Markete göndermek için depodan bir ürün seçin."); }
            };
            body.Controls.Add(btnSend); bx += 210;

            if (AuthHelper.CurrentUser?.Role == "Admin") {
                var btnClear = new Button { Text = "💥 TÜMÜNÜ SİL", Location = new Point(bx, 480), Size = new Size(155, 48), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                btnClear.FlatAppearance.BorderSize = 0;
                btnClear.Click += (s, e) => {
                    if (MessageBox.Show("Depodaki ve Tüm Marketlerdeki ÜRÜN kayıtları silinecek. Emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        try { DataAccess.TruncateStorage(); ShowPage("Depo"); } catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
                    }
                };
                body.Controls.Add(btnClear);
            }
        }

        private void ShowAddStorageForm()
        {
            var f = new Form { Text = "Depoya Ürün Kaydı", Size = new Size(420, 480), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20; f.Controls.Add(new Label { Text = "Depoya Yeni Ürün", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 45;

            TextBox Field(string lbl, string ph, string val = "") {
                f.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
                var t = new TextBox { Text = val, Location = new Point(30, y), Size = new Size(340, 28), PlaceholderText = ph, BorderStyle = BorderStyle.FixedSingle }; f.Controls.Add(t); y += 40; return t;
            }

            var txtBarcode = Field("Barkod:", "Örn: 8691234567890");
            var txtName = Field("Ürün Adı:", "Örn: Coca Cola 1L");
            
            f.Controls.Add(new Label { Text = "Eklenecek Adet:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
            var numQty = new NumericUpDown { Location = new Point(30, y), Size = new Size(160, 28), Minimum = 1, Maximum = 99999, Value = 1 }; f.Controls.Add(numQty); y += 40;

            var txtPrice = Field("Fiyat (TL):", "Örn: 25.50");
            var txtLoc = Field("Konum:", "Örn: A-Raf");

            var btn = new Button { Text = "KAYDET", Location = new Point(30, y), Size = new Size(340, 45), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                try {
                    if (string.IsNullOrWhiteSpace(txtBarcode.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Barkod ve Ürün Adı zorunludur."); return; }
                    decimal price = 0; decimal.TryParse(txtPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price);
                    DataAccess.AddOrUpdateStorageItem(txtBarcode.Text.Trim(), txtName.Text.Trim(), price, txtLoc.Text.Trim(), (int)numQty.Value);
                    MessageBox.Show("Ürün depoya eklendi/güncellendi!", "Başarılı"); f.Close();
                } catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn); f.ShowDialog();
        }

        private void ShowTransferForm(DataGridViewRow row)
        {
            var f = new Form { Text = "Markete Transfer", Size = new Size(420, 360), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20; f.Controls.Add(new Label { Text = "Markete Gönder", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 45;

            var itemName = row.Cells["Ürün Adı"].Value?.ToString();
            var storageQty = Convert.ToInt32(row.Cells["Depo Stok"].Value);
            var productId = Convert.ToInt32(row.Cells["ID"].Value);

            f.Controls.Add(new Label { Text = $"Ürün: {itemName}\nMevcut Depo Stoğu: {storageQty}", Font = new Font("Segoe UI", 10), Location = new Point(30, y), AutoSize = true }); y += 50;

            f.Controls.Add(new Label { Text = "Mağaza (Market ID):", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
            var cbStore = new ComboBox { Location = new Point(30, y), Size = new Size(340, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cbStore.Items.Add("MARKET_1"); cbStore.Items.Add("MARKET_2"); cbStore.SelectedIndex = 0; f.Controls.Add(cbStore); y += 40;

            f.Controls.Add(new Label { Text = "Gönderilecek Adet:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
            var numQty = new NumericUpDown { Location = new Point(30, y), Size = new Size(160, 28), Minimum = 1, Maximum = storageQty == 0 ? 1 : storageQty, Value = 1 }; f.Controls.Add(numQty); y += 40;

            var btn = new Button { Text = "TRANSFER ET", Location = new Point(30, y), Size = new Size(340, 45), BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                try {
                    if (storageQty < numQty.Value) { MessageBox.Show("Depoda yeterli stok yok!"); return; }
                    DataAccess.TransferToMarket(productId, cbStore.Text, (int)numQty.Value, "");
                    MessageBox.Show("Transfer başarılı! Market stoğu güncellendi.", "Başarılı"); f.Close();
                } catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn); f.ShowDialog();
        }

        // ═══════════════════ MARKET PAGE ═══════════════════
        private void PageMarket(Panel body)
        {
            body.Controls.Add(new Label { Text = "🛒 Market & Satış", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true });
            body.Controls.Add(new Label { Text = "Marketteki stoklar ve satış ekranı", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(0, 32), AutoSize = true });

            DataGridView dgv;
            try {
                var dt = DataAccess.GetAllMarketStocks("MARKET_1"); // Örnek olması için default MARKET_1 yükleniyor.
                if (dt.Columns.Contains("ProductID"))   dt.Columns["ProductID"].ColumnName   = "ID";
                if (dt.Columns.Contains("Barcode"))     dt.Columns["Barcode"].ColumnName     = "Barkod";
                if (dt.Columns.Contains("ItemName"))    dt.Columns["ItemName"].ColumnName    = "Ürün Adı";
                if (dt.Columns.Contains("MarketQuantity")) dt.Columns["MarketQuantity"].ColumnName = "Mağaza Stok";
                if (dt.Columns.Contains("Price"))       dt.Columns["Price"].ColumnName       = "Fiyat (TL)";
                
                dgv = new DataGridView {
                    Name = "dgvMarket", Location = new Point(0, 60), Size = new Size(body.Width - 40, 380), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, DataSource = dt, Font = new Font("Segoe UI", 10)
                };
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); dgv.ColumnHeadersHeight = 38; dgv.RowTemplate.Height = 32; dgv.EnableHeadersVisualStyles = false; dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                dgv.DataBindingComplete += (s, e) => {
                    if (dgv.Columns.Contains("StoreID")) dgv.Columns["StoreID"].Visible = false;
                };
                body.Controls.Add(dgv);
                body.Controls.Add(new Label { Text = $"Toplam Çeşit: {dt.Rows.Count}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(0, 450), AutoSize = true });
            } catch (Exception ex) { body.Controls.Add(new Label { Text = "Market verisi yüklenemedi: " + ex.Message, ForeColor = Color.Red, Location = new Point(0, 65), AutoSize = true }); return; }

            int bx = 0;
            var btnSell = new Button { Text = "💳 SATIŞ YAP", Location = new Point(bx, 480), Size = new Size(160, 48), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSell.FlatAppearance.BorderSize = 0;
            btnSell.Click += (s, e) => { ShowSellForm(); ShowPage("Market"); };
            body.Controls.Add(btnSell); bx += 170;

            if (AuthHelper.CurrentUser?.Role == "Admin") {
                var btnClear = new Button { Text = "💥 SIFIRLA", Location = new Point(bx, 480), Size = new Size(155, 48), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                btnClear.FlatAppearance.BorderSize = 0;
                btnClear.Click += (s, e) => {
                    if (MessageBox.Show("Sadece MARKET_1 stoğu silinecek. Emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        try { DataAccess.TruncateMarket(); ShowPage("Market"); } catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
                    }
                };
                body.Controls.Add(btnClear);
            }
        }

        private void ShowSellForm()
        {
            var f = new Form { Text = "Market Satış Ekranı", Size = new Size(420, 360), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20; f.Controls.Add(new Label { Text = "Barkod Okut (Satış)", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 45;

            f.Controls.Add(new Label { Text = "Ürün Barkodu:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
            var txtBarcode = new TextBox { Location = new Point(30, y), Size = new Size(340, 28), BorderStyle = BorderStyle.FixedSingle }; f.Controls.Add(txtBarcode); y += 40;

            f.Controls.Add(new Label { Text = "Adet:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(30, y), AutoSize = true }); y += 22;
            var numQty = new NumericUpDown { Location = new Point(30, y), Size = new Size(160, 28), Minimum = 1, Maximum = 9999, Value = 1 }; f.Controls.Add(numQty); y += 40;

            var btn = new Button { Text = "SATIŞI TAMAMLA", Location = new Point(30, y), Size = new Size(340, 45), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                try {
                    if (string.IsNullOrWhiteSpace(txtBarcode.Text)) { MessageBox.Show("Barkod girmediniz."); return; }
                    DataAccess.SellFromMarket(txtBarcode.Text.Trim(), "MARKET_1", (int)numQty.Value);
                    MessageBox.Show("Satış yapıldı! Market stoğundan düşüldü.", "Başarılı"); f.Close();
                } catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            };
            f.Controls.Add(btn); 
            f.Shown += (s, e) => txtBarcode.Focus(); // barkod okuyucu için direkt odaklan
            f.ShowDialog();
        }
    }
}
