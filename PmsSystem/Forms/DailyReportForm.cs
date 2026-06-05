using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using PmsSystem.Database;
using PmsSystem.Helpers;

namespace PmsSystem.Forms;

public class DailyReportForm : Panel
{
    private Label lblTitle;
    private Label lblDate;
    private TableLayoutPanel tlpMain;
    private FlowLayoutPanel flpCards;
    private FlowLayoutPanel flpButtons;
    private TableLayoutPanel tlpTables;
    
    private DataGridView dgvTransactions;
    private DataGridView dgvPastReports;
    private Button btnArchivePdf;
    private Button btnDetailsPdf;
    private Button btnCloseDay;

    private Color primaryColor = Color.FromArgb(102, 126, 234);
    private Color secondaryColor = Color.FromArgb(118, 75, 162);
    private Color successColor = Color.FromArgb(34, 197, 94);
    private Color warningColor = Color.FromArgb(245, 158, 11);
    private Color dangerColor = Color.FromArgb(239, 68, 68);
    private Color bgLight = Color.FromArgb(245, 247, 250);
    private Color textDark = Color.FromArgb(31, 41, 55);
    private Color textGray = Color.FromArgb(107, 114, 128);

    public DailyReportForm()
    {
        Dock = DockStyle.Fill;
        BackColor = bgLight;
        AutoScroll = true;

        tlpMain = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(20)
        };
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f)); // Header
        tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons
        tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Cards
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Tables
        
        Controls.Add(tlpMain);

        BuildHeader();
        BuildActionButtons();
        BuildSummaryCards();
        BuildTables();
        
        LoadData();
    }

    private void BuildHeader()
    {
        Panel pnlHeader = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15),
            BackColor = primaryColor
        };

        pnlHeader.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(pnlHeader.ClientRectangle, primaryColor, secondaryColor, 45f);
            e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            
            // Add a subtle pattern or line
            using var pen = new Pen(Color.FromArgb(40, Color.White), 2);
            e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        lblTitle = new Label
        {
            Text = "KASA VE GÜN SONU YÖNETİMİ",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(25, 22)
        };

        lblDate = new Label
        {
            Text = GetTodayDate(),
            Font = new Font("Segoe UI", 13),
            ForeColor = Color.FromArgb(220, Color.White),
            AutoSize = true,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        Button btnRefresh = new Button
        {
            Text = "🔄 Yenile",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(50, Color.White),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 35),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += (s, e) => LoadData();

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblDate);
        pnlHeader.Controls.Add(btnRefresh);

        pnlHeader.Resize += (s, e) => { 
            lblDate.Left = pnlHeader.Width - lblDate.Width - 25; 
            lblDate.Top = 20; 
            btnRefresh.Left = pnlHeader.Width - btnRefresh.Width - 25;
            btnRefresh.Top = 50;
        };

        tlpMain.Controls.Add(pnlHeader, 0, 0);
    }

    private void BuildActionButtons()
    {
        flpButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        btnArchivePdf = CreateActionButton("📋 KASA ARŞİV (PDF)", primaryColor);
        btnDetailsPdf = CreateActionButton("📄 İŞLEM DETAYI (PDF)", primaryColor);
        btnCloseDay = CreateActionButton("🔒 GÜNÜ KAPAT (Z-RAPORU)", dangerColor);

        btnArchivePdf.Click += BtnArchivePdf_Click;
        btnDetailsPdf.Click += BtnDetailsPdf_Click;
        btnCloseDay.Click += BtnCloseDay_Click;

        flpButtons.Controls.Add(btnArchivePdf);
        flpButtons.Controls.Add(btnDetailsPdf);
        flpButtons.Controls.Add(btnCloseDay);

        tlpMain.Controls.Add(flpButtons, 0, 1);
    }

    private Button CreateActionButton(string text, Color bgColor)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = bgColor,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(240, 45),
            Margin = new Padding(0, 0, 15, 10),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void BuildSummaryCards()
    {
        flpCards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };
        tlpMain.Controls.Add(flpCards, 0, 2);
    }

    private void UpdateSummaryCards(decimal cash, decimal cc, decimal exp, decimal rev)
    {
        flpCards.Controls.Clear();
        flpCards.Controls.Add(CreateCard("Nakit Giriş", $"{cash:N2} ₺", successColor, "💵"));
        flpCards.Controls.Add(CreateCard("Kredi Kartı", $"{cc:N2} ₺", primaryColor, "💳"));
        flpCards.Controls.Add(CreateCard("Giderler", $"{exp:N2} ₺", dangerColor, "📉"));
        flpCards.Controls.Add(CreateCard("Net Ciro", $"{rev:N2} ₺", warningColor, "📊"));
    }

    private Panel CreateCard(string title, string value, Color accentColor, string icon)
    {
        var card = new Panel
        {
            Size = new Size(270, 120),
            BackColor = Color.White,
            Margin = new Padding(0, 0, 20, 15)
        };

        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Border
            using var pen = new Pen(Color.FromArgb(230, 235, 245), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            
            // Accent bar
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, 6, card.Height);
            
            // Subtle shadow effect (simulated)
            using var shadowBrush = new SolidBrush(Color.FromArgb(5, 0, 0, 0));
            e.Graphics.FillRectangle(shadowBrush, 5, 5, card.Width - 5, card.Height - 5);
        };

        Label lblTitle = new Label { 
            Text = title, 
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), 
            ForeColor = textGray, 
            AutoSize = true, 
            Location = new Point(22, 22),
            BackColor = Color.Transparent
        };
        
        Label lblValue = new Label { 
            Text = value, 
            Font = new Font("Segoe UI", 20, FontStyle.Bold), 
            ForeColor = textDark, 
            AutoSize = true, 
            Location = new Point(20, 52),
            BackColor = Color.Transparent
        };
        
        Label lblIcon = new Label { 
            Text = icon, 
            Font = new Font("Segoe UI", 28), 
            AutoSize = true, 
            Location = new Point(card.Width - 65, 35),
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(40, accentColor)
        };

        card.Controls.Add(lblTitle);
        card.Controls.Add(lblValue);
        card.Controls.Add(lblIcon);

        return card;
    }

    private void BuildTables()
    {
        tlpTables = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        tlpTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlpTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        Panel pnlLeft = CreateTablePanel("📋 Bugünkü İşlemler", out dgvTransactions);
        Panel pnlRight = CreateTablePanel("📊 Geçmiş Gün Sonu Raporları", out dgvPastReports);

        tlpTables.Controls.Add(pnlLeft, 0, 0);
        tlpTables.Controls.Add(pnlRight, 1, 0);

        tlpMain.Controls.Add(tlpTables, 0, 3);
    }

    private Panel CreateTablePanel(string title, out DataGridView dgv)
    {
        Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };
        
        pnl.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
        };

        Panel pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(248, 250, 252)
        };

        Label lbl = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = textDark,
            Location = new Point(15, 15),
            AutoSize = true
        };
        pnlHeader.Controls.Add(lbl);

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            GridColor = Color.FromArgb(241, 245, 249),
            RowTemplate = { Height = 40 }
        };

        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 250, 252),
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Padding = new Padding(8, 0, 8, 0),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        
        dgv.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = textDark,
            Font = new Font("Segoe UI", 10),
            Padding = new Padding(8, 0, 8, 0),
            SelectionBackColor = Color.FromArgb(241, 245, 249),
            SelectionForeColor = textDark
        };

        pnl.Controls.Add(dgv);
        pnl.Controls.Add(pnlHeader);
        return pnl;
    }

    private void LoadData()
    {
        try
        {
            var totals = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
            UpdateSummaryCards(totals.Item1, totals.Item2, totals.Item3, totals.Item4);

            dgvTransactions.DataSource = EnterpriseDataAccess.GetDailyTransactions(DateTime.Today);
            dgvPastReports.DataSource = EnterpriseDataAccess.GetEndOfDayReports();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnArchivePdf_Click(object sender, EventArgs e)
    {
        try
        {
            DataTable reports = EnterpriseDataAccess.GetEndOfDayReports();
            ReportHelper.GenerateEndOfDayPdf(reports);
        }
        catch (Exception ex) { MessageBox.Show("PDF oluşturma hatası: " + ex.Message); }
    }

    private void BtnDetailsPdf_Click(object sender, EventArgs e)
    {
        try
        {
            var totals = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
            DataTable trans = EnterpriseDataAccess.GetDailyTransactions(DateTime.Today);
            ReportHelper.GenerateDailyTransactionsPdf(trans, totals.Item1, totals.Item2, totals.Item3);
        }
        catch (Exception ex) { MessageBox.Show("PDF oluşturma hatası: " + ex.Message); }
    }

    private void BtnCloseDay_Click(object sender, EventArgs e)
    {
        var totals = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
        string msg = $"💰 GÜN SONU ÖZETİ ({DateTime.Today:dd.MM.yyyy})\n\n" +
                     $"Nakit Giriş: {totals.Item1:N2} ₺\n" +
                     $"Kart Giriş: {totals.Item2:N2} ₺\n" +
                     $"Giderler: {totals.Item3:N2} ₺\n" +
                     $"Net Ciro: {totals.Item4:N2} ₺\n\n" +
                     "Kasayı kapatmak ve yedek almak istiyor musunuz?";

        if (MessageBox.Show(msg, "Gün Sonu Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
            try
            {
                if (EnterpriseDataAccess.CreateEndOfDayReport(DateTime.Today, totals.Item1, totals.Item2, totals.Item3, totals.Item4, AuthHelper.CurrentUser?.FullName ?? "Admin"))
                {
                    try { DatabaseBackupHelper.BackupDatabase(); } catch { }
                    MessageBox.Show("Gün başarıyla kapatıldı ve veriler mühürlendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Bu tarih için zaten gün sonu alınmış.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private string GetTodayDate()
    {
        return DateTime.Now.ToString("dd MMMM yyyy, dddd", new CultureInfo("tr-TR")).ToUpper();
    }
}
