using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PmsSystem.Database;
using MySql.Data.MySqlClient;

namespace PmsSystem.Components
{
    public partial class OnlineReservationsControl : UserControl
    {
        private DataGridView dgvReservations;
        private Label lblTitle;
        private Label lblSubTitle;
        
        // Bottom Panels
        private RoundedPanel pnlGridCard;
        private RoundedPanel pnlChartCard;
        private RoundedPanel pnlActionCard;

        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private ComboBox cmbDate;
        private Panel pnlChartDraw;

        private Button btnTopluOnayla;
        private Button btnMusteriGiris;
        private Button btnEposta;
        private Button btnFatura;

        public OnlineReservationsControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(241, 245, 249);
            InitializeComponentLayout();
            LoadData();
        }

        private void InitializeComponentLayout()
        {
            // Title
            lblTitle = new Label {
                Text = "Online Rezervasyonlar",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            lblSubTitle = new Label {
                Text = "(Web Portal Talepleri)",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(30, 55),
                AutoSize = true
            };
            this.Controls.Add(lblSubTitle);

            // Bottom Area (Chart + Actions)
            int bottomHeight = 320;
            Panel pnlBottomArea = new Panel {
                Dock = DockStyle.Bottom,
                Height = bottomHeight,
                BackColor = Color.Transparent,
                Padding = new Padding(30, 15, 30, 30)
            };
            this.Controls.Add(pnlBottomArea);

            // Grid Area
            pnlGridCard = new RoundedPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                BorderRadius = 15
            };
            Panel pnlGridPadding = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 90, 30, 10)
            };
            pnlGridPadding.Controls.Add(pnlGridCard);
            this.Controls.Add(pnlGridPadding);
            pnlGridPadding.SendToBack();

            // Set up Grid
            dgvReservations = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10.5f),
                GridColor = Color.FromArgb(241, 245, 249),
                RowTemplate = { Height = 55 },
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 50,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            
            dgvReservations.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvReservations.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvReservations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvReservations.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            
            dgvReservations.DefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dgvReservations.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgvReservations.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);

            dgvReservations.CellPainting += DgvReservations_CellPainting;
            dgvReservations.CellContentClick += DgvReservations_CellContentClick;
            dgvReservations.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) dgvReservations.InvalidateCell(e.ColumnIndex, e.RowIndex); };
            dgvReservations.CellMouseLeave += (s, e) => { if (e.RowIndex >= 0) dgvReservations.InvalidateCell(e.ColumnIndex, e.RowIndex); };

            pnlGridCard.Controls.Add(dgvReservations);

            // Set up Bottom Cards
            TableLayoutPanel tlpBottom = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            pnlBottomArea.Controls.Add(tlpBottom);

            // Chart Card
            pnlChartCard = new RoundedPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0),
                BorderRadius = 15,
                Padding = new Padding(20)
            };
            tlpBottom.Controls.Add(pnlChartCard, 0, 0);

            // Chart Controls
            Panel pnlChartFilters = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            pnlChartCard.Controls.Add(pnlChartFilters);

            txtSearch = new TextBox { Width = 250, Font = new Font("Segoe UI", 11), Text = "Rezervasyon Ara...", ForeColor = Color.Gray };
            txtSearch.Location = new Point(0, 10);
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Rezervasyon Ara...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Rezervasyon Ara..."; txtSearch.ForeColor = Color.Gray; } };
            
            cmbStatus = new ComboBox { Width = 150, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Durum", "Bekliyor", "Onaylandı" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Location = new Point(270, 10);
            cmbStatus.SelectedIndexChanged += (s, e) => LoadData();

            cmbDate = new ComboBox { Width = 150, Font = new Font("Segoe UI", 11) };
            cmbDate.Items.AddRange(new object[] { "Bugün", "Dün", "Bu Hafta" });
            cmbDate.Text = "Dün - Bugün";
            cmbDate.Location = new Point(440, 10);

            pnlChartFilters.Controls.Add(txtSearch);
            pnlChartFilters.Controls.Add(cmbStatus);
            pnlChartFilters.Controls.Add(cmbDate);

            pnlChartDraw = new Panel { Dock = DockStyle.Fill };
            pnlChartDraw.Paint += PnlChartDraw_Paint;
            pnlChartCard.Controls.Add(pnlChartDraw);
            pnlChartDraw.BringToFront();

            // Action Card
            pnlActionCard = new RoundedPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(15, 0, 0, 0),
                BorderRadius = 15,
                Padding = new Padding(20)
            };
            tlpBottom.Controls.Add(pnlActionCard, 1, 0);

            Label lblActionTitle = new Label { Text = "Aksiyonlar", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), Dock = DockStyle.Top, Height = 40 };
            pnlActionCard.Controls.Add(lblActionTitle);

            btnTopluOnayla = CreateActionButton("✔ Toplu Onayla", Color.FromArgb(99, 102, 241), Color.White, 0);
            btnMusteriGiris = CreateActionButton("🔑 Müşteri Girişi Yap", Color.FromArgb(34, 197, 94), Color.White, 0);
            btnEposta = CreateActionButton("✉ E-posta Gönder", Color.White, Color.FromArgb(51, 65, 85), 1);
            btnFatura = CreateActionButton("📄 Fatura Oluştur", Color.White, Color.FromArgb(51, 65, 85), 1);

            Panel pnlActionBtns = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            pnlActionCard.Controls.Add(pnlActionBtns);
            pnlActionBtns.BringToFront();

            btnTopluOnayla.Dock = DockStyle.Top;
            Panel sp1 = new Panel { Dock = DockStyle.Top, Height = 12 };
            btnMusteriGiris.Dock = DockStyle.Top;
            Panel sp2 = new Panel { Dock = DockStyle.Top, Height = 12 };
            btnEposta.Dock = DockStyle.Top;
            Panel sp3 = new Panel { Dock = DockStyle.Top, Height = 12 };
            btnFatura.Dock = DockStyle.Top;

            pnlActionBtns.Controls.Add(btnFatura);
            pnlActionBtns.Controls.Add(sp3);
            pnlActionBtns.Controls.Add(btnEposta);
            pnlActionBtns.Controls.Add(sp2);
            pnlActionBtns.Controls.Add(btnMusteriGiris);
            pnlActionBtns.Controls.Add(sp1);
            pnlActionBtns.Controls.Add(btnTopluOnayla);
            
            btnTopluOnayla.Click += BtnConfirm_Click;
            btnMusteriGiris.Click += BtnCheckIn_Click;
        }

        private Button CreateActionButton(string text, Color bg, Color fg, int border)
        {
            Button b = new Button {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Height = 45,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = border;
            b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return b;
        }

        private void PnlChartDraw_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnlChartDraw.Width;
            int h = pnlChartDraw.Height;
            if (w < 100 || h < 50) return;

            // Draw horizontal grid lines
            using (Pen gridPen = new Pen(Color.FromArgb(241, 245, 249), 2))
            {
                for (int i = 0; i <= 5; i++)
                {
                    int y = h - 30 - (int)((h - 50) * (i / 5f));
                    e.Graphics.DrawLine(gridPen, 40, y, w - 20, y);
                    string lbl = (i * 20).ToString();
                    TextRenderer.DrawText(e.Graphics, lbl, new Font("Segoe UI", 9), new Point(10, y - 7), Color.FromArgb(148, 163, 184));
                }
            }

            // Draw X Axis labels
            string[] xLabels = { "1 Ay", "2 Ay", "4 Ay", "6 Ay", "8 Ay", "10 Ay", "2 Ay", "4 Ay", "6 Ay", "8 Ay", "10 Ay" };
            for(int i=0; i<xLabels.Length; i++) {
                int px = 40 + (i * ((w - 60) / 10));
                TextRenderer.DrawText(e.Graphics, xLabels[i], new Font("Segoe UI", 9), new Point(px - 10, h - 20), Color.FromArgb(148, 163, 184));
            }

            // Spline points (Mock aesthetic data matching the image)
            Point[] pts = new Point[11];
            int[] vals = { 25, 45, 20, 30, 18, 65, 33, 28, 85, 80, 25 };
            for (int i = 0; i < 11; i++) {
                int px = 40 + (i * ((w - 60) / 10));
                int py = h - 30 - (int)((h - 50) * (vals[i] / 100f));
                pts[i] = new Point(px, py);
            }

            // Create closed curve for fill
            Point[] fillPts = new Point[13];
            fillPts[0] = new Point(pts[0].X, h - 30);
            for (int i = 0; i < 11; i++) fillPts[i + 1] = pts[i];
            fillPts[12] = new Point(pts[10].X, h - 30);

            using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(100, 56, 189, 248), Color.FromArgb(5, 56, 189, 248), 90f))
            {
                e.Graphics.FillClosedCurve(brush, fillPts, FillMode.Winding, 0.5f);
            }

            using (Pen linePen = new Pen(Color.FromArgb(56, 189, 248), 3))
            {
                e.Graphics.DrawCurve(linePen, pts, 0.5f);
            }

            // Draw dots
            using (SolidBrush dotBrush = new SolidBrush(Color.White))
            using (Pen dotPen = new Pen(Color.FromArgb(56, 189, 248), 2))
            {
                foreach(var p in pts) {
                    e.Graphics.FillEllipse(dotBrush, p.X - 4, p.Y - 4, 8, 8);
                    e.Graphics.DrawEllipse(dotPen, p.X - 4, p.Y - 4, 8, 8);
                }
            }
        }

        private void DgvReservations_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                // Custom Header Drawing
                e.PaintBackground(e.ClipBounds, true);
                string colName = dgvReservations.Columns[e.ColumnIndex].Name;
                string text = "";
                if (colName == "Musteri") text = "👤 MÜŞTERİ ADI";
                else if (colName == "Oda") text = "🚪 ODA";
                else if (colName == "Yatak") text = "🛏️ YATAK";
                else if (colName == "Giris") text = "📅 GİRİŞ TARİHİ";
                else if (colName == "Cikis") text = "📅 ÇIKIŞ TARİHİ";
                else if (colName == "Durum") text = "📋 DURUM";
                else if (colName == "Tutar") text = "💰 TUTAR";
                else if (colName == "ActionColumn") text = "⚡ İŞLEM";

                TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, e.CellBounds, e.CellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                e.Handled = true;
                return;
            }

            if (e.RowIndex >= 0)
            {
                // Draw bottom border for rows
                e.PaintBackground(e.ClipBounds, true);
                using (Pen p = new Pen(Color.FromArgb(241, 245, 249))) {
                    e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                if (dgvReservations.Columns[e.ColumnIndex].Name == "Durum")
                {
                    string val = e.Value?.ToString() ?? "";
                    Color bg = Color.Transparent;
                    Color fg = Color.Black;
                    string text = "";

                    if (val == "Pending") { bg = Color.FromArgb(254, 243, 199); fg = Color.FromArgb(217, 119, 6); text = "Bekliyor"; }
                    else if (val == "Reserved") { bg = Color.FromArgb(220, 252, 231); fg = Color.FromArgb(22, 163, 74); text = "Onaylandı"; }
                    else if (val == "CheckedIn") { bg = Color.FromArgb(224, 242, 254); fg = Color.FromArgb(2, 132, 199); text = "Kayıtlı"; }
                    else return;

                    int badgeWidth = 90;
                    int badgeHeight = 28;
                    Rectangle badgeRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2, badgeWidth, badgeHeight);
                    
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = GetRoundedRect(badgeRect, 14))
                    using (SolidBrush bBg = new SolidBrush(bg))
                    using (Pen p = new Pen(fg, 1f))
                    {
                        e.Graphics.FillPath(bBg, path);
                        e.Graphics.DrawPath(p, path);
                    }
                    TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 9.5f, FontStyle.Bold), badgeRect, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
                else if (dgvReservations.Columns[e.ColumnIndex].Name == "ActionColumn")
                {
                    Rectangle btnRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 12, 100, 30);
                    Point mousePos = dgvReservations.PointToClient(Cursor.Position);
                    bool isHover = e.CellBounds.Contains(mousePos);
                    
                    Color fg = isHover ? Color.FromArgb(79, 70, 229) : Color.FromArgb(100, 116, 139);
                    TextRenderer.DrawText(e.Graphics, "👁 Detaylar", new Font("Segoe UI", 10, FontStyle.Bold), btnRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
                else if (dgvReservations.Columns[e.ColumnIndex].Name == "Tutar")
                {
                    if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal dec)) {
                        TextRenderer.DrawText(e.Graphics, $"💰 {dec:N2}", e.CellStyle.Font, e.CellBounds, Color.FromArgb(71, 85, 105), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                        e.Handled = true;
                    }
                }
                else
                {
                    e.PaintContent(e.ClipBounds);
                    e.Handled = true;
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DgvReservations_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvReservations.Columns[e.ColumnIndex].Name == "ActionColumn")
            {
                int resId = Convert.ToInt32(dgvReservations.Rows[e.RowIndex].Cells["ReservationID"].Value);
                string status = dgvReservations.Rows[e.RowIndex].Cells["Durum"].Value.ToString();
                
                if (status == "Pending") {
                    if (MessageBox.Show("Rezervasyonu onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        DataAccess.ConfirmReservation(resId);
                        MessageBox.Show("Onaylandı. Müşteri geldiğinde Giriş Yap butonunu kullanabilirsiniz.");
                        LoadData();
                    }
                } else if (status == "Reserved") {
                    if (MessageBox.Show("Müşteri girişi yapılsın mı? Bu işlem odayı DOLU hale getirecektir.", "Giriş Onayı", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        DataAccess.PerformCheckIn(resId);
                        MessageBox.Show("Giriş işlemi tamamlandı.");
                        LoadData();
                    }
                }
            }
        }

        private void LoadData()
        {
            try
            {
                string filter = "WHERE r.IsOnline = 1 AND r.Status IN ('Pending', 'Reserved')";
                if (cmbStatus != null) {
                    if (cmbStatus.SelectedIndex == 1) filter = "WHERE r.IsOnline = 1 AND r.Status = 'Pending'";
                    else if (cmbStatus.SelectedIndex == 2) filter = "WHERE r.IsOnline = 1 AND r.Status = 'Reserved'";
                }

                var dt = new DataTable();
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand($@"
                    SELECT r.ReservationID, 
                        CONCAT(c.FirstName, ' ', c.LastName) AS Musteri,
                        rm.RoomNumber AS Oda,
                        r.BedNumber AS Yatak,
                        DATE_FORMAT(r.CheckInDate, '%d.%m.%Y') AS Giris,
                        DATE_FORMAT(r.CheckOutDate, '%d.%m.%Y') AS Cikis,
                        r.Status AS Durum,
                        r.TotalAmount AS Tutar
                    FROM RESERVATIONS r
                    JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                    JOIN ROOMS rm ON r.RoomID = rm.RoomID
                    {filter}
                    ORDER BY r.Status ASC, r.CheckInDate ASC", conn);
                using var da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                
                dgvReservations.DataSource = dt;
                if (dgvReservations.Columns["ReservationID"] != null) dgvReservations.Columns["ReservationID"].Visible = false;

                if (!dgvReservations.Columns.Contains("ActionColumn")) {
                    var btnCol = new DataGridViewButtonColumn();
                    btnCol.Name = "ActionColumn";
                    btnCol.HeaderText = "İşlem";
                    dgvReservations.Columns.Add(btnCol);
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {
            if (dgvReservations.SelectedRows.Count == 0) return;
            int resId = Convert.ToInt32(dgvReservations.SelectedRows[0].Cells["ReservationID"].Value);
            string status = dgvReservations.SelectedRows[0].Cells["Durum"].Value.ToString();
            
            if (status == "Pending") {
                MessageBox.Show("Lütfen önce rezervasyonu onaylayın.");
                return;
            }
            if (status != "Reserved") {
                MessageBox.Show("Giriş işlemi sadece 'Onaylı' rezervasyonlar için yapılabilir.");
                return;
            }

            if (MessageBox.Show("Müşteri girişi yapılsın mı?", "Giriş Onayı", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                DataAccess.PerformCheckIn(resId);
                MessageBox.Show("Giriş işlemi tamamlandı.");
                LoadData();
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (dgvReservations.SelectedRows.Count == 0) return;

            if (MessageBox.Show("Seçili rezervasyonları onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                int count = 0;
                foreach (DataGridViewRow row in dgvReservations.SelectedRows)
                {
                    int resId = Convert.ToInt32(row.Cells["ReservationID"].Value);
                    string status = row.Cells["Durum"].Value.ToString();
                    if (status == "Pending") {
                        DataAccess.ConfirmReservation(resId);
                        count++;
                    }
                }
                
                if (count > 0)
                {
                    MessageBox.Show($"{count} rezervasyon onaylandı. Müşteri geldiğinde Giriş Yap butonunu kullanabilirsiniz.");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Seçili kayıtlar arasında onaylanacak 'Bekliyor' durumunda rezervasyon bulunamadı.");
                }
            }
        }
    }
}
