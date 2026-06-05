using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using PmsSystem.Components;
using PmsSystem.Database;
using PmsSystem.Helpers;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WinForms;

namespace PmsSystem.Forms;

public class DashboardForm : Form
{
	private Panel pnlSidebar;

	private Panel pnlPageArea;

	private Panel pnlUser;

	private System.Windows.Forms.Label lblPageTitle;

	private System.Drawing.Color accentBlue = System.Drawing.Color.FromArgb(79, 102, 241);

	private System.Drawing.Color sidebarDark = System.Drawing.Color.FromArgb(85, 102, 133);

	private System.Drawing.Color sidebarSub = System.Drawing.Color.FromArgb(75, 92, 123);

	private System.Drawing.Color sidebarHover = System.Drawing.Color.FromArgb(105, 122, 153);

	private System.Drawing.Color pageBg = System.Drawing.Color.FromArgb(241, 245, 249);

	private System.Drawing.Color successGreen = System.Drawing.Color.FromArgb(16, 185, 129);

	private System.Drawing.Color dangerRed = System.Drawing.Color.FromArgb(239, 68, 68);

	private System.Drawing.Color warningAmber = System.Drawing.Color.FromArgb(245, 158, 11);

	private List<Button> sidebarButtons = new List<Button>();

	private FlowLayoutPanel menuFlow;

	private System.Windows.Forms.Timer _kitchenTimer;

	private System.Windows.Forms.Timer _onlineNotificationTimer;

	private int _lastSeenOnlineResId = -1;

	static DashboardForm()
	{
		Settings.License = LicenseType.Community;
	}

	public DashboardForm()
	{
		Text = "PMS | Pansiyon Yonetim Sistemi";
		base.Size = new System.Drawing.Size(1280, 820);
		base.StartPosition = FormStartPosition.CenterScreen;
		BackColor = pageBg;
		Font = new Font("Segoe UI", 10f);
		Task.Run(() => GeoHelper.PreloadGeoDataAsync());
		BuildSidebar();
		BuildPageArea();
		base.Load += delegate
		{
			ShowPage("Ana Sayfa");
			InitializeOnlineNotificationTimer();
		};
	}

	private void InitializeOnlineNotificationTimer()
	{
		try
		{
			using MySqlConnection mySqlConnection = DatabaseHelper.GetConnection();
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("SELECT MAX(ReservationID) FROM RESERVATIONS WHERE IsOnline = 1", mySqlConnection);
			object obj = mySqlCommand.ExecuteScalar();
			_lastSeenOnlineResId = ((obj != null && obj != DBNull.Value) ? Convert.ToInt32(obj) : 0);
		}
		catch
		{
			_lastSeenOnlineResId = 0;
		}
		_onlineNotificationTimer = new System.Windows.Forms.Timer
		{
			Interval = 10000
		};
		_onlineNotificationTimer.Tick += async delegate
		{
			await CheckNewOnlineReservations();
		};
		_onlineNotificationTimer.Start();
	}

	private async Task CheckNewOnlineReservations()
	{
		try
		{
			DataTable dt = new DataTable();
			await Task.Run(delegate
			{
				using MySqlConnection mySqlConnection = DatabaseHelper.GetConnection();
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("\r\n                        SELECT r.ReservationID, CONCAT(c.FirstName, ' ', c.LastName) as Musteri, rm.RoomNumber\r\n                        FROM RESERVATIONS r\r\n                        JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID\r\n                        JOIN ROOMS rm ON r.RoomID = rm.RoomID\r\n                        WHERE r.IsOnline = 1 AND r.ReservationID > @lastId AND r.Status = 'Pending'\r\n                        ORDER BY r.ReservationID ASC", mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@lastId", _lastSeenOnlineResId);
				using MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				mySqlDataAdapter.Fill(dt);
			});
			if (dt.Rows.Count <= 0)
			{
				return;
			}
			foreach (DataRow row in dt.Rows)
			{
				int resId = Convert.ToInt32(row["ReservationID"]);
				string name = row["Musteri"].ToString();
				string room = row["RoomNumber"].ToString();
				ShowToastNotification("Yeni Online Rezervasyon", name + " isimli müşteri " + room + " nolu oda için rezervasyon yaptı.");
				if (resId > _lastSeenOnlineResId)
				{
					_lastSeenOnlineResId = resId;
				}
			}
			SystemSounds.Exclamation.Play();
		}
		catch
		{
		}
	}

	private void ShowToastNotification(string title, string message)
	{
		SafeInvoke(delegate
		{
			Form toast = new Form
			{
				Text = title,
				Size = new System.Drawing.Size(350, 100),
				FormBorderStyle = FormBorderStyle.None,
				StartPosition = FormStartPosition.Manual,
				BackColor = System.Drawing.Color.FromArgb(34, 36, 44),
				ShowInTaskbar = false,
				TopMost = true
			};
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(0, 0, 20, 20, 180f, 90f);
			graphicsPath.AddArc(toast.Width - 20, 0, 20, 20, 270f, 90f);
			graphicsPath.AddArc(toast.Width - 20, toast.Height - 20, 20, 20, 0f, 90f);
			graphicsPath.AddArc(0, toast.Height - 20, 20, 20, 90f, 90f);
			toast.Region = new Region(graphicsPath);
			System.Windows.Forms.Label label = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udd14 " + title,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(245, 158, 11),
				Location = new Point(15, 15),
				AutoSize = true
			};
			System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
			{
				Text = message,
				Font = new Font("Segoe UI", 9f),
				ForeColor = System.Drawing.Color.White,
				Location = new Point(15, 45),
				Size = new System.Drawing.Size(320, 45)
			};
			toast.Controls.Add(label);
			toast.Controls.Add(label2);
			System.Drawing.Rectangle workingArea = Screen.FromControl(this).WorkingArea;
			toast.Location = new Point(workingArea.Right - toast.Width - 10, workingArea.Bottom);
			toast.Click += delegate
			{
				ShowPage("Online");
				toast.Close();
			};
			label.Click += delegate
			{
				ShowPage("Online");
				toast.Close();
			};
			label2.Click += delegate
			{
				ShowPage("Online");
				toast.Close();
			};
			toast.Show();
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
			{
				Interval = 10
			};
			int targetY = workingArea.Bottom - toast.Height - 10;
			timer.Tick += delegate
			{
				if (toast.Top > targetY)
				{
					toast.Top -= 5;
				}
				else
				{
					timer.Stop();
				}
			};
			timer.Start();
			Task.Delay(8000).ContinueWith(delegate
			{
				SafeInvoke(delegate
				{
					if (!toast.IsDisposed)
					{
						System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer
						{
							Interval = 10
						};
						fadeTimer.Tick += delegate
						{
							if (toast.Opacity > 0.0)
							{
								toast.Opacity -= 0.05;
							}
							else
							{
								fadeTimer.Stop();
								toast.Close();
							}
						};
						fadeTimer.Start();
					}
				});
			});
		});
	}

	private void BuildSidebar()
	{
		pnlSidebar = new Panel
		{
			Width = 260,
			Dock = DockStyle.Left,
			BackColor = System.Drawing.Color.FromArgb(20, 25, 40) // Dark Theme #141928
		};
		base.Controls.Add(pnlSidebar);

		Panel panel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 90,
			BackColor = System.Drawing.Color.FromArgb(15, 19, 31) // Slightly darker for logo area
		};
		
		panel.Paint += delegate(object? s, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			using Pen borderPen = new Pen(System.Drawing.Color.FromArgb(50, 255, 255, 255), 1); // Border Bottom
			e.Graphics.DrawLine(borderPen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
			
			using Font font = new Font("Segoe MDL2 Assets", 22f, System.Drawing.FontStyle.Bold);
			using SolidBrush iconBrush = new SolidBrush(System.Drawing.Color.FromArgb(79, 102, 241));
			e.Graphics.DrawString("\uea8a", font, iconBrush, 20f, 25f);
			
			using Font font2 = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
			e.Graphics.DrawString("SOM-PMS", font2, Brushes.White, 65f, 25f);
		};
		pnlSidebar.Controls.Add(panel);

		menuFlow = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true,
			BackColor = System.Drawing.Color.Transparent,
			Padding = new Padding(10, 15, 10, 20) // Padding on sides for rounded buttons
		};
		pnlSidebar.Controls.Add(menuFlow);
		menuFlow.BringToFront();

		// Home button is special
		AddMenuItem("Ana Sayfa", "Ana Sayfa", "\ue80f", false);

		AddGroupHeader("Resepsiyon");
		AddMenuItem("Konaklayanlar", "Aktif Misafirler", "\ue716", true);
		AddMenuItem("Rezervasyonlar", "Rezervasyonlar", "\ue787", true);
		AddMenuItem("Online", "Online İşlemler", "\ue12a", true);
		AddMenuItem("Takvim", "Görsel Takvim", "\ue9a9", true);
		AddMenuItem("Müşteriler", "Müşteriler", "\ue8fa", true);
		AddMenuItem("Odalar", "Oda Yönetimi", "\ue78a", true);

		AddGroupHeader("Satış & Stok");
		AddMenuItem("Depo", "Depo Stok", "\ue7b8", true);
		AddMenuItem("Lokanta", "Lokanta Satış", "\ue9a9", true);
		AddMenuItem("Mutfak", "Mutfak Ekranı", "\ue8d2", true);

		AddGroupHeader("Muhasebe & Rapor");
		AddMenuItem("Ödeme", "Ödeme & Kasa", "\ue8c7", true);
		AddMenuItem("Giderler", "Gider Takibi", "\ue106", true);
		AddMenuItem("OdaRaporu", "Oda Raporu", "\ue9f9", true);
		AddMenuItem("RestoranRaporu", "Restoran Raporu", "\ue9f9", true);
		AddMenuItem("Analiz", "İşletme Raporu", "\ue9d9", true);
		AddMenuItem("GünSonu", "Gün Sonu", "\ue1dc", true);

		AddGroupHeader("İşletme");
		AddMenuItem("Arizalar", "Teknik Arızalar", "\ue776", true);
		AddMenuItem("Personel", "Çalışan & Vardiya", "\ue716", true);
		AddMenuItem("Housekeeping", "Temizlik Görevleri", "\uea8f", true);

		pnlUser = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 90,
			BackColor = System.Drawing.Color.FromArgb(15, 19, 31),
			Cursor = Cursors.Hand
		};
		pnlUser.Paint += delegate(object? s, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using Pen borderPen = new Pen(System.Drawing.Color.FromArgb(50, 255, 255, 255), 1);
			e.Graphics.DrawLine(borderPen, 0, 0, pnlUser.Width, 0);

			using SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(79, 102, 241));
			e.Graphics.FillEllipse(brush, 15, 20, 48, 48);
			
			string initials = "U";
			if (AuthHelper.CurrentUser != null && !string.IsNullOrEmpty(AuthHelper.CurrentUser.FullName))
			{
				var parts = AuthHelper.CurrentUser.FullName.Split(' ');
				initials = parts[0].Substring(0,1);
				if(parts.Length > 1) initials += parts[parts.Length-1].Substring(0,1);
			}

			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			e.Graphics.DrawString(initials.ToUpper(), new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold), Brushes.White, 23f, 31f);
			
			using Font iconFont = new Font("Segoe MDL2 Assets", 12f);
			e.Graphics.DrawString("\ue712", iconFont, new SolidBrush(System.Drawing.Color.FromArgb(107, 114, 128)), pnlUser.Width - 30, 35f);
		};
		
		System.Windows.Forms.Label label = new System.Windows.Forms.Label
		{
			Text = (AuthHelper.CurrentUser?.FullName ?? "Yönetici"),
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.White,
			Location = new Point(75, 25),
			AutoSize = true
		};
		System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
		{
			Text = (AuthHelper.CurrentUser?.Role ?? "Sistem Admin"),
			Font = new Font("Segoe UI", 8.5f),
			ForeColor = System.Drawing.Color.FromArgb(147, 197, 253),
			Location = new Point(75, 45),
			AutoSize = true
		};
		pnlUser.Controls.AddRange(new Control[2] { label, label2 });
		
		pnlUser.MouseEnter += (s, e) => pnlUser.BackColor = System.Drawing.Color.FromArgb(26, 33, 51);
		pnlUser.MouseLeave += (s, e) => pnlUser.BackColor = System.Drawing.Color.FromArgb(15, 19, 31);
		label.MouseEnter += (s, e) => pnlUser.BackColor = System.Drawing.Color.FromArgb(26, 33, 51);
		label2.MouseEnter += (s, e) => pnlUser.BackColor = System.Drawing.Color.FromArgb(26, 33, 51);

		pnlUser.Click += (s, e) => ShowUserMenu();
		label.Click += (s, e) => ShowUserMenu();
		label2.Click += (s, e) => ShowUserMenu();

		pnlSidebar.Controls.Add(pnlUser);
	}

	private void ShowUserMenu()
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.Items.Add("⚙️ Profil Ayarları", null, delegate { MessageBox.Show("Profil ayarları yakında eklenecek."); });
		contextMenuStrip.Items.Add("🔑 Şifre Değiştir", null, delegate { MessageBox.Show("Şifre değiştirme paneli."); });
		contextMenuStrip.Items.Add("-");
		contextMenuStrip.Items.Add("🚪 Güvenli Çıkış", null, delegate { Application.Restart(); });
		contextMenuStrip.Show(pnlUser, new Point(pnlUser.Width, 0), ToolStripDropDownDirection.AboveRight);
	}

	private void AddGroupHeader(string title)
	{
		System.Windows.Forms.Label lblGroup = new System.Windows.Forms.Label
		{
			Text = title.ToUpper(),
			Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(107, 114, 128), // text-gray-500
			AutoSize = true,
			Margin = new Padding(15, 20, 0, 5)
		};
		menuFlow.Controls.Add(lblGroup);
	}

	private void AddMenuItem(string page, string s, string icon, bool isSub)
	{
		Button b = new Button
		{
			Tag = page,
			Size = new System.Drawing.Size(220, 42),
			FlatStyle = FlatStyle.Flat,
			Cursor = Cursors.Hand,
			Margin = new Padding(5, 2, 5, 2),
			BackColor = System.Drawing.Color.Transparent
		};

		b.FlatAppearance.BorderSize = 0;
		b.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 255, 255, 255);
		b.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(50, 255, 255, 255);

		b.Paint += delegate (object? obj, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			bool isActive = b.BackColor == System.Drawing.Color.FromArgb(79, 102, 241); // Active state color

			// Draw rounded background if active
			if (isActive)
			{
				GraphicsPath gp = new GraphicsPath();
				gp.AddArc(0, 0, 10, 10, 180, 90);
				gp.AddArc(b.Width - 10, 0, 10, 10, 270, 90);
				gp.AddArc(b.Width - 10, b.Height - 10, 10, 10, 0, 90);
				gp.AddArc(0, b.Height - 10, 10, 10, 90, 90);
				gp.CloseAllFigures();
				e.Graphics.FillPath(new SolidBrush(System.Drawing.Color.FromArgb(79, 102, 241)), gp);
			}
			else if (b.ClientRectangle.Contains(b.PointToClient(Cursor.Position)))
			{
				// Hover drawing handled by FlatAppearance
				GraphicsPath gp = new GraphicsPath();
				gp.AddArc(0, 0, 10, 10, 180, 90);
				gp.AddArc(b.Width - 10, 0, 10, 10, 270, 90);
				gp.AddArc(b.Width - 10, b.Height - 10, 10, 10, 0, 90);
				gp.AddArc(0, b.Height - 10, 10, 10, 90, 90);
				gp.CloseAllFigures();
				e.Graphics.FillPath(new SolidBrush(System.Drawing.Color.FromArgb(30, 255, 255, 255)), gp);
			}

			System.Drawing.Color textColor = isActive ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(156, 163, 175); // text-gray-400
			if (!isActive && b.ClientRectangle.Contains(b.PointToClient(Cursor.Position))) textColor = System.Drawing.Color.White;

			using System.Drawing.Font font = new System.Drawing.Font("Segoe MDL2 Assets", 12f);
			e.Graphics.DrawString(icon, font, new SolidBrush(textColor), 15f, 11f);

			using System.Drawing.Font font2 = new System.Drawing.Font("Segoe UI", 10f, isActive ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
			e.Graphics.DrawString(s, font2, new SolidBrush(textColor), 45f, 10f);
		};

		b.MouseEnter += (s, e) => b.Invalidate();
		b.MouseLeave += (s, e) => b.Invalidate();

		b.Click += delegate
		{
			ShowPage(page);
		};
		menuFlow.Controls.Add(b);
		sidebarButtons.Add(b);
	}

	private void BuildPageArea()
	{
		pnlPageArea = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = pageBg
		};
		base.Controls.Add(pnlPageArea);
		pnlPageArea.BringToFront();
	}

	private void Btn_PaintActiveBorder(object? sender, PaintEventArgs e)
	{
		// Not needed anymore with the new design, kept to prevent compilation errors
	}

	private async void ShowPage(string page)
	{
		foreach (Button btn in sidebarButtons)
		{
			if (btn.Tag != null && btn.Tag.ToString() == page)
			{
				btn.BackColor = System.Drawing.Color.FromArgb(79, 102, 241); // Active state
				btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(79, 102, 241);
			}
			else
			{
				btn.BackColor = System.Drawing.Color.Transparent;
				btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 255, 255, 255);
			}
			btn.Invalidate();
		}
		pnlPageArea.Controls.Clear();
		Panel pnlHead = new Panel
		{
			Dock = DockStyle.Top,
			Height = 80,
			BackColor = System.Drawing.Color.White
		};
		pnlHead.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
			e.Graphics.DrawLine(pen, 0, 79, pnlHead.Width, 79);
		};
		Button btnNotify = new Button
		{
			Text = "\ud83d\udd14",
			Size = new System.Drawing.Size(45, 45),
			Location = new Point(pnlHead.Width - 65, 17),
			FlatStyle = FlatStyle.Flat,
			Cursor = Cursors.Hand,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
			Font = new Font("Segoe UI", 14f),
			Anchor = (AnchorStyles.Top | AnchorStyles.Right)
		};
		btnNotify.FlatAppearance.BorderSize = 0;
		try
		{
			DataTable dtCheckIns = EnterpriseDataAccess.GetPendingCheckInsToday();
			DataTable dtCheckOuts = EnterpriseDataAccess.GetPendingCheckOutsToday();
			int checkInCount = dtCheckIns.Rows.Count;
			int checkOutCount = dtCheckOuts.Rows.Count;
			int dirtyCount = DataAccess.GetRooms().AsEnumerable().Count((DataRow r) => r["Status"].ToString() == "Dirty");
			int lowStock = DataAccess.GetCombinedStockStatus().AsEnumerable().Count((DataRow r) => Convert.ToInt32(r["Depo Stok"]) < 3);
			int totalAlerts = dirtyCount + lowStock + checkInCount + checkOutCount;
			if (totalAlerts > 0)
			{
				btnNotify.Paint += delegate(object? s, PaintEventArgs e)
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					using SolidBrush brush = new SolidBrush(System.Drawing.Color.Red);
					e.Graphics.FillEllipse(brush, 25, 5, 18, 18);
					e.Graphics.DrawString(totalAlerts.ToString(), new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold), Brushes.White, (totalAlerts > 9) ? 26 : 30, 7f);
				};
			}
			btnNotify.Click += delegate
			{
				Form fNotify = new Form
				{
					Text = "Bildirimler",
					Size = new System.Drawing.Size(350, 500),
					StartPosition = FormStartPosition.Manual,
					Location = PointToScreen(new Point(base.Width - 370, 150)),
					FormBorderStyle = FormBorderStyle.None,
					ShowInTaskbar = false,
					BackColor = System.Drawing.Color.White
				};
				fNotify.Deactivate += delegate
				{
					fNotify.Close();
				};
				Panel panel = new Panel
				{
					Dock = DockStyle.Fill,
					BorderStyle = BorderStyle.FixedSingle,
					Padding = new Padding(15)
				};
				fNotify.Controls.Add(panel);
				panel.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "\ud83d\udd14 Bildirimler",
					Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
					Dock = DockStyle.Top,
					Height = 40
				});
				FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
				{
					Dock = DockStyle.Fill,
					AutoScroll = true,
					FlowDirection = FlowDirection.TopDown,
					WrapContents = false
				};
				panel.Controls.Add(flowLayoutPanel);
				flowLayoutPanel.BringToFront();
				if (checkInCount > 0)
				{
					flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "\ud83d\udce5 BUGÜN GELECEK MÜŞTERİLER",
						Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(79, 70, 229),
						Size = new System.Drawing.Size(300, 25)
					});
					foreach (DataRow row in dtCheckIns.Rows)
					{
						int resId = Convert.ToInt32(row["ReservationID"]);
						string cName = row["CustomerName"].ToString();
						string text = row["RoomNumber"].ToString();
						string text2 = row["RoomStatus"].ToString();
						Panel panel2 = new Panel
						{
							Size = new System.Drawing.Size(300, 65),
							BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
							Margin = new Padding(0, 0, 0, 8)
						};
						panel2.Controls.Add(new System.Windows.Forms.Label
						{
							Text = cName + " (Oda: " + text + ")",
							Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
							Location = new Point(10, 10),
							AutoSize = true
						});
						if (text2 == "Available" || text2 == "Partial")
						{
							Button button = new Button
							{
								Text = "Girişi Onayla",
								Size = new System.Drawing.Size(100, 25),
								Location = new Point(10, 32),
								BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
								ForeColor = System.Drawing.Color.White,
								FlatStyle = FlatStyle.Flat
							};
							button.FlatAppearance.BorderSize = 0;
							button.Cursor = Cursors.Hand;
							button.Click += delegate
							{
								try
								{
									DataAccess.ConfirmCheckInToday(resId);
									MessageBox.Show(cName + " için giriş onaylandı.");
									fNotify.Close();
									ShowPage("Ana Sayfa");
								}
								catch (Exception ex2)
								{
									MessageBox.Show("Hata: " + ex2.Message);
								}
							};
							panel2.Controls.Add(button);
						}
						else
						{
							panel2.Controls.Add(new System.Windows.Forms.Label
							{
								Text = "⚠\ufe0f Oda Dolu / Kirli!",
								ForeColor = System.Drawing.Color.Red,
								Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
								Location = new Point(10, 35),
								AutoSize = true
							});
						}
						flowLayoutPanel.Controls.Add(panel2);
					}
				}
				if (checkOutCount > 0)
				{
					flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "\ud83d\udce4 BUGÜN ÇIKIŞ YAPACAKLAR",
						Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(245, 158, 11),
						Size = new System.Drawing.Size(300, 25),
						Margin = new Padding(0, 15, 0, 0)
					});
					foreach (DataRow row2 in dtCheckOuts.Rows)
					{
						int resId2 = Convert.ToInt32(row2["ReservationID"]);
						string cName2 = row2["CustomerName"].ToString();
						string rNum = row2["RoomNumber"].ToString();
						Panel panel3 = new Panel
						{
							Size = new System.Drawing.Size(300, 65),
							BackColor = System.Drawing.Color.FromArgb(254, 252, 232),
							Margin = new Padding(0, 0, 0, 8)
						};
						panel3.Controls.Add(new System.Windows.Forms.Label
						{
							Text = cName2 + " (Oda: " + rNum + ")",
							Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
							Location = new Point(10, 10),
							AutoSize = true
						});
						Button button2 = new Button
						{
							Text = "Çıkış İşlemi",
							Size = new System.Drawing.Size(100, 25),
							Location = new Point(10, 32),
							BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
							ForeColor = System.Drawing.Color.White,
							FlatStyle = FlatStyle.Flat
						};
						button2.FlatAppearance.BorderSize = 0;
						button2.Cursor = Cursors.Hand;
						button2.Click += delegate
						{
							fNotify.Close();
							ShowCheckoutDialog(resId2, cName2, rNum);
						};
						panel3.Controls.Add(button2);
						flowLayoutPanel.Controls.Add(panel3);
					}
				}
				if (dirtyCount > 0)
				{
					flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = $"\ud83e\uddf9 {dirtyCount} oda temizlik bekliyor.",
						Size = new System.Drawing.Size(300, 30),
						ForeColor = System.Drawing.Color.FromArgb(154, 52, 18),
						Margin = new Padding(0, 10, 0, 0)
					});
				}
				if (lowStock > 0)
				{
					flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = $"\ud83d\udce6 {lowStock} ürünün stoğu azalıyor.",
						Size = new System.Drawing.Size(300, 30),
						ForeColor = System.Drawing.Color.Red
					});
				}
				if (totalAlerts == 0)
				{
					flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "Tüm işler yolunda! ✨",
						Size = new System.Drawing.Size(300, 30),
						ForeColor = System.Drawing.Color.Gray
					});
				}
				fNotify.Show();
			};
		}
		catch
		{
		}
		pnlHead.Controls.Add(btnNotify);
		pnlPageArea.Controls.Add(pnlHead);
		lblPageTitle = new System.Windows.Forms.Label
		{
			Text = page.ToUpper(),
			Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
			Location = new Point(30, 25),
			AutoSize = true
		};
		pnlHead.Controls.Add(lblPageTitle);
		System.Windows.Forms.Label lblDate = new System.Windows.Forms.Label
		{
			Text = DateTime.Now.ToString("dd MMMM yyyy, dddd"),
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(32, 53),
			AutoSize = true
		};
		pnlHead.Controls.Add(lblDate);
		string userText = ((AuthHelper.CurrentUser != null) ? (AuthHelper.CurrentUser.FullName ?? "") : "Sistem Yöneticisi");
		System.Windows.Forms.Label lblUser = new System.Windows.Forms.Label
		{
			Text = userText,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			AutoSize = true
		};
		lblUser.Location = new Point(pnlHead.Width - 180, 28);
		pnlHead.Controls.Add(lblUser);
		Button btnAddReg = new Button
		{
			Text = "+ Yeni Kayıt",
			Size = new System.Drawing.Size(120, 36),
			Location = new Point(pnlHead.Width - 320, 22),
			BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand,
			Anchor = (AnchorStyles.Top | AnchorStyles.Right)
		};
		btnAddReg.FlatAppearance.BorderSize = 0;
		btnAddReg.Click += delegate
		{
			ShowReservationForm();
		};
		pnlHead.Controls.Add(btnAddReg);
		Panel pnlBody = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = pageBg,
			Padding = new Padding(20, 20, 20, 20),
			AutoScroll = true
		};
		pnlPageArea.Controls.Add(pnlBody);
		pnlBody.BringToFront();
		try
		{
			switch (page)
			{
			case "Ana Sayfa":
				await Task.Run(delegate
				{
					PageDashboard(pnlBody);
				});
				break;
			case "Konaklayanlar":
				await Task.Run(delegate
				{
					PageReservations(pnlBody);
				});
				break;
			case "Rezervasyonlar":
				await Task.Run(delegate
				{
					PageBookings(pnlBody);
				});
				break;
			case "Online":
				SafeInvoke(delegate
				{
					pnlBody.Controls.Clear();
					pnlBody.Controls.Add(new OnlineReservationsControl
					{
						Dock = DockStyle.Fill
					});
				});
				break;
			case "Odalar":
				await Task.Run(delegate
				{
					PageRooms(pnlBody);
				});
				break;
			case "Müşteriler":
				await Task.Run(delegate
				{
					PageCustomers(pnlBody);
				});
				break;
			case "Depo":
				await Task.Run(delegate
				{
					PageStorage(pnlBody);
				});
				break;
			case "Lokanta":
				await Task.Run(delegate
				{
					PageLokanta(pnlBody);
				});
				break;
			case "Mutfak":
				await Task.Run(delegate
				{
					PageKitchen(pnlBody);
				});
				break;
			case "Takvim":
				await Task.Run(delegate
				{
					PageCalendar(pnlBody);
				});
				break;
			case "Ödeme":
				await Task.Run(delegate
				{
					PagePayments(pnlBody);
				});
				break;
			case "Giderler":
				await Task.Run(delegate
				{
					PageExpenses(pnlBody);
				});
				break;
			case "Arizalar":
				await Task.Run(delegate
				{
					PageMaintenance(pnlBody);
				});
				break;
			case "OdaRaporu":
				await Task.Run(delegate
				{
					PageRoomReport(pnlBody);
				});
				break;
			case "RestoranRaporu":
				await Task.Run(delegate
				{
					PageRestaurantReport(pnlBody);
				});
				break;
			case "Analiz":
				await Task.Run(delegate
				{
					PageReports(pnlBody);
				});
				break;
			case "Personel":
				await Task.Run(delegate
				{
					PageEmployees(pnlBody);
				});
				break;
			case "Housekeeping":
				await Task.Run(delegate
				{
					PageHousekeeping(pnlBody);
				});
				break;
			case "GünSonu":
				await Task.Run(delegate
				{
					PageEndOfDay(pnlBody);
				});
				break;
			case "Payments":
				ShowPage("Ödeme");
				break;
			default:
				pnlBody.Controls.Add(new System.Windows.Forms.Label
				{
					Text = page + " modulu yakin zamanda...",
					AutoSize = true,
					Location = new Point(30, 30)
				});
				break;
			}
		}
		catch (Exception ex)
		{
			pnlBody.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Hata: " + ex.Message,
				ForeColor = System.Drawing.Color.Red,
				AutoSize = true,
				Location = new Point(20, 20)
			});
		}
	}

	private void SafeInvoke(Action action)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (base.InvokeRequired)
		{
			if (base.IsHandleCreated)
			{
				Invoke(action);
				return;
			}
			EventHandler handler = null;
			handler = delegate
			{
				base.HandleCreated -= handler;
				SafeInvoke(action);
			};
			base.HandleCreated += handler;
		}
		else
		{
			action();
		}
	}

	private void PageDashboard(Panel body)
	{
		DataTable rooms = DataAccess.GetAllRooms();
		(decimal todayRevenue, int checkInsToday, int checkOutsToday, int activeGuests) kpi = EnterpriseDataAccess.GetTodayKPIs();
		List<(DateTime, decimal)> weeklyRevenueTrend = EnterpriseDataAccess.GetWeeklyRevenueTrend();
		(int total, int occupied, int available, int dirty, int maintenance) occ = EnterpriseDataAccess.GetOccupancySummary();
		DataTable lowStock;
		try
		{
			lowStock = EnterpriseDataAccess.GetLowStockAlerts();
		}
		catch
		{
			lowStock = new DataTable();
		}
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.AutoScroll = true;
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			int item = occ.total;
			int item2 = occ.available;
			int item3 = occ.dirty;
			int item4 = occ.maintenance;
			int item5 = occ.occupied;
			Panel pnlKpis = new Panel
			{
				Location = new Point(0, 0),
				Size = new System.Drawing.Size(body.Width - 30, 110),
				BackColor = System.Drawing.Color.Transparent,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			body.Controls.Add(pnlKpis);
			AddKpi("\ud83d\udcb0", "BUGÜNKÜ GELİR", kpi.todayRevenue.ToString("N0") + " ₺", System.Drawing.Color.FromArgb(16, 185, 129), 0);
			AddKpi("\ud83d\udece\ufe0f", "AKTİF MİSAFİR", kpi.activeGuests.ToString(), System.Drawing.Color.FromArgb(99, 102, 241), 195);
			AddKpi("\ud83d\udce5", "BUGÜN GİRİŞ", kpi.checkInsToday.ToString(), System.Drawing.Color.FromArgb(59, 130, 246), 390);
			AddKpi("\ud83d\udce4", "BUGÜN ÇIKIŞ", kpi.checkOutsToday.ToString(), System.Drawing.Color.FromArgb(245, 158, 11), 585);
			AddKpi("\ud83c\udfe8", "TOPLAM ODA", item.ToString(), System.Drawing.Color.FromArgb(100, 116, 139), 780);
			int num = 120;
			if (lowStock.Rows.Count > 0)
			{
				RoundedPanel roundedPanel = new RoundedPanel
				{
					Location = new Point(0, num),
					Size = new System.Drawing.Size(body.Width - 30, 50),
					BackColor = System.Drawing.Color.FromArgb(254, 243, 199),
					BorderRadius = 10,
					Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
				};
				body.Controls.Add(roundedPanel);
				string text = string.Join("  •  ", (from r in lowStock.AsEnumerable()
					select $"{r["Ürün"]} ({r["Stok"]} adet)").Take(5));
				roundedPanel.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "⚠\ufe0f  DÜŞÜK STOK ALARMI:  " + text,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(92, 67, 0),
					Location = new Point(15, 15),
					AutoSize = true
				});
				num += 60;
			}
			num += 10;
			DataTable todaysArrivals = DataAccess.GetTodaysArrivals();
			if (todaysArrivals.Rows.Count > 0)
			{
				RoundedPanel roundedPanel2 = new RoundedPanel
				{
					Location = new Point(0, num),
					Size = new System.Drawing.Size(body.Width - 30, 50),
					BackColor = System.Drawing.Color.FromArgb(209, 231, 221),
					BorderRadius = 10,
					Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
					Cursor = Cursors.Hand
				};
				body.Controls.Add(roundedPanel2);
				roundedPanel2.Click += delegate
				{
					ShowPage("Online");
				};
				string text2 = string.Join(", ", from r in todaysArrivals.AsEnumerable()
					select $"{r["Musteri"]} (Oda {r["RoomNumber"]})");
				roundedPanel2.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "\ud83d\udd14  BUGÜN GELECEK MİSAFİRLER:  " + text2 + " — Check-In yapmak için buraya tıklayın.",
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(20, 108, 67),
					Location = new Point(15, 15),
					AutoSize = true,
					Cursor = Cursors.Hand
				});
				num += 60;
			}
			Panel panel = new Panel
			{
				Location = new Point(0, num),
				BackColor = System.Drawing.Color.FromArgb(243, 244, 246),
				Padding = new Padding(15)
			};
			panel.Size = new System.Drawing.Size(body.Width - 30, Math.Max(400, body.Height - num - 20));
			panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panel.AutoScroll = true;
			body.Controls.Add(panel);
			int num2 = 10;
			IOrderedEnumerable<IGrouping<int, DataRow>> orderedEnumerable = from r in rooms.AsEnumerable()
				group r by (r["FloorNumber"] != DBNull.Value) ? Convert.ToInt32(r["FloorNumber"]) : 0 into g
				orderby g.Key
				select g;
			foreach (IGrouping<int, DataRow> item6 in orderedEnumerable)
			{
				int key = item6.Key;
				System.Windows.Forms.Label value = new System.Windows.Forms.Label
				{
					Text = $"{key}.kat",
					Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
					Location = new Point(20, num2),
					AutoSize = true
				};
				panel.Controls.Add(value);
				num2 += 30;
				FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
				{
					Location = new Point(20, num2),
					AutoSize = true,
					MaximumSize = new System.Drawing.Size(panel.Width - 40, 0),
					WrapContents = true
				};
				panel.Controls.Add(flowLayoutPanel);
				foreach (DataRow item7 in item6)
				{
					string num3 = item7["RoomNumber"].ToString();
					string st = item7["Status"].ToString();
					string tip = item7["OdaTipi"].ToString();
					decimal price = Convert.ToDecimal(item7["Fiyat"]);
					decimal toplamTutar = ((item7["ToplamTutar"] != DBNull.Value) ? Convert.ToDecimal(item7["ToplamTutar"]) : 0m);
					int kalinanGun = ((item7["KalinanGun"] != DBNull.Value) ? Convert.ToInt32(item7["KalinanGun"]) : 0);
					string guestNames = ((item7["Musteriler"] != DBNull.Value) ? item7["Musteriler"].ToString() : "");
					DateTime? inDate = ((item7["GirisTarihi"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(item7["GirisTarihi"])) : ((DateTime?)null));
					DateTime? outDate = ((item7["CikisTarihi"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(item7["CikisTarihi"])) : ((DateTime?)null));
					DateTime? nextResDate = ((item7["NextResDate"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(item7["NextResDate"])) : ((DateTime?)null));
					int capacity = Convert.ToInt32(item7["Capacity"]);
					int occupied = Convert.ToInt32(item7["OccupiedBeds"]);
					AddRoom(flowLayoutPanel, num3, st, tip, price, toplamTutar, kalinanGun, guestNames, inDate, outDate, nextResDate, capacity, occupied);
				}
				num2 += flowLayoutPanel.PreferredSize.Height + 20;
				Panel panel2 = new Panel
				{
					Location = new Point(20, num2),
					Size = new System.Drawing.Size(panel.Width - 50, 1),
					BackColor = System.Drawing.Color.LightGray
				};
				panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				panel.Controls.Add(panel2);
				num2 += 20;
			}
			void AddKpi(string icon, string label, string text3, System.Drawing.Color accent, int xPos)
			{
				RoundedPanel card = new RoundedPanel
				{
					Location = new Point(xPos, 5),
					Size = new System.Drawing.Size(180, 95),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12
				};
				card.Paint += delegate(object? s, PaintEventArgs e)
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					using SolidBrush brush = new SolidBrush(accent);
					GraphicsPath graphicsPath = new GraphicsPath();
					graphicsPath.AddArc(0, 0, 12, 12, 180f, 90f);
					graphicsPath.AddLine(6, 0, 6, card.Height);
					graphicsPath.AddArc(0, card.Height - 12, 12, 12, 90f, 90f);
					graphicsPath.CloseFigure();
					e.Graphics.FillPath(brush, graphicsPath);
				};
				card.Controls.Add(new System.Windows.Forms.Label
				{
					Text = icon,
					Font = new Font("Segoe UI", 16f),
					ForeColor = accent,
					Location = new Point(50, 12),
					AutoSize = true
				});
				card.Controls.Add(new System.Windows.Forms.Label
				{
					Text = text3,
					Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
					Location = new Point(48, 38),
					AutoSize = true
				});
				card.Controls.Add(new System.Windows.Forms.Label
				{
					Text = label,
					Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
					Location = new Point(48, 72),
					AutoSize = true
				});
				pnlKpis.Controls.Add(card);
			}
		});
	}

	private void PageReports(Panel body)
	{
		DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		DateTime end = DateTime.Today;
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = System.Drawing.Color.White,
				Padding = new Padding(20, 10, 20, 10)
			};
			body.Controls.Add(panel);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcca RAPORLAR",
				Font = new Font("Segoe UI Semibold", 16f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(25, 10),
				AutoSize = true
			};
			panel.Controls.Add(value);
			FlowLayoutPanel flowFilters = new FlowLayoutPanel
			{
				Location = new Point(25, 50),
				Width = panel.Width - 50,
				Height = 45,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			panel.Controls.Add(flowFilters);
			DateTimePicker dt1 = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Width = 110,
				Value = start,
				Font = new Font("Segoe UI", 9f)
			};
			DateTimePicker dt2 = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Width = 110,
				Value = end,
				Font = new Font("Segoe UI", 9f)
			};
			System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
			{
				Text = "-",
				AutoSize = true,
				Padding = new Padding(0, 5, 0, 0)
			};
			flowFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Tarih Aralığı: ",
				AutoSize = true,
				Padding = new Padding(0, 5, 0, 0),
				Font = new Font("Segoe UI", 9f)
			});
			flowFilters.Controls.Add(dt1);
			flowFilters.Controls.Add(value2);
			flowFilters.Controls.Add(dt2);
			Panel pnlMain = new Panel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				Padding = new Padding(25)
			};
			body.Controls.Add(pnlMain);
			pnlMain.BringToFront();
			Action refresh = delegate
			{
				RenderImageStyleReport(pnlMain, dt1.Value, dt2.Value);
			};
			string[] btnLabels = new string[4] { "Bugün", "Bu Hafta", "Bu Ay", "Özel Aralık" };
			string[] array = btnLabels;
			foreach (string l in array)
			{
				Button b = new Button
				{
					Text = l,
					Height = 34,
					Width = 95,
					FlatStyle = FlatStyle.Flat,
					BackColor = System.Drawing.Color.White,
					ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					Margin = new Padding(8, 0, 0, 0),
					Cursor = Cursors.Hand
				};
				b.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
				b.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
				b.Click += delegate
				{
					foreach (Control control in flowFilters.Controls)
					{
						if (control is Button button2 && Enumerable.Contains(btnLabels, button2.Text))
						{
							button2.BackColor = System.Drawing.Color.White;
						}
					}
					b.BackColor = System.Drawing.Color.FromArgb(238, 242, 255);
					if (l == "Bugün")
					{
						dt1.Value = DateTime.Today;
						dt2.Value = DateTime.Today;
					}
					else if (l == "Bu Hafta")
					{
						dt1.Value = DateTime.Today.AddDays(0 - DateTime.Today.DayOfWeek + ((DateTime.Today.DayOfWeek != DayOfWeek.Sunday) ? 1 : (-6)));
						dt2.Value = DateTime.Today;
					}
					else if (l == "Bu Ay")
					{
						dt1.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
						dt2.Value = DateTime.Today;
					}
					refresh();
				};
				flowFilters.Controls.Add(b);
			}
			flowFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Oda Tipi:",
				AutoSize = true,
				Padding = new Padding(20, 5, 0, 0),
				Font = new Font("Segoe UI", 9f)
			});
			ComboBox comboBox = new ComboBox
			{
				Width = 110,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			comboBox.Items.Add("Tümü");
			comboBox.SelectedIndex = 0;
			flowFilters.Controls.Add(comboBox);
			flowFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Ödeme Türü:",
				AutoSize = true,
				Padding = new Padding(15, 5, 0, 0),
				Font = new Font("Segoe UI", 9f)
			});
			ComboBox comboBox2 = new ComboBox
			{
				Width = 100,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			ComboBox.ObjectCollection items = comboBox2.Items;
			object[] items2 = new string[3] { "Tümü", "Nakit", "Kredi Kartı" };
			items.AddRange(items2);
			comboBox2.SelectedIndex = 0;
			flowFilters.Controls.Add(comboBox2);
			Button button = new Button
			{
				Text = "\ud83d\udd04 Raporu Yenile",
				Width = 140,
				Height = 34,
				BackColor = accentBlue,
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Margin = new Padding(20, 0, 0, 0)
			};
			button.FlatAppearance.BorderSize = 0;
			button.Click += delegate
			{
				refresh();
			};
			flowFilters.Controls.Add(button);
			refresh();
		});
	}

	private void RenderImageStyleReport(Panel container, DateTime start, DateTime end)
	{
		SafeInvoke(delegate
		{
			container.Controls.Clear();
			int num = 0;
			int w = container.Width - 40;
			DataTable comprehensiveFinanceReport = EnterpriseDataAccess.GetComprehensiveFinanceReport(start, end);
			decimal num2 = (from r in comprehensiveFinanceReport.AsEnumerable()
				where r["Category"].ToString() == "Oda Gelirleri"
				select r).Sum((DataRow r) => Convert.ToDecimal(r["Amount"]));
			decimal num3 = (from r in comprehensiveFinanceReport.AsEnumerable()
				where r["Category"].ToString() == "Restoran Gelirleri"
				select r).Sum((DataRow r) => Convert.ToDecimal(r["Amount"]));
			decimal num4 = (from r in comprehensiveFinanceReport.AsEnumerable()
				where r["Category"].ToString() == "Ekstra Hizmetler"
				select r).Sum((DataRow r) => Convert.ToDecimal(r["Amount"]));
			decimal num5 = num2 + num3 + num4;
			decimal num6 = (from r in comprehensiveFinanceReport.AsEnumerable()
				where r["Type"].ToString() == "Gider"
				select r).Sum((DataRow r) => Convert.ToDecimal(r["Amount"]));
			(int, int, int, int, int) occupancySummary = EnterpriseDataAccess.GetOccupancySummary();
			(string, string) roomUsageAnalysisSummary = EnterpriseDataAccess.GetRoomUsageAnalysisSummary(start, end);
			DataTable detailedReservationReport = EnterpriseDataAccess.GetDetailedReservationReport(start, end);
			DataTable detailedRestaurantSales = EnterpriseDataAccess.GetDetailedRestaurantSales(start, end);
			(decimal, int, int, int) todayKPIs = EnterpriseDataAccess.GetTodayKPIs();
			Panel panel = new Panel
			{
				Location = new Point(0, num),
				Width = w,
				Height = 85,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			container.Controls.Add(panel);
			panel.BringToFront();
			int num7 = (w - 75) / 6;
			AddPremiumCard(panel, 0, "TOPLAM GELİR", $"₺{num5:N0}", "Toplam Kazanç", "\ud83d\udcb0", System.Drawing.Color.FromArgb(59, 130, 246), num7);
			AddPremiumCard(panel, num7 + 15, "ODA GELİRİ", $"₺{num2:N0}", $"%{((num5 > 0m) ? (num2 * 100m / num5) : 0m):N1} ↑", "\ud83c\udfe8", System.Drawing.Color.FromArgb(34, 197, 94), num7);
			AddPremiumCard(panel, (num7 + 15) * 2, "RESTORAN GELİRİ", $"₺{num3:N0}", $"%{((num5 > 0m) ? (num3 * 100m / num5) : 0m):N1} ↑", "\ud83c\udf74", System.Drawing.Color.FromArgb(249, 115, 22), num7);
			AddPremiumCard(panel, (num7 + 15) * 3, "TOPLAM MÜŞTERİ", todayKPIs.Item4.ToString(), "Aktif Müşteri", "\ud83d\udc65", System.Drawing.Color.FromArgb(168, 85, 247), num7);
			AddPremiumCard(panel, (num7 + 15) * 4, "DOLU ODA", occupancySummary.Item2.ToString(), $"%{((occupancySummary.Item1 > 0) ? (occupancySummary.Item2 * 100 / occupancySummary.Item1) : 0)} Doluluk", "✅", System.Drawing.Color.FromArgb(20, 184, 166), num7);
			AddPremiumCard(panel, (num7 + 15) * 5, "BOŞ ODA", occupancySummary.Item3.ToString(), $"%{((occupancySummary.Item1 > 0) ? (occupancySummary.Item3 * 100 / occupancySummary.Item1) : 0)} Boş", "\ud83d\udeaa", System.Drawing.Color.FromArgb(239, 68, 68), num7);
			num += 100;
			int num8 = 380;
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(0, num),
				Width = (int)((double)w * 0.35),
				Height = num8,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left)
			};
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83c\udfe8 ODA RAPORU",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Location = new Point(20, 15),
				AutoSize = true
			});
			Button value = new Button
			{
				Text = "Detaylı Rapor",
				Location = new Point(roundedPanel.Width - 110, 12),
				Size = new System.Drawing.Size(100, 25),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 7f),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			roundedPanel.Controls.Add(value);
			DataGridView dataGridView = new DataGridView
			{
				Location = new Point(15, 50),
				Size = new System.Drawing.Size(roundedPanel.Width - 30, num8 - 140),
				DataSource = detailedReservationReport,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
				RowTemplate = 
				{
					Height = 28
				}
			};
			dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
			dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
			dataGridView.EnableHeadersVisualStyles = false;
			roundedPanel.Controls.Add(dataGridView);
			Panel panel2 = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 80,
				Padding = new Padding(15, 5, 15, 5)
			};
			roundedPanel.Controls.Add(panel2);
			Action<Panel, string, string, int, int> action = delegate(Panel p, string label, string val, int sx, int sy)
			{
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = label,
					Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Regular),
					Location = new Point(sx, sy),
					AutoSize = true,
					ForeColor = System.Drawing.Color.Gray
				});
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					Location = new Point(sx, sy + 15),
					AutoSize = true
				});
			};
			action(panel2, "T.Oda", occupancySummary.Item1.ToString(), 10, 10);
			action(panel2, "Dolu", occupancySummary.Item2.ToString(), 70, 10);
			action(panel2, "Boş", occupancySummary.Item3.ToString(), 125, 10);
			action(panel2, "Doluluk", $"%{((occupancySummary.Item1 > 0) ? (occupancySummary.Item2 * 100 / occupancySummary.Item1) : 0)}", 180, 10);
			action(panel2, "Geceleme", roomUsageAnalysisSummary.Item1, 250, 10);
			action(panel2, "Gelir", $"₺{num2:N0}", 330, 10);
			container.Controls.Add(roundedPanel);
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Location = new Point((int)((double)w * 0.365), num),
				Width = (int)((double)w * 0.35),
				Height = num8,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12
			};
			roundedPanel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83c\udf74 RESTORAN SATIŞ RAPORU",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Location = new Point(20, 15),
				AutoSize = true
			});
			Button value2 = new Button
			{
				Text = "Detaylı Rapor",
				Location = new Point(roundedPanel2.Width - 110, 12),
				Size = new System.Drawing.Size(100, 25),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 7f),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			roundedPanel2.Controls.Add(value2);
			DataGridView dataGridView2 = new DataGridView
			{
				Location = new Point(15, 50),
				Size = new System.Drawing.Size(roundedPanel2.Width - 30, num8 - 140),
				DataSource = detailedRestaurantSales,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
				RowTemplate = 
				{
					Height = 28
				}
			};
			dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
			dataGridView2.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
			dataGridView2.EnableHeadersVisualStyles = false;
			roundedPanel2.Controls.Add(dataGridView2);
			Panel panel3 = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 80
			};
			roundedPanel2.Controls.Add(panel3);
			action(panel3, "Toplam Ürün Satışı", detailedRestaurantSales.AsEnumerable().Sum((DataRow r) => Convert.ToInt32(r["Adet"])).ToString(), 20, 10);
			action(panel3, "Toplam Ciro", $"₺{num3:N2}", 220, 10);
			container.Controls.Add(roundedPanel2);
			RoundedPanel pFin = new RoundedPanel
			{
				Location = new Point((int)((double)w * 0.73), num),
				Width = (int)((double)w * 0.27),
				Height = num8,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			pFin.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcb2 FİNANSAL ÖZET",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Location = new Point(20, 15),
				AutoSize = true
			});
			Button value3 = new Button
			{
				Text = "Detaylı Rapor",
				Location = new Point(pFin.Width - 110, 12),
				Size = new System.Drawing.Size(100, 25),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 7f),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			pFin.Controls.Add(value3);
			int fy = 55;
			Action<string, decimal, System.Drawing.Color, bool, bool> action2 = delegate(string txt, decimal val, System.Drawing.Color c, bool isTitle, bool isResult)
			{
				System.Windows.Forms.Label label = new System.Windows.Forms.Label
				{
					Text = txt,
					Font = new Font("Segoe UI", 8.5f, isTitle ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
					Location = new Point(20, fy),
					AutoSize = true,
					ForeColor = (isTitle ? System.Drawing.Color.FromArgb(71, 85, 105) : System.Drawing.Color.Black)
				};
				System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
				{
					Text = (isTitle ? "Tutar (₺)" : $"{val:N2}"),
					Font = new Font("Segoe UI", 8.5f, isTitle ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
					Location = new Point(pFin.Width - 100, fy),
					AutoSize = true,
					ForeColor = (isResult ? c : (isTitle ? System.Drawing.Color.FromArgb(71, 85, 105) : System.Drawing.Color.Black)),
					TextAlign = ContentAlignment.TopRight
				};
				pFin.Controls.Add(label);
				pFin.Controls.Add(label2);
				if (isResult)
				{
					label.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
					label2.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
				}
				fy += 24;
			};
			action2("GELİRLER", 0m, System.Drawing.Color.Gray, arg4: true, arg5: false);
			action2("Oda Gelirleri", num2, System.Drawing.Color.Black, arg4: false, arg5: false);
			action2("Restoran Gelirleri", num3, System.Drawing.Color.Black, arg4: false, arg5: false);
			action2("Ekstra Hizmet Gelirleri", num4, System.Drawing.Color.Black, arg4: false, arg5: false);
			action2("Toplam Gelir", num5, System.Drawing.Color.FromArgb(34, 197, 94), arg4: false, arg5: true);
			fy += 10;
			action2("GİDERLER", 0m, System.Drawing.Color.Gray, arg4: true, arg5: false);
			foreach (DataRow item in from r in comprehensiveFinanceReport.AsEnumerable()
				where r["Type"].ToString() == "Gider"
				select r)
			{
				action2(item["Category"].ToString(), Convert.ToDecimal(item["Amount"]), System.Drawing.Color.Black, arg4: false, arg5: false);
			}
			action2("Toplam Gider", num6, System.Drawing.Color.FromArgb(239, 68, 68), arg4: false, arg5: true);
			fy += 15;
			Panel panel4 = new Panel
			{
				Location = new Point(10, fy),
				Width = pFin.Width - 20,
				Height = 40,
				BackColor = System.Drawing.Color.FromArgb(240, 253, 244)
			};
			panel4.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "NET KÂR",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Location = new Point(10, 10),
				AutoSize = true,
				ForeColor = System.Drawing.Color.FromArgb(21, 128, 61)
			});
			panel4.Controls.Add(new System.Windows.Forms.Label
			{
				Text = $"₺{num5 - num6:N2}",
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Location = new Point(panel4.Width - 130, 8),
				AutoSize = true,
				ForeColor = System.Drawing.Color.FromArgb(21, 128, 61),
				TextAlign = ContentAlignment.TopRight
			});
			pFin.Controls.Add(panel4);
			container.Controls.Add(pFin);
			num += num8 + 15;
			int num9 = 300;
			Panel panel5 = new Panel
			{
				Location = new Point(0, num),
				Width = w,
				Height = num9,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			container.Controls.Add(panel5);
			RoundedPanel roundedPanel3 = new RoundedPanel
			{
				Location = new Point(0, 0),
				Width = (int)((double)w * 0.35),
				Height = num9,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12
			};
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "GELİR GRAFİĞİ (Günlük)",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(15, 15),
				AutoSize = true
			});
			FormsPlot formsPlot = new FormsPlot
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10, 40, 10, 10)
			};
			DataTable dailyRevenueTrend = EnterpriseDataAccess.GetDailyRevenueTrend(start, end);
			if (dailyRevenueTrend.Rows.Count > 0)
			{
				double[] array = ((IEnumerable<DataRow>)dailyRevenueTrend.AsEnumerable()).Select((Func<DataRow, int, double>)((DataRow r, int i) => i)).ToArray();
				double[] ys = (from r in dailyRevenueTrend.AsEnumerable()
					select Convert.ToDouble(r["Revenue"])).ToArray();
				string[] dates = (from r in dailyRevenueTrend.AsEnumerable()
					select ((DateTime)r["Date"]).ToString("dd.MM")).ToArray();
				Scatter scatter = formsPlot.Plot.Add.Scatter(array, ys);
				scatter.LineWidth = 2f;
				scatter.Color = ScottPlot.Color.FromHex("#3B82F6");
				formsPlot.Plot.Axes.Bottom.TickGenerator = new NumericManual(array.Select((double x, int i) => new Tick(x, dates[i])).ToArray());
			}
			roundedPanel3.Controls.Add(formsPlot);
			panel5.Controls.Add(roundedPanel3);
			RoundedPanel roundedPanel4 = new RoundedPanel
			{
				Location = new Point((int)((double)w * 0.365), 0),
				Width = (int)((double)w * 0.25),
				Height = num9,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12
			};
			roundedPanel4.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "ODA DOLULUK ORANI",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(15, 15),
				AutoSize = true
			});
			FormsPlot formsPlot2 = new FormsPlot
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10, 40, 10, 10)
			};
			List<PieSlice> list = new List<PieSlice>();
			if (occupancySummary.Item2 > 0)
			{
				list.Add(new PieSlice
				{
					Value = occupancySummary.Item2,
					FillColor = ScottPlot.Color.FromHex("#22C55E"),
					Label = $"Dolu Oda: {occupancySummary.Item2}"
				});
			}
			if (occupancySummary.Item3 > 0)
			{
				list.Add(new PieSlice
				{
					Value = occupancySummary.Item3,
					FillColor = ScottPlot.Color.FromHex("#E2E8F0"),
					Label = $"Boş Oda: {occupancySummary.Item3}"
				});
			}
			Pie pie = formsPlot2.Plot.Add.Pie(list);
			pie.ExplodeFraction = 0.05;
			formsPlot2.Plot.HideGrid();
			formsPlot2.Plot.Axes.Frameless();
			roundedPanel4.Controls.Add(formsPlot2);
			panel5.Controls.Add(roundedPanel4);
			RoundedPanel roundedPanel5 = new RoundedPanel
			{
				Location = new Point((int)((double)w * 0.63), 0),
				Width = (int)((double)w * 0.37),
				Height = num9,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			roundedPanel5.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "EN ÇOK SATILAN ÜRÜNLER",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(15, 15),
				AutoSize = true
			});
			FormsPlot formsPlot3 = new FormsPlot
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10, 40, 10, 10)
			};
			DataTable topSoldProducts = EnterpriseDataAccess.GetTopSoldProducts(start, end);
			if (topSoldProducts.Rows.Count > 0)
			{
				double[] values = (from r in topSoldProducts.AsEnumerable()
					select Convert.ToDouble(r["Count"])).Reverse().ToArray();
				string[] labels = (from r in topSoldProducts.AsEnumerable()
					select r["Product"].ToString()).Reverse().ToArray();
				BarPlot barPlot = formsPlot3.Plot.Add.Bars(values);
				foreach (Bar bar in barPlot.Bars)
				{
					bar.FillColor = ScottPlot.Color.FromHex("#3B82F6");
				}
				formsPlot3.Plot.Axes.Left.TickGenerator = new NumericManual((from i in Enumerable.Range(0, labels.Length)
					select new Tick(i, labels[i])).ToArray());
				formsPlot3.Plot.Axes.Margins(0.0, 0.0);
			}
			roundedPanel5.Controls.Add(formsPlot3);
			panel5.Controls.Add(roundedPanel5);
			num += num9 + 20;
			Panel pFooter = new Panel
			{
				Location = new Point(0, num),
				Width = w,
				Height = 60,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			container.Controls.Add(pFooter);
			AddAction("PDF Olarak Kaydet", "\ud83d\udcc4", System.Drawing.Color.FromArgb(16, 185, 129), 0, delegate
			{
				ExportReportToPdf(start, end);
			});
			AddAction("Excel'e Aktar", "\ud83d\udcca", System.Drawing.Color.FromArgb(34, 197, 94), w / 3, delegate
			{
				ExportReportToExcel(start, end);
			});
			AddAction("E-posta ile Gönder", "✉\ufe0f", System.Drawing.Color.FromArgb(249, 115, 22), (int)((double)w * 0.66), delegate
			{
				SendReportEmail(start, end);
			});
			num += 80;
			container.AutoScrollMinSize = new System.Drawing.Size(w, num);
			void AddAction(string txt, string icon, System.Drawing.Color col, int x, Action click)
			{
				Button button = new Button
				{
					Text = icon + " " + txt,
					Location = new Point(x, 0),
					Size = new System.Drawing.Size(w / 3 - 20, 45),
					BackColor = col,
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				button.FlatAppearance.BorderSize = 0;
				button.Click += delegate
				{
					click();
				};
				pFooter.Controls.Add(button);
			}
		});
	}

	private void ExportReportToExcel(DateTime start, DateTime end)
	{
		try
		{
			DataTable comprehensiveFinanceReport = EnterpriseDataAccess.GetComprehensiveFinanceReport(start, end);
			DataTable detailedReservationReport = EnterpriseDataAccess.GetDetailedReservationReport(start, end);
			DataTable detailedRestaurantSales = EnterpriseDataAccess.GetDetailedRestaurantSales(start, end);
			using XLWorkbook xLWorkbook = new XLWorkbook();
			IXLWorksheet iXLWorksheet = xLWorkbook.Worksheets.Add("Finansal Özet");
			iXLWorksheet.Cell(1, 1).Value = "Kategori";
			iXLWorksheet.Cell(1, 2).Value = "Tutar";
			iXLWorksheet.Cell(1, 3).Value = "Tür";
			iXLWorksheet.Range(1, 1, 1, 3).Style.Font.Bold = true;
			iXLWorksheet.Cell(2, 1).InsertData(comprehensiveFinanceReport.AsEnumerable());
			iXLWorksheet.Columns().AdjustToContents();
			IXLWorksheet iXLWorksheet2 = xLWorkbook.Worksheets.Add("Oda Raporu");
			iXLWorksheet2.Cell(1, 1).InsertTable(detailedReservationReport);
			iXLWorksheet2.Columns().AdjustToContents();
			IXLWorksheet iXLWorksheet3 = xLWorkbook.Worksheets.Add("Restoran Raporu");
			iXLWorksheet3.Cell(1, 1).InsertTable(detailedRestaurantSales);
			iXLWorksheet3.Columns().AdjustToContents();
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Excel Dosyası|*.xlsx",
				FileName = $"Isletme_Raporu_{DateTime.Now:yyyyMMdd}.xlsx"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				xLWorkbook.SaveAs(saveFileDialog.FileName);
				MessageBox.Show("Rapor başarıyla Excel'e aktarıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Excel Aktarma Hatası: " + ex.Message);
		}
	}

	private void ExportReportToPdf(DateTime start, DateTime end)
	{
		try
		{
			DataTable fin = EnterpriseDataAccess.GetComprehensiveFinanceReport(start, end);
			DataTable dtRoom = EnterpriseDataAccess.GetDetailedReservationReport(start, end);
			DataTable detailedRestaurantSales = EnterpriseDataAccess.GetDetailedRestaurantSales(start, end);
			Document document = Document.Create(delegate(IDocumentContainer container)
			{
				container.Page(delegate(PageDescriptor page)
				{
					page.Size(PageSizes.A4);
					page.Margin(1f, Unit.Centimetre);
					page.Header().Text("PMS İŞLETME RAPORU").FontSize(20f)
						.Bold()
						.FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
					page.Content().Column(delegate(ColumnDescriptor col)
					{
						col.Spacing(10f);
						col.Item().Text($"Rapor Aralığı: {start:dd.MM.yyyy} - {end:dd.MM.yyyy}").FontSize(10f);
						col.Item().Text("FİNANSAL ÖZET").Bold()
							.FontSize(14f);
						col.Item().Table(delegate(TableDescriptor table)
						{
							table.ColumnsDefinition(delegate(TableColumnsDefinitionDescriptor columns)
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});
							table.Header(delegate(TableCellDescriptor header)
							{
								header.Cell().Text("Kategori").Bold();
								header.Cell().Text("Tutar").Bold();
								header.Cell().Text("Tür").Bold();
							});
							foreach (DataRow row in fin.Rows)
							{
								table.Cell().Text(row["Category"].ToString());
								table.Cell().Text(Convert.ToDecimal(row["Amount"]).ToString("N2") + " ₺");
								table.Cell().Text(row["Type"].ToString());
							}
						});
						col.Item().PageBreak();
						col.Item().Text("ODA RAPORU DETAYI").Bold()
							.FontSize(14f);
						col.Item().Table(delegate(TableDescriptor table)
						{
							table.ColumnsDefinition(delegate(TableColumnsDefinitionDescriptor c)
							{
								for (int i = 0; i < dtRoom.Columns.Count; i++)
								{
									c.RelativeColumn();
								}
							});
							foreach (DataColumn column in dtRoom.Columns)
							{
								table.Cell().Text(column.ColumnName).Bold()
									.FontSize(8f);
							}
							foreach (DataRow row2 in dtRoom.Rows)
							{
								object[] itemArray = row2.ItemArray;
								foreach (object obj in itemArray)
								{
									table.Cell().Text(obj?.ToString() ?? "").FontSize(7f);
								}
							}
						});
					});
					page.Footer().AlignCenter().Text(delegate(TextDescriptor x)
					{
						x.Span("Sayfa ");
						x.CurrentPageNumber();
					});
				});
			});
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "PDF Dosyası|*.pdf",
				FileName = $"Isletme_Raporu_{DateTime.Now:yyyyMMdd}.pdf"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				document.GeneratePdf(saveFileDialog.FileName);
				MessageBox.Show("Rapor başarıyla PDF olarak kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("PDF Oluşturma Hatası: " + ex.Message);
		}
	}

	private void SendReportEmail(DateTime start, DateTime end)
	{
		MessageBox.Show("E-posta gönderim özelliği için lütfen sistem yöneticinizden SMTP ayarlarını isteyin. Mevcut durumda raporu PDF olarak kaydedip manuel olarak gönderebilirsiniz.", "E-posta Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void AddPremiumCard(Panel parent, int x, string title, string val, string sub, string icon, System.Drawing.Color color, int w)
	{
		RoundedPanel roundedPanel = new RoundedPanel
		{
			Location = new Point(x, 0),
			Size = new System.Drawing.Size(w, 85),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 12
		};
		parent.Controls.Add(roundedPanel);
		Panel panel = new Panel
		{
			Location = new Point(12, 12),
			Size = new System.Drawing.Size(34, 34),
			BackColor = System.Drawing.Color.FromArgb(20, color)
		};
		panel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = icon,
			Font = new Font("Segoe UI", 14f),
			Location = new Point(4, 4),
			ForeColor = color,
			AutoSize = true
		});
		roundedPanel.Controls.Add(panel);
		roundedPanel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = title,
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(55, 15),
			AutoSize = true
		});
		roundedPanel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = val,
			Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
			Location = new Point(54, 32),
			AutoSize = true
		});
		roundedPanel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = sub,
			Font = new Font("Segoe UI", 7f),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(55, 52),
			AutoSize = true
		});
	}

	private void AddStat(Panel body, ref int x, string title, string val, System.Drawing.Color color, string icon = "\ud83d\udcca")
	{
		RoundedPanel p = new RoundedPanel
		{
			Size = new System.Drawing.Size(245, 120),
			Location = new Point(x, 50),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 20
		};
		p.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(243, 244, 246), 1f);
			e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
			using LinearGradientBrush brush = new LinearGradientBrush(new System.Drawing.Rectangle(0, 20, 4, p.Height - 40), color, System.Drawing.Color.FromArgb(100, color), 90f);
			e.Graphics.FillRectangle(brush, 0, 20, 4, p.Height - 40);
		};
		p.Controls.Add(new System.Windows.Forms.Label
		{
			Text = title,
			Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(25, 20),
			AutoSize = true
		});
		string text = val;
		if ((val.Contains("$") || title.Contains("GELİR") || title.Contains("HASILAT") || title.Contains("CİRO")) && decimal.TryParse(val.Replace("$", "").Replace("₺", "").Replace("TL", "")
			.Trim(), out var result))
		{
			text = result.ToString("N0") + " ₺";
		}
		p.Controls.Add(new System.Windows.Forms.Label
		{
			Text = text,
			Font = new Font("Segoe UI", 24f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
			Location = new Point(22, 42),
			AutoSize = true
		});
		RoundedPanel roundedPanel = new RoundedPanel
		{
			Size = new System.Drawing.Size(45, 45),
			Location = new Point(p.Width - 60, 15),
			BackColor = System.Drawing.Color.FromArgb(15, color),
			BorderRadius = 22
		};
		roundedPanel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = icon,
			Font = new Font("Segoe UI", 14f),
			Location = new Point(8, 8),
			AutoSize = true,
			ForeColor = color
		});
		p.Controls.Add(roundedPanel);
		body.Controls.Add(p);
		x += 265;
	}

	private void AddRoom(FlowLayoutPanel flow, string num, string st, string tip, decimal price, decimal toplamTutar, int kalinanGun, string guestNames, DateTime? inDate, DateTime? outDate, DateTime? nextResDate, int capacity, int occupied)
	{
		System.Drawing.Color backColor = System.Drawing.Color.White;
		System.Drawing.Color foreColor = System.Drawing.Color.FromArgb(30, 41, 59);
		System.Drawing.Color color = System.Drawing.Color.FromArgb(71, 85, 105);
		System.Drawing.Color backColor2 = System.Drawing.Color.FromArgb(239, 68, 68);
		if (st == "Occupied")
		{
			backColor = System.Drawing.Color.FromArgb(134, 239, 172);
			foreColor = System.Drawing.Color.FromArgb(20, 83, 45);
		}
		else if (st == "Partial")
		{
			backColor = System.Drawing.Color.FromArgb(96, 165, 250);
			foreColor = System.Drawing.Color.FromArgb(30, 58, 138);
		}
		else if (st == "Dirty")
		{
			backColor = System.Drawing.Color.FromArgb(254, 215, 170);
			foreColor = System.Drawing.Color.FromArgb(154, 52, 18);
		}
		else if (st == "Maintenance")
		{
			backColor = System.Drawing.Color.FromArgb(226, 232, 240);
			foreColor = System.Drawing.Color.FromArgb(71, 85, 105);
		}
		decimal num2 = toplamTutar;
		bool flag = kalinanGun >= 5;
		if (flag && toplamTutar <= 0m && kalinanGun > 0)
		{
			num2 = price * (decimal)kalinanGun;
		}
		int height = ((st != "Available" && inDate.HasValue) ? 185 : 165);
		RoundedPanel p = new RoundedPanel
		{
			Size = new System.Drawing.Size(170, height),
			Margin = new Padding(6),
			BackColor = backColor,
			Cursor = Cursors.Hand,
			BorderRadius = 12
		};
		p.Paint += delegate(object? s, PaintEventArgs e)
		{
			if (st == "Available")
			{
				using (Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 2f))
				{
					e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
				}
			}
		};
		System.Windows.Forms.Label value = new System.Windows.Forms.Label
		{
			Text = "⏷ İşlem",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			ForeColor = foreColor,
			Location = new Point(105, 10),
			AutoSize = true,
			Cursor = Cursors.Hand
		};
		p.Controls.Add(value);
		System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
		{
			Text = ((st != "Available") ? "\ud83d\udecf" : ""),
			Font = new Font("Segoe UI", 12f),
			ForeColor = foreColor,
			Location = new Point(10, 10),
			AutoSize = true
		};
		p.Controls.Add(value2);
		System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
		{
			Text = num,
			Font = new Font("Segoe UI", 24f, System.Drawing.FontStyle.Bold),
			ForeColor = foreColor,
			Location = new Point(10, 30),
			Size = new System.Drawing.Size(165, 40),
			TextAlign = ContentAlignment.MiddleCenter
		};
		p.Controls.Add(value3);
		System.Windows.Forms.Label value4 = new System.Windows.Forms.Label
		{
			Text = tip.ToUpper(),
			Font = new Font("Segoe UI", 8f),
			ForeColor = foreColor,
			Location = new Point(15, 72),
			Size = new System.Drawing.Size(155, 18),
			TextAlign = ContentAlignment.MiddleCenter
		};
		p.Controls.Add(value4);
		System.Windows.Forms.Label value5 = new System.Windows.Forms.Label
		{
			Text = $"\ud83d\udc64 {occupied}/{capacity} Yatak Dolu",
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			ForeColor = ((occupied >= capacity) ? System.Drawing.Color.Red : ((occupied > 0) ? System.Drawing.Color.FromArgb(30, 58, 138) : System.Drawing.Color.FromArgb(16, 185, 129))),
			Location = new Point(15, 90),
			Size = new System.Drawing.Size(155, 15),
			TextAlign = ContentAlignment.MiddleCenter
		};
		p.Controls.Add(value5);
		if (st != "Available" && inDate.HasValue && outDate.HasValue && !string.IsNullOrEmpty(guestNames))
		{
			System.Windows.Forms.Label value6 = new System.Windows.Forms.Label
			{
				Text = guestNames,
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = foreColor,
				Location = new Point(8, 108),
				Size = new System.Drawing.Size(169, 18),
				TextAlign = ContentAlignment.MiddleCenter,
				AutoEllipsis = true
			};
			p.Controls.Add(value6);
			string text = ((kalinanGun <= 0) ? "Bugün girdi" : $"{kalinanGun} Gecelik Konaklama");
			System.Windows.Forms.Label value7 = new System.Windows.Forms.Label
			{
				Text = "\ud83c\udf19 " + text,
				Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
				ForeColor = (flag ? System.Drawing.Color.FromArgb(120, 53, 15) : color),
				Location = new Point(8, 128),
				Size = new System.Drawing.Size(169, 16),
				TextAlign = ContentAlignment.MiddleCenter
			};
			p.Controls.Add(value7);
			string text2 = $"{inDate.Value:dd MMM} → {outDate.Value:dd MMM}";
			System.Windows.Forms.Label value8 = new System.Windows.Forms.Label
			{
				Text = "\ud83d\uddd3 " + text2,
				Font = new Font("Segoe UI", 7f),
				ForeColor = color,
				Location = new Point(8, 146),
				Size = new System.Drawing.Size(169, 16),
				TextAlign = ContentAlignment.MiddleCenter
			};
			p.Controls.Add(value8);
			string text3 = ((num2 > 0m) ? $"Oda Fiyatı: {num2:N0} ₺" : $"Oda Fiyatı: {price:N0} ₺/gece");
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Size = new System.Drawing.Size(169, 22),
				Location = new Point(8, 162),
				BackColor = backColor2,
				BorderRadius = 11
			};
			System.Windows.Forms.Label value9 = new System.Windows.Forms.Label
			{
				Text = text3,
				Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.White,
				Location = new Point(0, 4),
				Size = new System.Drawing.Size(169, 14),
				TextAlign = ContentAlignment.MiddleCenter
			};
			roundedPanel.Controls.Add(value9);
			p.Controls.Add(roundedPanel);
			if (flag)
			{
				RoundedPanel roundedPanel2 = new RoundedPanel
				{
					Size = new System.Drawing.Size(169, 18),
					Location = new Point(8, 177),
					BackColor = System.Drawing.Color.FromArgb(245, 158, 11),
					BorderRadius = 9
				};
				System.Windows.Forms.Label value10 = new System.Windows.Forms.Label
				{
					Text = $"⚡ {kalinanGun} Gün — Uzun Konaklama",
					Font = new Font("Segoe UI", 6f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.White,
					Location = new Point(0, 3),
					Size = new System.Drawing.Size(169, 12),
					TextAlign = ContentAlignment.MiddleCenter
				};
				roundedPanel2.Controls.Add(value10);
				p.Controls.Add(roundedPanel2);
			}
			p.Paint += delegate(object? s, PaintEventArgs e)
			{
				if (inDate.HasValue && outDate.HasValue)
				{
					DateTime value12 = inDate.Value;
					DateTime value13 = outDate.Value;
					DateTime today = DateTime.Today;
					double num3 = (value13 - value12).TotalDays;
					if (num3 <= 0.0)
					{
						num3 = 1.0;
					}
					double num4 = (today - value12).TotalDays;
					if (num4 < 0.0)
					{
						num4 = 0.0;
					}
					if (num4 > num3)
					{
						num4 = num3;
					}
					float num5 = (float)(num4 / num3);
					int num6 = p.Width - 16;
					int width = (int)((float)num6 * num5);
					e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(40, System.Drawing.Color.Black)), 8, p.Height - 8, num6, 4);
					e.Graphics.FillRectangle(new SolidBrush((num5 > 0.9f) ? System.Drawing.Color.Red : System.Drawing.Color.FromArgb(99, 102, 241)), 8, p.Height - 8, width, 4);
				}
			};
		}
		else
		{
			if (nextResDate.HasValue)
			{
				System.Windows.Forms.Label value11 = new System.Windows.Forms.Label
				{
					Text = "\ud83d\udcc5 GELECEK: " + nextResDate.Value.ToString("dd MMM"),
					Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(245, 158, 11),
					Location = new Point(8, 95),
					Size = new System.Drawing.Size(169, 16),
					TextAlign = ContentAlignment.MiddleCenter
				};
				p.Controls.Add(value11);
			}
			System.Windows.Forms.Label label = new System.Windows.Forms.Label
			{
				Text = "MÜSAİT",
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(16, 185, 129),
				Location = new Point(0, 125),
				Size = new System.Drawing.Size(185, 30),
				TextAlign = ContentAlignment.MiddleCenter
			};
			if (nextResDate.HasValue)
			{
				label.Location = new Point(0, 130);
			}
			p.Controls.Add(label);
		}
		p.Click += delegate
		{
			DoOpen();
		};
		foreach (Control control in p.Controls)
		{
			control.Click += delegate
			{
				DoOpen();
			};
			control.Cursor = Cursors.Hand;
		}
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.Items.Add("\ud83e\uddf9 Temizlendi İşaretle", null, delegate
		{
			DataAccess.SetRoomStatus(num, "Available");
			ShowPage("Ana Sayfa");
		});
		contextMenuStrip.Items.Add("\ud83d\udd27 Bakıma Al", null, delegate
		{
			DataAccess.SetRoomStatus(num, "Maintenance");
			ShowPage("Ana Sayfa");
		});
		contextMenuStrip.Items.Add("\ud83d\udd27 Bakım Bitti (Kirli)", null, delegate
		{
			DataAccess.SetRoomStatus(num, "Dirty");
			ShowPage("Ana Sayfa");
		});
		p.ContextMenuStrip = contextMenuStrip;
		flow.Controls.Add(p);
		void DoOpen()
		{
			if (st == "Available")
			{
				Form fChoice = new Form
				{
					Text = "Oda İşlemi Seçin",
					Size = new System.Drawing.Size(320, 200),
					StartPosition = FormStartPosition.CenterParent,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					MaximizeBox = false,
					MinimizeBox = false,
					BackColor = System.Drawing.Color.White
				};
				System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
				{
					Text = "\ud83c\udfe8 Oda " + num + " - Lütfen işlem seçin:",
					Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
					Location = new Point(25, 20),
					AutoSize = true
				};
				Button button = new Button
				{
					Text = "\ud83d\udfe2 WALK-IN (Hemen Giriş)",
					Location = new Point(25, 60),
					Size = new System.Drawing.Size(255, 38),
					BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				button.FlatAppearance.BorderSize = 0;
				Button button2 = new Button
				{
					Text = "\ud83d\udcc5 REZERVASYON (Gelecek Tarih)",
					Location = new Point(25, 105),
					Size = new System.Drawing.Size(255, 38),
					BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				button2.FlatAppearance.BorderSize = 0;
				button.Click += delegate
				{
					fChoice.Close();
					ShowReservationForm(num);
					ShowPage("Ana Sayfa");
				};
				button2.Click += delegate
				{
					fChoice.Close();
					ShowReservationForm(num, "", isWalkIn: false);
					ShowPage("Ana Sayfa");
				};
				fChoice.Controls.AddRange(new Control[3] { label2, button, button2 });
				fChoice.ShowDialog();
			}
			else if (st == "Dirty")
			{
				DialogResult dialogResult = MessageBox.Show(num + " nolu oda kirli. Temizlendi olarak işaretlensin mi?", "Oda Temizliği", MessageBoxButtons.YesNo);
				if (dialogResult == DialogResult.Yes)
				{
					DataAccess.SetRoomStatus(num, "Available");
					ShowPage("Ana Sayfa");
				}
			}
			else if (st == "Maintenance")
			{
				DialogResult dialogResult2 = MessageBox.Show(num + " nolu oda bakımda. Bakım tamamlandı mı?", "Oda Bakımı", MessageBoxButtons.YesNo);
				if (dialogResult2 == DialogResult.Yes)
				{
					DataAccess.SetRoomStatus(num, "Dirty");
					ShowPage("Ana Sayfa");
				}
			}
			else if (st == "Occupied" || st == "Partial")
			{
				if (capacity > 1)
				{
					ShowBedSelectionDialog(num, capacity, guestNames);
				}
				else
				{
					MessageBox.Show(num + " nolu oda şu an dolu.\nMisafirler: " + guestNames, "Oda Bilgisi");
				}
			}
		}
	}

	private void ShowBedSelectionDialog(string roomNumber, int capacity, string guestNames)
	{
		Form f = new Form
		{
			Text = $"🚪 Oda {roomNumber} - Yatak Seçim Paneli",
			Size = new System.Drawing.Size(460, 480),
			StartPosition = FormStartPosition.CenterParent,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false,
			MinimizeBox = false,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252)
		};

		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 65,
			BackColor = System.Drawing.Color.FromArgb(30, 41, 59)
		};
		pnlHeader.Controls.Add(new System.Windows.Forms.Label
		{
			Text = $"🛏️ Oda {roomNumber} Yatak Dağılımı",
			Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.White,
			Location = new Point(20, 18),
			AutoSize = true
		});
		f.Controls.Add(pnlHeader);

		FlowLayoutPanel flpBeds = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true,
			Padding = new Padding(15, 15, 15, 15),
			BackColor = System.Drawing.Color.Transparent
		};
		f.Controls.Add(flpBeds);
		flpBeds.BringToFront();

		Dictionary<int, string> occupants = new Dictionary<int, string>();
		try
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();
			using var cmd = new MySqlCommand(@"
				SELECT r.BedNumber, CONCAT(c.FirstName, ' ', c.LastName) AS GuestName
				FROM RESERVATIONS r
				JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
				JOIN ROOMS rm ON r.RoomID = rm.RoomID
				WHERE rm.RoomNumber = @rn AND r.Status = 'CheckedIn'", conn);
			cmd.Parameters.AddWithValue("@rn", roomNumber);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int bn = reader.GetInt32(0);
				string name = reader.GetString(1);
				occupants[bn] = name;
			}
		}
		catch { }

		for (int i = 1; i <= capacity; i++)
		{
			int bedNum = i;
			bool isOccupied = occupants.ContainsKey(bedNum);
			string occupantName = isOccupied ? occupants[bedNum] : "";

			Panel pnlBed = new Panel
			{
				Size = new System.Drawing.Size(410, 85),
				BackColor = System.Drawing.Color.White,
				Margin = new Padding(0, 0, 0, 10),
				Padding = new Padding(10)
			};
			pnlBed.Paint += (s, e) =>
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
				e.Graphics.DrawRectangle(pen, 0, 0, pnlBed.Width - 1, pnlBed.Height - 1);
			};

			System.Windows.Forms.Label lblBedTitle = new System.Windows.Forms.Label
			{
				Text = $"🛏️ Yatak #{bedNum}",
				Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(15, 15),
				AutoSize = true
			};
			pnlBed.Controls.Add(lblBedTitle);

			System.Windows.Forms.Label lblStatus = new System.Windows.Forms.Label
			{
				Text = isOccupied ? $"🔴 DOLU - {occupantName}" : "🟢 BOŞ (MÜSAİT)",
				Font = new Font("Segoe UI", 9f, isOccupied ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
				ForeColor = isOccupied ? System.Drawing.Color.FromArgb(239, 68, 68) : System.Drawing.Color.FromArgb(16, 185, 129),
				Location = new Point(15, 42),
				Width = 200,
				AutoEllipsis = true
			};
			pnlBed.Controls.Add(lblStatus);

			if (isOccupied)
			{
				System.Windows.Forms.Label lblTag = new System.Windows.Forms.Label
				{
					Text = "KONAKLIYOR",
					Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
					BackColor = System.Drawing.Color.FromArgb(254, 226, 226),
					ForeColor = System.Drawing.Color.FromArgb(220, 38, 38),
					Location = new Point(315, 15),
					Size = new System.Drawing.Size(80, 22),
					TextAlign = ContentAlignment.MiddleCenter
				};
				pnlBed.Controls.Add(lblTag);
			}
			else
			{
				Button btnWalkIn = new Button
				{
					Text = "⚡ Walk-In",
					Size = new System.Drawing.Size(80, 28),
					Location = new Point(230, 28),
					BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				btnWalkIn.FlatAppearance.BorderSize = 0;
				btnWalkIn.Click += (s, e) =>
				{
					f.Close();
					ShowReservationForm(roomNumber, "", isWalkIn: true, preSelectedBed: bedNum);
					ShowPage("Ana Sayfa");
				};
				pnlBed.Controls.Add(btnWalkIn);

				Button btnRez = new Button
				{
					Text = "📅 Rezervasyon",
					Size = new System.Drawing.Size(85, 28),
					Location = new Point(315, 28),
					BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				btnRez.FlatAppearance.BorderSize = 0;
				btnRez.Click += (s, e) =>
				{
					f.Close();
					ShowReservationForm(roomNumber, "", isWalkIn: false, preSelectedBed: bedNum);
					ShowPage("Ana Sayfa");
				};
				pnlBed.Controls.Add(btnRez);
			}

			flpBeds.Controls.Add(pnlBed);
		}

		f.ShowDialog();
	}

	private async void ShowCheckoutDialog(int resId, string guestName, string roomNum)
	{
		DataRow dr = DataAccess.GetReservationDetailsForCheckout(resId);
		if (dr != null)
		{
			await ShowCheckoutPaymentDialog(dr);
			ShowPage("Ana Sayfa");
		}
	}

	private async Task ShowCheckoutPaymentDialog(DataRow row)
	{
		int resId = Convert.ToInt32(row["ReservationID"]);
		string mName = row["Musteri"]?.ToString() ?? "";
		string oda = row["Oda"]?.ToString() ?? "";
		string yatak = ((!row.Table.Columns.Contains("Yatak")) ? "1" : (row["Yatak"]?.ToString() ?? "1"));
		decimal odaBorc = Convert.ToDecimal(row["ToplamTutar"]);
		decimal odenenSorgu = Convert.ToDecimal(row["OdenenMiktar"]);
		string roomInfo = "Oda " + oda + " - " + mName;
		decimal lokantaBorc = DataAccess.GetLokantaTotalForGuest(roomInfo);
		DataTable dtLokantaHistory = DataAccess.GetLokantaSalesForGuest(roomInfo);
		DataTable dtServices = DataAccess.GetReservationServices(resId);
		decimal hizmetBorc = default(decimal);
		foreach (DataRow drS in dtServices.Rows)
		{
			hizmetBorc += Convert.ToDecimal(drS["Tutar"]);
		}
		DateTime giris = Convert.ToDateTime(row["Giris"]);
		DateTime cikisPlanlanan = Convert.ToDateTime(row["Cikis"]);
		int planliGun = Math.Max(1, (int)(cikisPlanlanan.Date - giris.Date).TotalDays);
		decimal gunlukFiyat = odaBorc / (decimal)planliGun;
		int ekstraGun = Math.Max(0, (int)(DateTime.Today - cikisPlanlanan.Date).TotalDays);
		decimal ekstraUcret = (decimal)ekstraGun * gunlukFiyat;
		decimal genelToplam = odaBorc + lokantaBorc + ekstraUcret + hizmetBorc;
		decimal kalanBorc = genelToplam - odenenSorgu;
		Dictionary<string, decimal> rates = await ExchangeRateHelper.GetRatesAsync();
		Form f = new Form
		{
			Text = "\ud83d\udcb3 TAHAKKUK VE ÖDEME İŞLEMLERİ",
			Size = new System.Drawing.Size(680, 980),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
			FormBorderStyle = FormBorderStyle.FixedDialog,
			AutoScroll = true
		};
		int curY = 25;
		Panel pnlHead = new Panel
		{
			Location = new Point(0, 0),
			Size = new System.Drawing.Size(680, 110),
			BackColor = System.Drawing.Color.White
		};
		pnlHead.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83e\uddfe ADİSYON VE TAHAKKUK DETAYI",
			Font = new Font("Segoe UI", 15f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
			Location = new Point(25, 15),
			AutoSize = true
		});
		System.Windows.Forms.Label lblInfo = new System.Windows.Forms.Label
		{
			Text = $"\ud83d\udc64 Misafir: {mName}   |   \ud83d\udeaa Oda No: {oda}   |   \ud83d\udecf\ufe0f Yatak No: {yatak}   |   \ud83c\udd94 Rezervasyon No: #{resId}",
			Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(79, 70, 229),
			BackColor = System.Drawing.Color.FromArgb(243, 244, 246),
			Location = new Point(25, 52),
			Size = new System.Drawing.Size(610, 38),
			TextAlign = ContentAlignment.MiddleCenter
		};
		pnlHead.Controls.Add(lblInfo);
		f.Controls.Add(pnlHead);
		curY = 130;
		AddSectionTitle("ODA KONAKLAMA", System.Drawing.Color.FromArgb(79, 70, 229), "\ud83c\udfe8");
		RoundedPanel pnlOda = new RoundedPanel
		{
			Location = new Point(25, curY),
			Size = new System.Drawing.Size(610, 100),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 15
		};
		pnlOda.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
			e.Graphics.DrawPath(pen, CustGetPath(pnlOda.ClientRectangle, 15));
		};
		AddOdaLine($"{planliGun} Gece Konaklama (Birim: {gunlukFiyat:N2} ₺)", odaBorc.ToString("N2") + " ₺", 15, b: true);
		AddOdaLine($"{giris:dd.MM.yyyy} - {cikisPlanlanan:dd.MM.yyyy}", "", 40);
		if (ekstraGun > 0)
		{
			AddOdaLine($"⚠\ufe0f {ekstraGun} Gece Gecikme Bedeli", ekstraUcret.ToString("N2") + " ₺", 65, b: true);
		}
		f.Controls.Add(pnlOda);
		curY += 125;
		AddSectionTitle("LOKANTA VE SERVİS", System.Drawing.Color.FromArgb(220, 38, 38), "\ud83c\udf7d\ufe0f");
		RoundedPanel pnlLok = new RoundedPanel
		{
			Location = new Point(25, curY),
			Size = new System.Drawing.Size(610, 160),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 15
		};
		pnlLok.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
			e.Graphics.DrawPath(pen, CustGetPath(pnlLok.ClientRectangle, 15));
		};
		if (dtLokantaHistory.Rows.Count > 0)
		{
			DataGridView dgvL = new DataGridView
			{
				Location = new Point(10, 10),
				Size = new System.Drawing.Size(590, 110),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				Font = new Font("Segoe UI", 8.5f),
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				EnableHeadersVisualStyles = false
			};
			dgvL.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgvL.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
			dgvL.DataSource = dtLokantaHistory;
			pnlLok.Controls.Add(dgvL);
			dgvL.DataBindingComplete += delegate
			{
				foreach (DataGridViewColumn column in dgvL.Columns)
				{
					if (column.Name == "SaleID" || column.Name == "SaleDate")
					{
						column.Visible = false;
					}
				}
				if (dgvL.Columns.Contains("ItemName"))
				{
					dgvL.Columns["ItemName"].HeaderText = "Ürün";
				}
				if (dgvL.Columns.Contains("TotalPrice"))
				{
					dgvL.Columns["TotalPrice"].DefaultCellStyle.Format = "N2 ₺";
				}
			};
		}
		else
		{
			pnlLok.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Lokanta harcaması bulunmuyor.",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(20, 50),
				AutoSize = true
			});
		}
		System.Windows.Forms.Label lblLokTot = new System.Windows.Forms.Label
		{
			Text = $"Toplam: {lokantaBorc:N2} ₺",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(220, 38, 38),
			Location = new Point(450, 130),
			AutoSize = true
		};
		pnlLok.Controls.Add(lblLokTot);
		f.Controls.Add(pnlLok);
		curY += 185;
		if (dtServices.Rows.Count > 0)
		{
			AddSectionTitle("EK HİZMETLER", System.Drawing.Color.FromArgb(124, 58, 237), "\ud83d\udd27");
			RoundedPanel pnlSvc = new RoundedPanel
			{
				Location = new Point(25, curY),
				Size = new System.Drawing.Size(610, 100),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15
			};
			pnlSvc.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
				e.Graphics.DrawPath(pen, CustGetPath(pnlSvc.ClientRectangle, 15));
			};
			DataGridView dgvS = new DataGridView
			{
				Location = new Point(10, 10),
				Size = new System.Drawing.Size(590, 60),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				Font = new Font("Segoe UI", 8.5f),
				EnableHeadersVisualStyles = false
			};
			dgvS.DataSource = dtServices;
			pnlSvc.Controls.Add(dgvS);
			pnlSvc.Controls.Add(new System.Windows.Forms.Label
			{
				Text = $"Toplam: {hizmetBorc:N2} ₺",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(124, 58, 237),
				Location = new Point(450, 72),
				AutoSize = true
			});
			f.Controls.Add(pnlSvc);
			curY += 125;
		}
		RoundedPanel pnlSummary = new RoundedPanel
		{
			Location = new Point(25, curY),
			Size = new System.Drawing.Size(610, 180),
			BackColor = System.Drawing.Color.FromArgb(15, 23, 42),
			BorderRadius = 15
		};
		AddSumLine("GENEL TOPLAM", genelToplam.ToString("N2") + " ₺", System.Drawing.Color.White, 25, 12f);
		AddSumLine("ÖDENEN MİKTAR", odenenSorgu.ToString("N2") + " ₺", System.Drawing.Color.FromArgb(16, 185, 129), 70);
		AddSumLine("KALAN BORÇ", kalanBorc.ToString("N2") + " ₺", (kalanBorc > 0m) ? System.Drawing.Color.FromArgb(244, 63, 94) : System.Drawing.Color.FromArgb(34, 197, 94), 120, 14f);
		f.Controls.Add(pnlSummary);
		curY += 210;
		RoundedPanel pnlPayAction = new RoundedPanel
		{
			Location = new Point(25, curY),
			Size = new System.Drawing.Size(610, 240),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 15
		};
		pnlPayAction.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(99, 102, 241), 2f);
			e.Graphics.DrawPath(pen, CustGetPath(pnlPayAction.ClientRectangle, 15));
		};
		f.Controls.Add(pnlPayAction);
		pnlPayAction.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcb3 ÖDEME YÖNTEMİ",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(20, 20),
			AutoSize = true
		});
		ComboBox cmbMethod = new ComboBox
		{
			Location = new Point(20, 42),
			Size = new System.Drawing.Size(270, 35),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Font = new Font("Segoe UI", 11f),
			FlatStyle = FlatStyle.Flat,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252)
		};
		ComboBox.ObjectCollection items = cmbMethod.Items;
		object[] items2 = new string[3] { "\ud83d\udcb5 Nakit Ödeme", "\ud83d\udcb3 Kredi Kartı", "\ud83c\udfe6 Havale / EFT" };
		items.AddRange(items2);
		cmbMethod.SelectedIndex = 0;
		pnlPayAction.Controls.Add(cmbMethod);
		pnlPayAction.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcb1 PARA BİRİMİ",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(310, 20),
			AutoSize = true
		});
		ComboBox cmbCurr = new ComboBox
		{
			Location = new Point(310, 42),
			Size = new System.Drawing.Size(270, 35),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Font = new Font("Segoe UI", 11f),
			FlatStyle = FlatStyle.Flat,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252)
		};
		ComboBox.ObjectCollection items3 = cmbCurr.Items;
		items2 = new string[3] { "₺ Türk Lirası (TRY)", "$ ABD Doları (USD)", "€ Euro (EUR)" };
		items3.AddRange(items2);
		cmbCurr.SelectedIndex = 0;
		pnlPayAction.Controls.Add(cmbCurr);
		pnlPayAction.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcb0 TAHSİL EDİLECEK TUTAR",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			Location = new Point(20, 95),
			AutoSize = true
		});
		NumericUpDown txtPay = new NumericUpDown
		{
			Location = new Point(20, 122),
			Size = new System.Drawing.Size(270, 45),
			Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
			Minimum = 0m,
			Maximum = 1000000m,
			DecimalPlaces = 2,
			Value = ((kalanBorc > 0m) ? kalanBorc : 0m),
			BorderStyle = BorderStyle.FixedSingle
		};
		pnlPayAction.Controls.Add(txtPay);
		System.Windows.Forms.Label lblEq = new System.Windows.Forms.Label
		{
			Text = "",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Italic),
			ForeColor = System.Drawing.Color.FromArgb(79, 70, 229),
			Location = new Point(310, 130),
			AutoSize = true
		};
		pnlPayAction.Controls.Add(lblEq);
		cmbCurr.SelectedIndexChanged += delegate
		{
			UpdateEq();
		};
		txtPay.ValueChanged += delegate
		{
			UpdateEq();
		};
		Button btnPart = new Button
		{
			Text = "KISMİ ÖDEME",
			Size = new System.Drawing.Size(270, 50),
			Location = new Point(20, 175),
			BackColor = System.Drawing.Color.FromArgb(59, 130, 246),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		btnPart.FlatAppearance.BorderSize = 0;
		pnlPayAction.Controls.Add(btnPart);
		Button btnFull = new Button
		{
			Text = "TAMAMINI TAHSİL ET VE ÇIKAR",
			Size = new System.Drawing.Size(270, 50),
			Location = new Point(310, 175),
			BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		btnFull.FlatAppearance.BorderSize = 0;
		pnlPayAction.Controls.Add(btnFull);
		btnPart.Click += delegate
		{
			if (txtPay.Value <= 0m)
			{
				return;
			}
			try
			{
				string text = cmbCurr.SelectedItem.ToString();
				string text2 = (text.Contains("USD") ? "USD" : (text.Contains("EUR") ? "EUR" : "TRY"));
				decimal value = txtPay.Value;
				if (text2 != "TRY" && rates.ContainsKey(text2))
				{
					value *= rates[text2];
				}
				DataAccess.RecordPayment(resId, value, $"{cmbMethod.SelectedItem} ({text2})");
				MessageBox.Show($"{value:N2} ₺ ödeme kaydedildi.");
				f.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		btnFull.Click += delegate
		{
			try
			{
				string text = cmbCurr.SelectedItem.ToString();
				string text2 = (text.Contains("USD") ? "USD" : (text.Contains("EUR") ? "EUR" : "TRY"));
				decimal value = txtPay.Value;
				if (text2 != "TRY" && rates.ContainsKey(text2))
				{
					value *= rates[text2];
				}
				if (!(value < kalanBorc - 1m) || MessageBox.Show("Borç tam kapanmıyor, yine de çıkış yapılsın mı?", "Eksik Ödeme", MessageBoxButtons.YesNo) != DialogResult.No)
				{
					DataAccess.RecordPayment(resId, value, $"{cmbMethod.SelectedItem} ({text2} - Final)");
					DataAccess.MarkLokantaSalesAsPaid(roomInfo);
					if (ekstraGun > 0)
					{
						using MySqlConnection mySqlConnection = DatabaseHelper.GetConnection();
						mySqlConnection.Open();
						using MySqlCommand mySqlCommand = new MySqlCommand("UPDATE RESERVATIONS SET TotalAmount = @ta, CheckOutDate = @cod WHERE ReservationID = @id", mySqlConnection);
						mySqlCommand.Parameters.AddWithValue("@ta", odaBorc + ekstraUcret);
						mySqlCommand.Parameters.AddWithValue("@cod", DateTime.Today.ToString("yyyy-MM-dd"));
						mySqlCommand.Parameters.AddWithValue("@id", resId);
						mySqlCommand.ExecuteNonQuery();
					}
					DataAccess.CompleteReservation(resId);
					MessageBox.Show("İşlem başarılı, oda boşaltıldı.");
					f.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		f.ShowDialog();
		void AddOdaLine(string l, string v, int ly, bool b = false)
		{
			pnlOda.Controls.Add(new System.Windows.Forms.Label
			{
				Text = l,
				Font = new Font("Segoe UI", 9.5f, b ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
				Location = new Point(15, ly),
				AutoSize = true
			});
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = v,
				Font = new Font("Segoe UI", 10f, b ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
				ForeColor = (b ? System.Drawing.Color.FromArgb(79, 70, 229) : System.Drawing.Color.Black),
				Location = new Point(450, ly),
				AutoSize = true
			};
			pnlOda.Controls.Add(value);
		}
		void AddSectionTitle(string title, System.Drawing.Color c, string icon)
		{
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = icon + "  " + title,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				ForeColor = c,
				Location = new Point(25, curY),
				AutoSize = true
			};
			f.Controls.Add(value);
			curY += 30;
		}
		void AddSumLine(string l, string v, System.Drawing.Color c, int ly, float fontSize = 11f, bool b = true)
		{
			pnlSummary.Controls.Add(new System.Windows.Forms.Label
			{
				Text = l,
				Font = new Font("Segoe UI", fontSize, b ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
				ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
				Location = new Point(20, ly),
				AutoSize = true
			});
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = v,
				Font = new Font("Segoe UI", fontSize + 2f, System.Drawing.FontStyle.Bold),
				ForeColor = c,
				Location = new Point(420, ly - 2),
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleRight
			};
			pnlSummary.Controls.Add(value);
		}
		void UpdateEq()
		{
			string text = cmbCurr.SelectedItem.ToString();
			string text2 = (text.Contains("USD") ? "USD" : (text.Contains("EUR") ? "EUR" : "TRY"));
			if (text2 == "TRY")
			{
				lblEq.Text = "";
			}
			else if (rates.ContainsKey(text2))
			{
				lblEq.Text = $"≈ {txtPay.Value * rates[text2]:N2} ₺ (Kur: {rates[text2]:N4})";
			}
		}
	}

	private void PageBookings(Panel body)
	{
		DataTable dt = DataAccess.GetReservations("Future");
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel pnlTop = new Panel
			{
				Dock = DockStyle.Top,
				Height = 110,
				Padding = new Padding(20, 10, 20, 10)
			};
			pnlTop.Paint += delegate(object? s, PaintEventArgs e)
			{
				using var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(
					new System.Drawing.Point(0, 0), new System.Drawing.Point(pnlTop.Width, pnlTop.Height),
					System.Drawing.Color.FromArgb(15, 23, 42), System.Drawing.Color.FromArgb(30, 41, 59));
				e.Graphics.FillRectangle(lgb, new System.Drawing.Rectangle(0, 0, pnlTop.Width, pnlTop.Height));
				using var goldPen = new Pen(System.Drawing.Color.FromArgb(201, 151, 58), 4f);
				e.Graphics.DrawLine(goldPen, 0, pnlTop.Height - 2, pnlTop.Width, pnlTop.Height - 2);
			};
			body.Controls.Add(pnlTop);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "📅 Gelecek Rezervasyonlar",
				Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.White,
				Location = new Point(20, 15),
				AutoSize = true,
				BackColor = System.Drawing.Color.Transparent
			};
			System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
			{
				Text = "(Bekleyen Konaklamalar)",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Regular),
				ForeColor = System.Drawing.Color.FromArgb(203, 213, 225),
				Location = new Point(25, 52),
				AutoSize = true,
				BackColor = System.Drawing.Color.Transparent
			};
			pnlTop.Controls.Add(value);
			pnlTop.Controls.Add(value2);
			int count = dt.DefaultView.Count;
			int num = dt.AsEnumerable().Count((DataRow r) => r.Field<string>("Status") == "Reserved" && Convert.ToDateTime(r["Giris"]).Date <= DateTime.Today);
			decimal value3 = (from r in dt.AsEnumerable()
				where r.Field<string>("Status") == "Reserved"
				select r).Sum((DataRow r) => Convert.ToDecimal(r["ToplamTutar"]));
			AddStatCard("Ödeme Bekleyen Toplam", $"₺ {value3:N2}", 210);
			AddStatCard("Bugün Giriş", num.ToString(), 410);
			AddStatCard("Toplam Bekleyen", count.ToString(), 610);
			RoundedPanel pnlFilters = new RoundedPanel
			{
				Dock = DockStyle.Top,
				Height = 85,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Padding = new Padding(20, 10, 20, 10),
				Margin = new Padding(20, 0, 20, 10)
			};
			pnlFilters.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
				e.Graphics.DrawPath(pen, CustGetPath(pnlFilters.ClientRectangle, 12));
			};
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 105,
				Padding = new Padding(20, 10, 20, 10),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			panel.Controls.Add(pnlFilters);
			body.Controls.Add(panel);
			pnlFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Hızlı Arama ve Filtreleme",
				Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
				Location = new Point(15, 12),
				AutoSize = true
			});
			TextBox txtSearch = new TextBox
			{
				PlaceholderText = "Müşteri Ara...",
				Size = new System.Drawing.Size(240, 30),
				Location = new Point(15, 36),
				Font = new Font("Segoe UI", 11f),
				BorderStyle = BorderStyle.FixedSingle
			};
			pnlFilters.Controls.Add(txtSearch);
			System.Windows.Forms.Label value4 = new System.Windows.Forms.Label
			{
				Text = "Giriş Aralığı",
				Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
				Location = new Point(310, 12),
				AutoSize = true,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			Panel panel2 = new Panel
			{
				Size = new System.Drawing.Size(280, 32),
				Location = new Point(310, 36),
				BorderStyle = BorderStyle.FixedSingle,
				BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			DateTimePicker dtpStart = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Width = 110,
				Location = new Point(5, 2),
				Font = new Font("Segoe UI", 9.5f),
				Value = DateTime.Today,
				ShowUpDown = false
			};
			System.Windows.Forms.Label label = new System.Windows.Forms.Label
			{
				Text = "—",
				Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(125, 6),
				AutoSize = true
			};
			DateTimePicker dtpEnd = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Width = 110,
				Location = new Point(145, 2),
				Font = new Font("Segoe UI", 9.5f),
				Value = DateTime.Today.AddMonths(4),
				ShowUpDown = false
			};
			panel2.Controls.AddRange(new Control[3] { dtpStart, label, dtpEnd });
			pnlFilters.Controls.Add(value4);
			pnlFilters.Controls.Add(panel2);
			DataGridView dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				DataSource = dt.DefaultView,
				Font = new Font("Segoe UI", 10f),
				GridColor = System.Drawing.Color.FromArgb(241, 245, 249),
				RowTemplate = 
				{
					MinimumHeight = 48
				}
			};
			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersHeight = 40;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 120, 215);
			dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
			dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			txtSearch.TextChanged += delegate
			{
				ApplyFilters();
			};
			dtpStart.ValueChanged += delegate
			{
				ApplyFilters();
			};
			dtpEnd.ValueChanged += delegate
			{
				ApplyFilters();
			};
			dgv.DataBindingComplete += delegate
			{
				foreach (DataGridViewColumn column in dgv.Columns)
				{
					column.Visible = false;
				}
				if (dgv.Columns.Contains("ReservationID"))
				{
					dgv.Columns["ReservationID"].Visible = true;
					dgv.Columns["ReservationID"].HeaderText = "ID";
					dgv.Columns["ReservationID"].Width = 50;
					dgv.Columns["ReservationID"].DisplayIndex = 0;
				}
				if (dgv.Columns.Contains("Musteri"))
				{
					dgv.Columns["Musteri"].Visible = true;
					dgv.Columns["Musteri"].HeaderText = "\ud83d\udc64 İSİM SOYADI";
					dgv.Columns["Musteri"].DisplayIndex = 1;
					dgv.Columns["Musteri"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
				}
				if (dgv.Columns.Contains("IdentityNumber"))
				{
					dgv.Columns["IdentityNumber"].Visible = true;
					dgv.Columns["IdentityNumber"].HeaderText = "TC KİMLİK NO";
					dgv.Columns["IdentityNumber"].DisplayIndex = 2;
					dgv.Columns["IdentityNumber"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
				}
				if (dgv.Columns.Contains("Phone"))
				{
					dgv.Columns["Phone"].Visible = true;
					dgv.Columns["Phone"].HeaderText = "TELEFON";
					dgv.Columns["Phone"].DisplayIndex = 3;
					dgv.Columns["Phone"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
				}
				if (dgv.Columns.Contains("Oda"))
				{
					dgv.Columns["Oda"].Visible = true;
					dgv.Columns["Oda"].HeaderText = "\ud83d\udeaa ODA";
					dgv.Columns["Oda"].Width = 80;
					dgv.Columns["Oda"].DisplayIndex = 4;
				}
				if (dgv.Columns.Contains("Giris"))
				{
					dgv.Columns["Giris"].Visible = true;
					dgv.Columns["Giris"].HeaderText = "\ud83d\udcc5 GİRİŞ TARİH";
					dgv.Columns["Giris"].DefaultCellStyle.Format = "dd/MM/yyyy";
					dgv.Columns["Giris"].DisplayIndex = 5;
				}
				if (dgv.Columns.Contains("Cikis"))
				{
					dgv.Columns["Cikis"].Visible = true;
					dgv.Columns["Cikis"].HeaderText = "\ud83d\udcc5 ÇIKIŞ TARİH";
					dgv.Columns["Cikis"].DefaultCellStyle.Format = "dd/MM/yyyy";
					dgv.Columns["Cikis"].DisplayIndex = 6;
				}
				if (dgv.Columns.Contains("ToplamTutar"))
				{
					dgv.Columns["ToplamTutar"].Visible = true;
					dgv.Columns["ToplamTutar"].HeaderText = "\ud83d\udcb0 ÖD. FİYAT";
					dgv.Columns["ToplamTutar"].DefaultCellStyle.Format = "₺ N2";
					dgv.Columns["ToplamTutar"].DisplayIndex = 7;
				}
				if (dgv.Columns.Contains("Notlar"))
				{
					dgv.Columns["Notlar"].Visible = true;
					dgv.Columns["Notlar"].HeaderText = "📝 NOTLAR";
					dgv.Columns["Notlar"].DisplayIndex = 8;
					dgv.Columns["Notlar"].MinimumWidth = 250;
					dgv.Columns["Notlar"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				}
				if (!dgv.Columns.Contains("Aksiyon"))
				{
					DataGridViewTextBoxColumn dataGridViewColumn2 = new DataGridViewTextBoxColumn
					{
						Name = "Aksiyon",
						HeaderText = "Aksiyon",
						Width = 160,
						DisplayIndex = 9
					};
					dgv.Columns.Add(dataGridViewColumn2);
				}
				else
				{
					dgv.Columns["Aksiyon"].Visible = true;
					dgv.Columns["Aksiyon"].Width = 160;
					dgv.Columns["Aksiyon"].DisplayIndex = 9;
				}
			};
			dgv.CellPainting += delegate(object? s, DataGridViewCellPaintingEventArgs e)
			{
				if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Aksiyon")
				{
					e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.Focus | DataGridViewPaintParts.SelectionBackground);
					using (SolidBrush brush = new SolidBrush(((dgv.Rows[e.RowIndex].State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected) ? dgv.DefaultCellStyle.SelectionBackColor : ((e.RowIndex % 2 == 0) ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(248, 250, 252))))
					{
						e.Graphics.FillRectangle(brush, e.CellBounds);
					}
					using (Pen pen = new Pen(System.Drawing.Color.FromArgb(241, 245, 249)))
					{
						e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
					}
					System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.CellBounds.Left + 10, e.CellBounds.Top + (e.CellBounds.Height - 32) / 2, 90, 32);
					using (GraphicsPath path = CustGetPath(rectangle, 8))
					{
						using SolidBrush brush2 = new SolidBrush(System.Drawing.Color.FromArgb(34, 197, 94));
						e.Graphics.FillPath(brush2, path);
					}
					using (Font font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold))
					{
						using SolidBrush brush3 = new SolidBrush(System.Drawing.Color.White);
						StringFormat format = new StringFormat
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Center
						};
						e.Graphics.DrawString("✓ Check-in", font, brush3, rectangle, format);
					}
					System.Drawing.Rectangle rectangle2 = new System.Drawing.Rectangle(e.CellBounds.Left + 110, e.CellBounds.Top + (e.CellBounds.Height - 32) / 2, 36, 32);
					using (GraphicsPath path2 = CustGetPath(rectangle2, 8))
					{
						using SolidBrush brush4 = new SolidBrush(System.Drawing.Color.FromArgb(203, 213, 225));
						e.Graphics.FillPath(brush4, path2);
					}
					using (Font font2 = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold))
					{
						using SolidBrush brush5 = new SolidBrush(System.Drawing.Color.FromArgb(71, 85, 105));
						StringFormat format2 = new StringFormat
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Center
						};
						e.Graphics.DrawString("✏\ufe0f", font2, brush5, rectangle2, format2);
					}
					e.Handled = true;
				}
			};
			dgv.CellMouseClick += delegate(object? s, DataGridViewCellMouseEventArgs e)
			{
				if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Aksiyon")
				{
					System.Drawing.Rectangle cellDisplayRectangle = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, cutOverflow: false);
					int num2 = dgv.PointToClient(System.Windows.Forms.Cursor.Position).X - cellDisplayRectangle.Left;
					int reservationId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ReservationID"].Value);
					if (num2 >= 10 && num2 <= 100)
					{
						DataGridViewRow dataGridViewRow = dgv.Rows[e.RowIndex];
						DialogResult dialogResult = MessageBox.Show($"Müşteri {dataGridViewRow.Cells["Musteri"].Value} için Check-In kaydını onaylıyor musunuz?", "Check-In Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (dialogResult == DialogResult.Yes)
						{
							try
							{
								DataAccess.PerformCheckIn(reservationId);
								MessageBox.Show("Müşteri girişi başarıyla tamamlandı!");
								PageBookings(body);
							}
							catch (Exception ex)
							{
								MessageBox.Show("Hata: " + ex.Message);
							}
						}
					}
					else if (num2 >= 110 && num2 <= 146)
					{
						DataGridViewRow dataGridViewRow2 = dgv.Rows[e.RowIndex];
						string preSelectedRoom = dataGridViewRow2.Cells["Oda"].Value?.ToString() ?? "";
						ShowReservationForm(preSelectedRoom, "", isWalkIn: false);
						PageBookings(body);
					}
				}
			};
			Panel panel3 = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(20)
			};
			panel3.Controls.Add(dgv);
			body.Controls.Add(panel3);
			panel3.BringToFront();
			void AddStatCard(string title, string val, int offsetRight)
			{
				RoundedPanel p = new RoundedPanel
				{
					Size = new System.Drawing.Size(180, 80),
					BackColor = System.Drawing.Color.FromArgb(30, 41, 59),
					BorderRadius = 12,
					Anchor = (AnchorStyles.Top | AnchorStyles.Right)
				};
				p.Location = new Point(pnlTop.Width - offsetRight, 15);
				p.Paint += delegate(object? s, PaintEventArgs e)
				{
					using Pen pen = new Pen(System.Drawing.Color.FromArgb(71, 85, 105), 1f);
					e.Graphics.DrawPath(pen, CustGetPath(p.ClientRectangle, 12));
				};
				System.Windows.Forms.Label value5 = new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
					Location = new Point(15, 12),
					AutoSize = true,
					BackColor = System.Drawing.Color.Transparent
				};
				System.Windows.Forms.Label value6 = new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 15f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.White,
					Location = new Point(13, 36),
					AutoSize = true,
					BackColor = System.Drawing.Color.Transparent
				};
				p.Controls.Add(value5);
				p.Controls.Add(value6);
				pnlTop.Controls.Add(p);
			}
			void ApplyFilters()
			{
				string value5 = txtSearch.Text.Trim().Replace("'", "''");
				string text = "Status = 'Reserved'";
				if (!string.IsNullOrEmpty(value5))
				{
					text += $" AND (Musteri LIKE '%{value5}%' or Oda LIKE '%{value5}%')";
				}
				text += $" AND Giris >= '{dtpStart.Value:yyyy-MM-dd}' AND Giris <= '{dtpEnd.Value:yyyy-MM-dd}'";
				dt.DefaultView.RowFilter = text;
			}
		});
	}

	private void PageReservations(Panel body)
	{
		DataTable dt = DataAccess.GetActiveReservations();
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel pnlStats = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(20, 15, 20, 15)
			};
			body.Controls.Add(pnlStats);
			int sx = 20;
			AddResStat("TOPLAM REZERVASYON", dt.Rows.Count.ToString(), System.Drawing.Color.FromArgb(79, 70, 229), "\ud83d\udccb");
			AddResStat("AKTİF DİNLENENLER", dt.Select("Status='CheckedIn'").Length.ToString(), System.Drawing.Color.FromArgb(16, 185, 129), "\ud83d\udc65");
			RoundedPanel pnlFilters = new RoundedPanel
			{
				Location = new Point(20, 115),
				Size = new System.Drawing.Size(body.Width - 40, 80),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			pnlFilters.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
				e.Graphics.DrawPath(pen, ResGetPath(pnlFilters.ClientRectangle, 12));
			};
			body.Controls.Add(pnlFilters);
			int num = 25;
			pnlFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udd0d FİLTRE & ARAMA:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(99, 102, 241),
				Location = new Point(20, num + 3),
				AutoSize = true
			});
			TextBox txtSearch = new TextBox
			{
				Name = "filterSearch",
				PlaceholderText = "İsim veya Oda...",
				Size = new System.Drawing.Size(180, 28),
				Location = new Point(160, num),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox cmbType = new ComboBox
			{
				Name = "filterType",
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(140, 28),
				Location = new Point(350, num),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items = cmbType.Items;
			object[] items2 = new string[4] { "Tüm Tipler", "Tek Kişilik", "Çift Kişilik", "Suit" };
			items.AddRange(items2);
			cmbType.SelectedIndex = 0;
			ComboBox cmbStatus = new ComboBox
			{
				Name = "filterStatus",
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(160, 28),
				Location = new Point(500, num),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items3 = cmbStatus.Items;
			items2 = new string[3] { "Aktif Konaklayanlar", "Geçmiş Kayıtlar", "Tüm Rezervasyonlar" };
			items3.AddRange(items2);
			cmbStatus.SelectedIndex = 0;
			Button button = new Button
			{
				Text = "\ud83d\udccb POLİS/JANDARMA XML",
				Size = new System.Drawing.Size(180, 28),
				Location = new Point(670, num),
				BackColor = System.Drawing.Color.FromArgb(15, 23, 42),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button.Click += delegate
			{
				ExportPoliceXML();
			};
			pnlFilters.Controls.AddRange(new Control[4] { txtSearch, cmbType, cmbStatus, button });
			Panel panel = new Panel
			{
				Location = new Point(20, 210),
				Size = new System.Drawing.Size(body.Width - 40, body.Height - 230),
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(panel);
			FlowLayoutPanel flowRes = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(0, 0, 0, 20)
			};
			panel.Controls.Add(flowRes);
			DataGridView hiddenDgv = new DataGridView
			{
				DataSource = dt,
				Visible = false
			};
			body.Controls.Add(hiddenDgv);
			DataGridView dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				DataSource = dt.DefaultView,
				Font = new Font("Segoe UI", 10f),
				GridColor = System.Drawing.Color.FromArgb(241, 245, 249),
				RowTemplate = 
				{
					Height = 48
				},
				Visible = false
			};
			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(238, 242, 255);
			dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
			dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
			dgv.DataBindingComplete += delegate
			{
				if (dgv.Columns.Contains("ReservationID"))
				{
					dgv.Columns["ReservationID"].Visible = false;
				}
				if (dgv.Columns.Contains("Musteri"))
				{
					dgv.Columns["Musteri"].HeaderText = "\ud83d\udc64 MİSAFİR";
				}
				if (dgv.Columns.Contains("Oda"))
				{
					dgv.Columns["Oda"].HeaderText = "\ud83d\udeaa ODA";
				}
				if (dgv.Columns.Contains("Giris"))
				{
					dgv.Columns["Giris"].HeaderText = "\ud83d\udcc5 GİRİŞ";
				}
				if (dgv.Columns.Contains("Cikis"))
				{
					dgv.Columns["Cikis"].HeaderText = "\ud83d\udcc5 ÇIKIŞ";
				}
				if (dgv.Columns.Contains("ToplamTutar"))
				{
					dgv.Columns["ToplamTutar"].HeaderText = "\ud83d\udcb0 TOPLAM (₺)";
				}
			};
			dgv.CellDoubleClick += async delegate(object? s, DataGridViewCellEventArgs e)
			{
				if (e.RowIndex >= 0)
				{
					DataGridViewRow row = dgv.Rows[e.RowIndex];
					if (row.Cells["Status"].Value.ToString() == "CheckedIn")
					{
						await ShowCheckoutPaymentDialog(((DataRowView)row.DataBoundItem).Row);
						ShowPage("Konaklayanlar");
					}
				}
			};
			panel.Controls.Add(dgv);
			txtSearch.TextChanged += delegate
			{
				ApplyResFilters();
			};
			cmbType.SelectedIndexChanged += delegate
			{
				ApplyResFilters();
			};
			cmbStatus.SelectedIndexChanged += delegate
			{
				ApplyResFilters();
			};
			ApplyResFilters();
			void AddResStat(string title, string val, System.Drawing.Color color, string icon)
			{
				RoundedPanel p = new RoundedPanel
				{
					Location = new Point(sx, 12),
					Size = new System.Drawing.Size(240, 78),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12
				};
				p.Paint += delegate(object? s, PaintEventArgs e)
				{
					using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
					e.Graphics.DrawPath(pen, ResGetPath(p.ClientRectangle, 12));
				};
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = icon,
					Font = new Font("Segoe UI", 16f),
					Location = new Point(15, 25),
					AutoSize = true,
					ForeColor = color
				});
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
					Location = new Point(55, 20),
					AutoSize = true
				});
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
					Location = new Point(53, 35),
					AutoSize = true
				});
				pnlStats.Controls.Add(p);
				sx += 255;
			}
			void ApplyResFilters()
			{
				string value = txtSearch.Text.Trim().Replace("'", "''");
				string text = ((cmbType.SelectedIndex > 0) ? cmbType.SelectedItem.ToString() : "");
				string text2 = "1=1";
				if (!string.IsNullOrEmpty(value))
				{
					text2 += $" AND (Musteri LIKE '%{value}%' OR Oda LIKE '%{value}%')";
				}
				if (!string.IsNullOrEmpty(text))
				{
					text2 = text2 + " AND OdaTipi = '" + text + "'";
				}
				if (cmbStatus.SelectedIndex == 0)
				{
					text2 += " AND Status = 'CheckedIn'";
					dt.DefaultView.RowFilter = text2;
					flowRes.Visible = true;
					dgv.Visible = false;
					RenderCards();
				}
				else
				{
					string text3 = ((cmbStatus.SelectedIndex == 1) ? "CheckedOut" : "");
					if (!string.IsNullOrEmpty(text3))
					{
						text2 = text2 + " AND Status = '" + text3 + "'";
					}
					dt.DefaultView.RowFilter = text2;
					flowRes.Visible = false;
					dgv.Visible = true;
				}
			}
			void RenderCards()
			{
				flowRes.SuspendLayout();
				flowRes.Controls.Clear();
				foreach (DataRowView item in dt.DefaultView)
				{
					if (!(item["Status"].ToString() != "CheckedIn"))
					{
						int resId = Convert.ToInt32(item["ReservationID"]);
						string text = item["Oda"].ToString();
						string musteri = item["Musteri"].ToString();
						DateTime value = Convert.ToDateTime(item["Giris"]);
						DateTime value2 = Convert.ToDateTime(item["Cikis"]);
						decimal num2 = Convert.ToDecimal(item["ToplamTutar"]);
						decimal num3 = Convert.ToDecimal(item["OdenenMiktar"]);
						decimal num4 = num2 - num3;
						RoundedPanel card = new RoundedPanel
						{
							Size = new System.Drawing.Size(295, 230),
							BackColor = System.Drawing.Color.White,
							BorderRadius = 15,
							Margin = new Padding(0, 0, 20, 20)
						};
						card.Paint += delegate(object? s, PaintEventArgs e)
						{
							using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
							e.Graphics.DrawPath(pen, ResGetPath(card.ClientRectangle, 15));
							using SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(52, 211, 153));
							e.Graphics.FillRectangle(brush, 0, 0, 6, card.Height);
						};
						card.Controls.Add(new System.Windows.Forms.Label
						{
							Text = "Aktif Konaklama",
							Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(16, 185, 129),
							Location = new Point(20, 15),
							AutoSize = true
						});
						card.Controls.Add(new System.Windows.Forms.Label
						{
							Text = "Oda " + text,
							Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
							Location = new Point(16, 30),
							AutoSize = true
						});
						card.Controls.Add(new System.Windows.Forms.Label
						{
							Text = "\ud83d\udc64 " + musteri,
							Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
							Location = new Point(16, 65),
							AutoSize = true,
							MaximumSize = new System.Drawing.Size(240, 25),
							AutoEllipsis = true
						});
						card.Controls.Add(new System.Windows.Forms.Label
						{
							Text = $"\ud83d\udcc5 {value:dd MMM}  ➔  {value2:dd MMM}",
							Font = new Font("Segoe UI", 9f),
							ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
							Location = new Point(18, 92),
							AutoSize = true
						});
						RoundedPanel roundedPanel = new RoundedPanel
						{
							Location = new Point(20, 120),
							Size = new System.Drawing.Size(255, 52),
							BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
							BorderRadius = 8
						};
						card.Controls.Add(roundedPanel);
						roundedPanel.Controls.Add(new System.Windows.Forms.Label
						{
							Text = "Toplam",
							Font = new Font("Segoe UI", 7f),
							ForeColor = System.Drawing.Color.Gray,
							Location = new Point(10, 8),
							AutoSize = true
						});
						roundedPanel.Controls.Add(new System.Windows.Forms.Label
						{
							Text = $"{num2:N0} ₺",
							Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
							Location = new Point(10, 22),
							AutoSize = true
						});
						roundedPanel.Controls.Add(new System.Windows.Forms.Label
						{
							Text = "Kalan Borç",
							Font = new Font("Segoe UI", 7f),
							ForeColor = System.Drawing.Color.Gray,
							Location = new Point(130, 8),
							AutoSize = true
						});
						roundedPanel.Controls.Add(new System.Windows.Forms.Label
						{
							Text = $"{num4:N0} ₺",
							Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
							ForeColor = ((num4 > 0m) ? System.Drawing.Color.FromArgb(239, 68, 68) : System.Drawing.Color.FromArgb(16, 185, 129)),
							Location = new Point(130, 22),
							AutoSize = true
						});
						Button button2 = new Button
						{
							Text = "➕ EKSTRA YAZ",
							Size = new System.Drawing.Size(120, 36),
							Location = new Point(20, 180),
							BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
							ForeColor = System.Drawing.Color.White,
							FlatStyle = FlatStyle.Flat,
							Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
							Cursor = Cursors.Hand
						};
						button2.FlatAppearance.BorderSize = 0;
						button2.Click += delegate
						{
							Form fS = new Form
							{
								Text = "➕ Ekstra Tüketim: " + musteri,
								Size = new System.Drawing.Size(380, 270),
								StartPosition = FormStartPosition.CenterParent,
								BackColor = System.Drawing.Color.White,
								FormBorderStyle = FormBorderStyle.FixedDialog
							};
							fS.Controls.Add(new System.Windows.Forms.Label
							{
								Text = "Hizmet / Tüketim Adı :",
								Location = new Point(30, 25),
								AutoSize = true,
								Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
							});
							TextBox txtHN = new TextBox
							{
								Location = new Point(30, 45),
								Width = 300,
								Font = new Font("Segoe UI", 10f)
							};
							fS.Controls.Add(txtHN);
							fS.Controls.Add(new System.Windows.Forms.Label
							{
								Text = "Tutar (₺) :",
								Location = new Point(30, 85),
								AutoSize = true,
								Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
							});
							TextBox txtHP = new TextBox
							{
								Location = new Point(30, 105),
								Width = 300,
								Font = new Font("Segoe UI", 10f)
							};
							fS.Controls.Add(txtHP);
							Button button4 = new Button
							{
								Text = "HESABA EKLE",
								Location = new Point(30, 155),
								Size = new System.Drawing.Size(300, 45),
								BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
								ForeColor = System.Drawing.Color.White,
								FlatStyle = FlatStyle.Flat,
								Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold)
							};
							fS.Controls.Add(button4);
							button4.Click += delegate
							{
								if (decimal.TryParse(txtHP.Text, out var result))
								{
									DataAccess.AddServiceToReservation(resId, txtHN.Text, result, "Ekstra");
									MessageBox.Show("Başarıyla hesabına yazıldı.");
									fS.Close();
								}
								else
								{
									MessageBox.Show("Hata: Geçerli bir tutar giriniz.");
								}
							};
							fS.ShowDialog();
						};
						Button button3 = new Button
						{
							Text = "\ud83d\udcb3 ÇIKIŞ YAP",
							Size = new System.Drawing.Size(125, 36),
							Location = new Point(150, 180),
							BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
							ForeColor = System.Drawing.Color.White,
							FlatStyle = FlatStyle.Flat,
							Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
							Cursor = Cursors.Hand
						};
						button3.FlatAppearance.BorderSize = 0;
						button3.Click += delegate
						{
							ShowPage("Ödeme");
						};
						card.Controls.Add(button2);
						card.Controls.Add(button3);
						flowRes.Controls.Add(card);
					}
				}
				flowRes.ResumeLayout();
			}
		});
		static GraphicsPath ResGetPath(System.Drawing.Rectangle r, int d)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(r.X, r.Y, d, d, 180f, 90f);
			graphicsPath.AddArc(r.X + r.Width - d, r.Y, d, d, 270f, 90f);
			graphicsPath.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0f, 90f);
			graphicsPath.AddArc(r.X, r.Y + r.Height - d, d, d, 90f, 90f);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
	}

	private void ShowReservationForm(string preSelectedRoom = "", string preSelectedTC = "", bool isWalkIn = true, int preSelectedBed = 0)
	{
		int targetBed = preSelectedBed;
		Form f = new Form
		{
			Text = "🏨 Rezervasyon İşlemleri",
			Size = new System.Drawing.Size(1060, 660),
			StartPosition = FormStartPosition.CenterScreen,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			BackColor = System.Drawing.Color.FromArgb(245, 247, 251),
			MinimizeBox = false,
			MaximizeBox = false
		};
		Panel panel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 68,
			BackColor = System.Drawing.Color.FromArgb(15, 23, 42)
		};
		panel.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(
				new System.Drawing.Point(0, 0), new System.Drawing.Point(panel.Width, 0),
				System.Drawing.Color.FromArgb(15, 23, 42), System.Drawing.Color.FromArgb(30, 41, 59));
			e.Graphics.FillRectangle(lgb, e.ClipRectangle);
			// gold accent line at bottom
			using var goldPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(201, 151, 58), 2f);
			e.Graphics.DrawLine(goldPen, 0, 67, panel.Width, 67);
		};
		var iconLbl = new System.Windows.Forms.Label
		{
			Text = "🏨",
			Font = new Font("Segoe UI Emoji", 18f),
			ForeColor = System.Drawing.Color.FromArgb(201, 151, 58),
			Location = new System.Drawing.Point(18, 14),
			AutoSize = true
		};
		panel.Controls.Add(iconLbl);
		System.Windows.Forms.Label value = new System.Windows.Forms.Label
		{
			Text = "Rezervasyon İşlemleri",
			Font = new Font("Segoe UI", 15f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.White,
			Location = new System.Drawing.Point(58, 20),
			AutoSize = true
		};
		panel.Controls.Add(value);
		System.Windows.Forms.Label lblDatesHeader = new System.Windows.Forms.Label
		{
			Text = "",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
			Location = new Point(260, 21),
			AutoSize = true
		};
		panel.Controls.Add(lblDatesHeader);
		Button button = new Button
		{
			Text = "✕",
			Size = new System.Drawing.Size(40, 40),
			Location = new System.Drawing.Point(1005, 14),
			FlatStyle = FlatStyle.Flat,
			BackColor = System.Drawing.Color.FromArgb(40, 255, 255, 255),
			ForeColor = System.Drawing.Color.FromArgb(200, 200, 200),
			Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand,
			TabStop = false
		};
		button.FlatAppearance.BorderSize = 0;
		button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(239, 68, 68);
		button.Click += delegate
		{
			f.Close();
		};
		panel.Controls.Add(button);
		f.Controls.Add(panel);
		Panel panel2 = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(20)
		};
		f.Controls.Add(panel2);
		int num = 340;
		RoundedPanel pnlLeft = new RoundedPanel
		{
			Location = new System.Drawing.Point(20, 20),
			Size = new System.Drawing.Size(num, 520),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 12
		};
		pnlLeft.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(225, 230, 240), 1f);
			e.Graphics.DrawRectangle(pen, 0, 0, pnlLeft.Width - 1, pnlLeft.Height - 1);
		};
		panel2.Controls.Add(pnlLeft);
		Panel pnlLH = new Panel
		{
			Dock = DockStyle.Top,
			Height = 46,
			BackColor = System.Drawing.Color.FromArgb(15, 23, 42)
		};
		pnlLH.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(
				new System.Drawing.Point(0, 0), new System.Drawing.Point(pnlLH.Width, 0),
				System.Drawing.Color.FromArgb(15, 23, 42), System.Drawing.Color.FromArgb(30, 41, 59));
			e.Graphics.FillRectangle(lgb, new System.Drawing.Rectangle(0, 0, pnlLH.Width, pnlLH.Height));
			using var goldPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(201, 151, 58), 3f);
			e.Graphics.DrawLine(goldPen, 0, 0, 0, pnlLH.Height);
		};
		pnlLH.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "📅  Rezervasyon Detayları",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.White,
			Location = new System.Drawing.Point(14, 13),
			AutoSize = true
		});
		pnlLeft.Controls.Add(pnlLH);
		int num2 = 60;
		LLbl("Giriş Tarihi", 15, num2);
		LLbl("Saat", 205, num2);
		num2 += 18;
		DateTimePicker dtpGiris = new DateTimePicker
		{
			Format = DateTimePickerFormat.Short,
			Value = (isWalkIn ? DateTime.Today : DateTime.Today.AddDays(1.0)),
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(180, 28)
		};
		StyleInput(dtpGiris);
		pnlLeft.Controls.Add(dtpGiris);
		NumericUpDown numericUpDown = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 23m,
			Value = 14m,
			Location = new Point(205, num2),
			Size = new System.Drawing.Size(60, 28)
		};
		StyleInput(numericUpDown);
		pnlLeft.Controls.Add(numericUpDown);
		pnlLeft.Controls.Add(new System.Windows.Forms.Label
		{
			Text = ":00",
			Font = new Font("Segoe UI", 9f),
			Location = new Point(268, num2 + 4),
			AutoSize = true
		});
		num2 += 42;
		LLbl("Çıkış Tarihi", 15, num2);
		LLbl("Saat", 205, num2);
		num2 += 18;
		DateTimePicker dtpCikis = new DateTimePicker
		{
			Format = DateTimePickerFormat.Short,
			Value = DateTime.Today.AddDays(1.0),
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(180, 28)
		};
		StyleInput(dtpCikis);
		pnlLeft.Controls.Add(dtpCikis);
		NumericUpDown numericUpDown2 = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 23m,
			Value = 11m,
			Location = new Point(205, num2),
			Size = new System.Drawing.Size(60, 28)
		};
		StyleInput(numericUpDown2);
		pnlLeft.Controls.Add(numericUpDown2);
		pnlLeft.Controls.Add(new System.Windows.Forms.Label
		{
			Text = ":00",
			Font = new Font("Segoe UI", 9f),
			Location = new Point(268, num2 + 4),
			AutoSize = true
		});
		num2 += 42;
		LLbl("Oda Tipi Tercihi", 15, num2);
		LLbl("Gece", 225, num2);
		num2 += 18;
		ComboBox comboBox = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(200, 28)
		};
		ComboBox.ObjectCollection items = comboBox.Items;
		object[] items2 = new string[5] { "-- Seçiniz --", "Standart", "Deniz Manzaralı", "Suit", "Deluxe" };
		items.AddRange(items2);
		comboBox.SelectedIndex = 0;
		StyleInput(comboBox);
		pnlLeft.Controls.Add(comboBox);
		NumericUpDown numGece = new NumericUpDown
		{
			Minimum = 1m,
			Maximum = 365m,
			Value = 1m,
			Location = new Point(225, num2),
			Size = new System.Drawing.Size(70, 28),
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold)
		};
		StyleInput(numGece);
		pnlLeft.Controls.Add(numGece);
		num2 += 42;
		Action updateDatesHeader = delegate
		{
			lblDatesHeader.Text = $"|  \ud83d\udcc5 {dtpGiris.Value:dd MMMM} ➔ {dtpCikis.Value:dd MMMM} ({numGece.Value} Gece)";
		};
		dtpGiris.ValueChanged += delegate
		{
			if (dtpCikis.Value <= dtpGiris.Value)
			{
				dtpCikis.Value = dtpGiris.Value.AddDays(1.0);
			}
			numGece.Value = Math.Max(1, (dtpCikis.Value - dtpGiris.Value).Days);
			updateDatesHeader();
		};
		dtpCikis.ValueChanged += delegate
		{
			numGece.Value = Math.Max(1, (dtpCikis.Value - dtpGiris.Value).Days);
			updateDatesHeader();
		};
		numGece.ValueChanged += delegate
		{
			dtpCikis.Value = dtpGiris.Value.AddDays((int)numGece.Value);
			updateDatesHeader();
		};
		updateDatesHeader();
		LLbl("Tahsis Edilen Oda", 15, num2);
		LLbl("Acente / Kanal", 185, num2);
		num2 += 18;
		ComboBox cmbOda = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(160, 28)
		};
		UpdateRoomList();
		StyleInput(cmbOda);
		pnlLeft.Controls.Add(cmbOda);
		dtpGiris.ValueChanged += delegate
		{
			UpdateRoomList();
		};
		dtpCikis.ValueChanged += delegate
		{
			UpdateRoomList();
		};
		ComboBox cmbAcente = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(185, num2),
			Size = new System.Drawing.Size(140, 28)
		};
		ComboBox.ObjectCollection items3 = cmbAcente.Items;
		items2 = new string[7] { "Direct", "Booking.com", "Expedia", "Tatilsepeti", "Otelz", "Ets Tur", "Diğer" };
		items3.AddRange(items2);
		cmbAcente.SelectedIndex = 0;
		StyleInput(cmbAcente);
		pnlLeft.Controls.Add(cmbAcente);
		num2 += 42;
		LLbl("Firma (Kurumsal)", 15, num2);
		LLbl("Komisyon (₺)", 225, num2);
		num2 += 18;
		ComboBox cmbFirma = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(200, 28)
		};
		cmbFirma.Items.Add("-- Bireysel --");
		try
		{
			DataTable companies = DataAccess.GetCompanies();
			foreach (DataRow row in companies.Rows)
			{
				cmbFirma.Items.Add(row["CompanyName"].ToString());
			}
		}
		catch
		{
		}
		cmbFirma.SelectedIndex = 0;
		StyleInput(cmbFirma);
		pnlLeft.Controls.Add(cmbFirma);
		NumericUpDown numComm = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 10000m,
			Value = 0m,
			Location = new Point(225, num2),
			Size = new System.Drawing.Size(70, 28)
		};
		StyleInput(numComm);
		pnlLeft.Controls.Add(numComm);
		num2 += 42;
		LLbl("Pansiyon Tipi", 15, num2);
		LLbl("Etiket", 225, num2);
		num2 += 18;
		ComboBox comboBox2 = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(200, 28)
		};
		ComboBox.ObjectCollection items4 = comboBox2.Items;
		items2 = new string[4] { "Sadece Oda (RO)", "Oda + Kahvaltı (BB)", "Yarım Pansiyon (HB)", "Tam Pansiyon (FB)" };
		items4.AddRange(items2);
		comboBox2.SelectedIndex = 0;
		StyleInput(comboBox2);
		pnlLeft.Controls.Add(comboBox2);
		Panel pnlRenk = new Panel
		{
			Location = new Point(225, num2),
			Size = new System.Drawing.Size(28, 28),
			BackColor = accentBlue,
			Cursor = Cursors.Hand
		};
		pnlRenk.Paint += delegate(object? s, PaintEventArgs e)
		{
			e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, 27, 27);
		};
		pnlRenk.Click += delegate
		{
			using ColorDialog colorDialog = new ColorDialog();
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				pnlRenk.BackColor = colorDialog.Color;
			}
		};
		pnlLeft.Controls.Add(pnlRenk);
		CheckBox chkCI = new CheckBox
		{
			Text = "Walk-in (Hemen Giriş)",
			Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(16, 185, 129),
			Location = new Point(15, num2 + 35),
			AutoSize = true,
			Checked = isWalkIn
		};
		pnlLeft.Controls.Add(chkCI);
		num2 += 65;
		LLbl("Yatak Seçimi", 15, num2);
		num2 += 18;
		ComboBox cmbYatak = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num2),
			Size = new System.Drawing.Size(110, 28)
		};
		StyleInput(cmbYatak);
		pnlLeft.Controls.Add(cmbYatak);
		cmbOda.SelectedIndexChanged += delegate
		{
			UpdateBeds();
		};
		dtpGiris.ValueChanged += delegate
		{
			UpdateBeds();
		};
		dtpCikis.ValueChanged += delegate
		{
			UpdateBeds();
		};
		UpdateBeds();
		Button btnSave = new Button
		{
			Text = "⚡  WALK-IN GİRİŞİ YAP",
			Location = new System.Drawing.Point(20, 555),
			Size = new System.Drawing.Size(num, 48),
			BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		// Gold bottom border effect via paint
		btnSave.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(80, 255, 255, 255), 1f);
			e.Graphics.DrawLine(pen, 4, btnSave.Height - 2, btnSave.Width - 4, btnSave.Height - 2);
		};
		btnSave.FlatAppearance.BorderSize = 0;
		chkCI.CheckedChanged += delegate
		{
			if (chkCI.Checked)
			{
				btnSave.Text = "⚡ WALK-IN GİRİŞİ YAP";
				btnSave.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
			}
			else
			{
				btnSave.Text = "\ud83d\udcc5 REZERVASYONU KAYDET";
				btnSave.BackColor = accentBlue;
			}
		};
		panel2.Controls.Add(btnSave);
		int num3 = num + 40;
		int num4 = 1020 - num3 - 20;
		RoundedPanel pnlRight = new RoundedPanel
		{
			Location = new System.Drawing.Point(num3, 20),
			Size = new System.Drawing.Size(num4, 520),
			BackColor = System.Drawing.Color.White,
			BorderRadius = 12
		};
		pnlRight.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(225, 230, 240), 1f);
			e.Graphics.DrawRectangle(pen, 0, 0, pnlRight.Width - 1, pnlRight.Height - 1);
		};
		panel2.Controls.Add(pnlRight);
		Panel pnlRH = new Panel
		{
			Dock = DockStyle.Top,
			Height = 46,
			BackColor = System.Drawing.Color.FromArgb(15, 23, 42)
		};
		pnlRH.Paint += delegate(object? s, PaintEventArgs e)
		{
			using var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(
				new System.Drawing.Point(0, 0), new System.Drawing.Point(pnlRH.Width, 0),
				System.Drawing.Color.FromArgb(15, 23, 42), System.Drawing.Color.FromArgb(30, 41, 59));
			e.Graphics.FillRectangle(lgb, new System.Drawing.Rectangle(0, 0, pnlRH.Width, pnlRH.Height));
			using var goldPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(201, 151, 58), 3f);
			e.Graphics.DrawLine(goldPen, 0, 0, 0, pnlRH.Height);
		};
		pnlRH.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "👥  Misafir Bilgileri",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.White,
			Location = new System.Drawing.Point(14, 13),
			AutoSize = true
		});
		pnlRight.Controls.Add(pnlRH);
		int num5 = 60;
		int num6 = (num4 - 30) / 3;
		RLbl("Kimlik / Pasaport No", 15, num5);
		RLbl("Ad Soyad", 15 + num6, num5);
		RLbl("Telefon", 15 + num6 * 2, num5);
		num5 += 18;
		TextBox txtTC = new TextBox
		{
			Name = "txtTC",
			PlaceholderText = "11 Haneli TC",
			MaxLength = 11,
			Text = preSelectedTC,
			Location = new Point(15, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		TextBox txtAdSoyad = new TextBox
		{
			Name = "txtAdSoyad",
			PlaceholderText = "Ad Soyad",
			Location = new Point(15 + num6, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		TextBox txtTelRt = new TextBox
		{
			Name = "txtTel",
			PlaceholderText = "Telefon",
			Location = new Point(15 + num6 * 2, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		StyleInput(txtTC);
		StyleInput(txtAdSoyad);
		StyleInput(txtTelRt);
		pnlRight.Controls.AddRange(new Control[3] { txtTC, txtAdSoyad, txtTelRt });
		num5 += 42;
		RLbl("Ülke", 15, num5);
		RLbl("Doğum Tarihi", 15 + num6, num5);
		RLbl("Cinsiyet", 15 + num6 * 2, num5);
		num5 += 18;
		ComboBox cmbUlke = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		ComboBox.ObjectCollection items5 = cmbUlke.Items;
		items2 = new string[10] { "Türkiye", "Almanya", "İngiltere", "Fransa", "İtalya", "İspanya", "Hollanda", "Rusya", "ABD", "Diğer" };
		items5.AddRange(items2);
		cmbUlke.SelectedIndex = 0;
		DateTimePicker dtpDogum = new DateTimePicker
		{
			Format = DateTimePickerFormat.Short,
			Value = new DateTime(1990, 1, 1),
			Location = new Point(15 + num6, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		ComboBox cmbCins = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(15 + num6 * 2, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		ComboBox.ObjectCollection items6 = cmbCins.Items;
		items2 = new string[3] { "Erkek", "Kadın", "Belirtilmemiş" };
		items6.AddRange(items2);
		cmbCins.SelectedIndex = 0;
		StyleInput(cmbUlke);
		StyleInput(dtpDogum);
		StyleInput(cmbCins);
		pnlRight.Controls.AddRange(new Control[3] { cmbUlke, dtpDogum, cmbCins });
		num5 += 42;
		RLbl("Baba Adı", 15, num5);
		RLbl("Anne Adı", 15 + num6, num5);
		RLbl("Doğum Yeri", 15 + num6 * 2, num5);
		num5 += 18;
		TextBox txtBaba = new TextBox
		{
			PlaceholderText = "Baba Adı",
			Location = new Point(15, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		TextBox txtAnne = new TextBox
		{
			PlaceholderText = "Anne Adı",
			Location = new Point(15 + num6, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		TextBox txtDYer = new TextBox
		{
			PlaceholderText = "Doğum Yeri",
			Location = new Point(15 + num6 * 2, num5),
			Size = new System.Drawing.Size(num6 - 10, 28)
		};
		StyleInput(txtBaba);
		StyleInput(txtAnne);
		StyleInput(txtDYer);
		pnlRight.Controls.AddRange(new Control[3] { txtBaba, txtAnne, txtDYer });
		num5 += 42;
		Button button2 = new Button
		{
			Text = "✚  Misafiri Listeye Ekle",
			Location = new System.Drawing.Point(15, num5),
			Size = new System.Drawing.Size(190, 34),
			BackColor = System.Drawing.Color.FromArgb(201, 151, 58),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button2.FlatAppearance.BorderSize = 0;
		pnlRight.Controls.Add(button2);
		num5 += 48;
		pnlRight.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcdd EKLENEN MİSAFİRLER",
			Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
			ForeColor = accentBlue,
			Location = new Point(15, num5),
			AutoSize = true
		});
		num5 += 18;
		ListBox lstMis = new ListBox
		{
			Location = new Point(15, num5),
			Size = new System.Drawing.Size(num4 - 30, 130),
			Font = new Font("Segoe UI", 9f),
			BorderStyle = BorderStyle.FixedSingle,
			BackColor = System.Drawing.Color.FromArgb(252, 253, 255),
			ItemHeight = 24
		};
		pnlRight.Controls.Add(lstMis);
		num5 += 145;
		pnlRight.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcb0 ÜCRET VE KONAKLAMA NOTU",
			Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(15, num5),
			AutoSize = true
		});
		num5 += 18;
		Panel pnlFiy = new Panel
		{
			Location = new Point(15, num5),
			Size = new System.Drawing.Size(num4 - 30, 100),
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252)
		};
		pnlFiy.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(210, 215, 228), 1f);
			e.Graphics.DrawRectangle(pen, 0, 0, pnlFiy.Width - 1, pnlFiy.Height - 1);
		};
		pnlRight.Controls.Add(pnlFiy);
		pnlFiy.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "ODA ÜCRETİ",
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			Location = new Point(10, 8),
			AutoSize = true
		});
		NumericUpDown numOdaTutar = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 9999999m,
			Value = 0m,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Location = new Point(10, 25),
			Size = new System.Drawing.Size(110, 30),
			BorderStyle = BorderStyle.FixedSingle
		};
		pnlFiy.Controls.Add(numOdaTutar);
		pnlFiy.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "EKSTRA / HİZMET",
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			Location = new Point(130, 8),
			AutoSize = true
		});
		NumericUpDown numEkstra = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 99999m,
			Value = 0m,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Location = new Point(130, 25),
			Size = new System.Drawing.Size(100, 30),
			BorderStyle = BorderStyle.FixedSingle
		};
		pnlFiy.Controls.Add(numEkstra);
		pnlFiy.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "TOPLAM",
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			ForeColor = accentBlue,
			Location = new Point(245, 8),
			AutoSize = true
		});
		System.Windows.Forms.Label lblGenelToplam = new System.Windows.Forms.Label
		{
			Text = "0 ₺",
			Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
			ForeColor = accentBlue,
			Location = new Point(245, 24),
			AutoSize = true
		};
		pnlFiy.Controls.Add(lblGenelToplam);
		ComboBox cmbDvz = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(pnlFiy.Width - 90, 25),
			Size = new System.Drawing.Size(70, 28),
			BackColor = System.Drawing.Color.White
		};
		ComboBox.ObjectCollection items7 = cmbDvz.Items;
		items2 = new string[3] { "TL", "USD", "EUR" };
		items7.AddRange(items2);
		cmbDvz.SelectedIndex = 0;
		pnlFiy.Controls.Add(cmbDvz);
		pnlFiy.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "REZERVASYON NOTLARI / ÖZEL İSTEKLER",
			Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
			Location = new Point(10, 60),
			AutoSize = true
		});
		TextBox txtNotlar = new TextBox
		{
			Location = new Point(10, 75),
			Size = new System.Drawing.Size(pnlFiy.Width - 150, 20),
			Font = new Font("Segoe UI", 8.5f),
			PlaceholderText = "Örn: Bebek yatağı istiyor, Sessiz oda..."
		};
		pnlFiy.Controls.Add(txtNotlar);
		CheckBox chkOdY = new CheckBox
		{
			Text = "Ödeme Yapıldı",
			Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
			ForeColor = successGreen,
			Location = new Point(pnlFiy.Width - 130, 73),
			AutoSize = true
		};
		pnlFiy.Controls.Add(chkOdY);
		Action updateTotal = delegate
		{
			decimal value2 = numOdaTutar.Value + numEkstra.Value;
			lblGenelToplam.Text = $"{value2:N0} {cmbDvz.SelectedItem}";
		};
		numOdaTutar.ValueChanged += delegate
		{
			updateTotal();
		};
		numEkstra.ValueChanged += delegate
		{
			updateTotal();
		};
		cmbDvz.SelectedIndexChanged += delegate
		{
			updateTotal();
		};
		Action updatePrice = delegate
		{
			string text = cmbOda.SelectedItem?.ToString() ?? "";
			if (text == "Seçiniz" || string.IsNullOrEmpty(text))
			{
				return;
			}
			try
			{
				DataRow roomInfo = DataAccess.GetRoomInfo(text);
				if (roomInfo != null)
				{
					decimal num7 = Convert.ToDecimal(roomInfo["CurrentPrice"]);
					numOdaTutar.Value = Math.Min(numOdaTutar.Maximum, num7 * (decimal)(int)numGece.Value);
				}
			}
			catch
			{
			}
		};
		cmbOda.SelectedIndexChanged += delegate
		{
			updatePrice();
		};
		numGece.ValueChanged += delegate
		{
			updatePrice();
		};
		List<(string tc, string adSoyad, string tel, string ulke, string cinsiyet, string father, string mother, string birthPlace, DateTime birthDate)> guestList = new List<(string, string, string, string, string, string, string, string, DateTime)>();
		ListBox lstSug = new ListBox
		{
			Visible = false,
			Size = new System.Drawing.Size(num6 * 2, 120),
			Font = new Font("Segoe UI", 9f),
			BorderStyle = BorderStyle.FixedSingle,
			BackColor = System.Drawing.Color.White,
			Cursor = Cursors.Hand
		};
		pnlRight.Controls.Add(lstSug);
		lstSug.BringToFront();
		Action<DataRow> FillFromDataRow = delegate(DataRow dr)
		{
			txtTC.Text = dr["IdentityNumber"].ToString();
			txtAdSoyad.Text = dr["FirstName"]?.ToString() + " " + dr["LastName"];
			txtTelRt.Text = dr["Phone"]?.ToString() ?? "";
			txtBaba.Text = dr["FatherName"]?.ToString() ?? "";
			txtAnne.Text = dr["MotherName"]?.ToString() ?? "";
			txtDYer.Text = dr["BirthPlace"]?.ToString() ?? "";
			if (dr["BirthDate"] != DBNull.Value)
			{
				dtpDogum.Value = Convert.ToDateTime(dr["BirthDate"]);
			}
			string value2 = dr["Nationality"]?.ToString() ?? "Türkiye";
			int num7 = cmbUlke.Items.IndexOf(value2);
			cmbUlke.SelectedIndex = ((num7 >= 0) ? num7 : 0);
			string value3 = dr["Gender"]?.ToString() ?? "Erkek";
			int num8 = cmbCins.Items.IndexOf(value3);
			cmbCins.SelectedIndex = ((num8 >= 0) ? num8 : 0);
			lstSug.Visible = false;
		};
		bool isDeleting = false;
		txtTC.KeyDown += (s, e) =>
		{
			isDeleting = (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete);
		};

		txtTC.TextChanged += delegate
		{
			if (txtTC.Focused)
			{
				string text = txtTC.Text.Trim();
				if (string.IsNullOrEmpty(text))
				{
					txtAdSoyad.Clear();
					txtTelRt.Clear();
					lstSug.Visible = false;
				}
				else if (text.Length >= 3)
				{
					DataTable dt = DataAccess.GetCustomersByIdentityPrefix(text);
					ShowSuggestions(dt, txtTC);
					if (dt.Rows.Count > 0 && !isDeleting)
					{
						DataRow dr = dt.Rows[0];
						string fullTC = dr["IdentityNumber"].ToString();
						if (fullTC.StartsWith(text, StringComparison.OrdinalIgnoreCase))
						{
							int prefixLen = text.Length;
							FillFromDataRow(dr);
							txtTC.SelectionStart = prefixLen;
							txtTC.SelectionLength = fullTC.Length - prefixLen;
							lstSug.Visible = true;
						}
					}
				}
				else
				{
					lstSug.Visible = false;
				}
			}
		};
		txtAdSoyad.TextChanged += delegate
		{
			if (txtAdSoyad.Focused)
			{
				string text = txtAdSoyad.Text.Trim();
				if (string.IsNullOrEmpty(text))
				{
					txtTC.Clear();
					txtTelRt.Clear();
					lstSug.Visible = false;
				}
				else if (text.Length >= 1)
				{
					ShowSuggestions(DataAccess.GetCustomersByNamePrefix(text), txtAdSoyad);
				}
				else
				{
					lstSug.Visible = false;
				}
			}
		};
		lstSug.Click += delegate
		{
			if (lstSug.SelectedItem is DataRowView dataRowView)
			{
				FillFromDataRow(dataRowView.Row);
			}
			txtTC.Focus();
		};
		button2.Click += delegate
		{
			string text = txtTC.Text.Trim();
			string text2 = txtAdSoyad.Text.Trim();
			string textTelRtStr = txtTelRt.Text.Trim();
			
			if (string.IsNullOrEmpty(text) || text2.Length < 3)
			{
				MessageBox.Show("Kimlik No ve Ad Soyad eksik veya hatalı.", "Uyarı");
			}
			else if (text.Length != 11 || !System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d{11}$"))
			{
				MessageBox.Show("TC Kimlik numarası 11 haneli rakamlardan oluşmalıdır.", "Geçersiz TC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (!string.IsNullOrEmpty(textTelRtStr) && !System.Text.RegularExpressions.Regex.IsMatch(textTelRtStr, @"^(05|5)\d{9}$"))
			{
				MessageBox.Show("Lütfen geçerli bir cep telefonu numarası giriniz (Örn: 05xx veya 5xx).", "Geçersiz Telefon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (DataAccess.IsCustomerStaying(text))
			{
				MessageBox.Show("Bu müşteri zaten konaklıyor!", "Mükerrer Kayıt");
			}
			else
			{
				guestList.Add((text, text2, textTelRtStr, cmbUlke.SelectedItem?.ToString() ?? "Türkiye", cmbCins.SelectedItem?.ToString() ?? "Erkek", txtBaba.Text, txtAnne.Text, txtDYer.Text, dtpDogum.Value));
				lstMis.Items.Add("\ud83d\udc64 " + text2 + " | " + text);
				txtTC.Clear();
				txtAdSoyad.Clear();
				txtTelRt.Clear();
				txtBaba.Clear();
				txtAnne.Clear();
				txtDYer.Clear();
				txtTC.Focus();
			}
		};
		lstMis.DoubleClick += delegate
		{
			if (lstMis.SelectedIndex >= 0)
			{
				guestList.RemoveAt(lstMis.SelectedIndex);
				lstMis.Items.RemoveAt(lstMis.SelectedIndex);
			}
		};
		btnSave.Click += delegate
		{
			string text = cmbOda.SelectedItem?.ToString() ?? "";
			if (text == "Seçiniz" || string.IsNullOrEmpty(text))
			{
				string text2 = ((!string.IsNullOrEmpty(preSelectedRoom)) ? preSelectedRoom : "");
				if (!string.IsNullOrEmpty(text2))
				{
					string roomConflictDetails = DataAccess.GetRoomConflictDetails(text2, dtpGiris.Value, dtpCikis.Value);
					if (roomConflictDetails != null)
					{
						MessageBox.Show(roomConflictDetails, "Oda Dolu", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
				}
				MessageBox.Show("Lütfen müsait bir oda seçin.", "Uyarı");
			}
			else
			{
				List<(string, string, string, string, string, string, string, string, DateTime)> list = new List<(string, string, string, string, string, string, string, string, DateTime)>(guestList);
				if (list.Count == 0)
				{
					string text3 = txtTC.Text.Trim();
					string text4 = txtAdSoyad.Text.Trim();
					string textTel = txtTelRt.Text.Trim();
					
					if (string.IsNullOrEmpty(text3) || text4.Length < 3)
					{
						MessageBox.Show("Müşteri bilgisi eksik veya hatalı.", "Uyarı");
						return;
					}
					
					if (text3.Length != 11 || !System.Text.RegularExpressions.Regex.IsMatch(text3, @"^\d{11}$"))
					{
						MessageBox.Show("TC Kimlik numarası 11 haneli rakamlardan oluşmalıdır.", "Geçersiz TC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}
					
					if (!string.IsNullOrEmpty(textTel) && !System.Text.RegularExpressions.Regex.IsMatch(textTel, @"^(05|5)\d{9}$"))
					{
						MessageBox.Show("Lütfen geçerli bir cep telefonu numarası giriniz (Örn: 05xx veya 5xx).", "Geçersiz Telefon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					if (DataAccess.IsCustomerStaying(text3))
					{
						MessageBox.Show("Müşteri zaten konaklıyor!", "Hata");
						return;
					}
					list.Add((text3, text4, textTel, cmbUlke.SelectedItem?.ToString() ?? "Türkiye", cmbCins.SelectedItem?.ToString() ?? "Erkek", txtBaba.Text, txtAnne.Text, txtDYer.Text, dtpDogum.Value));
				}
				if (cmbYatak.Items.Count != 0 && !(cmbYatak.SelectedItem?.ToString() == "Dolu"))
				{
					try
					{
						int bedNumber = Convert.ToInt32(cmbYatak.SelectedItem);
						decimal value2 = numOdaTutar.Value;
						decimal value3 = numEkstra.Value;
						decimal num7 = value2 + value3;
						decimal value4 = numComm.Value;
						int num8 = Math.Max(1, list.Count);
						decimal num9 = num7 / (decimal)num8;
						decimal commission = value4 / (decimal)num8;
						foreach (var item in list)
						{
							string[] array = item.Item2.Split(new char[1] { ' ' }, 2);
							string firstName = array[0];
							string lastName = ((array.Length > 1) ? array[1] : "");
							DataRow customerByIdentity = DataAccess.GetCustomerByIdentity(item.Item1);
							int customerId = ((customerByIdentity == null) ? DataAccess.AddCustomer(firstName, lastName, item.Item3, "", text, bedNumber, "", item.Item1, item.Item6, item.Item7, item.Rest.Item1, item.Rest.Item2, item.Item5, item.Item4) : Convert.ToInt32(customerByIdentity["CustomerID"]));
							if (customerByIdentity != null)
							{
								DataAccess.UpdateCustomer(item.Item1, firstName, lastName, item.Item3, "", "", item.Item6, item.Item7, item.Rest.Item1, item.Rest.Item2, item.Item5, item.Item4);
							}
							int? companyId = null;
							if (cmbFirma.SelectedIndex > 0)
							{
								DataTable companies2 = DataAccess.GetCompanies();
								companyId = Convert.ToInt32(companies2.Rows[cmbFirma.SelectedIndex - 1]["CompanyID"]);
							}
							string targetStatus = chkCI.Checked ? "CheckedIn" : "Reserved";
							int reservationId = DataAccess.AddReservation(customerId, text, bedNumber, dtpGiris.Value, dtpCikis.Value, cmbAcente.Text, commission, companyId, num9, txtNotlar.Text, value3 / (decimal)num8, targetStatus);
							if (chkOdY.Checked && num9 > 0m)
							{
								string text5 = cmbDvz.SelectedItem?.ToString() ?? "TL";
								DataAccess.RecordPayment(reservationId, num9, "Girişte Tahsil Edildi (" + text5 + ")");
							}
						}
						string performedBy = AuthHelper.CurrentUser?.FullName ?? "Admin";
						EnterpriseDataAccess.LogAuditEvent("YENİ REZ", "RESERVATIONS", $"Oda {text} → {string.Join(", ", list.Select<(string, string, string, string, string, string, string, string, DateTime), string>(((string tc, string adSoyad, string tel, string ulke, string cinsiyet, string father, string mother, string birthPlace, DateTime birthDate) g) => g.adSoyad))} | {dtpGiris.Value:dd.MM.yy}–{dtpCikis.Value:dd.MM.yy} | {value2 + value3:N0} ₺", performedBy);
						MessageBox.Show("Rezervasyon başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						f.Close();
						ShowPage("Ana Sayfa");
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Hata: " + ex.Message);
						return;
					}
				}
				MessageBox.Show("Boş yatak yok!", "Uyarı");
			}
		};
		f.ShowDialog();
		void LLbl(string t, int x, int y2)
		{
			pnlLeft.Controls.Add(new System.Windows.Forms.Label
			{
				Text = t.ToUpper(),
				Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Location = new Point(x, y2),
				AutoSize = true
			});
		}
		void RLbl(string t, int x2, int y2)
		{
			pnlRight.Controls.Add(new System.Windows.Forms.Label
			{
				Text = t.ToUpper(),
				Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Location = new Point(x2, y2),
				AutoSize = true
			});
		}
		void ShowSuggestions(DataTable dt, Control targetCtrl)
		{
			if (dt.Rows.Count > 0)
			{
				if (!dt.Columns.Contains("DisplayTxt"))
				{
					dt.Columns.Add("DisplayTxt", typeof(string), "IdentityNumber + ' - ' + FirstName + ' ' + LastName");
				}
				lstSug.DataSource = dt;
				lstSug.DisplayMember = "DisplayTxt";
				lstSug.Location = new Point(targetCtrl.Left, targetCtrl.Bottom);
				lstSug.Width = targetCtrl.Width * 2;
				lstSug.Visible = true;
				lstSug.BringToFront();
			}
			else
			{
				lstSug.Visible = false;
			}
		}
		static void StyleInput(Control c)
		{
			c.Font = new Font("Segoe UI", 9f);
			if (c is TextBox textBox)
			{
				textBox.BorderStyle = BorderStyle.FixedSingle;
			}
			if (c is NumericUpDown numericUpDown3)
			{
				numericUpDown3.BorderStyle = BorderStyle.FixedSingle;
			}
		}
		void UpdateBeds()
		{
			cmbYatak.Items.Clear();
			string text = cmbOda.SelectedItem?.ToString() ?? "";
			if (text == "Seçiniz" || string.IsNullOrEmpty(text))
			{
				return;
			}
			try
			{
				int roomCapacity = DataAccess.GetRoomCapacity(text);
				for (int i = 1; i <= roomCapacity; i++)
				{
					if (!DataAccess.IsBedOccupied(text, i, dtpGiris.Value, dtpCikis.Value))
					{
						cmbYatak.Items.Add(i);
					}
				}
				if (targetBed > 0 && cmbYatak.Items.Contains(targetBed))
				{
					cmbYatak.SelectedItem = targetBed;
					targetBed = 0;
				}
				else if (cmbYatak.Items.Count > 0)
				{
					cmbYatak.SelectedIndex = 0;
				}
				else
				{
					cmbYatak.Items.Add("Dolu");
				}
			}
			catch
			{
			}
		}
		void UpdateRoomList()
		{
			string text = cmbOda.SelectedItem?.ToString();
			cmbOda.Items.Clear();
			cmbOda.Items.Add("Seçiniz");
			try
			{
				DataTable availableRoomsForDates = DataAccess.GetAvailableRoomsForDates(dtpGiris.Value, dtpCikis.Value);
				foreach (DataRow row2 in availableRoomsForDates.Rows)
				{
					cmbOda.Items.Add(row2["RoomNumber"].ToString());
				}
			}
			catch
			{
			}
			if (!string.IsNullOrEmpty(text) && text != "Seçiniz" && text != "Seçiniz")
			{
				if (cmbOda.Items.Contains(text))
				{
					cmbOda.SelectedItem = text;
				}
				else
				{
					string roomConflictDetails = DataAccess.GetRoomConflictDetails(text, dtpGiris.Value, dtpCikis.Value);
					if (roomConflictDetails != null)
					{
						MessageBox.Show(roomConflictDetails, "Oda Dolu / Çakışma", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					cmbOda.SelectedIndex = 0;
				}
			}
			else if (!string.IsNullOrEmpty(preSelectedRoom) && cmbOda.Items.Contains(preSelectedRoom))
			{
				cmbOda.SelectedItem = preSelectedRoom;
			}
			else if (!string.IsNullOrEmpty(preSelectedRoom))
			{
				string roomConflictDetails2 = DataAccess.GetRoomConflictDetails(preSelectedRoom, dtpGiris.Value, dtpCikis.Value);
				if (roomConflictDetails2 != null)
				{
					MessageBox.Show(roomConflictDetails2, "Oda Dolu / Çakışma", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				cmbOda.SelectedIndex = 0;
			}
			else
			{
				cmbOda.SelectedIndex = 0;
			}
		}
	}

	private void PageRooms(Panel body)
	{
		DataTable dt;
		try
		{
			dt = DataAccess.GetAllRoomsDetailed();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Exception ex3 = ex2;
			SafeInvoke(delegate
			{
				body.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Oda yuklenemedi: " + ex3.Message,
					ForeColor = System.Drawing.Color.Red,
					Location = new Point(0, 50),
					AutoSize = true
				});
			});
			return;
		}
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 110,
				Padding = new Padding(20, 20, 20, 10),
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(panel);
			int count = dt.Rows.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (DataRow row in dt.Rows)
			{
				string text = row["Durum"].ToString();
				if (text == "Available")
				{
					num++;
				}
				else if (text == "Occupied" || text == "Partial")
				{
					num2++;
				}
				else if (text == "Dirty" || text == "Maintenance")
				{
					num3++;
				}
			}
			panel.Controls.Add(AddStatCard("📊 TOPLAM ODA", count.ToString(), System.Drawing.Color.FromArgb(15, 23, 42), 20));
			panel.Controls.Add(AddStatCard("🟢 MÜSAİT ODA", num.ToString(), System.Drawing.Color.FromArgb(16, 185, 129), 225));
			panel.Controls.Add(AddStatCard("🛌 DOLU ODA", num2.ToString(), System.Drawing.Color.FromArgb(239, 68, 68), 430));
			panel.Controls.Add(AddStatCard("🧹 TEMİZLİK / BAKIM", num3.ToString(), System.Drawing.Color.FromArgb(245, 158, 11), 635));
			RoundedPanel pnlContent = new RoundedPanel
			{
				Location = new Point(20, 120),
				Size = new System.Drawing.Size(body.Width - 40, body.Height - 140),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			pnlContent.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
				e.Graphics.DrawPath(pen, RmGetRoundedPath(pnlContent.ClientRectangle, 15));
			};
			body.Controls.Add(pnlContent);
			Panel panel2 = new Panel
			{
				Dock = DockStyle.Top,
				Height = 70,
				Padding = new Padding(15, 15, 15, 5),
				BackColor = System.Drawing.Color.White
			};
			pnlContent.Controls.Add(panel2);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "🏨 Oda Listesi",
				Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
				Location = new Point(15, 20),
				AutoSize = true
			};
			panel2.Controls.Add(value);
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Right,
				Width = 650,
				FlowDirection = FlowDirection.RightToLeft,
				WrapContents = false,
				Padding = new Padding(0, 10, 0, 0)
			};
			panel2.Controls.Add(flowLayoutPanel);
			Button button = MkBtn("➕ YENİ ODA", System.Drawing.Color.FromArgb(16, 185, 129), System.Drawing.Color.White);
			Button button2 = MkBtn("🗑️ ODA SİL", System.Drawing.Color.FromArgb(254, 226, 226), System.Drawing.Color.FromArgb(220, 38, 38));
			Button button3 = MkBtn("💰 FİYAT YAP", System.Drawing.Color.FromArgb(99, 102, 241), System.Drawing.Color.White);
			Button button4 = MkBtn("📋 GEÇMİŞ", System.Drawing.Color.FromArgb(241, 245, 249), System.Drawing.Color.FromArgb(71, 85, 105));
			flowLayoutPanel.Controls.Add(button4);
			flowLayoutPanel.Controls.Add(button3);
			flowLayoutPanel.Controls.Add(button2);
			flowLayoutPanel.Controls.Add(button);
			Panel panel3 = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(15)
			};
			pnlContent.Controls.Add(panel3);
			panel3.BringToFront();
			DataGridView dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				DataSource = dt,
				Font = new Font("Segoe UI", 10.5f),
				GridColor = System.Drawing.Color.FromArgb(226, 232, 240),
				RowTemplate = 
				{
					Height = 50
				}
			};
			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(15, 10, 10, 10);
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(238, 242, 255);
			dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
			dgv.DefaultCellStyle.Padding = new Padding(15, 0, 10, 0);
			dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
			dgv.DataBindingComplete += delegate
			{
				if (dgv.Columns.Contains("RoomID"))
				{
					dgv.Columns["RoomID"].Visible = false;
				}
			};
			dgv.CellFormatting += delegate(object? s, DataGridViewCellFormattingEventArgs e)
			{
				if (dgv.Columns[e.ColumnIndex].Name == "GuncelFiyat" && e.Value != null && decimal.TryParse(e.Value.ToString(), out var result))
				{
					e.Value = result.ToString("N2") + " ₺";
					e.FormattingApplied = true;
				}
				if (dgv.Columns[e.ColumnIndex].Name == "Durum" && e.Value != null)
				{
					switch (e.Value.ToString() ?? "")
					{
					case "Available":
						e.Value = "Müsait";
						if (e.CellStyle != null)
						{
							e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
							e.CellStyle.Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
						}
						e.FormattingApplied = true;
						break;
					case "Occupied":
						e.Value = "Dolu";
						if (e.CellStyle != null)
						{
							e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
							e.CellStyle.Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
						}
						e.FormattingApplied = true;
						break;
					case "Partial":
						e.Value = "Kısmi Dolu";
						if (e.CellStyle != null)
						{
							e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
							e.CellStyle.Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
						}
						e.FormattingApplied = true;
						break;
					case "Dirty":
						e.Value = "Kirli";
						if (e.CellStyle != null)
						{
							e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(245, 158, 11);
							e.CellStyle.Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
						}
						e.FormattingApplied = true;
						break;
					case "Maintenance":
						e.Value = "Bakımda";
						if (e.CellStyle != null)
						{
							e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
							e.CellStyle.Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
						}
						e.FormattingApplied = true;
						break;
					}
				}
			};
			panel3.Controls.Add(dgv);
			button.Click += delegate
			{
				ShowAddRoomForm();
				ShowPage("Odalar");
			};
			button2.Click += delegate
			{
				if (dgv.SelectedRows.Count == 0)
				{
					MessageBox.Show("Silmek için bir oda seçin.");
				}
				else
				{
					string text2 = dgv.SelectedRows[0].Cells["RoomNumber"].Value?.ToString() ?? "";
					if (MessageBox.Show("Oda " + text2 + " silinecek. Emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						try
						{
							DataAccess.DeleteRoom(text2);
							ShowPage("Odalar");
						}
						catch (Exception ex4)
						{
							MessageBox.Show("Hata: " + ex4.Message);
						}
					}
				}
			};
			button3.Click += delegate
			{
				if (dgv.SelectedRows.Count == 0)
				{
					MessageBox.Show("Fiyat girmek için bir oda seçin.");
				}
				else
				{
					string roomNum = dgv.SelectedRows[0].Cells["RoomNumber"].Value?.ToString() ?? "";
					ShowSetPriceForm(roomNum);
					ShowPage("Odalar");
				}
			};
			button4.Click += delegate
			{
				if (dgv.SelectedRows.Count == 0)
				{
					MessageBox.Show("Fiyat geçmişi için bir oda seçin.");
				}
				else
				{
					string roomNum = dgv.SelectedRows[0].Cells["RoomNumber"].Value?.ToString() ?? "";
					ShowPriceHistoryForm(roomNum);
				}
			};
		});
		static Panel AddStatCard(string title, string val, System.Drawing.Color c, int x)
		{
			RoundedPanel p = new RoundedPanel
			{
				Location = new Point(x, 10),
				Size = new System.Drawing.Size(185, 85),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12
			};
			p.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
				e.Graphics.DrawPath(pen, RmGetRoundedPath(p.ClientRectangle, 12));
			};
			p.Controls.Add(new System.Windows.Forms.Label
			{
				Text = title,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(15, 15),
				AutoSize = true
			});
			p.Controls.Add(new System.Windows.Forms.Label
			{
				Text = val,
				Font = new Font("Segoe UI", 22f, System.Drawing.FontStyle.Bold),
				ForeColor = c,
				Location = new Point(12, 35),
				AutoSize = true
			});
			return p;
		}
		static Button MkBtn(string t, System.Drawing.Color bg, System.Drawing.Color fg)
		{
			Button button = new Button
			{
				Text = t,
				Size = new System.Drawing.Size(130, 42),
				BackColor = bg,
				ForeColor = fg,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Margin = new Padding(8, 0, 0, 0)
			};
			button.FlatAppearance.BorderSize = 0;
			return button;
		}
		static GraphicsPath RmGetRoundedPath(System.Drawing.Rectangle r, int d)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(r.X, r.Y, d, d, 180f, 90f);
			graphicsPath.AddArc(r.X + r.Width - d, r.Y, d, d, 270f, 90f);
			graphicsPath.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0f, 90f);
			graphicsPath.AddArc(r.X, r.Y + r.Height - d, d, d, 90f, 90f);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
	}

	private void ShowAddRoomForm()
	{
		Form f = new Form
		{
			Text = "Yeni Oda Ekle",
			Size = new System.Drawing.Size(400, 340),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		int y = 20;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Yeni Oda Ekle",
			Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, y),
			AutoSize = true
		});
		y += 45;
		TextBox txtNo = Field("Oda Numarasi:", "Ornek: 101, 102, 201...");
		TextBox txtKat = Field("Kat Numarasi:", "Ornek: 4");
		TextBox txtTip = Field("Oda Tipi:", "Standart veya Deniz Manzarali");
		TextBox txtKap = Field("Kapasite (Yatak):", "Ornek: 2");
		Button button = new Button
		{
			Text = "KAYDET",
			Location = new Point(30, y + 5),
			Size = new System.Drawing.Size(320, 42),
			BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold)
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			try
			{
				int result;
				int result2;
				if (string.IsNullOrWhiteSpace(txtNo.Text))
				{
					MessageBox.Show("Oda numarası zorunludur.");
				}
				else if (!int.TryParse(txtKat.Text, out result))
				{
					MessageBox.Show("Geçerli bir kat numarası girin.");
				}
				else if (!int.TryParse(txtKap.Text, out result2) || result2 < 1)
				{
					MessageBox.Show("Geçerli bir kapasite girin.");
				}
				else
				{
					DataAccess.AddRoom(txtNo.Text.Trim(), result, (txtTip.Text.Trim() == "") ? "Standart" : txtTip.Text.Trim(), result2);
					MessageBox.Show("Oda " + txtNo.Text + " eklendi!", "Başarılı");
					f.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		f.Controls.Add(button);
		f.ShowDialog();
		TextBox Field(string lbl, string ph)
		{
			f.Controls.Add(new System.Windows.Forms.Label
			{
				Text = lbl,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(30, y),
				AutoSize = true
			});
			y += 22;
			TextBox textBox = new TextBox
			{
				Location = new Point(30, y),
				Size = new System.Drawing.Size(320, 28),
				PlaceholderText = ph,
				BorderStyle = BorderStyle.FixedSingle
			};
			f.Controls.Add(textBox);
			y += 38;
			return textBox;
		}
	}

	private void ShowSetPriceForm(string roomNum)
	{
		DataRow roomInfo = DataAccess.GetRoomInfo(roomNum);
		decimal currentPrice = ((roomInfo != null) ? Convert.ToDecimal(roomInfo["CurrentPrice"]) : 0m);
		Form f = new Form
		{
			Text = "Oda " + roomNum + " - Fiyat Belirle",
			Size = new System.Drawing.Size(380, 220),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = $"Mevcut Fiyat: {currentPrice:N0} TL",
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			ForeColor = accentBlue,
			Location = new Point(30, 20),
			AutoSize = true
		});
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Yeni Fiyat (TL):",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, 60),
			AutoSize = true
		});
		TextBox txtP = new TextBox
		{
			Location = new Point(30, 82),
			Size = new System.Drawing.Size(300, 30),
			PlaceholderText = "Ornek: 1500",
			BorderStyle = BorderStyle.FixedSingle
		};
		f.Controls.Add(txtP);
		Button button = new Button
		{
			Text = "FİYAT KAYDET (eski fiyat korunur)",
			Location = new Point(30, 122),
			Size = new System.Drawing.Size(300, 42),
			BackColor = accentBlue,
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			if (!decimal.TryParse(txtP.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				MessageBox.Show("Geçerli bir fiyat girin.");
			}
			else
			{
				DataAccess.SetRoomPrice(roomNum, result);
				MessageBox.Show($"Oda {roomNum} için yeni fiyat {result:N0} TL olarak kaydedildi!\nEski fiyat ({currentPrice:N0} TL) geçmişte tutuldu.", "Başarılı");
				f.Close();
			}
		};
		f.Controls.Add(button);
		f.ShowDialog();
	}

	private void ShowPriceHistoryForm(string roomNum)
	{
		Form form = new Form
		{
			Text = "Oda " + roomNum + " - Fiyat Geçmişi",
			Size = new System.Drawing.Size(400, 380),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		form.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Oda " + roomNum + " Fiyat Geçmişi",
			Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
			Location = new Point(20, 15),
			AutoSize = true
		});
		try
		{
			DataTable roomPriceHistory = DataAccess.GetRoomPriceHistory(roomNum);
			DataGridView dataGridView = new DataGridView
			{
				Location = new Point(20, 50),
				Size = new System.Drawing.Size(340, 280),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				DataSource = roomPriceHistory,
				Font = new Font("Segoe UI", 10f)
			};
			dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
			dataGridView.EnableHeadersVisualStyles = false;
			dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
			form.Controls.Add(dataGridView);
		}
		catch
		{
			form.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Fiyat geçmişi bulunamadı.",
				Location = new Point(20, 55),
				AutoSize = true
			});
		}
		form.ShowDialog();
	}

	private void LoadReservationList(Panel body)
	{
		Control[] array = body.Controls.Find("pnlResList", searchAllChildren: false);
		Control[] array2 = array;
		foreach (Control value in array2)
		{
			body.Controls.Remove(value);
		}
		Panel pnlList = new Panel
		{
			Name = "pnlResList",
			Location = new Point(470, 0),
			Size = new System.Drawing.Size(480, 600),
			BackColor = System.Drawing.Color.White
		};
		pnlList.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(System.Drawing.Color.FromArgb(220, 220, 220));
			e.Graphics.DrawRectangle(pen, 0, 0, pnlList.Width - 1, pnlList.Height - 1);
		};
		body.Controls.Add(pnlList);
		pnlList.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Mevcut Rezervasyonlar (DB)",
			Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
			Location = new Point(15, 15),
			AutoSize = true
		});
		try
		{
			DataTable reservations = DataAccess.GetReservations();
			DataGridView dgv = new DataGridView
			{
				Name = "dgvRes",
				Location = new Point(15, 50),
				Size = new System.Drawing.Size(450, 450),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				DataSource = reservations
			};
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			pnlList.Controls.Add(dgv);
			Button button = new Button
			{
				Text = "\ud83d\udd14 CHECK-OUT (ÇIKIŞ YAP)",
				Location = new Point(15, 520),
				Size = new System.Drawing.Size(200, 45),
				BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button.FlatAppearance.BorderSize = 0;
			button.Click += delegate
			{
				if (dgv.SelectedRows.Count == 0)
				{
					MessageBox.Show("Lütfen çıkış yapacak rezervasyonu seçin.");
				}
				else
				{
					int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
					string text = dgv.SelectedRows[0].Cells["Status"].Value.ToString() ?? "";
					if (text == "CheckedOut")
					{
						MessageBox.Show("Bu rezervasyon zaten tamamlanmış.");
					}
					else if (MessageBox.Show("Seçili rezervasyon için çıkış işlemi yapılacak. Emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						try
						{
							DataAccess.CompleteReservation(reservationId);
							MessageBox.Show("Oda başarıyla boşaltıldı ve çıkış yapıldı!");
							LoadReservationList(body);
						}
						catch (Exception ex2)
						{
							MessageBox.Show("Hata: " + ex2.Message);
						}
					}
				}
			};
			pnlList.Controls.Add(button);
		}
		catch (Exception ex)
		{
			pnlList.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Hata: " + ex.Message,
				Location = new Point(15, 50),
				AutoSize = true
			});
		}
	}

	private void PageCustomers(Panel body)
	{
		DataTable dtAll;
		try
		{
			dtAll = DataAccess.GetAllCustomers();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Exception ex3 = ex2;
			SafeInvoke(delegate
			{
				body.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Müşteriler yüklenemedi: " + ex3.Message,
					ForeColor = System.Drawing.Color.Red,
					AutoSize = true
				});
			});
			return;
		}
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel pnlStats = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(20, 15, 20, 15)
			};
			body.Controls.Add(pnlStats);
			int count = dtAll.Rows.Count;
			int num = dtAll.AsEnumerable().Count((DataRow r) => r["ResStatus"] != DBNull.Value && r["ResStatus"].ToString() == "Aktif");
			int sx = 20;
			AddCustStat("TOPLAM ARŞİV", count.ToString(), System.Drawing.Color.FromArgb(99, 102, 241), "\ud83d\uddc2");
			AddCustStat("AKTİF MİSAFİRLER", num.ToString(), System.Drawing.Color.FromArgb(16, 185, 129), "\ud83d\udc65");
			RoundedPanel pnlFilters = new RoundedPanel
			{
				Location = new Point(20, 115),
				Size = new System.Drawing.Size(body.Width - 40, 80),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			pnlFilters.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
				e.Graphics.DrawPath(pen, CustGetPath(pnlFilters.ClientRectangle, 12));
			};
			body.Controls.Add(pnlFilters);
			int num2 = 25;
			pnlFilters.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udd0d FİLTRE & ARAMA:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(99, 102, 241),
				Location = new Point(20, num2 + 3),
				AutoSize = true
			});
			TextBox txtTC = new TextBox
			{
				Name = "filterTC",
				PlaceholderText = "TC Kimlik No...",
				Size = new System.Drawing.Size(130, 30),
				Location = new Point(160, num2),
				Font = new Font("Segoe UI", 10f)
			};
			TextBox txtName = new TextBox
			{
				Name = "filterName",
				PlaceholderText = "Müşteri Ad Soyad...",
				Size = new System.Drawing.Size(160, 30),
				Location = new Point(300, num2),
				Font = new Font("Segoe UI", 10f)
			};
			TextBox txtPhone = new TextBox
			{
				Name = "filterPhone",
				PlaceholderText = "Telefon...",
				Size = new System.Drawing.Size(120, 30),
				Location = new Point(470, num2),
				Font = new Font("Segoe UI", 10f)
			};
			Button button = new Button
			{
				Text = "+ YENİ PROFİL",
				Size = new System.Drawing.Size(140, 36),
				Location = new Point(pnlFilters.Width - 160, num2 - 4),
				BackColor = System.Drawing.Color.FromArgb(79, 102, 241),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			button.FlatAppearance.BorderSize = 0;
			button.Click += delegate
			{
				ShowCustomerForm();
				ShowPage("Müşteriler");
			};
			pnlFilters.Controls.AddRange(new Control[4] { txtTC, txtName, txtPhone, button });
			Panel panel = new Panel
			{
				Location = new Point(20, 210),
				Size = new System.Drawing.Size(body.Width - 40, body.Height - 230),
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(panel);
			FlowLayoutPanel flowCust = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(0, 0, 0, 20)
			};
			panel.Controls.Add(flowCust);
			txtTC.TextChanged += delegate
			{
				ApplyCustomerFilters();
			};
			txtName.TextChanged += delegate
			{
				ApplyCustomerFilters();
			};
			txtPhone.TextChanged += delegate
			{
				ApplyCustomerFilters();
			};
			ApplyCustomerFilters();
			void AddCustStat(string title, string val, System.Drawing.Color accent, string icon)
			{
				RoundedPanel p = new RoundedPanel
				{
					Location = new Point(sx, 12),
					Size = new System.Drawing.Size(240, 78),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12
				};
				p.Paint += delegate(object? s, PaintEventArgs e)
				{
					using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
					e.Graphics.DrawPath(pen, CustGetPath(p.ClientRectangle, 12));
					e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 6, p.Height);
				};
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = icon,
					Font = new Font("Segoe UI Emoji", 16f),
					Location = new Point(14, 25),
					AutoSize = true,
					ForeColor = accent
				});
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
					Location = new Point(55, 20),
					AutoSize = true
				});
				p.Controls.Add(new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
					Location = new Point(53, 35),
					AutoSize = true
				});
				pnlStats.Controls.Add(p);
				sx += 255;
			}
			void ApplyCustomerFilters()
			{
				string text = txtTC.Text.Trim().Replace("'", "''");
				string value = txtName.Text.Trim().Replace("'", "''");
				string text2 = txtPhone.Text.Trim().Replace("'", "''");
				string text3 = "1=1";
				if (!string.IsNullOrEmpty(text))
				{
					text3 = text3 + " AND IdentityNumber LIKE '%" + text + "%'";
				}
				if (!string.IsNullOrEmpty(value))
				{
					text3 += $" AND (FirstName LIKE '%{value}%' OR LastName LIKE '%{value}%')";
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text3 = text3 + " AND Phone LIKE '%" + text2 + "%'";
				}
				dtAll.DefaultView.RowFilter = text3;
				RenderCustomerCards();
			}
			void RenderCustomerCards()
			{
				flowCust.SuspendLayout();
				flowCust.Controls.Clear();
				int num3 = 0;
				foreach (DataRowView item in dtAll.DefaultView)
				{
					if (num3 >= 60)
					{
						System.Windows.Forms.Label value = new System.Windows.Forms.Label
						{
							Text = $"+ {dtAll.DefaultView.Count - 60} kayıt daha var. Daraltmak için arama yapın.",
							Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic),
							AutoSize = true,
							ForeColor = System.Drawing.Color.Gray,
							Margin = new Padding(10)
						};
						flowCust.Controls.Add(value);
						break;
					}
					DataRow row = item.Row;
					string idNum = row["IdentityNumber"].ToString();
					string text = row["FirstName"].ToString();
					string text2 = row["LastName"].ToString();
					string text3 = row["Phone"].ToString();
					string text4 = ((row["ResStatus"] != DBNull.Value) ? row["ResStatus"].ToString() : "");
					DateTime dateTime = ((row["CreatedAt"] != DBNull.Value) ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Today);
					bool isAct = text4 == "Aktif";
					RoundedPanel card = new RoundedPanel
					{
						Size = new System.Drawing.Size(300, 185),
						BackColor = System.Drawing.Color.White,
						BorderRadius = 16,
						Margin = new Padding(0, 0, 20, 20),
						Cursor = Cursors.Hand
					};
					card.Paint += delegate(object? s, PaintEventArgs e)
					{
						using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
						e.Graphics.DrawPath(pen, CustGetPath(card.ClientRectangle, 16));
						if (isAct)
						{
							using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(16, 185, 129)))
							{
								e.Graphics.FillRectangle(brush, 0, 0, 6, card.Height);
								return;
							}
						}
					};
					
					// Hover Effect
					card.MouseEnter += (s, e) => { card.BackColor = System.Drawing.Color.FromArgb(248, 250, 252); card.Invalidate(); };
					card.MouseLeave += (s, e) => { card.BackColor = System.Drawing.Color.White; card.Invalidate(); };
					
					card.Controls.Add(new System.Windows.Forms.Label
					{
						Text = (isAct ? "Aktif Misafir" : "Arşiv"),
						Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
						ForeColor = (isAct ? System.Drawing.Color.FromArgb(16, 185, 129) : System.Drawing.Color.FromArgb(148, 163, 184)),
						Location = new Point(card.Width - 85, 15),
						AutoSize = true
					});
					string text5 = ((text.Length > 0) ? text.Substring(0, 1) : "");
					string text6 = ((text2.Length > 0) ? text2.Substring(0, 1) : "");
					RoundedPanel roundedPanel = new RoundedPanel
					{
						Size = new System.Drawing.Size(46, 46),
						Location = new Point(15, 15),
						BackColor = System.Drawing.Color.FromArgb(238, 242, 255),
						BorderRadius = 23
					};
					roundedPanel.Controls.Add(new System.Windows.Forms.Label
					{
						Text = (text5 + text6).ToUpper(),
						Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(79, 102, 241),
						Location = new Point(6, 12),
						AutoSize = true,
						TextAlign = ContentAlignment.MiddleCenter
					});
					card.Controls.Add(roundedPanel);
					card.Controls.Add(new System.Windows.Forms.Label
					{
						Text = text + " " + text2,
						Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
						Location = new Point(70, 25),
						AutoSize = true,
						MaximumSize = new System.Drawing.Size(150, 25),
						AutoEllipsis = true
					});
					card.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "👤 TC / Pasaport : " + idNum,
						Font = new Font("Segoe UI", 8.5f),
						ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
						Location = new Point(18, 75),
						AutoSize = true

					});
					card.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "📞 Telefon : " + text3,
						Font = new Font("Segoe UI", 8.5f),
						ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
						Location = new Point(18, 100),
						AutoSize = true
					});
					card.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "📅 Kayıt : " + dateTime.ToString("dd MMM yyyy"),
						Font = new Font("Segoe UI", 8.5f),
						ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
						Location = new Point(18, 125),
						AutoSize = true
					});
					Button button2 = new Button
					{
						Text = "DÜZENLE",
						Size = new System.Drawing.Size(90, 30),
						Location = new Point(card.Width - 110, 140),
						BackColor = System.Drawing.Color.FromArgb(241, 245, 249),
						ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
						FlatStyle = FlatStyle.Flat,
						Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
						Cursor = Cursors.Hand
					};
					button2.FlatAppearance.BorderSize = 0;
					
					// Hover Effect for button
					button2.MouseEnter += (s, e) => { button2.BackColor = System.Drawing.Color.FromArgb(226, 232, 240); };
					button2.MouseLeave += (s, e) => { button2.BackColor = System.Drawing.Color.FromArgb(241, 245, 249); };

					button2.Click += delegate
					{
						DataRow customerByIdentity = DataAccess.GetCustomerByIdentity(idNum);
						if (customerByIdentity != null)
						{
							ShowCustomerForm(customerByIdentity);
							ShowPage("Müşteriler");
						}
					};
					card.Controls.Add(button2);
					
					Button button3 = new Button
					{
						Text = "GÖRÜNTÜLE",
						Size = new System.Drawing.Size(95, 30),
						Location = new Point(card.Width - 215, 140),
						BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
						ForeColor = System.Drawing.Color.FromArgb(99, 102, 241),
						FlatStyle = FlatStyle.Flat,
						Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
						Cursor = Cursors.Hand
					};
					button3.FlatAppearance.BorderSize = 1;
					button3.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(199, 210, 254);
					
					// Hover Effect for button
					button3.MouseEnter += (s, e) => { button3.BackColor = System.Drawing.Color.FromArgb(224, 231, 255); };
					button3.MouseLeave += (s, e) => { button3.BackColor = System.Drawing.Color.FromArgb(248, 250, 252); };
					
					int cid = Convert.ToInt32(row["CustomerID"]);
					button3.Click += delegate
					{
						ShowCustomerCard(cid);
					};
					card.Controls.Add(button3);
					
					flowCust.Controls.Add(card);
					num3++;
				}
				flowCust.ResumeLayout();
			}
		});
	}

	private void ShowCustomerForm(DataRow? editRow = null)
	{
		int num = Screen.PrimaryScreen?.WorkingArea.Height ?? 1080;
		Form f = new Form
		{
			Text = ((editRow == null) ? "✨ Yeni Müşteri Kaydı" : "\ud83d\udcdd Müşteri Düzenleme"),
			Size = new System.Drawing.Size(480, Math.Min(750, num - 50)),
			StartPosition = FormStartPosition.CenterScreen,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.Sizable,
			MinimumSize = new System.Drawing.Size(400, 500)
		};
		Panel pnlHead = new Panel
		{
			Dock = DockStyle.Top,
			Height = 70,
			BackColor = System.Drawing.Color.FromArgb(249, 250, 251)
		};
		pnlHead.Paint += delegate(object? obj, PaintEventArgs e)
		{
			e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(229, 231, 235)), 0, 69, pnlHead.Width, 69);
		};
		f.Controls.Add(pnlHead);
		pnlHead.Controls.Add(new System.Windows.Forms.Label
		{
			Text = ((editRow == null) ? "✨ Yeni Müşteri Kaydı" : "\ud83d\udcdd Müşteri Bilgi Düzenleme"),
			Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(17, 24, 39),
			Location = new Point(25, 22),
			AutoSize = true
		});
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 80,
			BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
			Padding = new Padding(25, 15, 25, 15)
		};
		Button button = new Button
		{
			Text = ((editRow == null) ? "✅ KAYDI TAMAMLA" : "\ud83d\udcbe BİLGİLERİ GÜNCELLE"),
			Dock = DockStyle.Fill,
			BackColor = ((editRow == null) ? System.Drawing.Color.FromArgb(16, 185, 129) : System.Drawing.Color.FromArgb(79, 70, 229)),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderSize = 0;
		panel.Controls.Add(button);
		f.Controls.Add(panel);
		Panel scrollPnl = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			Padding = new Padding(25)
		};
		f.Controls.Add(scrollPnl);
		int curY = 5;
		Section("KİMLİK VE KİŞİSEL BİLGİLER", ref curY);
		TextBox txtTC = Field("TC KİMLİK NUMARASI *", "11 haneli", ref curY);
		txtTC.MaxLength = 11;
		TextBox txtAd = Field("MÜŞTERİ ADI *", "Ad", ref curY);
		TextBox txtSoyad = Field("MÜŞTERİ SOYADI *", "Soyad", ref curY);
		curY += 5;
		Section("İLETİŞİM BİLGİLERİ", ref curY);
		TextBox txtTel = Field("TELEFON NUMARASI", "05xx...", ref curY);
		TextBox txtMail = Field("E-POSTA ADRESİ", "örnek@mail.com", ref curY);
		curY += 5;
		Section("ADRES VE KONUM", ref curY);
		Dictionary<string, string[]> ilceler = GeoHelper.GetCachedIlceler();
		Panel pnlIl = new Panel
		{
			Width = f.Width - 80,
			Height = 75,
			Location = new Point(0, curY),
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
			BackColor = System.Drawing.Color.White
		};
		pnlIl.Paint += delegate(object? obj, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, pnlIl.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
		};
		pnlIl.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "ŞEHİR (İL)",
			Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(12, 12),
			AutoSize = true
		});
		ComboBox cmbIl = new ComboBox
		{
			Location = new Point(12, 35),
			Width = pnlIl.Width - 24,
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Font = new Font("Segoe UI", 11f),
			FlatStyle = FlatStyle.Flat
		};
		ComboBox.ObjectCollection items = cmbIl.Items;
		object[] items2 = ilceler.Keys.ToArray();
		items.AddRange(items2);
		if (cmbIl.Items.Count > 0)
		{
			cmbIl.SelectedIndex = 0;
		}
		pnlIl.Controls.Add(cmbIl);
		scrollPnl.Controls.Add(pnlIl);
		curY += 85;
		Panel pnlIlce = new Panel
		{
			Width = f.Width - 80,
			Height = 75,
			Location = new Point(0, curY),
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
			BackColor = System.Drawing.Color.White
		};
		pnlIlce.Paint += delegate(object? obj, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, pnlIlce.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
		};
		pnlIlce.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "İLÇE",
			Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
			Location = new Point(12, 12),
			AutoSize = true
		});
		ComboBox cmbIlce = new ComboBox
		{
			Location = new Point(12, 35),
			Width = pnlIlce.Width - 24,
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Font = new Font("Segoe UI", 11f),
			FlatStyle = FlatStyle.Flat
		};
		pnlIlce.Controls.Add(cmbIlce);
		scrollPnl.Controls.Add(pnlIlce);
		curY += 85;
		cmbIl.SelectedIndexChanged += delegate
		{
			cmbIlce.Items.Clear();
			string text2 = cmbIl.SelectedItem?.ToString() ?? "";
			if (!string.IsNullOrEmpty(text2) && ilceler.TryGetValue(text2, out string[] value))
			{
				ComboBox.ObjectCollection items4 = cmbIlce.Items;
				object[] items5 = value;
				items4.AddRange(items5);
			}
			if (cmbIlce.Items.Count > 0)
			{
				cmbIlce.SelectedIndex = 0;
			}
		};
		TextBox txtAdresDetay = Field("Adres Detayı:", "Mahalle, Sokak, Kapı No...", ref curY);
		if (editRow != null)
		{
			txtTC.Text = editRow["IdentityNumber"]?.ToString() ?? "";
			txtTC.ReadOnly = true;
			txtTC.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
			txtAd.Text = editRow["FirstName"]?.ToString() ?? "";
			txtSoyad.Text = editRow["LastName"]?.ToString() ?? "";
			txtTel.Text = editRow["Phone"]?.ToString() ?? "";
			txtMail.Text = editRow["Email"]?.ToString() ?? "";
			string text = editRow["Address"]?.ToString() ?? "";
			if (text.Contains("/"))
			{
				string[] array = text.Split('/');
				int num2 = cmbIl.FindStringExact(array[0]);
				if (num2 != -1)
				{
					cmbIl.SelectedIndex = num2;
				}
				if (array.Length > 1)
				{
					string s = array[1].Split('-')[0].Trim();
					int num3 = cmbIlce.FindStringExact(s);
					if (num3 != -1)
					{
						cmbIlce.SelectedIndex = num3;
					}
					if (array[1].Contains("-"))
					{
						txtAdresDetay.Text = array[1].Substring(array[1].IndexOf('-') + 1).Trim();
					}
				}
			}
		}
		else
		{
			cmbIl.SelectedIndex = 0;
			if (ilceler.ContainsKey("İstanbul"))
			{
				ComboBox.ObjectCollection items3 = cmbIlce.Items;
				items2 = ilceler["İstanbul"];
				items3.AddRange(items2);
				cmbIlce.SelectedIndex = 0;
			}
			txtAd.TextChanged += delegate
			{
				if (string.IsNullOrEmpty(txtAd.Text.Trim()))
				{
					txtSoyad.Text = "";
					txtTel.Text = "";
					txtMail.Text = "";
					txtAdresDetay.Text = "";
					txtTC.Text = "";
					if (cmbIl.Items.Count > 0)
					{
						cmbIl.SelectedIndex = 0;
					}
				}
			};
			txtTC.TextChanged += delegate
			{
				string text2 = txtTC.Text.Trim();
				if (string.IsNullOrEmpty(text2))
				{
					txtAd.Text = "";
					txtSoyad.Text = "";
					txtTel.Text = "";
					txtMail.Text = "";
					txtAdresDetay.Text = "";
					if (cmbIl.Items.Count > 0)
					{
						cmbIl.SelectedIndex = 0;
					}
				}
				else if (text2.Length >= 3)
				{
					DataTable customersByIdentityPrefix = DataAccess.GetCustomersByIdentityPrefix(text2);
					if (customersByIdentityPrefix.Rows.Count == 1)
					{
						DataRow dataRow = customersByIdentityPrefix.Rows[0];
						txtAd.Text = dataRow["FirstName"]?.ToString() ?? "";
						txtSoyad.Text = dataRow["LastName"]?.ToString() ?? "";
						txtTel.Text = dataRow["Phone"]?.ToString() ?? "";
						txtMail.Text = dataRow["Email"]?.ToString() ?? "";
						string text3 = dataRow["Address"]?.ToString() ?? "";
						if (text3.Contains("/"))
						{
							string[] array2 = text3.Split('/');
							int num4 = cmbIl.FindStringExact(array2[0]);
							if (num4 != -1)
							{
								cmbIl.SelectedIndex = num4;
							}
							if (array2.Length > 1)
							{
								string s2 = array2[1].Split('-')[0].Trim();
								int num5 = cmbIlce.FindStringExact(s2);
								if (num5 != -1)
								{
									cmbIlce.SelectedIndex = num5;
								}
								if (array2[1].Contains("-"))
								{
									txtAdresDetay.Text = array2[1].Substring(array2[1].IndexOf('-') + 1).Trim();
								}
							}
						}
					}
				}
			};
		}
		button.Click += delegate
		{
			try
			{
				if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text))
				{
					MessageBox.Show("Ad ve Soyad zorunludur.");
				}
				else if (!IsValidTc(txtTC.Text))
				{
					MessageBox.Show("Geçersiz TC!");
				}
				else
				{
					string address = $"{cmbIl.SelectedItem}/{cmbIlce.SelectedItem} - {txtAdresDetay.Text}";
					if (editRow == null)
					{
						DataAccess.AddCustomer(txtAd.Text.Trim(), txtSoyad.Text.Trim(), txtTel.Text.Trim(), txtMail.Text.Trim(), "", 0, address, txtTC.Text.Trim());
						MessageBox.Show("Müşteri kaydedildi!", "Başarılı");
					}
					else
					{
						DataAccess.UpdateCustomer(txtTC.Text.Trim(), txtAd.Text.Trim(), txtSoyad.Text.Trim(), txtTel.Text.Trim(), txtMail.Text.Trim(), address);
						MessageBox.Show("Müşteri bilgileri güncellendi!", "Başarılı");
					}
					f.Close();
					ShowPage("Müşteriler");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		scrollPnl.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "",
			Location = new Point(0, curY),
			Size = new System.Drawing.Size(1, 120)
		});
		f.ShowDialog();
		TextBox Field(string label, string ph, ref int reference)
		{
			RoundedPanel p = new RoundedPanel
			{
				Width = f.Width - 80,
				Height = 75,
				Location = new Point(0, reference),
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 10
			};
			p.Padding = new Padding(12);
			p.Paint += delegate(object? obj, PaintEventArgs e)
			{
				ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
			};
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = label.ToUpper(),
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Location = new Point(12, 12),
				AutoSize = true
			};
			p.Controls.Add(value);
			TextBox textBox = new TextBox
			{
				Location = new Point(12, 35),
				Width = p.Width - 24,
				PlaceholderText = ph,
				BorderStyle = BorderStyle.None,
				Font = new Font("Segoe UI", 11f),
				BackColor = System.Drawing.Color.White
			};
			p.Controls.Add(textBox);
			scrollPnl.Controls.Add(p);
			reference += 85;
			return textBox;
		}
		void Section(string title, ref int reference)
		{
			scrollPnl.Controls.Add(new System.Windows.Forms.Label
			{
				Text = title.ToUpper(),
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(107, 114, 128),
				Location = new Point(0, reference),
				AutoSize = true
			});
			reference += 25;
		}
	}

	private void PageStorage(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.AutoScroll = true;
			ComboBox cmbUrunSec = new ComboBox();
			Dictionary<int, string> productIdMap = new Dictionary<int, string>();
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udce6 Depo ve Stok Yönetimi",
				Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(20, 20),
				AutoSize = true
			};
			body.Controls.Add(value);
			FlowLayoutPanel pnlNav = new FlowLayoutPanel
			{
				Location = new Point(20, 65),
				Size = new System.Drawing.Size(body.Width - 60, 48),
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(pnlNav);
			TabControl tabControl = new TabControl
			{
				Location = new Point(20, 120),
				Size = new System.Drawing.Size(body.Width - 60, body.Height - 140),
				Font = new Font("Segoe UI", 10f),
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			tabControl.Appearance = TabAppearance.FlatButtons;
			tabControl.ItemSize = new System.Drawing.Size(0, 1);
			tabControl.SizeMode = TabSizeMode.Fixed;
			body.Controls.Add(tabControl);
			TabPage tabPage = new TabPage
			{
				Text = "Ürün Tanımlama",
				BackColor = System.Drawing.Color.White
			};
			tabControl.TabPages.Add(tabPage);
			BuildNavTab("\ud83d\udccb Ürün Tanımlama", tabPage, isFirst: true);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(20, 20),
				Size = new System.Drawing.Size(420, 530),
				BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
				BorderRadius = 15
			};
			tabPage.Controls.Add(roundedPanel);
			int num = 20;
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "✨ Yeni Ürün Tanımla",
				Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(25, num),
				AutoSize = true
			});
			num += 40;
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Üretici / Marka:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(25, num),
				AutoSize = true
			});
			num += 22;
			ComboBox cmbUretici = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(300, 30),
				Location = new Point(25, num),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel.Controls.Add(cmbUretici);
			Button button = new Button
			{
				Text = "➕",
				Size = new System.Drawing.Size(42, 30),
				Location = new Point(333, num),
				BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button.FlatAppearance.BorderSize = 0;
			button.Click += delegate
			{
				string res = "";
				Form fInp = new Form
				{
					Text = "Üretici Ekle",
					Size = new System.Drawing.Size(300, 180),
					StartPosition = FormStartPosition.CenterParent,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					MaximizeBox = false
				};
				try
				{
					System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
					{
						Text = "Üretici/Marka Adı:",
						Location = new Point(20, 20),
						AutoSize = true
					};
					fInp.Controls.Add(value2);
					TextBox t = new TextBox
					{
						Location = new Point(20, 45),
						Size = new System.Drawing.Size(240, 25)
					};
					fInp.Controls.Add(t);
					Button button9 = new Button
					{
						Text = "Ekle",
						Location = new Point(100, 85),
						Size = new System.Drawing.Size(80, 30),
						BackColor = System.Drawing.Color.Green,
						ForeColor = System.Drawing.Color.White,
						FlatStyle = FlatStyle.Flat
					};
					button9.Click += delegate
					{
						res = t.Text.Trim();
						fInp.Close();
					};
					fInp.Controls.Add(button9);
					fInp.ShowDialog();
				}
				finally
				{
					if (fInp != null)
					{
						((IDisposable)fInp).Dispose();
					}
				}
				if (!string.IsNullOrWhiteSpace(res))
				{
					DataAccess.AddManufacturer(res);
					LoadUreticiler(cmbUretici);
					cmbUretici.SelectedItem = res;
				}
			};
			roundedPanel.Controls.Add(button);
			num += 42;
			LoadUreticiler(cmbUretici);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Ürün İsmi:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(25, num),
				AutoSize = true
			});
			num += 22;
			TextBox txtUrunIsim = new TextBox
			{
				Size = new System.Drawing.Size(350, 30),
				Location = new Point(25, num),
				Font = new Font("Segoe UI", 10f),
				BorderStyle = BorderStyle.FixedSingle,
				PlaceholderText = "Örn: Coca-Cola 330ml Kutu"
			};
			roundedPanel.Controls.Add(txtUrunIsim);
			num += 42;
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Barkod:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(25, num),
				AutoSize = true
			});
			num += 22;
			TextBox txtBarkod = new TextBox
			{
				Size = new System.Drawing.Size(350, 30),
				Location = new Point(25, num),
				Font = new Font("Segoe UI", 10f),
				BorderStyle = BorderStyle.FixedSingle,
				PlaceholderText = "Barkod numarası girin veya okutun"
			};
			roundedPanel.Controls.Add(txtBarkod);
			num += 42;
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Kategori:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(25, num),
				AutoSize = true
			});
			ComboBox cmbKategori = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(160, 30),
				Location = new Point(25, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items = cmbKategori.Items;
			object[] items2 = new string[5] { "İçecek", "Gıda", "Temizlik", "Kırtasiye", "Diğer" };
			items.AddRange(items2);
			cmbKategori.SelectedIndex = 0;
			roundedPanel.Controls.Add(cmbKategori);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Birim:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(200, num),
				AutoSize = true
			});
			ComboBox cmbBirim = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(160, 30),
				Location = new Point(200, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items3 = cmbBirim.Items;
			items2 = new string[5] { "Adet", "Litre", "Kg", "Koli", "Paket" };
			items3.AddRange(items2);
			cmbBirim.SelectedIndex = 0;
			roundedPanel.Controls.Add(cmbBirim);
			num += 55;
			Button button2 = new Button
			{
				Text = "✅ ÜRÜNÜ SİSTEME TANIMLA",
				Size = new System.Drawing.Size(350, 50),
				Location = new Point(25, num),
				BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button2.FlatAppearance.BorderSize = 0;
			roundedPanel.Controls.Add(button2);
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Location = new Point(460, 20),
				Size = new System.Drawing.Size(tabPage.Width - 490, tabPage.Height - 40),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			tabPage.Controls.Add(roundedPanel2);
			roundedPanel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcd6 Tanımlı Ürün Kataloğu",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 15),
				AutoSize = true
			});
			DataGridView dgvKatalog = new DataGridView
			{
				Location = new Point(10, 50),
				Size = new System.Drawing.Size(roundedPanel2.Width - 20, roundedPanel2.Height - 65),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 38
				},
				Font = new Font("Segoe UI", 9f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
				GridColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			roundedPanel2.Controls.Add(dgvKatalog);
			LoadKatalog();
			button2.Click += delegate
			{
				if (string.IsNullOrWhiteSpace(txtUrunIsim.Text) || string.IsNullOrWhiteSpace(txtBarkod.Text))
				{
					MessageBox.Show("Ürün ismi ve barkod zorunludur!");
					return;
				}
				try
				{
					string barcode = txtBarkod.Text.Trim();
					if (DataAccess.GetProductByBarcode(barcode) != null)
					{
						MessageBox.Show("Bu barkod zaten kayıtlı!");
					}
					else
					{
						int productId = 0;
						using (MySqlConnection mySqlConnection = DatabaseHelper.GetConnection())
						{
							mySqlConnection.Open();
							using MySqlCommand mySqlCommand = new MySqlCommand("SELECT IFNULL(MAX(ProductID), 0) + 1 FROM PRODUCTS", mySqlConnection);
							productId = Convert.ToInt32(mySqlCommand.ExecuteScalar());
						}
						DataAccess.RegisterProduct(productId, barcode, txtUrunIsim.Text.Trim(), cmbKategori.SelectedItem?.ToString() ?? "Diğer", cmbUretici.SelectedItem?.ToString() ?? "Diğer", cmbBirim.SelectedItem?.ToString() ?? "Adet", 0m, 0m);
						txtUrunIsim.Text = "";
						txtBarkod.Text = "";
						LoadKatalog();
						LoadProductCombo();
						MessageBox.Show("Ürün başarıyla tanımlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Hata: " + ex.Message);
				}
			};
			TabPage tabPage2 = new TabPage
			{
				Text = "Mal Kabul",
				BackColor = System.Drawing.Color.White
			};
			tabControl.TabPages.Add(tabPage2);
			BuildNavTab("\ud83d\udce5 Mal Kabul (Giriş)", tabPage2);
			tabPage2.AutoScroll = true;
			RoundedPanel roundedPanel3 = new RoundedPanel
			{
				Location = new Point(10, 10),
				Size = new System.Drawing.Size(tabPage2.Width - 25, 420),
				BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			tabPage2.Controls.Add(roundedPanel3);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udce5 Mal Kabul — Stok Girişi",
				Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(20, 12),
				AutoSize = true
			});
			num = 50;
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Ürün Bul (İsim/Barkod):",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Location = new Point(20, num),
				AutoSize = true
			});
			num += 20;
			TextBox txtUrunFiltre = new TextBox
			{
				Size = new System.Drawing.Size(300, 28),
				Location = new Point(20, num),
				Font = new Font("Segoe UI", 9f),
				PlaceholderText = "Aramak için yazın..."
			};
			roundedPanel3.Controls.Add(txtUrunFiltre);
			num += 35;
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Seçili Ürün:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(20, num),
				AutoSize = true
			});
			num += 20;
			cmbUrunSec.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbUrunSec.Size = new System.Drawing.Size(500, 30);
			cmbUrunSec.Location = new Point(20, num);
			cmbUrunSec.Font = new Font("Segoe UI", 10f);
			roundedPanel3.Controls.Add(cmbUrunSec);
			num += 42;
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Kaç Adet Geldi:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(20, num),
				AutoSize = true
			});
			NumericUpDown numStokAdet = new NumericUpDown
			{
				Minimum = 1m,
				Maximum = 999999m,
				Value = 1m,
				Size = new System.Drawing.Size(100, 30),
				Location = new Point(20, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel3.Controls.Add(numStokAdet);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Alış Fiyatı (₺):",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(135, num),
				AutoSize = true
			});
			NumericUpDown numAlisFiyat = new NumericUpDown
			{
				Minimum = 0m,
				Maximum = 1000000m,
				DecimalPlaces = 2,
				Value = 0m,
				Size = new System.Drawing.Size(100, 30),
				Location = new Point(135, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel3.Controls.Add(numAlisFiyat);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Satış Fiyatı (₺):",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(250, num),
				AutoSize = true
			});
			NumericUpDown numSatisFiyat = new NumericUpDown
			{
				Minimum = 0m,
				Maximum = 1000000m,
				DecimalPlaces = 2,
				Value = 0m,
				Size = new System.Drawing.Size(100, 30),
				Location = new Point(250, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel3.Controls.Add(numSatisFiyat);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Geliş Tarihi:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(365, num),
				AutoSize = true
			});
			DateTimePicker dtpStokGelis = new DateTimePicker
			{
				Format = DateTimePickerFormat.Custom,
				CustomFormat = "dd/MM/yyyy HH:mm",
				Size = new System.Drawing.Size(160, 30),
				Location = new Point(365, num + 22),
				Font = new Font("Segoe UI", 10f),
				Value = DateTime.Now
			};
			roundedPanel3.Controls.Add(dtpStokGelis);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Nereye:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(540, num),
				AutoSize = true
			});
			ComboBox cmbHedef = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(120, 30),
				Location = new Point(540, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items4 = cmbHedef.Items;
			items2 = new string[2] { "DEPO", "LOKANTA" };
			items4.AddRange(items2);
			cmbHedef.SelectedIndex = 0;
			roundedPanel3.Controls.Add(cmbHedef);
			num += 65;
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Tedarikçi:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(20, num),
				AutoSize = true
			});
			ComboBox cmbTedarikci = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDown,
				Size = new System.Drawing.Size(200, 30),
				Location = new Point(20, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel3.Controls.Add(cmbTedarikci);
			Button button3 = new Button
			{
				Text = "+",
				Size = new System.Drawing.Size(30, 30),
				Location = new Point(225, num + 22),
				BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button3.FlatAppearance.BorderSize = 0;
			button3.Click += delegate
			{
				string res = "";
				Form fInp = new Form
				{
					Text = "Tedarikçi Ekle",
					Size = new System.Drawing.Size(300, 180),
					StartPosition = FormStartPosition.CenterParent,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					MaximizeBox = false
				};
				try
				{
					fInp.Controls.Add(new System.Windows.Forms.Label
					{
						Text = "Tedarikçi Adı:",
						Location = new Point(20, 20),
						AutoSize = true
					});
					TextBox t = new TextBox
					{
						Location = new Point(20, 45),
						Size = new System.Drawing.Size(240, 25)
					};
					fInp.Controls.Add(t);
					Button button9 = new Button
					{
						Text = "Ekle",
						Location = new Point(100, 85),
						Size = new System.Drawing.Size(80, 30),
						BackColor = System.Drawing.Color.Green,
						ForeColor = System.Drawing.Color.White,
						FlatStyle = FlatStyle.Flat
					};
					button9.Click += delegate
					{
						res = t.Text.Trim();
						fInp.Close();
					};
					fInp.Controls.Add(button9);
					fInp.ShowDialog();
				}
				finally
				{
					if (fInp != null)
					{
						((IDisposable)fInp).Dispose();
					}
				}
				if (!string.IsNullOrWhiteSpace(res))
				{
					DataAccess.AddSupplier(res);
					LoadTedarikciCombo();
					cmbTedarikci.SelectedItem = res;
				}
			};
			roundedPanel3.Controls.Add(button3);
			LoadTedarikciCombo();
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Mal Kabul Yapan Çalışan:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(270, num),
				AutoSize = true
			});
			ComboBox cmbPersonel = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(200, 30),
				Location = new Point(270, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			try
			{
				foreach (DataRow row in DataAccess.GetActiveEmployees().Rows)
				{
					cmbPersonel.Items.Add($"{row["FirstName"]} {row["LastName"]}");
				}
				if (cmbPersonel.Items.Count > 0)
				{
					cmbPersonel.SelectedIndex = 0;
				}
			}
			catch
			{
			}
			roundedPanel3.Controls.Add(cmbPersonel);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Fatura/İrsaliye No:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(485, num),
				AutoSize = true
			});
			TextBox txtFaturaNo = new TextBox
			{
				Size = new System.Drawing.Size(170, 30),
				Location = new Point(485, num + 22),
				Font = new Font("Segoe UI", 10f),
				PlaceholderText = "Opsiyonel"
			};
			roundedPanel3.Controls.Add(txtFaturaNo);
			num += 65;
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Ödeme Şekli:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(20, num),
				AutoSize = true
			});
			ComboBox cmbOdeme = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new System.Drawing.Size(140, 30),
				Location = new Point(20, num + 22),
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items5 = cmbOdeme.Items;
			items2 = new string[5] { "Nakit", "Kredi Kartı", "Havale/EFT", "Veresiye", "Çek" };
			items5.AddRange(items2);
			cmbOdeme.SelectedIndex = 0;
			roundedPanel3.Controls.Add(cmbOdeme);
			System.Windows.Forms.Label lblTotalPurchase = new System.Windows.Forms.Label
			{
				Text = "TOPLAM: 0,00 ₺",
				Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(220, 38, 38),
				Location = new Point(175, num + 21),
				AutoSize = true
			};
			roundedPanel3.Controls.Add(lblTotalPurchase);
			roundedPanel3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Notlar:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(55, 65, 81),
				Location = new Point(350, num),
				AutoSize = true
			});
			TextBox txtNotlar = new TextBox
			{
				Size = new System.Drawing.Size(200, 30),
				Location = new Point(350, num + 22),
				Font = new Font("Segoe UI", 10f),
				PlaceholderText = "Not giriniz..."
			};
			roundedPanel3.Controls.Add(txtNotlar);
			Button button4 = new Button
			{
				Text = "\ud83d\udce5 STOĞA EKLE",
				Size = new System.Drawing.Size(160, 52),
				Location = new Point(570, num + 2),
				BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button4.FlatAppearance.BorderSize = 0;
			roundedPanel3.Controls.Add(button4);
			Action updatePurchaseTotal = delegate
			{
				decimal value2 = numStokAdet.Value * numAlisFiyat.Value;
				lblTotalPurchase.Text = $"TOPLAM: {value2:N2} ₺";
			};
			numStokAdet.ValueChanged += delegate
			{
				updatePurchaseTotal();
			};
			numAlisFiyat.ValueChanged += delegate
			{
				updatePurchaseTotal();
			};
			updatePurchaseTotal();
			LoadProductCombo();
			txtUrunFiltre.TextChanged += delegate
			{
				LoadProductCombo(txtUrunFiltre.Text.Trim());
			};
			cmbUrunSec.SelectedIndexChanged += delegate
			{
				if (cmbUrunSec.SelectedIndex >= 0)
				{
					try
					{
						string text = cmbUrunSec.SelectedItem?.ToString();
						if (text != null)
						{
							string s2 = text.Split(']')[0].Replace("[", "").Trim();
							DataRow productByID = DataAccess.GetProductByID(int.Parse(s2));
							if (productByID != null && productByID["SuggestedSalePrice"] != DBNull.Value)
							{
								decimal num2 = Convert.ToDecimal(productByID["SuggestedSalePrice"]);
								if (num2 > 0m)
								{
									numSatisFiyat.Value = num2;
								}
							}
						}
					}
					catch
					{
					}
				}
			};
			TabPage tabPage3 = new TabPage
			{
				Text = "Stok Hareketleri",
				BackColor = System.Drawing.Color.White
			};
			tabControl.TabPages.Add(tabPage3);
			BuildNavTab("\ud83d\udcca Hareket Listesi", tabPage3);
			tabPage3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = $"\ud83d\udcc5 Bugünkü Girişler ({DateTime.Today:dd.MM.yyyy})",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 10),
				AutoSize = true
			});
			System.Windows.Forms.Label lblBugunkuToplam = new System.Windows.Forms.Label
			{
				Text = "Toplam: 0 adet | Maliyet: 0,00 ₺",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(79, 70, 229),
				Location = new Point(15, 35),
				AutoSize = true
			};
			tabPage3.Controls.Add(lblBugunkuToplam);
			DataGridView dgvBugun = new DataGridView
			{
				Location = new Point(10, 60),
				Size = new System.Drawing.Size(tabPage3.Width - 25, 200),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 35
				},
				Font = new Font("Segoe UI", 9f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgvBugun.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(219, 234, 254);
			dgvBugun.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			dgvBugun.ColumnHeadersHeight = 38;
			tabPage3.Controls.Add(dgvBugun);
			tabPage3.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcdc Geçmiş Stok Hareketleri",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 270),
				AutoSize = true
			});
			Panel panel = new Panel
			{
				Location = new Point(10, 295),
				Size = new System.Drawing.Size(tabPage3.Width - 25, 40),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249),
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			tabPage3.Controls.Add(panel);
			panel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Başlangıç:",
				Font = new Font("Segoe UI", 8f),
				Location = new Point(5, 10),
				AutoSize = true
			});
			DateTimePicker dtpStart = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Size = new System.Drawing.Size(110, 25),
				Location = new Point(70, 7),
				Font = new Font("Segoe UI", 8f),
				Value = DateTime.Today.AddMonths(-1)
			};
			panel.Controls.Add(dtpStart);
			panel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Bitiş:",
				Font = new Font("Segoe UI", 8f),
				Location = new Point(190, 10),
				AutoSize = true
			});
			DateTimePicker dtpEnd = new DateTimePicker
			{
				Format = DateTimePickerFormat.Short,
				Size = new System.Drawing.Size(110, 25),
				Location = new Point(225, 7),
				Font = new Font("Segoe UI", 8f),
				Value = DateTime.Today
			};
			panel.Controls.Add(dtpEnd);
			TextBox txtFilterUrun2 = new TextBox
			{
				PlaceholderText = "Ürün ara...",
				Size = new System.Drawing.Size(120, 25),
				Location = new Point(345, 7),
				Font = new Font("Segoe UI", 8f)
			};
			panel.Controls.Add(txtFilterUrun2);
			TextBox txtFilterTedarikci2 = new TextBox
			{
				PlaceholderText = "Tedarikçi...",
				Size = new System.Drawing.Size(120, 25),
				Location = new Point(475, 7),
				Font = new Font("Segoe UI", 8f)
			};
			panel.Controls.Add(txtFilterTedarikci2);
			TextBox txtFilterCalisan2 = new TextBox
			{
				PlaceholderText = "Çalışan...",
				Size = new System.Drawing.Size(100, 25),
				Location = new Point(605, 7),
				Font = new Font("Segoe UI", 8f)
			};
			panel.Controls.Add(txtFilterCalisan2);
			Button button5 = new Button
			{
				Text = "\ud83d\udd0d Filtrele",
				Size = new System.Drawing.Size(85, 28),
				Location = new Point(715, 6),
				BackColor = System.Drawing.Color.FromArgb(59, 130, 246),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button5.FlatAppearance.BorderSize = 0;
			panel.Controls.Add(button5);
			DataGridView dgvGecmis = new DataGridView
			{
				Location = new Point(10, 340),
				Size = new System.Drawing.Size(tabPage3.Width - 25, tabPage3.Height - 355),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 35
				},
				Font = new Font("Segoe UI", 9f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgvGecmis.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgvGecmis.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			dgvGecmis.ColumnHeadersHeight = 38;
			tabPage3.Controls.Add(dgvGecmis);
			button5.Click += delegate
			{
				LoadHistoryGrid();
			};
			LoadTodayGrid();
			LoadHistoryGrid();
			TabPage tabPage4 = new TabPage
			{
				Text = "Tedarikçi Yönetimi",
				BackColor = System.Drawing.Color.White
			};
			tabControl.TabPages.Add(tabPage4);
			BuildNavTab("\ud83c\udfed Üretici / Tedarikçi", tabPage4);
			RoundedPanel roundedPanel4 = new RoundedPanel
			{
				Location = new Point(15, 15),
				Size = new System.Drawing.Size(tabPage4.Width / 2 - 25, tabPage4.Height - 30),
				BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left)
			};
			tabPage4.Controls.Add(roundedPanel4);
			roundedPanel4.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83c\udfed Üretici / Marka Yönetimi",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 15),
				AutoSize = true
			});
			TextBox txtYeniUretici = new TextBox
			{
				PlaceholderText = "Yeni üretici adı...",
				Size = new System.Drawing.Size(200, 30),
				Location = new Point(15, 55),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel4.Controls.Add(txtYeniUretici);
			Button button6 = new Button
			{
				Text = "➕ Ekle",
				Size = new System.Drawing.Size(80, 30),
				Location = new Point(225, 55),
				BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Cursor = Cursors.Hand
			};
			button6.FlatAppearance.BorderSize = 0;
			roundedPanel4.Controls.Add(button6);
			ListBox lstUreticiler = new ListBox
			{
				Location = new Point(15, 100),
				Size = new System.Drawing.Size(290, roundedPanel4.Height - 115),
				Font = new Font("Segoe UI", 10f),
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			roundedPanel4.Controls.Add(lstUreticiler);
			LoadUreticiList();
			button6.Click += delegate
			{
				if (!string.IsNullOrWhiteSpace(txtYeniUretici.Text))
				{
					DataAccess.AddManufacturer(txtYeniUretici.Text.Trim());
					txtYeniUretici.Text = "";
					LoadUreticiList();
					LoadUreticiler(cmbUretici);
				}
			};
			RoundedPanel roundedPanel5 = new RoundedPanel
			{
				Location = new Point(tabPage4.Width / 2 + 5, 15),
				Size = new System.Drawing.Size(tabPage4.Width / 2 - 25, tabPage4.Height - 30),
				BackColor = System.Drawing.Color.FromArgb(249, 250, 251),
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			tabPage4.Controls.Add(roundedPanel5);
			roundedPanel5.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\ude9a Tedarikçi Yönetimi",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 15),
				AutoSize = true
			});
			TextBox txtYeniTedarikci = new TextBox
			{
				PlaceholderText = "Yeni tedarikçi adı...",
				Size = new System.Drawing.Size(200, 30),
				Location = new Point(15, 55),
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel5.Controls.Add(txtYeniTedarikci);
			Button button7 = new Button
			{
				Text = "➕ Ekle",
				Size = new System.Drawing.Size(80, 30),
				Location = new Point(225, 55),
				BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Cursor = Cursors.Hand
			};
			button7.FlatAppearance.BorderSize = 0;
			roundedPanel5.Controls.Add(button7);
			ListBox lstTedarikci = new ListBox
			{
				Location = new Point(15, 100),
				Size = new System.Drawing.Size(290, roundedPanel5.Height - 110),
				Font = new Font("Segoe UI", 10f),
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			roundedPanel5.Controls.Add(lstTedarikci);
			LoadTedarikciList();
			button7.Click += delegate
			{
				if (!string.IsNullOrWhiteSpace(txtYeniTedarikci.Text))
				{
					DataAccess.AddSupplier(txtYeniTedarikci.Text.Trim());
					txtYeniTedarikci.Text = "";
					LoadTedarikciList();
					LoadTedarikciCombo();
				}
			};
			TabPage tabPage5 = new TabPage
			{
				Text = "Mevcut Stoklar",
				BackColor = System.Drawing.Color.White
			};
			tabControl.TabPages.Add(tabPage5);
			BuildNavTab("\ud83c\udfe2 Tüm Envanter", tabPage5);
			tabPage5.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udce6 Depo ve Lokanta Güncel Stok Durumu",
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(20, 15),
				AutoSize = true
			});
			TextBox txtCurrentSearch = new TextBox
			{
				PlaceholderText = "\ud83d\udd0d Ürün Ara...",
				Size = new System.Drawing.Size(300, 32),
				Location = new Point(20, 50),
				Font = new Font("Segoe UI", 10f)
			};
			tabPage5.Controls.Add(txtCurrentSearch);
			System.Windows.Forms.Label lblStokOzet = new System.Windows.Forms.Label
			{
				Text = "",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(79, 70, 229),
				Location = new Point(340, 55),
				AutoSize = true
			};
			tabPage5.Controls.Add(lblStokOzet);
			DataGridView dgvCurrentStatus = new DataGridView
			{
				Location = new Point(20, 90),
				Size = new System.Drawing.Size(tabPage5.Width - 45, tabPage5.Height - 110),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 40
				},
				Font = new Font("Segoe UI", 10f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgvCurrentStatus.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgvCurrentStatus.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
			dgvCurrentStatus.ColumnHeadersHeight = 42;
			tabPage5.Controls.Add(dgvCurrentStatus);
			DataTable dtCurrentStock = new DataTable();
			txtCurrentSearch.TextChanged += delegate
			{
				string value2 = txtCurrentSearch.Text.Trim().Replace("'", "''");
				if (dtCurrentStock?.DefaultView != null)
				{
					dtCurrentStock.DefaultView.RowFilter = (string.IsNullOrEmpty(value2) ? "" : $"[Ürün Adı] LIKE '%{value2}%' OR [Barkod] LIKE '%{value2}%'");
				}
			};
			LoadCurrentStock();
			button4.Click += delegate
			{
				try
				{
					if (cmbUrunSec.SelectedIndex < 0)
					{
						MessageBox.Show("Lütfen bir ürün seçin!");
					}
					else
					{
						string s2 = cmbUrunSec.SelectedItem.ToString().Split(']')[0].Replace("[", "").Trim();
						int productId = int.Parse(s2);
						DataRow productByID = DataAccess.GetProductByID(productId);
						if (productByID != null)
						{
							string text = cmbHedef.SelectedItem?.ToString() ?? "DEPO";
							DataAccess.AddOrUpdateStorageItemByID(productId, productByID["Barcode"].ToString(), productByID["ItemName"].ToString(), productByID["ManufacturerName"].ToString(), productByID["Category"]?.ToString() ?? "Diğer", productByID["Unit"]?.ToString() ?? "Adet", numSatisFiyat.Value, numSatisFiyat.Value, text, (int)numStokAdet.Value, dtpStokGelis.Value, numAlisFiyat.Value, cmbPersonel.SelectedItem?.ToString() ?? "", cmbTedarikci.Text.Trim(), txtFaturaNo.Text.Trim(), cmbOdeme.SelectedItem?.ToString() ?? "Nakit");
							MessageBox.Show($"✅ Stok girişi başarıyla yapıldı!\nÜrün: {productByID["ItemName"]}\nAdet: {numStokAdet.Value}\nHedef: {text}", "Mal Kabul Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
							numStokAdet.Value = 1m;
							numAlisFiyat.Value = 0m;
							numSatisFiyat.Value = 0m;
							if (cmbPersonel.Items.Count > 0)
							{
								cmbPersonel.SelectedIndex = 0;
							}
							txtFaturaNo.Text = "";
							txtNotlar.Text = "";
							LoadTodayGrid();
							LoadHistoryGrid();
							LoadCurrentStock();
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Hata: " + ex.Message);
				}
			};
			if (AuthHelper.CurrentUser?.Role == "Admin")
			{
				Button button8 = new Button
				{
					Text = "⚠\ufe0f SIFIRLA",
					Size = new System.Drawing.Size(100, 30),
					Location = new Point(tabControl.Width - 120, 20),
					BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Anchor = (AnchorStyles.Top | AnchorStyles.Right),
					Cursor = Cursors.Hand
				};
				button8.Click += delegate
				{
					if (MessageBox.Show("Tüm stok verileri sıfırlanacak. Emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
						DataAccess.TruncateStorage();
						ShowPage("Depo");
					}
				};
				body.Controls.Add(button8);
			}
			void BuildNavTab(string title, TabPage targetTab, bool isFirst = false)
			{
				Button btn = new Button
				{
					Text = title,
					Size = new System.Drawing.Size(200, 40),
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand,
					Margin = new Padding(0, 0, 10, 0)
				};
				btn.FlatAppearance.BorderSize = 0;
				btn.BackColor = (isFirst ? System.Drawing.Color.FromArgb(99, 102, 241) : System.Drawing.Color.White);
				btn.ForeColor = (isFirst ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(71, 85, 105));
				btn.Click += delegate
				{
					foreach (Control control in pnlNav.Controls)
					{
						if (control is Button button9)
						{
							button9.BackColor = System.Drawing.Color.White;
							button9.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
						}
					}
					btn.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
					btn.ForeColor = System.Drawing.Color.White;
					tabControl.SelectedTab = targetTab;
				};
				pnlNav.Controls.Add(btn);
			}
			void DgvStockFormat(object? s2, DataGridViewCellFormattingEventArgs e2)
			{
				if (e2.RowIndex >= 0 && e2.ColumnIndex >= 0)
				{
					string name = dgvCurrentStatus.Columns[e2.ColumnIndex].Name;
					if (name == "Depo Stok")
					{
						int result = 0;
						int.TryParse(e2.Value?.ToString(), out result);
						e2.CellStyle.ForeColor = ((result <= 2) ? System.Drawing.Color.Red : System.Drawing.Color.FromArgb(59, 130, 246));
						e2.CellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
						if (result <= 2 && result >= 0)
						{
							e2.CellStyle.BackColor = System.Drawing.Color.FromArgb(254, 226, 226);
						}
					}
					if (name == "Lokanta Stok")
					{
						int result2 = 0;
						int.TryParse(e2.Value?.ToString(), out result2);
						e2.CellStyle.ForeColor = ((result2 <= 2) ? System.Drawing.Color.Red : System.Drawing.Color.FromArgb(239, 68, 68));
						e2.CellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
						if (result2 <= 2 && result2 >= 0)
						{
							e2.CellStyle.BackColor = System.Drawing.Color.FromArgb(254, 226, 226);
						}
					}
				}
			}
			void LoadCurrentStock()
			{
				try
				{
					dtCurrentStock = DataAccess.GetCombinedStockStatus();
					dgvCurrentStatus.DataSource = null;
					dgvCurrentStatus.DataSource = dtCurrentStock;
					if (dgvCurrentStatus.Columns.Contains("ProductID"))
					{
						dgvCurrentStatus.Columns["ProductID"].Visible = false;
					}
					int count = dtCurrentStock.Rows.Count;
					int num2 = 0;
					int num3 = 0;
					foreach (DataRow row2 in dtCurrentStock.Rows)
					{
						num2 += Convert.ToInt32(row2["Depo Stok"]);
						num3 += Convert.ToInt32(row2["Lokanta Stok"]);
					}
					lblStokOzet.Text = $"Toplam {count} çeşit | Depo: {num2} adet | Lokanta: {num3} adet";
					dgvCurrentStatus.CellFormatting -= DgvStockFormat;
					dgvCurrentStatus.CellFormatting += DgvStockFormat;
				}
				catch
				{
				}
			}
			void LoadHistoryGrid()
			{
				try
				{
					DataTable storageEntryLogFiltered = DataAccess.GetStorageEntryLogFiltered(dtpStart.Value, dtpEnd.Value, txtFilterUrun2.Text.Trim(), txtFilterTedarikci2.Text.Trim(), txtFilterCalisan2.Text.Trim());
					dgvGecmis.DataSource = storageEntryLogFiltered;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Geçmiş yüklenemedi: " + ex.Message);
				}
			}
			void LoadKatalog()
			{
				try
				{
					DataTable allProducts = DataAccess.GetAllProducts();
					if (allProducts.Columns.Contains("ProductID"))
					{
						allProducts.Columns["ProductID"].ColumnName = "ID";
					}
					if (allProducts.Columns.Contains("Barcode"))
					{
						allProducts.Columns["Barcode"].ColumnName = "Barkod";
					}
					if (allProducts.Columns.Contains("ItemName"))
					{
						allProducts.Columns["ItemName"].ColumnName = "Ürün İsmi";
					}
					if (allProducts.Columns.Contains("ManufacturerName"))
					{
						allProducts.Columns["ManufacturerName"].ColumnName = "Üretici";
					}
					if (allProducts.Columns.Contains("Category"))
					{
						allProducts.Columns["Category"].ColumnName = "Kategori";
					}
					if (allProducts.Columns.Contains("Unit"))
					{
						allProducts.Columns["Unit"].ColumnName = "Birim";
					}
					if (allProducts.Columns.Contains("SuggestedSalePrice"))
					{
						allProducts.Columns["SuggestedSalePrice"].ColumnName = "Önerilen Fiyat";
					}
					dgvKatalog.DataSource = allProducts;
					if (dgvKatalog.Columns.Contains("Price"))
					{
						dgvKatalog.Columns["Price"].Visible = false;
					}
					if (dgvKatalog.Columns.Contains("Önerilen Fiyat"))
					{
						dgvKatalog.Columns["Önerilen Fiyat"].Visible = false;
					}
				}
				catch
				{
				}
			}
			void LoadProductCombo(string filter = "")
			{
				cmbUrunSec.Items.Clear();
				productIdMap.Clear();
				try
				{
					DataTable allProducts = DataAccess.GetAllProducts();
					int num2 = 0;
					foreach (DataRow row3 in allProducts.Rows)
					{
						int value2 = Convert.ToInt32(row3["ProductID"]);
						string text = row3["Barcode"]?.ToString() ?? "";
						string text2 = row3["ItemName"]?.ToString() ?? "";
						string value3 = row3["ManufacturerName"]?.ToString() ?? "";
						string text3 = $"[{value2}] {value3} - {text2} ({text})";
						if (string.IsNullOrEmpty(filter) || (text2 != null && text2.Contains(filter, StringComparison.OrdinalIgnoreCase)) || (text != null && text.Contains(filter, StringComparison.OrdinalIgnoreCase)))
						{
							cmbUrunSec.Items.Add(text3);
							productIdMap[num2++] = text3;
						}
					}
					if (cmbUrunSec.Items.Count > 0)
					{
						cmbUrunSec.SelectedIndex = 0;
					}
				}
				catch
				{
				}
			}
			void LoadTedarikciCombo()
			{
				cmbTedarikci.Items.Clear();
				try
				{
					DataTable allSuppliers = DataAccess.GetAllSuppliers();
					foreach (DataRow row4 in allSuppliers.Rows)
					{
						string text = row4["Name"]?.ToString();
						if (text != null)
						{
							cmbTedarikci.Items.Add(text);
						}
					}
				}
				catch
				{
				}
			}
			void LoadTedarikciList()
			{
				lstTedarikci.Items.Clear();
				try
				{
					DataTable allSuppliers = DataAccess.GetAllSuppliers();
					foreach (DataRow row5 in allSuppliers.Rows)
					{
						lstTedarikci.Items.Add(row5["Name"].ToString());
					}
				}
				catch
				{
				}
			}
			void LoadTodayGrid()
			{
				try
				{
					DataTable todayStorageLog = DataAccess.GetTodayStorageLog();
					dgvBugun.DataSource = todayStorageLog;
					int num2 = 0;
					decimal value2 = default(decimal);
					foreach (DataRow row6 in todayStorageLog.Rows)
					{
						num2 += Convert.ToInt32(row6["Adet"]);
						value2 += Convert.ToDecimal(row6["Alış Fiyatı"]) * (decimal)Convert.ToInt32(row6["Adet"]);
					}
					lblBugunkuToplam.Text = $"Bugün Toplam: {num2} adet | Maliyet: {value2:N2} ₺";
				}
				catch (Exception ex)
				{
					MessageBox.Show("Bugünkü girişler yüklenemedi: " + ex.Message);
				}
			}
			void LoadUreticiList()
			{
				lstUreticiler.Items.Clear();
				try
				{
					DataTable allManufacturers = DataAccess.GetAllManufacturers();
					foreach (DataRow row7 in allManufacturers.Rows)
					{
						string text = row7["Name"]?.ToString();
						if (text != null)
						{
							lstUreticiler.Items.Add(text);
						}
					}
				}
				catch
				{
				}
			}
		});
		static void LoadUreticiler(ComboBox combo)
		{
			combo.Items.Clear();
			DataTable allManufacturers = DataAccess.GetAllManufacturers();
			foreach (DataRow row8 in allManufacturers.Rows)
			{
				combo.Items.Add(row8["Name"].ToString());
			}
			if (combo.Items.Count > 0)
			{
				combo.SelectedIndex = 0;
			}
		}
	}

	private DataGridView CreateStyledDGV(DataTable dt, Point loc, System.Drawing.Size size)
	{
		DataGridView dataGridView = new DataGridView
		{
			DataSource = dt,
			Location = loc,
			Size = size,
			BackColor = System.Drawing.Color.White,
			BorderStyle = BorderStyle.None,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			RowHeadersVisible = false,
			AllowUserToAddRows = false,
			ReadOnly = true,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			RowTemplate = 
			{
				Height = 40
			},
			Font = new Font("Segoe UI", 9f),
			EnableHeadersVisualStyles = false,
			BackgroundImage = null,
			GridColor = System.Drawing.Color.FromArgb(240, 240, 240)
		};
		dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
		dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		dataGridView.ColumnHeadersHeight = 40;
		return dataGridView;
	}

	private void ShowDefineProductForm()
	{
		Form f = new Form
		{
			Text = "✨ Yeni Ürün Tanımla",
			Size = new System.Drawing.Size(420, 420),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false
		};
		int num = 25;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udce6 Yeni Ürün Kaydı",
			Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 55;
		TextBox txtID = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Ürün ID (Manuel)", txtID, ref num));
		TextBox txtName = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Ürün İsmi (Katalog Adı)", txtName, ref num));
		TextBox txtMan = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Üretici / Marka", txtMan, ref num));
		Button button = new Button
		{
			Text = "ÜRÜNÜ KATALOĞA EKLE",
			Location = new Point(30, num + 10),
			Size = new System.Drawing.Size(340, 50),
			BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			if (!int.TryParse(txtID.Text, out var result))
			{
				MessageBox.Show("ID sayısal olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(txtName.Text))
				{
					try
					{
						string barcode = "PRD-" + result;
						DataAccess.RegisterProduct(result, barcode, txtName.Text.Trim(), "Diğer", txtMan.Text.Trim(), "Adet", 0m, 0m);
						MessageBox.Show("Ürün kataloğa başarıyla eklendi! Artık 'Mal Kabul' adımından stok girişini yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						f.Close();
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
				}
				MessageBox.Show("Ürün ismi boş bırakılamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		};
		f.Controls.Add(button);
		f.ShowDialog();
		static Panel CreateField(string label, Control input, ref int curY)
		{
			Panel panel = new Panel
			{
				Location = new Point(30, curY),
				Size = new System.Drawing.Size(340, 65)
			};
			panel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = label,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(75, 85, 99),
				Location = new Point(0, 0),
				AutoSize = true
			});
			input.Location = new Point(0, 22);
			input.Size = new System.Drawing.Size(340, 30);
			panel.Controls.Add(input);
			curY += 75;
			return panel;
		}
	}

	private void ShowAddStorageForm(DataGridViewRow? existingRow = null)
	{
		Form f = new Form
		{
			Text = "\ud83d\udce5 Mal Kabul (Hızlı Stok Girişi)",
			Size = new System.Drawing.Size(420, 750),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false
		};
		int num = 25;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83d\udce5 Depoya Stok Girişi",
			Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 55;
		TextBox txtID = new TextBox
		{
			Name = "txtID",
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f),
			PlaceholderText = "ID yazın..."
		};
		Button button = new Button
		{
			Text = "\ud83d\udd0d KATALOGDAN SEÇ",
			Location = new Point(245, 23),
			Size = new System.Drawing.Size(125, 28),
			BackColor = accentBlue,
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		Panel panel = CreateField("Ürün ID (Katalog)", txtID, ref num);
		panel.Controls.Add(button);
		TextBox txtBar = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f),
			PlaceholderText = "Veya Barkod okutun..."
		};
		f.Controls.Add(CreateField("Ürün Barkodu", txtBar, ref num));
		TextBox txtName = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Ürün İsmi", txtName, ref num, isReadOnly: true));
		TextBox txtMan = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Üretici / Marka", txtMan, ref num, isReadOnly: true));
		NumericUpDown numQty = new NumericUpDown
		{
			Minimum = 1m,
			Maximum = 99999m,
			Value = 1m,
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(CreateField("Eklenecek Yeni Adet", numQty, ref num));
		TextBox txtLoc = new TextBox
		{
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10f),
			PlaceholderText = "Örn: Raf-B2"
		};
		f.Controls.Add(CreateField("Depo Konumu", txtLoc, ref num));
		DateTimePicker dtpDate = new DateTimePicker
		{
			Format = DateTimePickerFormat.Short,
			Font = new Font("Segoe UI", 10f),
			Value = DateTime.Now
		};
		f.Controls.Add(CreateField("Geliş Tarihi", dtpDate, ref num));
		if (existingRow != null)
		{
			txtID.Text = existingRow.Cells["ProductID"].Value.ToString();
			txtBar.Text = existingRow.Cells["Barcode"].Value?.ToString() ?? "";
			txtName.Text = existingRow.Cells["ItemName"].Value?.ToString() ?? "";
			txtMan.Text = existingRow.Cells["ManufacturerName"].Value?.ToString() ?? "";
			txtLoc.Text = existingRow.Cells["Location"].Value?.ToString() ?? "";
			numQty.Focus();
		}
		txtID.TextChanged += delegate
		{
			if (int.TryParse(txtID.Text, out var result))
			{
				DataRow productByID = DataAccess.GetProductByID(result);
				if (productByID != null && txtBar.Text != productByID["Barcode"].ToString())
				{
					SetProduct(productByID);
				}
			}
		};
		txtBar.TextChanged += delegate
		{
			DataRow productByBarcode = DataAccess.GetProductByBarcode(txtBar.Text.Trim());
			if (productByBarcode != null && txtID.Text != productByBarcode["ProductID"].ToString())
			{
				SetProduct(productByBarcode);
			}
		};
		button.Click += delegate
		{
			int num2 = ShowProductSelectionDialog();
			if (num2 > 0)
			{
				DataRow productByID = DataAccess.GetProductByID(num2);
				if (productByID != null)
				{
					SetProduct(productByID);
				}
			}
		};
		Button button2 = new Button
		{
			Text = "STOĞA EKLE VE KAYDET",
			Location = new Point(30, num + 5),
			Size = new System.Drawing.Size(340, 52),
			BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button2.FlatAppearance.BorderSize = 0;
		button2.Click += delegate
		{
			if (!int.TryParse(txtID.Text, out var result) || string.IsNullOrEmpty(txtName.Text) || txtName.Text == "Bulunamadı!")
			{
				MessageBox.Show("Ürün bilgileri eksik veya bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			try
			{
				DataAccess.AddOrUpdateStorageItemByID(result, txtBar.Text, txtName.Text, txtMan.Text, "Diğer", "Adet", 0m, 0m, txtLoc.Text.Trim(), (int)numQty.Value, dtpDate.Value);
				MessageBox.Show($"{txtName.Text} stok miktarı {(int)numQty.Value} adet artırıldı. Mevcut barkod ve ID korundu.", "Başarılı");
				f.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		f.Controls.Add(button2);
		f.ShowDialog();
		static Panel CreateField(string label, Control input, ref int curY, bool isReadOnly = false)
		{
			Panel panel2 = new Panel
			{
				Location = new Point(30, curY),
				Size = new System.Drawing.Size(340, 65)
			};
			panel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = label,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(75, 85, 99),
				Location = new Point(0, 0),
				AutoSize = true
			});
			input.Location = new Point(0, 22);
			input.Size = new System.Drawing.Size(340, 30);
			if (isReadOnly)
			{
				input.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
				if (input is TextBox textBox)
				{
					textBox.ReadOnly = true;
				}
			}
			panel2.Controls.Add(input);
			curY += 75;
			return panel2;
		}
		void SetProduct(DataRow row)
		{
			txtID.Text = row["ProductID"].ToString();
			txtBar.Text = row["Barcode"].ToString();
			txtName.Text = row["ItemName"].ToString();
			txtMan.Text = row["ManufacturerName"].ToString();
		}
	}

	private int ShowProductSelectionDialog()
	{
		int selectedId = 0;
		Form f = new Form
		{
			Text = "\ud83d\udcd6 Ürün Katalog Seçimi",
			Size = new System.Drawing.Size(600, 500),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		System.Windows.Forms.Label value = new System.Windows.Forms.Label
		{
			Text = "\ud83d\udcd6 Katalogdaki Ürünler",
			Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
			Location = new Point(20, 20),
			AutoSize = true
		};
		f.Controls.Add(value);
		DataTable allProducts = DataAccess.GetAllProducts();
		DataGridView dgv = new DataGridView
		{
			Location = new Point(20, 60),
			Size = new System.Drawing.Size(545, 300),
			BackgroundColor = System.Drawing.Color.White,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			RowHeadersVisible = false,
			AllowUserToAddRows = false,
			ReadOnly = true,
			DataSource = allProducts,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
		};
		f.Controls.Add(dgv);
		Button btn = new Button
		{
			Text = "✅ BU ÜRÜNÜ SEÇ",
			Location = new Point(200, 380),
			Size = new System.Drawing.Size(200, 45),
			BackColor = accentBlue,
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		btn.Click += delegate
		{
			if (dgv.SelectedRows.Count > 0)
			{
				selectedId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ProductID"].Value);
				f.DialogResult = DialogResult.OK;
				f.Close();
			}
			else
			{
				MessageBox.Show("Bir ürün seçin.");
			}
		};
		dgv.CellDoubleClick += delegate
		{
			btn.PerformClick();
		};
		f.Controls.Add(btn);
		f.ShowDialog();
		return selectedId;
	}

	private void ShowTransferForm(DataGridViewRow row)
	{
		Form f = new Form
		{
			Text = "\ud83d\udce6 Markete Ürün Gönder",
			Size = new System.Drawing.Size(420, 500),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		int num = 20;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Markete Stok Sevk",
			Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 45;
		string text = row.Cells["Depo ID"].Value?.ToString();
		string itemName = row.Cells["Ürün Adı"].Value?.ToString();
		int storageQty = Convert.ToInt32(row.Cells["Stok Miktarı"].Value);
		int productId = Convert.ToInt32(row.Cells["Ürün ID"].Value);
		string currentBarcode = row.Cells["Barkod"].Value?.ToString();
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = $"\ud83d\udce6 Seçili Ürün: {itemName}\n\ud83c\udd94 Ürün ID: {productId}\n\ud83d\udd22 Mevcut Depo Stoğu: {storageQty}",
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 65;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Gidecek Mağaza (Market):",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 22;
		ComboBox cbStore = new ComboBox
		{
			Location = new Point(30, num),
			Size = new System.Drawing.Size(340, 28),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		cbStore.Items.Add("MARKET_1");
		cbStore.Items.Add("MARKET_2");
		cbStore.SelectedIndex = 0;
		f.Controls.Add(cbStore);
		num += 40;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Market Satış Fiyatı (₺):",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 22;
		TextBox txtPrice = new TextBox
		{
			Location = new Point(30, num),
			Size = new System.Drawing.Size(340, 28),
			PlaceholderText = "Örn: 25.50"
		};
		f.Controls.Add(txtPrice);
		num += 40;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Gönderilecek Adet:",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num),
			AutoSize = true
		});
		num += 22;
		NumericUpDown numQty = new NumericUpDown
		{
			Location = new Point(30, num),
			Size = new System.Drawing.Size(160, 28),
			Minimum = 1m,
			Maximum = ((storageQty <= 0) ? 1 : storageQty),
			Value = 1m
		};
		f.Controls.Add(numQty);
		num += 45;
		Button button = new Button
		{
			Text = "\ud83d\uded2 TRANSFERİ BAŞLAT",
			Location = new Point(30, num),
			Size = new System.Drawing.Size(340, 48),
			BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			try
			{
				if (storageQty < (int)numQty.Value)
				{
					MessageBox.Show($"Depoda yeterli stok yok! Mevcut stok: {storageQty}");
				}
				else
				{
					decimal result = default(decimal);
					decimal.TryParse(txtPrice.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
					if (result <= 0m)
					{
						MessageBox.Show("Lütfen geçerli bir satış fiyatı giriniz.");
					}
					else
					{
						DataAccess.RegisterProduct(productId, currentBarcode ?? ("PRD-" + productId), itemName ?? "", "Diğer", "", "Adet", result, 0m);
						DataAccess.TransferToMarketWithPrice(productId, cbStore.Text, (int)numQty.Value, result, "Mağazaya ürün sevkiyatı");
						MessageBox.Show("Ürün başarıyla markete gönderildi!", "Başarılı");
						f.Close();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Transfer Hatası: " + ex.Message);
			}
		};
		f.Controls.Add(button);
		f.ShowDialog();
	}

	private void ShowLokantaSaleForm(DataGridViewRow row)
	{
		string value = row.Cells["Barkod"].Value.ToString() ?? "";
		string value2 = row.Cells["Ürün İsim"].Value.ToString() ?? "";
		int num = Convert.ToInt32(row.Cells["ID"].Value);
		int mevcutAdet = Convert.ToInt32(row.Cells["Gelen Adet"].Value);
		int pId = Convert.ToInt32(row.Cells["ProductID"].Value);
		Form f = new Form
		{
			Text = "\ud83c\udf74 Lokanta Satış / Stok Çıkışı",
			Size = new System.Drawing.Size(400, 450),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		int num2 = 20;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "\ud83c\udf74 Lokanta Satışı",
			Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num2),
			AutoSize = true
		});
		num2 += 40;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = $"Ürün: {value2}\nBarkod: {value}\nMevcut Stok: {mevcutAdet}",
			Font = new Font("Segoe UI", 9f),
			ForeColor = System.Drawing.Color.DarkSlateGray,
			Location = new Point(30, num2),
			AutoSize = true
		});
		num2 += 60;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Satış Adedi:",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num2),
			AutoSize = true
		});
		num2 += 22;
		NumericUpDown numAdet = new NumericUpDown
		{
			Minimum = 1m,
			Maximum = ((mevcutAdet <= 0) ? 1 : mevcutAdet),
			Value = 1m,
			Size = new System.Drawing.Size(150, 28),
			Location = new Point(30, num2),
			Font = new Font("Segoe UI", 10f)
		};
		f.Controls.Add(numAdet);
		num2 += 40;
		f.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Satış Fiyatı (₺):",
			Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			Location = new Point(30, num2),
			AutoSize = true
		});
		num2 += 22;
		TextBox txtFiyat = new TextBox
		{
			Size = new System.Drawing.Size(340, 28),
			Location = new Point(30, num2),
			Font = new Font("Segoe UI", 10f),
			BorderStyle = BorderStyle.FixedSingle,
			PlaceholderText = "Örn: 50.00"
		};
		f.Controls.Add(txtFiyat);
		num2 += 60;
		Button button = new Button
		{
			Text = "SATIŞI ONAYLA VE STOKTAN DÜŞ",
			Size = new System.Drawing.Size(340, 50),
			Location = new Point(30, num2),
			BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			try
			{
				decimal result = default(decimal);
				if (!decimal.TryParse(txtFiyat.Text.Replace(".", ","), out result))
				{
					MessageBox.Show("Lütfen geçerli bir fiyat giriniz!");
				}
				else
				{
					int num3 = (int)numAdet.Value;
					if (num3 > mevcutAdet)
					{
						MessageBox.Show("Stok yetersiz!");
					}
					else
					{
						DataAccess.TransferToMarketWithPrice(pId, "LOKANTA", num3, result, "Lokanta Satış Çıkışı");
						MessageBox.Show("Satış başarıyla gerçekleştirildi. Stok güncellendi.", "Başarılı");
						f.Close();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata: " + ex.Message);
			}
		};
		f.Controls.Add(button);
		f.ShowDialog();
	}

	private void PageLokanta(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.AutoScroll = false;
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			DataTable dtCart = new DataTable();
			dtCart.Columns.Add("Ürün");
			dtCart.Columns.Add("Barkod_");
			dtCart.Columns.Add("Adet", typeof(int));
			dtCart.Columns.Add("Birim Fiyat", typeof(decimal));
			dtCart.Columns.Add("Toplam ₺", typeof(decimal));
			DataTable dtMarket = DataAccess.GetAllMarketStocks("LOKANTA");
			string curRoomInfo = "";
			string currentCategory = "Tümü";
			string selectedTableName = "";
			FlowLayoutPanel flowCatalog = new FlowLayoutPanel
			{
				Padding = new Padding(10)
			};
			FlowLayoutPanel flowCart = new FlowLayoutPanel
			{
				Padding = new Padding(10)
			};
			FlowLayoutPanel flowGuests = new FlowLayoutPanel();
			System.Windows.Forms.Label lblCartTotal = new System.Windows.Forms.Label();
			System.Windows.Forms.Label lblSelGuest = new System.Windows.Forms.Label();
			System.Windows.Forms.Label lblDebt = new System.Windows.Forms.Label();
			System.Windows.Forms.Label lblSelectedTable = new System.Windows.Forms.Label();
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 3,
				RowCount = 2,
				Padding = new Padding(10),
				BackColor = System.Drawing.Color.Transparent
			};
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			body.Controls.Add(tableLayoutPanel);
			Panel pnlTop = new Panel
			{
				Dock = DockStyle.Fill
			};
			tableLayoutPanel.Controls.Add(pnlTop, 0, 0);
			tableLayoutPanel.SetColumnSpan(pnlTop, 3);
			pnlTop.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcb3 SOM-POS",
				Font = new Font("Segoe UI Black", 16f),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(0, 15),
				AutoSize = true
			});
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.LeftToRight,
				Padding = new Padding(0, 5, 10, 0)
			};
			pnlTop.Controls.Add(flowLayoutPanel);
			flowLayoutPanel.BringToFront();
			lblSelectedTable.Text = "Masa seçilmedi";
			lblSelectedTable.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);
			lblSelectedTable.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
			lblSelectedTable.AutoSize = true;
			lblSelectedTable.Margin = new Padding(0, 12, 10, 0);
			flowLayoutPanel.Controls.Add(lblSelectedTable);
			Button button = new Button
			{
				Text = "\ud83d\udcca GÜNLÜK SATIŞLAR",
				Size = new System.Drawing.Size(160, 42),
				BackColor = System.Drawing.Color.FromArgb(148, 163, 184),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Margin = new Padding(10, 0, 0, 0)
			};
			button.FlatAppearance.BorderSize = 0;
			flowLayoutPanel.Controls.Add(button);
			Button btnMasaSec = new Button
			{
				Text = "\ud83e\ude91 MASA SEÇ",
				Size = new System.Drawing.Size(130, 42),
				BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Margin = new Padding(10, 0, 0, 0)
			};
			btnMasaSec.FlatAppearance.BorderSize = 0;
			flowLayoutPanel.Controls.Add(btnMasaSec);
			button.Click += delegate
			{
				Form form = new Form
				{
					Text = "\ud83d\udcca Günlük Satışlar",
					Size = new System.Drawing.Size(950, 650),
					StartPosition = FormStartPosition.CenterParent,
					BackColor = System.Drawing.Color.White
				};
				Panel panel3 = new Panel
				{
					Dock = DockStyle.Fill,
					Padding = new Padding(25)
				};
				form.Controls.Add(panel3);
				DataGridView dataGridView = new DataGridView
				{
					Dock = DockStyle.Fill,
					BackgroundColor = System.Drawing.Color.White,
					BorderStyle = BorderStyle.None,
					RowHeadersVisible = false,
					AllowUserToAddRows = false,
					ReadOnly = true,
					AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
					RowTemplate = 
					{
						Height = 42
					},
					SelectionMode = DataGridViewSelectionMode.FullRowSelect,
					Font = new Font("Segoe UI", 9.5f),
					EnableHeadersVisualStyles = false
				};
				dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
				dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
				panel3.Controls.Add(dataGridView);
				DataTable recentSales = DataAccess.GetRecentSales(1000);
				DataTable dataTable = recentSales.Clone();
				foreach (DataRow row in recentSales.Rows)
				{
					if (Convert.ToDateTime(row["Tarih"]).Date == DateTime.Today)
					{
						dataTable.ImportRow(row);
					}
				}
				dataGridView.DataSource = dataTable;
				form.ShowDialog();
			};
			btnMasaSec.Click += delegate
			{
				Form dlg = new Form
				{
					Text = "\ud83e\ude91 Masa Seçimi",
					Size = new System.Drawing.Size(800, 500),
					StartPosition = FormStartPosition.CenterParent,
					BackColor = System.Drawing.Color.White
				};
				FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
				{
					Dock = DockStyle.Fill,
					Padding = new Padding(20),
					AutoScroll = true
				};
				dlg.Controls.Add(flowLayoutPanel2);
				DataTable restaurantTables = DataAccess.GetRestaurantTables();
				foreach (DataRow row2 in restaurantTables.Rows)
				{
					string tName = row2["TableName"].ToString();
					int tid = Convert.ToInt32(row2["TableID"]);
					bool avail = row2["Status"].ToString() == "Available";
					RoundedPanel roundedPanel5 = new RoundedPanel
					{
						Size = new System.Drawing.Size(120, 100),
						Margin = new Padding(10),
						BackColor = (avail ? System.Drawing.Color.FromArgb(240, 253, 244) : System.Drawing.Color.FromArgb(254, 226, 226)),
						BorderRadius = 12,
						Cursor = Cursors.Hand
					};
					roundedPanel5.Controls.Add(new System.Windows.Forms.Label
					{
						Text = tName,
						Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
						ForeColor = (avail ? System.Drawing.Color.FromArgb(22, 163, 74) : System.Drawing.Color.FromArgb(220, 38, 38)),
						Dock = DockStyle.Fill,
						TextAlign = ContentAlignment.MiddleCenter,
						BackColor = System.Drawing.Color.Transparent
					});
					Action act2 = delegate
					{
						if (avail)
						{
							DataAccess.UpdateTableStatus(tid, "Occupied");
							selectedTableName = tName;
							lblSelectedTable.Text = "\ud83d\udccd " + tName;
							lblSelectedTable.ForeColor = System.Drawing.Color.FromArgb(99, 102, 241);
							btnMasaSec.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
							btnMasaSec.Text = "\ud83e\ude91 " + tName;
							dlg.Close();
						}
						else if (MessageBox.Show(tName + " boşaltılsın mı?", "Masa Boşalt", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							DataAccess.UpdateTableStatus(tid, "Available");
							dlg.Close();
						}
					};
					roundedPanel5.Click += delegate
					{
						act2();
					};
					foreach (Control control3 in roundedPanel5.Controls)
					{
						control3.Click += delegate
						{
							act2();
						};
					}
					flowLayoutPanel2.Controls.Add(roundedPanel5);
				}
				dlg.ShowDialog();
			};
			string[] array = new string[5] { "Tümü", "Sıcak İçecekler", "Soğuk İçecekler", "Yemekler", "Tatlılar" };
			int num = 250;
			string[] array2 = array;
			foreach (string cat in array2)
			{
				RoundedPanel btnPill = new RoundedPanel
				{
					Size = new System.Drawing.Size(cat.Length * 8 + 35, 36),
					Location = new Point(num, 16),
					BorderRadius = 18,
					Cursor = Cursors.Hand,
					BackColor = ((cat == currentCategory) ? System.Drawing.Color.FromArgb(59, 130, 246) : System.Drawing.Color.White)
				};
				System.Windows.Forms.Label lblP = new System.Windows.Forms.Label
				{
					Text = cat,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					ForeColor = ((cat == currentCategory) ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(71, 85, 105)),
					AutoSize = true,
					BackColor = System.Drawing.Color.Transparent
				};
				lblP.Location = new Point((btnPill.Width - lblP.PreferredWidth) / 2, 9);
				btnPill.Controls.Add(lblP);
				Action act = delegate
				{
					currentCategory = cat;
					foreach (Control control4 in pnlTop.Controls)
					{
						if (control4 is RoundedPanel roundedPanel5)
						{
							roundedPanel5.BackColor = System.Drawing.Color.White;
							foreach (Control control5 in roundedPanel5.Controls)
							{
								control5.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
							}
						}
					}
					btnPill.BackColor = System.Drawing.Color.FromArgb(59, 130, 246);
					lblP.ForeColor = System.Drawing.Color.White;
					RefreshCatalog("");
				};
				btnPill.Click += delegate
				{
					act();
				};
				lblP.Click += delegate
				{
					act();
				};
				pnlTop.Controls.Add(btnPill);
				num += btnPill.Width + 10;
			}
			RoundedPanel cardGuestBg = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(3)
			};
			tableLayoutPanel.Controls.Add(cardGuestBg, 0, 1);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				Padding = new Padding(15)
			};
			cardGuestBg.Controls.Add(panel);
			flowGuests.Dock = DockStyle.Fill;
			flowGuests.AutoScroll = true;
			flowGuests.BackColor = System.Drawing.Color.White;
			cardGuestBg.Controls.Add(flowGuests);
			flowGuests.BringToFront();
			DataTable dtG = DataAccess.GetReservations();
			dtG.DefaultView.RowFilter = "Status = 'CheckedIn'";
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Dock = DockStyle.Bottom,
				Height = 80,
				BackColor = System.Drawing.Color.FromArgb(239, 246, 255),
				Padding = new Padding(15),
				BorderRadius = 15
			};
			cardGuestBg.Controls.Add(roundedPanel);
			roundedPanel.BringToFront();
			lblSelGuest.Text = "Seçim Bekleniyor";
			lblSelGuest.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
			lblSelGuest.Location = new Point(15, 15);
			lblSelGuest.AutoSize = true;
			lblDebt.Text = "Tutar yok";
			lblDebt.Font = new Font("Segoe UI", 9f);
			lblDebt.ForeColor = System.Drawing.Color.Red;
			lblDebt.Location = new Point(15, 45);
			lblDebt.AutoSize = true;
			roundedPanel.Controls.AddRange(new Control[2] { lblDebt, lblSelGuest });
			TextBox txtSearch = new TextBox
			{
				PlaceholderText = "Müşteri veya Oda Ara...",
				Font = new Font("Segoe UI", 10f),
				Width = 220,
				Location = new Point(15, 30),
				BorderStyle = BorderStyle.FixedSingle
			};
			panel.Controls.Add(txtSearch);
			Action refreshGuests = delegate
			{
				flowGuests.Controls.Clear();
				string text = txtSearch.Text.ToLower().Trim();
				foreach (DataRowView item in dtG.DefaultView)
				{
					string rNum = item["Oda"].ToString();
					string mName = item["Musteri"].ToString();
					if (!(text != "") || rNum.ToLower().Contains(text) || mName.ToLower().Contains(text))
					{
						decimal debt = DataAccess.GetLokantaTotalForGuest("Oda " + rNum + " - " + mName);
						RoundedPanel roundedPanel5 = new RoundedPanel
						{
							Size = new System.Drawing.Size(cardGuestBg.Width - 40, 75),
							Margin = new Padding(15, 6, 15, 6),
							BackColor = System.Drawing.Color.White,
							BorderRadius = 12,
							Cursor = Cursors.Hand
						};
						roundedPanel5.Controls.Add(new System.Windows.Forms.Label
						{
							Text = mName,
							Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
							Location = new Point(15, 15),
							AutoSize = true
						});
						roundedPanel5.Controls.Add(new System.Windows.Forms.Label
						{
							Text = $"Oda {rNum} | {debt:N2} ₺ Borç",
							Font = new Font("Segoe UI", 9f),
							Location = new Point(15, 40),
							AutoSize = true
						});
						Action selectAction = delegate
						{
							curRoomInfo = "Oda " + rNum + " - " + mName;
							lblSelGuest.Text = "\ud83d\udc64 " + mName;
							lblDebt.Text = $"Borç: {debt:N2} ₺";
						};
						roundedPanel5.Click += delegate
						{
							selectAction();
						};
						foreach (Control control6 in roundedPanel5.Controls)
						{
							control6.Click += delegate
							{
								selectAction();
							};
						}
						flowGuests.Controls.Add(roundedPanel5);
					}
				}
			};
			txtSearch.TextChanged += delegate
			{
				refreshGuests();
			};
			refreshGuests();
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.Transparent,
				BorderRadius = 0
			};
			tableLayoutPanel.Controls.Add(roundedPanel2, 1, 1);
			flowCatalog.Dock = DockStyle.Fill;
			flowCatalog.AutoScroll = true;
			flowCatalog.BackColor = System.Drawing.Color.Transparent;
			roundedPanel2.Controls.Add(flowCatalog);
			RoundedPanel roundedPanel3 = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(5)
			};
			tableLayoutPanel.Controls.Add(roundedPanel3, 2, 1);
			Panel panel2 = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				Padding = new Padding(15, 15, 15, 5)
			};
			panel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Sepet",
				Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
				Dock = DockStyle.Left,
				AutoSize = true
			});
			Button button2 = new Button
			{
				Text = "BOŞALT",
				Location = new Point(panel2.Width - 100, 15),
				Size = new System.Drawing.Size(70, 25),
				BackColor = System.Drawing.Color.FromArgb(254, 226, 226),
				ForeColor = System.Drawing.Color.Red,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				Anchor = AnchorStyles.Right,
				Cursor = Cursors.Hand
			};
			button2.FlatAppearance.BorderSize = 0;
			button2.Click += delegate
			{
				dtCart.Rows.Clear();
				RefreshCart();
			};
			panel2.Controls.Add(button2);
			roundedPanel3.Controls.Add(panel2);
			flowCart.Dock = DockStyle.Fill;
			flowCart.AutoScroll = true;
			flowCart.BackColor = System.Drawing.Color.White;
			roundedPanel3.Controls.Add(flowCart);
			flowCart.BringToFront();
			RoundedPanel roundedPanel4 = new RoundedPanel
			{
				Dock = DockStyle.Bottom,
				Height = 170,
				BackColor = System.Drawing.Color.FromArgb(30, 41, 59),
				BorderRadius = 15
			};
			roundedPanel3.Controls.Add(roundedPanel4);
			lblCartTotal.Location = new Point(20, 45);
			lblCartTotal.Font = new Font("Segoe UI", 26f, System.Drawing.FontStyle.Bold);
			lblCartTotal.ForeColor = System.Drawing.Color.White;
			lblCartTotal.AutoSize = true;
			roundedPanel4.Controls.Add(lblCartTotal);
			Button button3 = new Button
			{
				Text = "KASAYA GÖNDER",
				Size = new System.Drawing.Size(roundedPanel4.Width - 40, 55),
				Location = new Point(20, 100),
				BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			button3.FlatAppearance.BorderSize = 0;
			roundedPanel4.Controls.Add(button3);
			button3.Click += delegate
			{
				if (string.IsNullOrEmpty(curRoomInfo))
				{
					MessageBox.Show("Lütfen konaklayan misafir seçin veya masa seçimi yapın!");
				}
				else
				{
					if (dtCart.Rows.Count != 0)
					{
						try
						{
							string text = curRoomInfo;
							if (!string.IsNullOrEmpty(selectedTableName))
							{
								text = selectedTableName + " | " + curRoomInfo;
							}
							foreach (DataRow row3 in dtCart.Rows)
							{
								DataAccess.SellFromMarket(row3["Barkod_"].ToString(), "LOKANTA", (int)row3["Adet"], text);
							}
							ShowReceiptDialog(text, dtCart);
							MessageBox.Show("✅ Sipariş mutfağa ve hesaba aktarıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
							dtCart.Rows.Clear();
							RefreshCart();
							dtMarket = DataAccess.GetAllMarketStocks("LOKANTA");
							RefreshCatalog("");
							selectedTableName = "";
							lblSelectedTable.Text = "Masa seçilmedi";
							lblSelectedTable.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
							btnMasaSec.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
							btnMasaSec.Text = "\ud83e\ude91 MASA SEÇ";
							return;
						}
						catch (Exception ex)
						{
							MessageBox.Show("Hata: " + ex.Message);
							return;
						}
					}
					MessageBox.Show("Sepetiniz boş!");
				}
			};
			RefreshCatalog("");
			RefreshCart();
			void RefreshCart()
			{
				flowCart.Controls.Clear();
				decimal value = default(decimal);
				foreach (DataRow r in dtCart.Rows)
				{
					string text = r["Ürün"].ToString();
					int qty = (int)r["Adet"];
					decimal price = (decimal)r["Birim Fiyat"];
					decimal num3 = (decimal)r["Toplam ₺"];
					value += num3;
					RoundedPanel roundedPanel5 = new RoundedPanel
					{
						Size = new System.Drawing.Size(flowCart.Width - 25, 70),
						Margin = new Padding(0, 0, 0, 8),
						BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
						BorderRadius = 10
					};
					roundedPanel5.Controls.Add(new System.Windows.Forms.Label
					{
						Text = text,
						Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
						Location = new Point(15, 15),
						AutoSize = true
					});
					roundedPanel5.Controls.Add(new System.Windows.Forms.Label
					{
						Text = price.ToString("N2") + " ₺",
						Font = new Font("Segoe UI", 8.5f),
						Location = new Point(15, 38),
						AutoSize = true
					});
					RoundedPanel roundedPanel6 = new RoundedPanel
					{
						Size = new System.Drawing.Size(100, 36),
						Location = new Point(roundedPanel5.Width - 110, 17),
						BackColor = System.Drawing.Color.White,
						BorderRadius = 18
					};
					System.Windows.Forms.Label label = new System.Windows.Forms.Label
					{
						Text = "−",
						Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
						Location = new Point(8, 7),
						Cursor = Cursors.Hand,
						AutoSize = true
					};
					System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
					{
						Text = qty.ToString(),
						Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
						Location = new Point(40, 7),
						AutoSize = true
					};
					System.Windows.Forms.Label label3 = new System.Windows.Forms.Label
					{
						Text = "+",
						Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
						Location = new Point(78, 6),
						Cursor = Cursors.Hand,
						AutoSize = true
					};
					label.Click += delegate
					{
						if (qty > 1)
						{
							r["Adet"] = qty - 1;
							r["Toplam ₺"] = (decimal)(qty - 1) * price;
						}
						else
						{
							dtCart.Rows.Remove(r);
						}
						RefreshCart();
					};
					label3.Click += delegate
					{
						r["Adet"] = qty + 1;
						r["Toplam ₺"] = (decimal)(qty + 1) * price;
						RefreshCart();
					};
					roundedPanel6.Controls.AddRange(new Control[3] { label, label2, label3 });
					roundedPanel5.Controls.Add(roundedPanel6);
					flowCart.Controls.Add(roundedPanel5);
				}
				lblCartTotal.Text = $"{value:N2} ₺";
			}
			void RefreshCatalog(string search)
			{
				flowCatalog.Controls.Clear();
				foreach (DataRow row4 in dtMarket.Rows)
				{
					string name = row4["ItemName"].ToString();
					string text = row4["Category"].ToString();
					string barcode = row4["Barcode"].ToString();
					decimal price = Convert.ToDecimal(row4["Price"]);
					if ((!(currentCategory != "Tümü") || !(text != currentCategory)) && (string.IsNullOrEmpty(search) || name.ToLower().Contains(search.ToLower())))
					{
						RoundedPanel tile = new RoundedPanel
						{
							Size = new System.Drawing.Size(185, 200),
							Margin = new Padding(10),
							BackColor = System.Drawing.Color.White,
							BorderRadius = 15,
							Cursor = Cursors.Hand
						};
						tile.Paint += delegate(object? s, PaintEventArgs e)
						{
							using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
							e.Graphics.DrawPath(pen, CustGetPath(tile.ClientRectangle, 15));
						};
						tile.Controls.Add(new System.Windows.Forms.Label
						{
							Text = GetIcon(name),
							Font = new Font("Segoe UI", 24f),
							Location = new Point(65, 15),
							AutoSize = true,
							BackColor = System.Drawing.Color.Transparent
						});
						tile.Controls.Add(new System.Windows.Forms.Label
						{
							Text = name,
							Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
							Location = new Point(0, 75),
							Size = new System.Drawing.Size(185, 40),
							TextAlign = ContentAlignment.MiddleCenter,
							BackColor = System.Drawing.Color.Transparent
						});
						tile.Controls.Add(new System.Windows.Forms.Label
						{
							Text = price.ToString("N2") + " ₺",
							Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(59, 130, 246),
							Location = new Point(0, 130),
							Size = new System.Drawing.Size(185, 25),
							TextAlign = ContentAlignment.MiddleCenter,
							BackColor = System.Drawing.Color.Transparent
						});
						Action addAction = delegate
						{
							DataRow dataRow2 = dtCart.AsEnumerable().FirstOrDefault((DataRow x) => x.Field<string>("Barkod_") == barcode);
							if (dataRow2 != null)
							{
								dataRow2["Adet"] = (int)dataRow2["Adet"] + 1;
								dataRow2["Toplam ₺"] = (decimal)(int)dataRow2["Adet"] * price;
							}
							else
							{
								dtCart.Rows.Add(name, barcode, 1, price, price);
							}
							RefreshCart();
						};
						tile.Click += delegate
						{
							addAction();
						};
						foreach (Control control7 in tile.Controls)
						{
							control7.Click += delegate
							{
								addAction();
							};
						}
						flowCatalog.Controls.Add(tile);
					}
				}
			}
		});
	}

	private void PagePayments(Panel body)
	{
		DataTable dt = null;
		try
		{
			dt = DataAccess.GetReservations();
		}
		catch
		{
			dt = new DataTable();
		}
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 75,
				Padding = new Padding(20, 15, 20, 10),
				BackColor = System.Drawing.Color.White
			};
			body.Controls.Add(panel);
			Button btnTabActive = new Button
			{
				Text = "\ud83d\udcb3 AKTİF KONAKLAYANLAR",
				Size = new System.Drawing.Size(250, 42),
				Location = new Point(20, 15),
				BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnTabActive.FlatAppearance.BorderSize = 0;
			Button btnTabHist = new Button
			{
				Text = "\ud83d\udcdc ÖDEME GEÇMİŞİ",
				Size = new System.Drawing.Size(200, 42),
				Location = new Point(285, 15),
				BackColor = System.Drawing.Color.White,
				ForeColor = System.Drawing.Color.Gray,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnTabHist.FlatAppearance.BorderSize = 1;
			btnTabHist.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
			TextBox textBox = new TextBox
			{
				PlaceholderText = "Hızlı Ara...",
				Font = new Font("Segoe UI", 11f),
				Size = new System.Drawing.Size(250, 30),
				Location = new Point(panel.Width - 270, 19),
				Anchor = (AnchorStyles.Top | AnchorStyles.Right),
				Visible = false
			};
			panel.Controls.AddRange(new Control[3] { btnTabActive, btnTabHist, textBox });
			Panel pnlActiveLayer = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(20)
			};
			Panel pnlHistoryLayer = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(20),
				Visible = false
			};
			body.Controls.Add(pnlActiveLayer);
			pnlActiveLayer.BringToFront();
			body.Controls.Add(pnlHistoryLayer);
			btnTabActive.Click += delegate
			{
				pnlActiveLayer.Visible = true;
				pnlHistoryLayer.Visible = false;
				pnlActiveLayer.BringToFront();
				btnTabActive.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
				btnTabActive.ForeColor = System.Drawing.Color.White;
				btnTabActive.FlatAppearance.BorderSize = 0;
				btnTabHist.BackColor = System.Drawing.Color.White;
				btnTabHist.ForeColor = System.Drawing.Color.Gray;
				btnTabHist.FlatAppearance.BorderSize = 1;
			};
			btnTabHist.Click += delegate
			{
				pnlActiveLayer.Visible = false;
				pnlHistoryLayer.Visible = true;
				pnlHistoryLayer.BringToFront();
				btnTabHist.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
				btnTabHist.ForeColor = System.Drawing.Color.White;
				btnTabHist.FlatAppearance.BorderSize = 0;
				btnTabActive.BackColor = System.Drawing.Color.White;
				btnTabActive.ForeColor = System.Drawing.Color.Gray;
				btnTabActive.FlatAppearance.BorderSize = 1;
			};
			Panel panel2 = new Panel
			{
				Dock = DockStyle.Right,
				Width = (int)((double)body.Width * 0.45),
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(10)
			};
			pnlActiveLayer.Controls.Add(panel2);
			Panel pnlLeftWrapper = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.Transparent
			};
			Panel panel3 = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				Padding = new Padding(15),
				BackColor = System.Drawing.Color.White
			};
			System.Windows.Forms.Label label = new System.Windows.Forms.Label
			{
				Text = "Kat (1.)",
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(15, 5),
				AutoSize = true
			};
			ComboBox cmbFloor = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 10f),
				Width = 110,
				Location = new Point(15, 23)
			};
			System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
			{
				Text = "Oda (2.)",
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(135, 5),
				AutoSize = true
			};
			ComboBox cmbRoom = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 10f),
				Width = 110,
				Location = new Point(135, 23)
			};
			TextBox txtSearch = new TextBox
			{
				PlaceholderText = "Müşteri veya Oda Ara...",
				Font = new Font("Segoe UI", 10f),
				Width = 230,
				Location = new Point(15, 60),
				BorderStyle = BorderStyle.FixedSingle
			};
			panel3.Controls.AddRange(new Control[5] { label, cmbFloor, label2, cmbRoom, txtSearch });
			pnlLeftWrapper.Controls.Add(panel3);
			FlowLayoutPanel flowLeft = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				Padding = new Padding(10),
				BackColor = System.Drawing.Color.White
			};
			pnlLeftWrapper.Controls.Add(flowLeft);
			flowLeft.BringToFront();
			pnlActiveLayer.Controls.Add(pnlLeftWrapper);
			Dictionary<string, List<string>> floorRooms = new Dictionary<string, List<string>>();
			foreach (DataRow row2 in dt.Rows)
			{
				if (!(row2["Status"].ToString() != "CheckedIn"))
				{
					string key = ((row2["FloorNumber"] != DBNull.Value) ? (row2["FloorNumber"].ToString() + ". Kat") : "Diğer");
					string item = row2["Oda"].ToString();
					if (!floorRooms.ContainsKey(key))
					{
						floorRooms[key] = new List<string>();
					}
					if (!floorRooms[key].Contains(item))
					{
						floorRooms[key].Add(item);
					}
				}
			}
			List<string> list = floorRooms.Keys.ToList();
			list.Sort();
			cmbFloor.Items.Add("Tüm Katlar");
			foreach (string item2 in list)
			{
				cmbFloor.Items.Add(item2);
			}
			cmbFloor.SelectedIndex = 0;
			cmbRoom.Items.Add("Tüm Odalar");
			cmbRoom.SelectedIndex = 0;
			cmbFloor.SelectedIndexChanged += delegate
			{
				cmbRoom.Items.Clear();
				cmbRoom.Items.Add("Tüm Odalar");
				string text = cmbFloor.SelectedItem?.ToString();
				if (text != null && text != "Tüm Katlar" && floorRooms.ContainsKey(text))
				{
					List<string> list2 = floorRooms[text];
					list2.Sort();
					foreach (string item3 in list2)
					{
						cmbRoom.Items.Add(item3);
					}
				}
				cmbRoom.SelectedIndex = 0;
			};
			RoundedPanel receiptCard = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(5)
			};
			receiptCard.Paint += delegate(object? s, PaintEventArgs e)
			{
				using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
				e.Graphics.DrawPath(pen, CustGetPath(receiptCard.ClientRectangle, 15));
			};
			panel2.Controls.Add(receiptCard);
			System.Windows.Forms.Label label3 = new System.Windows.Forms.Label
			{
				Text = "\ud83e\uddfe DİJİTAL ADİSYON",
				Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Dock = DockStyle.Top,
				TextAlign = ContentAlignment.MiddleCenter,
				Height = 50,
				Padding = new Padding(0, 10, 0, 0)
			};
			FlowLayoutPanel flowRec = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				Padding = new Padding(15, 5, 15, 5)
			};
			Panel pnlRecActions = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 180,
				Padding = new Padding(15),
				BackColor = System.Drawing.Color.FromArgb(248, 250, 252)
			};
			receiptCard.Controls.Add(flowRec);
			receiptCard.Controls.Add(label3);
			receiptCard.Controls.Add(pnlRecActions);
			label3.BringToFront();
			pnlRecActions.SendToBack();
			flowRec.BringToFront();
			Button btnPartialPay = new Button
			{
				Text = "\ud83d\udcb5 ARA ÖDEME",
				Size = new System.Drawing.Size(receiptCard.Width / 2 - 20, 60),
				Location = new Point(10, 115),
				BackColor = System.Drawing.Color.FromArgb(59, 130, 246),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Enabled = false
			};
			btnPartialPay.FlatAppearance.BorderSize = 0;
			Button btnFinalPay = new Button
			{
				Text = "ÇIKIŞ YAP",
				Size = new System.Drawing.Size(receiptCard.Width / 2 - 20, 60),
				Location = new Point(receiptCard.Width / 2, 115),
				BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Enabled = false
			};
			btnFinalPay.FlatAppearance.BorderSize = 0;
			System.Windows.Forms.Label lblRecTotal = new System.Windows.Forms.Label
			{
				Text = "0.00 ₺",
				Font = new Font("Segoe UI", 32f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Dock = DockStyle.Top,
				TextAlign = ContentAlignment.MiddleRight,
				Height = 65
			};
			System.Windows.Forms.Label lblRecTitleT = new System.Windows.Forms.Label
			{
				Text = "KALAN BORÇ",
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Dock = DockStyle.Top,
				TextAlign = ContentAlignment.MiddleRight,
				Height = 30
			};
			pnlRecActions.Controls.Add(btnPartialPay);
			pnlRecActions.Controls.Add(btnFinalPay);
			pnlRecActions.Controls.Add(lblRecTotal);
			pnlRecActions.Controls.Add(lblRecTitleT);
			pnlRecActions.Layout += delegate
			{
				btnPartialPay.Size = new System.Drawing.Size(pnlRecActions.Width / 2 - 15, 60);
				btnFinalPay.Size = new System.Drawing.Size(pnlRecActions.Width / 2 - 15, 60);
				btnPartialPay.Location = new Point(10, 115);
				btnFinalPay.Location = new Point(pnlRecActions.Width / 2 + 5, 115);
			};
			dt.DefaultView.RowFilter = "Status = 'CheckedOut'";
			DataTable dataSource = dt.DefaultView.ToTable();
			DataGridView dataGridView = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				DataSource = dataSource,
				ReadOnly = true,
				AllowUserToAddRows = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 45
				},
				Font = new Font("Segoe UI", 10f),
				GridColor = System.Drawing.Color.FromArgb(241, 245, 249)
			};
			dataGridView.EnableHeadersVisualStyles = false;
			dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
			dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
			pnlHistoryLayer.Controls.Add(dataGridView);
			Action RefLoad = delegate
			{
				flowLeft.Controls.Clear();
				string text = txtSearch.Text.Trim().ToLower();
				string text2 = cmbFloor.SelectedItem?.ToString() ?? "Tüm Katlar";
				string text3 = cmbRoom.SelectedItem?.ToString() ?? "Tüm Odalar";
				dt.DefaultView.RowFilter = "Status = 'CheckedIn'";
				pnlLeftWrapper.Width = body.Width / 2 - 20;
				foreach (DataRowView row in dt.DefaultView)
				{
					string name = row["Musteri"].ToString();
					string text4 = row["Oda"].ToString();
					string text5 = ((row["FloorNumber"] != DBNull.Value) ? (row["FloorNumber"].ToString() + ". Kat") : "Diğer");
					if ((!(text != "") || name.ToLower().Contains(text) || text4.ToLower().Contains(text)) && (!(text2 != "Tüm Katlar") || !(text5 != text2)) && (!(text3 != "Tüm Odalar") || !(text4 != text3)))
					{
						int resId = Convert.ToInt32(row["ReservationID"]);
						decimal totalTutar = Convert.ToDecimal(row["ToplamTutar"]);
						decimal paid = Convert.ToDecimal(row["OdenenMiktar"]);
						string roomInfo = "Oda " + text4 + " - " + name;
						decimal lokantaTotal = DataAccess.GetLokantaTotalForGuest(roomInfo);
						decimal num = totalTutar + lokantaTotal;
						decimal num2 = num - paid;
						RoundedPanel card = new RoundedPanel
						{
							Size = new System.Drawing.Size(pnlLeftWrapper.Width - 30, 95),
							Margin = new Padding(10, 5, 10, 10),
							BackColor = System.Drawing.Color.White,
							BorderRadius = 15,
							Cursor = Cursors.Hand
						};
						card.Paint += delegate(object? s, PaintEventArgs e)
						{
							using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240));
							e.Graphics.DrawPath(pen, CustGetPath(card.ClientRectangle, 15));
						};
						RoundedPanel roundedPanel = new RoundedPanel
						{
							Size = new System.Drawing.Size(54, 54),
							Location = new Point(15, 20),
							BackColor = ((num2 > 0m) ? System.Drawing.Color.FromArgb(254, 242, 242) : System.Drawing.Color.FromArgb(240, 253, 244)),
							BorderRadius = 12
						};
						roundedPanel.Controls.Add(new System.Windows.Forms.Label
						{
							Text = text4,
							Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
							ForeColor = ((num2 > 0m) ? System.Drawing.Color.FromArgb(220, 38, 38) : System.Drawing.Color.FromArgb(16, 185, 129)),
							AutoSize = false,
							Dock = DockStyle.Fill,
							TextAlign = ContentAlignment.MiddleCenter,
							BackColor = System.Drawing.Color.Transparent
						});
						card.Controls.Add(roundedPanel);
						card.Controls.Add(new System.Windows.Forms.Label
						{
							Text = name,
							Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
							Location = new Point(85, 20),
							AutoSize = true
						});
						System.Windows.Forms.Label value = new System.Windows.Forms.Label
						{
							Text = ((num2 <= 0m) ? "● Ödeme Tamam" : $"● {num2:N2} ₺ Borç"),
							Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
							ForeColor = ((num2 <= 0m) ? System.Drawing.Color.FromArgb(16, 185, 129) : System.Drawing.Color.FromArgb(220, 38, 38)),
							Location = new Point(85, 45),
							AutoSize = true
						};
						card.Controls.Add(value);
						Panel panel4 = new Panel
						{
							Size = new System.Drawing.Size(card.Width - 110, 6),
							Location = new Point(87, 72),
							BackColor = System.Drawing.Color.FromArgb(241, 245, 249)
						};
						decimal num3 = ((num > 0m) ? (paid / num) : 1m);
						if (num3 > 1m)
						{
							num3 = 1m;
						}
						if (num3 < 0m)
						{
							num3 = default(decimal);
						}
						Panel value2 = new Panel
						{
							Size = new System.Drawing.Size((int)((decimal)panel4.Width * num3), 6),
							Location = new Point(0, 0),
							BackColor = ((num3 >= 1m) ? System.Drawing.Color.FromArgb(16, 185, 129) : System.Drawing.Color.FromArgb(59, 130, 246))
						};
						panel4.Controls.Add(value2);
						card.Controls.Add(panel4);
						Action DoSelect = delegate
						{
							foreach (Control control2 in flowLeft.Controls)
							{
								if (control2 is RoundedPanel roundedPanel2)
								{
									roundedPanel2.BackColor = System.Drawing.Color.White;
									roundedPanel2.Invalidate();
								}
							}
							card.BackColor = System.Drawing.Color.FromArgb(240, 253, 244);
							card.Invalidate();
							flowRec.Controls.Clear();
							DateTime value3 = Convert.ToDateTime(row["Giris"]);
							DateTime value4 = Convert.ToDateTime(row["Cikis"]);
							int num4 = Math.Max(1, (int)(value4.Date - value3.Date).TotalDays);
							decimal num5 = totalTutar / (decimal)num4;
							int num6 = Math.Max(0, (int)(DateTime.Today - value4.Date).TotalDays);
							decimal num7 = (decimal)num6 * num5;
							AddRecRow(flowRec, "Müşteri: " + name, "", bold: true);
							AddRecRow(flowRec, $"Konaklama: {value3:dd.MM.yyyy} - {value4:dd.MM.yyyy}", "");
							AddRecRow(flowRec, "", "", bold: false, line: true);
							AddRecRow(flowRec, $"Oda Konaklaması ({num4} Gün)", totalTutar.ToString("N2") + " ₺");
							if (num6 > 0)
							{
								AddRecRow(flowRec, $"⚠\ufe0f Ekstra Gün Ücreti ({num6} Gün)", num7.ToString("N2") + " ₺", bold: false, line: false, System.Drawing.Color.OrangeRed);
							}
							AddRecRow(flowRec, "", "", bold: false, line: true);
							DataTable lokantaSalesForGuest = DataAccess.GetLokantaSalesForGuest(roomInfo);
							if (lokantaSalesForGuest != null && lokantaSalesForGuest.Rows.Count > 0)
							{
								AddRecRow(flowRec, "Lokanta Harcamaları", "", bold: true);
								foreach (DataRow row3 in lokantaSalesForGuest.Rows)
								{
									AddRecRow(flowRec, $"  • {row3["ItemName"]} (x{row3["Quantity"]})", Convert.ToDecimal(row3["TotalPrice"]).ToString("N2") + " ₺");
								}
								AddRecRow(flowRec, "", "", bold: false, line: true);
							}
							decimal num8 = totalTutar + num7 + lokantaTotal;
							decimal num9 = num8 - paid;
							AddRecRow(flowRec, "GENEL TOPLAM", num8.ToString("N2") + " ₺", bold: true, line: false, System.Drawing.Color.FromArgb(79, 70, 229));
							AddRecRow(flowRec, "ÖDENEN MİKTAR", paid.ToString("N2") + " ₺", bold: true, line: false, System.Drawing.Color.FromArgb(16, 185, 129));
							lblRecTotal.Text = num9.ToString("N2") + " ₺";
							lblRecTotal.ForeColor = ((num9 > 0m) ? System.Drawing.Color.FromArgb(220, 38, 38) : System.Drawing.Color.FromArgb(16, 185, 129));
							lblRecTitleT.Text = ((num9 <= 0m) ? "BORÇ BULUNMUYOR" : "KALAN TAHSİLAT");
							btnPartialPay.Enabled = num9 > 0m;
							btnFinalPay.Enabled = true;
							btnPartialPay.Tag = Tuple.Create(resId, name, num9);
							btnFinalPay.Tag = Tuple.Create(resId, name, num9);
						};
						card.Click += delegate
						{
							DoSelect();
						};
						foreach (Control control3 in card.Controls)
						{
							control3.Click += delegate
							{
								DoSelect();
							};
						}
						flowLeft.Controls.Add(card);
					}
				}
			};
			txtSearch.TextChanged += delegate
			{
				RefLoad();
			};
			cmbFloor.SelectedIndexChanged += delegate
			{
				RefLoad();
			};
			cmbRoom.SelectedIndexChanged += delegate
			{
				RefLoad();
			};
			body.SizeChanged += delegate
			{
				RefLoad();
			};
			RefLoad();
			btnPartialPay.Click += async delegate
			{
				try
				{
					Tuple<int, string, decimal> tg = btnPartialPay.Tag as Tuple<int, string, decimal>;
					if (tg != null)
					{
						DataRow row = GetRow(tg.Item1);
						if (row != null)
						{
							await ShowCheckoutPaymentDialog(row);
							ShowPage("Ödeme");
						}
					}
				}
				catch
				{
				}
			};
			btnFinalPay.Click += async delegate
			{
				try
				{
					Tuple<int, string, decimal> tg = btnFinalPay.Tag as Tuple<int, string, decimal>;
					if (tg != null)
					{
						DataRow row = GetRow(tg.Item1);
						if (row != null)
						{
							await ShowCheckoutPaymentDialog(row);
							ShowPage("Ödeme");
						}
					}
				}
				catch
				{
				}
			};
		});
		DataRow? GetRow(int resid)
		{
			if (dt == null)
			{
				return null;
			}
			foreach (DataRow row4 in dt.Rows)
			{
				if (Convert.ToInt32(row4["ReservationID"]) == resid)
				{
					return row4;
				}
			}
			return null;
		}
	}

	private void PageRoomReport(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.AutoScroll = true;
			body.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			body.Padding = new Padding(25);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 70,
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(panel);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83c\udfe8 Konaklama ve Oda Gelir Raporu",
				Font = new Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(0, 0),
				AutoSize = true
			};
			panel.Controls.Add(value);
			System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
			{
				Text = "Tesisinizin doluluk ve kazanç verilerini detaylı analiz edin.",
				Font = new Font("Segoe UI", 9f),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Location = new Point(5, 38),
				AutoSize = true
			};
			panel.Controls.Add(value2);
			FlowLayoutPanel pnlStats = new FlowLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = 95,
				WrapContents = false,
				AutoScroll = true,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(0, 10, 0, 0)
			};
			body.Controls.Add(pnlStats);
			AddStatCard("TOPLAM ODA", "\ud83c\udfe8", System.Drawing.Color.FromArgb(99, 102, 241), out var lTotal);
			AddStatCard("DOLU / BOŞ", "\ud83d\udeaa", System.Drawing.Color.FromArgb(239, 68, 68), out var lOcc);
			System.Windows.Forms.Label label = new System.Windows.Forms.Label();
			AddStatCard("TOPLAM GECE", "\ud83c\udf19", System.Drawing.Color.FromArgb(245, 158, 11), out var lNights);
			AddStatCard("TOPLAM GELİR", "\ud83d\udcb0", System.Drawing.Color.FromArgb(16, 185, 129), out var lRev);
			AddStatCard("ORT. GÜNLÜK", "\ud83d\udcc8", System.Drawing.Color.FromArgb(14, 165, 233), out var lAvg);
			AddStatCard("DOLULUK %", "\ud83d\udcca", System.Drawing.Color.FromArgb(139, 92, 246), out var lRate);
			RoundedPanel pnlFilters = new RoundedPanel
			{
				Dock = DockStyle.Top,
				Height = 95,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Margin = new Padding(0, 20, 0, 20)
			};
			body.Controls.Add(pnlFilters);
			pnlFilters.Padding = new Padding(20, 12, 20, 12);
			pnlFilters.BringToFront();
			AddL("BAŞLANGIÇ", 20, 12);
			DateTimePicker dtpS = new DateTimePicker
			{
				Location = new Point(20, 32),
				Width = 110,
				Format = DateTimePickerFormat.Short,
				Font = new Font("Segoe UI", 9f)
			};
			dtpS.Value = DateTime.Today.AddDays(-7.0);
			pnlFilters.Controls.Add(dtpS);
			AddL("BİTİŞ", 140, 12);
			DateTimePicker dtpE = new DateTimePicker
			{
				Location = new Point(140, 32),
				Width = 110,
				Format = DateTimePickerFormat.Short,
				Font = new Font("Segoe UI", 9f)
			};
			pnlFilters.Controls.Add(dtpE);
			AddL("ODA TİPİ", 260, 12);
			ComboBox cmbType = new ComboBox
			{
				Location = new Point(260, 32),
				Width = 110,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			cmbType.Items.Add("Hepsi");
			DataTable roomTypes = DataAccess.GetRoomTypes();
			foreach (DataRow row in roomTypes.Rows)
			{
				cmbType.Items.Add(new
				{
					ID = row["RoomTypeID"],
					Name = row["TypeName"].ToString()
				});
			}
			cmbType.DisplayMember = "Name";
			cmbType.ValueMember = "ID";
			cmbType.SelectedIndex = 0;
			pnlFilters.Controls.Add(cmbType);
			AddL("KAT", 380, 12);
			ComboBox cmbFloor = new ComboBox
			{
				Location = new Point(380, 32),
				Width = 80,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			cmbFloor.Items.Add("Hepsi");
			DataTable floors = DataAccess.GetFloors();
			foreach (DataRow row2 in floors.Rows)
			{
				cmbFloor.Items.Add(new
				{
					ID = row2["FloorID"],
					Name = row2["FloorNumber"].ToString() + ". Kat"
				});
			}
			cmbFloor.DisplayMember = "Name";
			cmbFloor.ValueMember = "ID";
			cmbFloor.SelectedIndex = 0;
			pnlFilters.Controls.Add(cmbFloor);
			AddL("DURUM", 470, 12);
			ComboBox cmbSt = new ComboBox
			{
				Location = new Point(470, 32),
				Width = 80,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			ComboBox.ObjectCollection items = cmbSt.Items;
			object[] items2 = new string[3] { "Hepsi", "Dolu", "Boş" };
			items.AddRange(items2);
			cmbSt.SelectedIndex = 0;
			pnlFilters.Controls.Add(cmbSt);
			AddL("ODA SEÇ", 560, 12);
			ComboBox cmbRoomSel = new ComboBox
			{
				Location = new Point(560, 32),
				Width = 80,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9f)
			};
			cmbRoomSel.Items.Add("Hepsi");
			DataTable allRooms = DataAccess.GetAllRooms();
			foreach (DataRow row3 in allRooms.Rows)
			{
				cmbRoomSel.Items.Add(row3["RoomNumber"].ToString());
			}
			cmbRoomSel.SelectedIndex = 0;
			pnlFilters.Controls.Add(cmbRoomSel);
			Button btnQuery = new Button
			{
				Text = "\ud83d\udd0d SORGULA",
				Location = new Point(pnlFilters.Width - 270, 22),
				Size = new System.Drawing.Size(120, 42),
				BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			btnQuery.FlatAppearance.BorderSize = 0;
			pnlFilters.Controls.Add(btnQuery);
			Button button = new Button
			{
				Text = "\ud83d\udda8\ufe0f PDF",
				Location = new Point(pnlFilters.Width - 140, 22),
				Size = new System.Drawing.Size(100, 42),
				BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			button.FlatAppearance.BorderSize = 0;
			pnlFilters.Controls.Add(button);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(15)
			};
			body.Controls.Add(roundedPanel);
			roundedPanel.BringToFront();
			DataGridView dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowTemplate = 
				{
					Height = 40
				},
				GridColor = System.Drawing.Color.FromArgb(241, 245, 249),
				AlternatingRowsDefaultCellStyle = 
				{
					BackColor = System.Drawing.Color.FromArgb(252, 253, 254)
				}
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
			dgv.ColumnHeadersHeight = 45;
			dgv.EnableHeadersVisualStyles = false;
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(238, 242, 255);
			dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(79, 70, 229);
			roundedPanel.Controls.Add(dgv);
			btnQuery.Click += delegate
			{
				LoadData();
			};
			button.Click += delegate
			{
				using SaveFileDialog saveFileDialog = new SaveFileDialog
				{
					Filter = "PDF|*.pdf",
					FileName = "Oda_Raporu.pdf"
				};
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						int roomTypeId = ((cmbType.SelectedIndex > 0) ? ((int)((dynamic)cmbType.SelectedItem).ID) : 0);
						int floorId = ((cmbFloor.SelectedIndex > 0) ? ((int)((dynamic)cmbFloor.SelectedItem).ID) : 0);
						string status = cmbSt.SelectedItem.ToString();
						string roomNumber = cmbRoomSel.SelectedItem?.ToString() ?? "Hepsi";
						dynamic advancedRoomReportStats = EnterpriseDataAccess.GetAdvancedRoomReportStats(dtpS.Value, dtpE.Value, roomTypeId, floorId, status, roomNumber);
						DataTable advancedRoomReport = EnterpriseDataAccess.GetAdvancedRoomReport(dtpS.Value, dtpE.Value, roomTypeId, floorId, status, roomNumber);
						ReportService.GenerateRoomReportPdf(dtpS.Value, dtpE.Value, advancedRoomReport, advancedRoomReportStats, saveFileDialog.FileName);
						MessageBox.Show("Rapor PDF olarak kaydedildi.");
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Hata: " + ex.Message);
						return;
					}
				}
			};
			LoadData();
			void AddL(string t, int x, int y)
			{
				pnlFilters.Controls.Add(new System.Windows.Forms.Label
				{
					Text = t,
					Font = new Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
					Location = new Point(x, y),
					AutoSize = true
				});
			}
			void AddStatCard(string title, string icon, System.Drawing.Color accent, out System.Windows.Forms.Label val)
			{
				RoundedPanel roundedPanel2 = new RoundedPanel
				{
					Size = new System.Drawing.Size(160, 75),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12,
					Margin = new Padding(0, 0, 15, 0)
				};
				Panel value3 = new Panel
				{
					Dock = DockStyle.Left,
					Width = 4,
					BackColor = accent
				};
				roundedPanel2.Controls.Add(value3);
				roundedPanel2.Controls.Add(new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
					Location = new Point(12, 12),
					AutoSize = true
				});
				val = new System.Windows.Forms.Label
				{
					Text = "0",
					Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
					Location = new Point(10, 32),
					AutoSize = true
				};
				roundedPanel2.Controls.Add(val);
				System.Windows.Forms.Label value4 = new System.Windows.Forms.Label
				{
					Text = icon,
					Font = new Font("Segoe UI", 12f),
					Location = new Point(roundedPanel2.Width - 32, 10),
					AutoSize = true,
					ForeColor = System.Drawing.Color.FromArgb(30, accent)
				};
				roundedPanel2.Controls.Add(value4);
				pnlStats.Controls.Add(roundedPanel2);
			}
			async void LoadData()
			{
				btnQuery.Enabled = false;
				int tid = ((cmbType.SelectedIndex > 0) ? ((int)((dynamic)cmbType.SelectedItem).ID) : 0);
				int fid = ((cmbFloor.SelectedIndex > 0) ? ((int)((dynamic)cmbFloor.SelectedItem).ID) : 0);
				string st = cmbSt.SelectedItem.ToString();
				string rn = cmbRoomSel.SelectedItem?.ToString() ?? "Hepsi";
				dynamic stats = await Task.Run(() => EnterpriseDataAccess.GetAdvancedRoomReportStats(dtpS.Value, dtpE.Value, tid, fid, st, rn));
				DataTable data = await Task.Run(() => EnterpriseDataAccess.GetAdvancedRoomReport(dtpS.Value, dtpE.Value, tid, fid, st, rn));
				SafeInvoke(delegate
				{
					lTotal.Text = stats.TotalRooms.ToString();
					lOcc.Text = $"{(object?)stats.OccupiedRooms} / {(object?)stats.AvailableRooms}";
					lNights.Text = stats.TotalNights.ToString();
					lRev.Text = stats.TotalRevenue.ToString("N2") + " ₺";
					lAvg.Text = stats.AvgDailyRevenue.ToString("N2") + " ₺";
					lRate.Text = stats.OccupancyRate.ToString("N1") + " %";
					dgv.DataSource = data;
					btnQuery.Enabled = true;
				});
			}
		});
	}

	private void PageRestaurantReport(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			body.Padding = new Padding(25);
			System.Windows.Forms.Label lblTitle = new System.Windows.Forms.Label
			{
				Text = "\ud83c\udf7d\ufe0f Restoran ve Satış Analiz Raporu",
				Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(0, 0),
				AutoSize = true
			};
			body.Controls.Add(lblTitle);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Dock = DockStyle.Top,
				Height = 90,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Margin = new Padding(0, 50, 0, 20),
				Padding = new Padding(20, 15, 20, 15)
			};
			body.Controls.Add(roundedPanel);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "BAŞLANGIÇ",
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(20, 15),
				AutoSize = true
			});
			DateTimePicker dtpS = new DateTimePicker
			{
				Location = new Point(20, 38),
				Width = 150
			};
			dtpS.Value = DateTime.Today.AddDays(-7.0);
			roundedPanel.Controls.Add(dtpS);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "BİTİŞ",
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(190, 15),
				AutoSize = true
			});
			DateTimePicker dtpE = new DateTimePicker
			{
				Location = new Point(190, 38),
				Width = 150
			};
			roundedPanel.Controls.Add(dtpE);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "ÜRÜN SEÇİMİ",
				Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(350, 15),
				AutoSize = true
			});
			ComboBox cmbProd = new ComboBox
			{
				Location = new Point(350, 38),
				Width = 180,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 10f)
			};
			cmbProd.Items.Add("Hepsi");
			try
			{
				DataTable allProducts = DataAccess.GetAllProducts();
				foreach (DataRow row in allProducts.Rows)
				{
					cmbProd.Items.Add(new
					{
						ID = row["ProductID"],
						Name = row["ItemName"].ToString()
					});
				}
				cmbProd.DisplayMember = "Name";
				cmbProd.ValueMember = "ID";
			}
			catch
			{
			}
			cmbProd.SelectedIndex = 0;
			roundedPanel.Controls.Add(cmbProd);
			Button btnQuery = new Button
			{
				Text = "\ud83d\udd0d ANALİZ ET",
				Location = new Point(540, 25),
				Size = new System.Drawing.Size(160, 45),
				BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			roundedPanel.Controls.Add(btnQuery);
			Button button = new Button
			{
				Text = "\ud83d\udda8\ufe0f PDF",
				Location = new Point(710, 25),
				Size = new System.Drawing.Size(100, 45),
				BackColor = successGreen,
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			roundedPanel.Controls.Add(button);
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(20)
			};
			body.Controls.Add(roundedPanel2);
			roundedPanel2.BringToFront();
			DataGridView dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowTemplate = 
				{
					Height = 40
				},
				Font = new Font("Segoe UI", 10f)
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
			dgv.EnableHeadersVisualStyles = false;
			roundedPanel2.Controls.Add(dgv);
			btnQuery.Click += delegate
			{
				LoadData();
			};
			button.Click += delegate
			{
				using SaveFileDialog saveFileDialog = new SaveFileDialog
				{
					Filter = "PDF|*.pdf",
					FileName = "Restoran_Raporu.pdf"
				};
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						int productId = 0;
						if (cmbProd.SelectedIndex > 0)
						{
							object selectedItem = cmbProd.SelectedItem;
							PropertyInfo propertyInfo = selectedItem?.GetType().GetProperty("ID");
							if (propertyInfo != null)
							{
								productId = (int)propertyInfo.GetValue(selectedItem);
							}
						}
						DataTable restaurantProductReport = EnterpriseDataAccess.GetRestaurantProductReport(dtpS.Value, dtpE.Value, productId);
						ReportService.GenerateRestaurantReportPdf(dtpS.Value, dtpE.Value, restaurantProductReport, saveFileDialog.FileName, cmbProd.Text);
						MessageBox.Show("Rapor PDF olarak kaydedildi.");
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Hata: " + ex.Message);
						return;
					}
				}
			};
			LoadData();
			async void LoadData()
			{
				btnQuery.Enabled = false;
				int prodId = 0;
				if (cmbProd.SelectedIndex > 0)
				{
					object item = cmbProd.SelectedItem;
					PropertyInfo prop = item?.GetType().GetProperty("ID");
					if (prop != null)
					{
						prodId = (int)prop.GetValue(item);
					}
				}
				DataTable dt = await Task.Run(() => EnterpriseDataAccess.GetRestaurantProductReport(dtpS.Value, dtpE.Value, prodId));
				SafeInvoke(delegate
				{
					dgv.DataSource = dt;
					btnQuery.Enabled = true;
					decimal value = default(decimal);
					foreach (DataRow row2 in dt.Rows)
					{
						value += Convert.ToDecimal(row2["Toplam"]);
					}
					lblTitle.Text = $"\ud83c\udf7d\ufe0f Restoran Analiz Raporu - Toplam Ciro: {value:N2} ₺";
				});
			}
		});
	}

	private string ShowRoomSelectionDialog()
	{
		string selectedRoom = "";
		Form f = new Form
		{
			Text = "\ud83c\udfe8 Uygun Oda Seçimi",
			Size = new System.Drawing.Size(550, 600),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MinimizeBox = false,
			MaximizeBox = false
		};
		System.Windows.Forms.Label value = new System.Windows.Forms.Label
		{
			Text = "\ud83c\udfe8 Müsait Odalar",
			Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
			Location = new Point(20, 20),
			AutoSize = true
		};
		f.Controls.Add(value);
		TextBox txtSearch = new TextBox
		{
			Name = "txtSearch",
			Location = new Point(20, 60),
			Size = new System.Drawing.Size(300, 30),
			Font = new Font("Segoe UI", 11f),
			PlaceholderText = "Oda ara..."
		};
		f.Controls.Add(txtSearch);
		DataTable dt = DataAccess.GetAvailableRooms();
		if (dt.Columns.Contains("RoomNumber"))
		{
			dt.Columns["RoomNumber"].ColumnName = "Oda No";
		}
		if (dt.Columns.Contains("FloorNumber"))
		{
			dt.Columns["FloorNumber"].ColumnName = "Kat";
		}
		if (dt.Columns.Contains("TypeName"))
		{
			dt.Columns["TypeName"].ColumnName = "Oda Tipi";
		}
		if (dt.Columns.Contains("Price"))
		{
			dt.Columns["Price"].ColumnName = "Birim Fiyat (₺)";
		}
		DataGridView dgv = new DataGridView
		{
			Location = new Point(20, 100),
			Size = new System.Drawing.Size(495, 380),
			BackgroundColor = System.Drawing.Color.White,
			BorderStyle = BorderStyle.FixedSingle,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			RowHeadersVisible = false,
			AllowUserToAddRows = false,
			ReadOnly = true,
			DataSource = dt,
			Font = new Font("Segoe UI", 9f),
			RowTemplate = 
			{
				Height = 40
			}
		};
		dgv.EnableHeadersVisualStyles = false;
		dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
		dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		f.Controls.Add(dgv);
		txtSearch.TextChanged += delegate
		{
			string arg = txtSearch.Text.Trim().Replace("'", "''");
			dt.DefaultView.RowFilter = string.Format("[Oda No] LIKE '%{0}%' OR [Oda Tipi] LIKE '%{0}%'", arg);
		};
		Button btnSelect = new Button
		{
			Text = "✅ SEÇİLENİ ONAYLA",
			Location = new Point(160, 500),
			Size = new System.Drawing.Size(220, 45),
			BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		btnSelect.FlatAppearance.BorderSize = 0;
		btnSelect.Click += delegate
		{
			if (dgv.SelectedRows.Count > 0)
			{
				selectedRoom = dgv.SelectedRows[0].Cells["Oda No"].Value?.ToString() ?? "";
				f.DialogResult = DialogResult.OK;
				f.Close();
			}
			else
			{
				MessageBox.Show("Lütfen bir oda seçin.");
			}
		};
		dgv.CellDoubleClick += delegate
		{
			btnSelect.PerformClick();
		};
		f.Controls.Add(btnSelect);
		f.ShowDialog();
		return selectedRoom;
	}

	private bool IsValidTc(string tc)
	{
		if (string.IsNullOrWhiteSpace(tc))
		{
			return false;
		}
		if (!tc.All(char.IsDigit))
		{
			return false;
		}
		if (tc.Length != 11)
		{
			return false;
		}
		return true;
	}

	private string GetIcon(string name)
	{
		name = name.ToLower();
		if (name.Contains("çay"))
		{
			return "☕";
		}
		if (name.Contains("kahve"))
		{
			return "☕";
		}
		if (name.Contains("su"))
		{
			return "\ud83d\udca7";
		}
		if (name.Contains("cola") || name.Contains("pepsi"))
		{
			return "\ud83e\udd64";
		}
		if (name.Contains("fanta") || name.Contains("yedigün"))
		{
			return "\ud83c\udf4a";
		}
		if (name.Contains("çorba"))
		{
			return "\ud83e\udd63";
		}
		if (name.Contains("kebap") || name.Contains("et"))
		{
			return "\ud83c\udf56";
		}
		if (name.Contains("pide") || name.Contains("lahmacun"))
		{
			return "\ud83c\udf55";
		}
		if (name.Contains("tost") || name.Contains("sandviç"))
		{
			return "\ud83e\udd6a";
		}
		return "\ud83d\udce6";
	}

	private void PageExpenses(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcb0 Finans ve Muhasebe Yönetimi",
				Font = new Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(25, 20),
				AutoSize = true
			};
			body.Controls.Add(value);
			FlowLayoutPanel pnlStats = new FlowLayoutPanel
			{
				Location = new Point(25, 65),
				Size = new System.Drawing.Size(body.Width - 50, 110),
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
				BackColor = System.Drawing.Color.Transparent
			};
			body.Controls.Add(pnlStats);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(25, 185),
				Size = new System.Drawing.Size(body.Width - 50, 70),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 12,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			body.Controls.Add(roundedPanel);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcc5 Filtre:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
				Location = new Point(15, 25),
				AutoSize = true
			});
			DateTimePicker dtStart = new DateTimePicker
			{
				Location = new Point(75, 22),
				Width = 120,
				Value = DateTime.Today,
				Font = new Font("Segoe UI", 10f)
			};
			DateTimePicker dtEnd = new DateTimePicker
			{
				Location = new Point(205, 22),
				Width = 120,
				Value = DateTime.Today,
				Font = new Font("Segoe UI", 10f)
			};
			roundedPanel.Controls.Add(dtStart);
			roundedPanel.Controls.Add(dtEnd);
			Button button = new Button
			{
				Text = "➕ YENİ GİDER EKLE",
				Size = new System.Drawing.Size(180, 42),
				Location = new Point(roundedPanel.Width - 390, 14),
				BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			button.FlatAppearance.BorderSize = 0;
			roundedPanel.Controls.Add(button);
			Button button2 = new Button
			{
				Text = "\ud83d\udcca MALİ ÖZET",
				Size = new System.Drawing.Size(160, 42),
				Location = new Point(roundedPanel.Width - 200, 14),
				BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			button2.FlatAppearance.BorderSize = 0;
			roundedPanel.Controls.Add(button2);
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Location = new Point(25, 270),
				Size = new System.Drawing.Size(body.Width - 50, body.Height - 290),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			roundedPanel2.Padding = new Padding(15);
			body.Controls.Add(roundedPanel2);
			roundedPanel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcdd Finansal Hareketler (Gelir, Gider ve Alımlar)",
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(31, 41, 55),
				Location = new Point(15, 15),
				AutoSize = true
			});
			DataGridView dgv = new DataGridView
			{
				Location = new Point(15, 45),
				Size = new System.Drawing.Size(roundedPanel2.Width - 30, roundedPanel2.Height - 60),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 40
				},
				Font = new Font("Segoe UI", 9.5f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			dgv.GridColor = System.Drawing.Color.FromArgb(243, 244, 246);
			roundedPanel2.Controls.Add(dgv);
			dgv.CellFormatting += delegate(object? s, DataGridViewCellFormattingEventArgs e)
			{
				if (dgv.Columns[e.ColumnIndex].Name == "Tutar" && e.Value != null)
				{
					decimal num = Convert.ToDecimal(e.Value);
					e.CellStyle.ForeColor = ((num >= 0m) ? System.Drawing.Color.FromArgb(16, 185, 129) : System.Drawing.Color.FromArgb(239, 68, 68));
					e.CellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
					if (num < 0m)
					{
						e.Value = Math.Abs(num).ToString("N2") + " ₺ (Çıkış)";
					}
					else
					{
						e.Value = num.ToString("N2") + " ₺ (Giriş)";
					}
				}
				if (dgv.Columns[e.ColumnIndex].Name == "Tip" && e.Value != null)
				{
					e.CellStyle.ForeColor = ((e.Value.ToString() == "GELİR") ? System.Drawing.Color.Green : System.Drawing.Color.Red);
				}
			};
			dtStart.ValueChanged += delegate
			{
				RefreshFinance();
			};
			dtEnd.ValueChanged += delegate
			{
				RefreshFinance();
			};
			RefreshFinance();
			button2.Click += delegate
			{
				Form form = new Form
				{
					Text = "Mali Özet Tablosu",
					Size = new System.Drawing.Size(400, 350),
					StartPosition = FormStartPosition.CenterParent,
					BackColor = System.Drawing.Color.White,
					FormBorderStyle = FormBorderStyle.FixedDialog
				};
				DataGridView dataGridView = new DataGridView
				{
					Dock = DockStyle.Fill,
					BackgroundColor = System.Drawing.Color.White,
					BorderStyle = BorderStyle.None,
					RowHeadersVisible = false,
					AllowUserToAddRows = false,
					ReadOnly = true,
					AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
				};
				form.Controls.Add(dataGridView);
				dataGridView.DataSource = EnterpriseDataAccess.GetFinanceSummary(dtStart.Value, dtEnd.Value);
				form.ShowDialog();
			};
			button.Click += delegate
			{
				Form f = new Form
				{
					Text = "\ud83d\udcb5 Yeni Gider Girişi",
					Size = new System.Drawing.Size(420, 520),
					StartPosition = FormStartPosition.CenterParent,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					BackColor = System.Drawing.Color.White
				};
				int num = 20;
				f.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Gider Başlığı:",
					Location = new Point(25, num),
					AutoSize = true,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
				});
				TextBox txtTitle = new TextBox
				{
					Location = new Point(25, num + 22),
					Width = 350,
					Font = new Font("Segoe UI", 10f)
				};
				f.Controls.Add(txtTitle);
				num += 60;
				f.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Kategori Seçin:",
					Location = new Point(25, num),
					AutoSize = true,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
				});
				ComboBox cmbCat = new ComboBox
				{
					Location = new Point(25, num + 22),
					Width = 350,
					Font = new Font("Segoe UI", 10f),
					DropDownStyle = ComboBoxStyle.DropDownList
				};
				ComboBox.ObjectCollection items = cmbCat.Items;
				object[] items2 = new string[8] { "Mutfak / Gıda", "Personel Maaş", "Elektrik / Su / Gaz", "Temizlik Malzemesi", "Tamirat / Bakım", "Kırtasiye", "Reklam / Pazarlama", "Diğer" };
				items.AddRange(items2);
				cmbCat.SelectedIndex = 0;
				f.Controls.Add(cmbCat);
				num += 60;
				f.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Tutar (₺):",
					Location = new Point(25, num),
					AutoSize = true,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
				});
				NumericUpDown numAmt = new NumericUpDown
				{
					Location = new Point(25, num + 22),
					Width = 150,
					Font = new Font("Segoe UI", 11f),
					DecimalPlaces = 2,
					Maximum = 1000000m
				};
				f.Controls.Add(numAmt);
				f.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Ödeyen / Kasa:",
					Location = new Point(200, num),
					AutoSize = true,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
				});
				TextBox txtBy = new TextBox
				{
					Location = new Point(200, num + 22),
					Width = 175,
					Font = new Font("Segoe UI", 10f),
					Text = "Ana Kasa"
				};
				f.Controls.Add(txtBy);
				num += 60;
				f.Controls.Add(new System.Windows.Forms.Label
				{
					Text = "Detaylı Açıklama (Opsiyonel):",
					Location = new Point(25, num),
					AutoSize = true,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
				});
				TextBox txtDesc = new TextBox
				{
					Location = new Point(25, num + 22),
					Width = 350,
					Height = 80,
					Multiline = true,
					Font = new Font("Segoe UI", 10f)
				};
				f.Controls.Add(txtDesc);
				num += 100;
				Button button3 = new Button
				{
					Text = "GİDERİ KAYDET",
					Location = new Point(25, num + 20),
					Size = new System.Drawing.Size(350, 48),
					BackColor = System.Drawing.Color.FromArgb(239, 68, 68),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				button3.FlatAppearance.BorderSize = 0;
				f.Controls.Add(button3);
				button3.Click += delegate
				{
					if (numAmt.Value <= 0m)
					{
						MessageBox.Show("Lütfen tutar giriniz!");
					}
					else
					{
						DataAccess.AddExpense(txtTitle.Text, cmbCat.SelectedItem.ToString(), numAmt.Value, txtDesc.Text, txtBy.Text);
						f.Close();
						RefreshFinance();
						MessageBox.Show("Gider başarıyla kaydedildi.", "Başarılı");
					}
				};
				f.ShowDialog();
			};
			void AddFinanceCard(string title, string val, System.Drawing.Color c, string icon)
			{
				RoundedPanel roundedPanel3 = new RoundedPanel
				{
					Size = new System.Drawing.Size(240, 95),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12,
					Margin = new Padding(0, 0, 15, 0)
				};
				roundedPanel3.Controls.Add(new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
					Location = new Point(15, 15),
					AutoSize = true
				});
				roundedPanel3.Controls.Add(new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
					Location = new Point(15, 40),
					AutoSize = true
				});
				roundedPanel3.Controls.Add(new System.Windows.Forms.Label
				{
					Text = icon,
					Font = new Font("Segoe UI", 18f),
					ForeColor = System.Drawing.Color.FromArgb(40, c),
					Location = new Point(roundedPanel3.Width - 45, 15),
					AutoSize = true
				});
				pnlStats.Controls.Add(roundedPanel3);
			}
			void RefreshFinance()
			{
				DataTable combinedTransactions = EnterpriseDataAccess.GetCombinedTransactions(dtStart.Value, dtEnd.Value);
				dgv.DataSource = combinedTransactions;
				decimal num = default(decimal);
				decimal num2 = default(decimal);
				foreach (DataRow row in combinedTransactions.Rows)
				{
					decimal num3 = Convert.ToDecimal(row["Tutar"]);
					if (num3 > 0m)
					{
						num += num3;
					}
					else
					{
						num2 += Math.Abs(num3);
					}
				}
				pnlStats.Controls.Clear();
				AddFinanceCard("TOPLAM GELİR", $"{num:N2} ₺", System.Drawing.Color.Green, "\ud83d\udcc8");
				AddFinanceCard("TOPLAM GİDER", $"{num2:N2} ₺", System.Drawing.Color.Red, "\ud83d\udcc9");
				AddFinanceCard("NET DURUM", $"{num - num2:N2} ₺", (num >= num2) ? System.Drawing.Color.Blue : System.Drawing.Color.Orange, "⚖\ufe0f");
				AddFinanceCard("İŞLEM SAYISI", $"{combinedTransactions.Rows.Count} Adet", System.Drawing.Color.Gray, "\ud83d\udcc4");
			}
		});
	}

	private void PageKitchen(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 70,
				Padding = new Padding(20, 15, 20, 10),
				BackColor = System.Drawing.Color.White
			};
			body.Controls.Add(panel);
			Button btnCardView = new Button
			{
				Text = "\ud83c\udfb4 KART GÖRÜNÜMÜ",
				Size = new System.Drawing.Size(180, 42),
				BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnCardView.FlatAppearance.BorderSize = 0;
			Button btnListView = new Button
			{
				Text = "\ud83d\udccb TÜM SİPARİŞ LİSTESİ",
				Size = new System.Drawing.Size(200, 42),
				Location = new Point(210, 15),
				BackColor = System.Drawing.Color.White,
				ForeColor = System.Drawing.Color.Gray,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnListView.FlatAppearance.BorderSize = 1;
			btnListView.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
			panel.Controls.AddRange(new Control[2] { btnCardView, btnListView });
			FlowLayoutPanel flowOrders = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(25),
				AutoScroll = true,
				BackColor = System.Drawing.Color.Transparent
			};
			Panel pnlList = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(25),
				Visible = false
			};
			body.Controls.Add(flowOrders);
			body.Controls.Add(pnlList);
			DataGridView dgvList = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowTemplate = 
				{
					Height = 45
				},
				Font = new Font("Segoe UI", 10f),
				EnableHeadersVisualStyles = false
			};
			dgvList.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgvList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
			pnlList.Controls.Add(dgvList);
			btnCardView.Click += delegate
			{
				flowOrders.Visible = true;
				pnlList.Visible = false;
				flowOrders.BringToFront();
				btnCardView.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
				btnCardView.ForeColor = System.Drawing.Color.White;
				btnListView.BackColor = System.Drawing.Color.White;
				btnListView.ForeColor = System.Drawing.Color.Gray;
			};
			btnListView.Click += delegate
			{
				pnlList.Visible = true;
				flowOrders.Visible = false;
				pnlList.BringToFront();
				btnListView.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
				btnListView.ForeColor = System.Drawing.Color.White;
				btnCardView.BackColor = System.Drawing.Color.White;
				btnCardView.ForeColor = System.Drawing.Color.Gray;
				dgvList.DataSource = DataAccess.GetKitchenOrders();
			};
			RenderOrders();
			if (_kitchenTimer != null)
			{
				_kitchenTimer.Stop();
				_kitchenTimer.Dispose();
			}
			_kitchenTimer = new System.Windows.Forms.Timer
			{
				Interval = 10000
			};
			_kitchenTimer.Tick += delegate
			{
				if (body.IsDisposed || !body.Visible)
				{
					_kitchenTimer.Stop();
				}
				else
				{
					RenderOrders();
				}
			};
			_kitchenTimer.Start();
			void RenderOrders()
			{
				DataTable kitchenOrders = DataAccess.GetKitchenOrders();
				flowOrders.SuspendLayout();
				flowOrders.Controls.Clear();
				if (kitchenOrders.Rows.Count == 0)
				{
					System.Windows.Forms.Label value = new System.Windows.Forms.Label
					{
						Text = "Sipariş kuyruğu yok. Mutfak sakin!",
						Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Italic),
						ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
						AutoSize = true,
						Location = new Point(50, 50)
					};
					flowOrders.Controls.Add(value);
				}
				IEnumerable<IGrouping<string, DataRow>> enumerable = from r in kitchenOrders.AsEnumerable()
					group r by r.Field<string>("RoomInfo") ?? "Hızlı Satış";
				foreach (IGrouping<string, DataRow> item in enumerable)
				{
					string key = item.Key;
					List<DataRow> itemsInGroup = item.ToList();
					DataRow dataRow = itemsInGroup.First();
					DateTime dateTime = Convert.ToDateTime(dataRow["SaleDate"]);
					TimeSpan timeSpan = DateTime.Now - dateTime;
					string text = "";
					string text2 = "";
					if (string.IsNullOrWhiteSpace(key) || key == "Hızlı Satış")
					{
						text = "\ud83d\udccd HIZLI SATIŞ";
					}
					else if (key.Contains(" | "))
					{
						string[] array = key.Split(new string[1] { " | " }, StringSplitOptions.None);
						text = "\ud83d\udccd " + array[0];
						text2 = array[1];
					}
					else
					{
						text = (key.Contains("Masa") ? "\ud83d\udccd " : "\ud83c\udfe0 ") + key;
					}
					int height = 150 + itemsInGroup.Count * 45;
					RoundedPanel card = new RoundedPanel
					{
						Size = new System.Drawing.Size(350, height),
						BackColor = System.Drawing.Color.White,
						BorderRadius = 15,
						Margin = new Padding(0, 0, 20, 20)
					};
					card.Paint += delegate(object? s, PaintEventArgs e)
					{
						using Pen pen = new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f);
						e.Graphics.DrawPath(pen, CustGetPath(card.ClientRectangle, 15));
						using SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(15, 23, 42));
						e.Graphics.FillRectangle(brush, 0, 0, 12, card.Height);
					};
					System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
					{
						Text = text.ToUpper(),
						Font = new Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
						Location = new Point(28, 20),
						AutoSize = true
					};
					card.Controls.Add(value2);
					if (!string.IsNullOrEmpty(text2))
					{
						System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
						{
							Text = text2,
							Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Italic),
							ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
							Location = new Point(28, 45),
							AutoSize = true
						};
						card.Controls.Add(value3);
					}
					string text3 = ((timeSpan.TotalMinutes < 1.0) ? "Az önce" : $"{(int)timeSpan.TotalMinutes} dk önce");
					System.Windows.Forms.Label value4 = new System.Windows.Forms.Label
					{
						Text = text3,
						Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
						ForeColor = System.Drawing.Color.FromArgb(239, 68, 68),
						Location = new Point(card.Width - 110, 22),
						Size = new System.Drawing.Size(100, 20),
						TextAlign = ContentAlignment.TopRight
					};
					card.Controls.Add(value4);
					int num = 80;
					foreach (DataRow item2 in itemsInGroup)
					{
						string text4 = item2["ItemName"].ToString() ?? "";
						int value5 = Convert.ToInt32(item2["Quantity"]);
						System.Windows.Forms.Label value6 = new System.Windows.Forms.Label
						{
							Text = GetIcon(text4),
							Font = new Font("Segoe UI", 16f),
							Location = new Point(30, num),
							AutoSize = true
						};
						System.Windows.Forms.Label value7 = new System.Windows.Forms.Label
						{
							Text = $"{value5} x {text4}",
							Font = new Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold),
							ForeColor = System.Drawing.Color.FromArgb(51, 65, 85),
							Location = new Point(70, num + 5),
							AutoSize = true
						};
						card.Controls.Add(value6);
						card.Controls.Add(value7);
						num += 45;
					}
					Button button = new Button
					{
						Text = "✅ SİPARİŞİ TAMAMLA VE GÖNDER",
						Size = new System.Drawing.Size(card.Width - 60, 50),
						Location = new Point(30, card.Height - 70),
						BackColor = System.Drawing.Color.FromArgb(34, 197, 94),
						ForeColor = System.Drawing.Color.White,
						FlatStyle = FlatStyle.Flat,
						Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
						Cursor = Cursors.Hand
					};
					button.FlatAppearance.BorderSize = 0;
					button.Click += delegate
					{
						foreach (DataRow item3 in itemsInGroup)
						{
							DataAccess.UpdateSaleStatus(Convert.ToInt32(item3["SaleID"]), "Served");
						}
						RenderOrders();
					};
					card.Controls.Add(button);
					flowOrders.Controls.Add(card);
				}
				flowOrders.ResumeLayout();
			}
		});
	}

	private void PageCalendar(Panel body)
	{
		DataTable dtRooms = DataAccess.GetRooms();
		DataTable dtRes = DataAccess.GetReservations();
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				BackColor = System.Drawing.Color.Transparent,
				Padding = new Padding(25, 10, 25, 0)
			};
			body.Controls.Add(panel);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83d\uddd3\ufe0f GÖRSEL REZERVASYON TAKVİMİ",
				Font = new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Dock = DockStyle.Left,
				TextAlign = ContentAlignment.MiddleLeft,
				AutoSize = true
			};
			panel.Controls.Add(value);
			System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udd35 Aktif  |  \ud83d\udd34 Uzun (7+ Gün)  |  ⚪ Boş",
				Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
				Dock = DockStyle.Right,
				TextAlign = ContentAlignment.MiddleRight,
				AutoSize = true
			};
			panel.Controls.Add(value2);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(25, 75),
				Size = new System.Drawing.Size(body.Width - 50, body.Height - 120),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			body.Controls.Add(roundedPanel);
			Panel pnlGantt = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				AutoScroll = true,
				Padding = new Padding(0)
			};
			roundedPanel.Controls.Add(pnlGantt);
			pnlGantt.Paint += delegate(object? s, PaintEventArgs e)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				int num = 110;
				int num2 = 75;
				int num3 = 42;
				int num4 = 45;
				DateTime dateTime = DateTime.Today.AddDays(-3.0);
				int num5 = 21;
				Region clip = e.Graphics.Clip;
				e.Graphics.SetClip(new System.Drawing.Rectangle(num, 0, pnlGantt.Width - num, pnlGantt.Height));
				for (int i = 0; i < num5; i++)
				{
					DateTime dateTime2 = dateTime.AddDays(i);
					int num6 = num + i * num2;
					if (dateTime2.Date == DateTime.Today)
					{
						using SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(15, 79, 102, 241));
						e.Graphics.FillRectangle(brush, num6, 0, num2, pnlGantt.Height);
					}
					e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(241, 245, 249), 1f), num6, 0, num6, pnlGantt.Height);
				}
				int num7 = num4;
				foreach (DataRow row in dtRooms.Rows)
				{
					string roomNum = row["RoomNumber"].ToString();
					e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(241, 245, 249), 1f), num, num7, pnlGantt.Width, num7);
					EnumerableRowCollection<DataRow> enumerableRowCollection = from x in dtRes.AsEnumerable()
						where x["Oda"].ToString() == roomNum && x["Status"].ToString() != "CheckedOut"
						select x;
					foreach (DataRow item in enumerableRowCollection)
					{
						DateTime dateTime3 = Convert.ToDateTime(item["Giris"]);
						DateTime dateTime4 = Convert.ToDateTime(item["Cikis"]);
						string text = item["Status"].ToString();
						if (dateTime4 >= dateTime && dateTime3 <= dateTime.AddDays(num5))
						{
							double totalDays = (dateTime3 - dateTime).TotalDays;
							double totalDays2 = (dateTime4 - dateTime3).TotalDays;
							float num8 = (float)((double)num + totalDays * (double)num2);
							float num9 = (float)(totalDays2 * (double)num2);
							RectangleF rect = new RectangleF(num8 + 4f, num7 + 8, num9 - 8f, num3 - 16);
							if (rect.Width < 5f)
							{
								rect.Width = 10f;
							}
							using (GraphicsPath path = CustGetPath(new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height), 6))
							{
								using LinearGradientBrush brush2 = new LinearGradientBrush(rect, System.Drawing.Color.FromArgb(99, 102, 241), System.Drawing.Color.FromArgb(79, 70, 229), 45f);
								e.Graphics.FillPath(brush2, path);
							}
							e.Graphics.DrawString(item["Musteri"].ToString(), new Font("Segoe UI Semibold", 7.5f), Brushes.White, rect.X + 8f, rect.Y + rect.Height / 2f - 7f);
						}
						if (text == "CheckedIn" && DateTime.Today > dateTime4)
						{
							double totalDays3 = (dateTime4 - dateTime).TotalDays;
							double totalDays4 = (DateTime.Today - dateTime4).TotalDays;
							if (totalDays4 > 0.0)
							{
								float num10 = (float)((double)num + totalDays3 * (double)num2);
								float num11 = (float)(totalDays4 * (double)num2);
								RectangleF rect2 = new RectangleF(num10 + 4f, num7 + 8, num11 - 8f, num3 - 16);
								using GraphicsPath path2 = CustGetPath(new System.Drawing.Rectangle((int)rect2.X, (int)rect2.Y, (int)rect2.Width, (int)rect2.Height), 6);
								using LinearGradientBrush brush3 = new LinearGradientBrush(rect2, System.Drawing.Color.FromArgb(239, 68, 68), System.Drawing.Color.FromArgb(185, 28, 28), 45f);
								e.Graphics.FillPath(brush3, path2);
							}
						}
					}
					num7 += num3;
				}
				int num12 = num + (int)((DateTime.Now - dateTime).TotalDays * (double)num2);
				using (Pen pen = new Pen(System.Drawing.Color.FromArgb(239, 68, 68), 2f))
				{
					pen.DashStyle = DashStyle.Dash;
					e.Graphics.DrawLine(pen, num12, 0, num12, pnlGantt.Height);
				}
				e.Graphics.Clip = clip;
				using (SolidBrush brush4 = new SolidBrush(System.Drawing.Color.White))
				{
					e.Graphics.FillRectangle(brush4, 0, 0, num, pnlGantt.Height);
					e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(249, 250, 251)), num, 0, pnlGantt.Width - num, num4);
				}
				e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f), 0, num4, pnlGantt.Width, num4);
				e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1f), num, 0, num, pnlGantt.Height);
				for (int num13 = 0; num13 < num5; num13++)
				{
					DateTime dateTime5 = dateTime.AddDays(num13);
					int num14 = num + num13 * num2;
					SolidBrush brush5 = ((dateTime5.Date == DateTime.Today) ? new SolidBrush(System.Drawing.Color.FromArgb(79, 102, 241)) : new SolidBrush(System.Drawing.Color.FromArgb(100, 116, 139)));
					e.Graphics.DrawString(dateTime5.ToString("ddd").ToUpper(), new Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold), brush5, num14 + 15, 8f);
					e.Graphics.DrawString(dateTime5.ToString("dd"), new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold), brush5, num14 + 15, 18f);
				}
				num7 = num4;
				foreach (DataRow row2 in dtRooms.Rows)
				{
					e.Graphics.DrawString(row2["RoomNumber"].ToString(), new Font("Segoe UI Semibold", 9f), new SolidBrush(System.Drawing.Color.FromArgb(30, 41, 59)), 15f, num7 + num3 / 2 - 8);
					e.Graphics.DrawLine(new Pen(System.Drawing.Color.FromArgb(241, 245, 249), 1f), 0, num7, num, num7);
					num7 += num3;
				}
				e.Graphics.FillEllipse(Brushes.Red, num12 - 4, num4 - 4, 8, 8);
			};
		});
	}

	private void ShowReceiptDialog(string guest, DataTable cart)
	{
		Form f = new Form
		{
			Text = "\ud83e\uddfe Adisyon Önizleme",
			Size = new System.Drawing.Size(380, 550),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.White,
			FormBorderStyle = FormBorderStyle.FixedDialog
		};
		Panel panel = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(25)
		};
		f.Controls.Add(panel);
		System.Windows.Forms.Label value = new System.Windows.Forms.Label
		{
			Text = "✨ SOM-PMS RESTORAN\n" + DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
			Font = new Font("Courier New", 10f, System.Drawing.FontStyle.Bold),
			Dock = DockStyle.Top,
			Height = 60,
			TextAlign = ContentAlignment.MiddleCenter
		};
		panel.Controls.Add(value);
		panel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "----------------------------",
			Dock = DockStyle.Top,
			AutoSize = true,
			TextAlign = ContentAlignment.MiddleCenter
		});
		panel.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "Müşteri: " + guest,
			Font = new Font("Courier New", 9f, System.Drawing.FontStyle.Bold),
			Dock = DockStyle.Top,
			Height = 35
		});
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false
		};
		decimal total = default(decimal);
		foreach (DataRow row in cart.Rows)
		{
			string text = row["Ürün"].ToString();
			int value2 = (int)row["Adet"];
			decimal num = (decimal)row["Toplam ₺"];
			total += num;
			flowLayoutPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = $"{text.PadRight(18)} {value2}x {num:N2}",
				Font = new Font("Courier New", 9f),
				Width = 300,
				AutoSize = true
			});
		}
		panel.Controls.Add(flowLayoutPanel);
		flowLayoutPanel.BringToFront();
		System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
		{
			Text = "----------------------------\nTOPLAM: " + total.ToString("N2") + " ₺",
			Font = new Font("Courier New", 12f, System.Drawing.FontStyle.Bold),
			Dock = DockStyle.Bottom,
			Height = 65,
			TextAlign = ContentAlignment.MiddleRight
		};
		panel.Controls.Add(value3);
		Button button = new Button
		{
			Text = "\ud83d\udda8\ufe0f FİŞİ KAYDET VE YAZDIR",
			Dock = DockStyle.Bottom,
			Height = 55,
			BackColor = System.Drawing.Color.FromArgb(15, 23, 42),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		f.Controls.Add(button);
		button.Click += async delegate
		{
			try
			{
				string receiptFolder = Path.Combine(Application.StartupPath, "Receipts");
				if (!Directory.Exists(receiptFolder))
				{
					Directory.CreateDirectory(receiptFolder);
				}
				string fileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
				string filePath = Path.Combine(receiptFolder, fileName);
				Settings.License = LicenseType.Community;
				Document document = Document.Create(delegate(IDocumentContainer container)
				{
					container.Page(delegate(PageDescriptor page)
					{
						page.Size(PageSizes.A6);
						page.Margin(1f, Unit.Centimetre);
						page.Header().Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text("SOM-PMS RESTORAN").FontSize(16f)
								.Bold()
								.AlignCenter();
							col.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9f)
								.AlignCenter();
							col.Item().PaddingTop(5f).LineHorizontal(1f);
						});
						page.Content().PaddingVertical(10f).Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text("Müşteri: " + guest).FontSize(10f)
								.Bold();
							col.Item().PaddingVertical(5f).LineHorizontal(0.5f);
							foreach (DataRow r in cart.Rows)
							{
								col.Item().Row(delegate(RowDescriptor row)
								{
									row.RelativeItem().Text(r["Ürün"].ToString());
									row.ConstantItem(30f).Text(r["Adet"].ToString() + "x").AlignCenter();
									row.ConstantItem(50f).Text(r["Toplam ₺"].ToString() + " ₺").AlignRight();
								});
							}
						});
						page.Footer().Column(delegate(ColumnDescriptor col)
						{
							col.Item().LineHorizontal(1f);
							col.Item().PaddingTop(5f).Row(delegate(RowDescriptor row)
							{
								row.RelativeItem().Text("TOPLAM").FontSize(12f)
									.Bold();
								row.RelativeItem().Text(total.ToString("N2") + " ₺").FontSize(12f)
									.Bold()
									.AlignRight();
							});
							col.Item().PaddingTop(10f).Text("Bizi tercih ettiğiniz için teşekkürler!")
								.FontSize(8f)
								.Italic()
								.AlignCenter();
						});
					});
				});
				document.GeneratePdf(filePath);
				MessageBox.Show("✅ Fiş başarıyla kaydedildi:\n" + filePath, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				Process.Start(new ProcessStartInfo(filePath)
				{
					UseShellExecute = true
				});
				f.Close();
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				MessageBox.Show("Fiş oluşturma hatası: " + ex2.Message);
			}
		};
		f.ShowDialog();
	}

	private void PageMaintenance(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = System.Drawing.Color.White,
				Padding = new Padding(20)
			};
			body.Controls.Add(panel);
			Button button = new Button
			{
				Text = "\ud83d\udee0\ufe0f YENİ ARIZA KAYDI",
				Size = new System.Drawing.Size(220, 48),
				Location = new Point(20, 25),
				BackColor = System.Drawing.Color.FromArgb(249, 115, 22),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			panel.Controls.Add(button);
			DataGridView dgv = new DataGridView
			{
				Location = new Point(25, 120),
				Size = new System.Drawing.Size(body.Width - 50, body.Height - 150),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 42
				},
				Font = new Font("Segoe UI", 9.5f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			body.Controls.Add(dgv);
			RefreshGrid();
			button.Click += delegate
			{
				Form f = new Form
				{
					Text = "⚠\ufe0f Teknik Arıza Bildirimi",
					Size = new System.Drawing.Size(420, 520),
					StartPosition = FormStartPosition.CenterScreen,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					BackColor = System.Drawing.Color.White
				};
				int ly = 25;
				ComboBox cmbRoomM = new ComboBox
				{
					DropDownStyle = ComboBoxStyle.DropDownList
				};
				DataTable dtR = DataAccess.GetAllRoomsDetailed();
				foreach (DataRow row in dtR.Rows)
				{
					cmbRoomM.Items.Add(row["RoomNumber"].ToString());
				}
				AddInp("Arızalı Oda", out var _, cmbRoomM);
				AddInp("Arıza Detayı", out var ctrl2, new TextBox
				{
					Multiline = true,
					Height = 70
				});
				TextBox txtFault = (TextBox)ctrl2;
				AddInp("Servis / Teknisyen", out var ctrl3, new TextBox
				{
					PlaceholderText = "Örn: Klima Servisi"
				});
				TextBox txtTech = (TextBox)ctrl3;
				AddInp("Tahmini Maliyet (₺)", out var ctrl4, new TextBox());
				TextBox txtCost = (TextBox)ctrl4;
				Button button2 = new Button
				{
					Text = "ARIYAYI KAYDET VE ODAYI BLOKE ET",
					Location = new Point(30, ly),
					Size = new System.Drawing.Size(340, 48),
					BackColor = System.Drawing.Color.FromArgb(15, 23, 42),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold)
				};
				f.Controls.Add(button2);
				button2.Click += delegate
				{
					if (cmbRoomM.SelectedIndex < 0)
					{
						MessageBox.Show("Hata: Lütfen bir oda seçin.");
					}
					else
					{
						string text = cmbRoomM.SelectedItem.ToString();
						int roomId = Convert.ToInt32(dtR.Select("RoomNumber='" + text + "'")[0]["RoomID"]);
						decimal.TryParse(txtCost.Text, out var result);
						DataAccess.AddMaintenanceLog(roomId, txtFault.Text, txtTech.Text, result);
						f.Close();
						RefreshGrid();
					}
				};
				f.ShowDialog();
				void AddInp(string label, out Control reference, Control type)
				{
					f.Controls.Add(new System.Windows.Forms.Label
					{
						Text = label,
						Location = new Point(30, ly),
						AutoSize = true,
						Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
					});
					reference = type;
					reference.Location = new Point(30, ly + 22);
					reference.Width = 340;
					reference.Font = new Font("Segoe UI", 10f);
					f.Controls.Add(reference);
					ly += 65;
				}
			};
			void RefreshGrid()
			{
				dgv.DataSource = DataAccess.GetMaintenanceLogs();
			}
		});
	}

	private void PageEmployees(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = System.Drawing.Color.White,
				Padding = new Padding(20)
			};
			body.Controls.Add(panel);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83d\udc65 Personel ve Vardiya Yönetimi",
				Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(22, 33, 62),
				Location = new Point(20, 30),
				AutoSize = true
			};
			panel.Controls.Add(value);
			Button button = new Button
			{
				Text = "\ud83d\udccb HAREKET LOGLARI",
				Size = new System.Drawing.Size(180, 48),
				Location = new Point(panel.Width - 400, 25),
				BackColor = System.Drawing.Color.FromArgb(59, 130, 246),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			panel.Controls.Add(button);
			Button button2 = new Button
			{
				Text = "➕ YENİ PERSONEL",
				Size = new System.Drawing.Size(180, 48),
				Location = new Point(panel.Width - 200, 25),
				BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			panel.Controls.Add(button2);
			DataGridView dgv = new DataGridView
			{
				Location = new Point(25, 120),
				Size = new System.Drawing.Size(body.Width - 50, body.Height - 150),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 42
				},
				Font = new Font("Segoe UI", 9.5f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
			body.Controls.Add(dgv);
			RefreshGrid();
			button.Click += delegate
			{
				Form form = new Form
				{
					Text = "Sistem Hareket Logları",
					Size = new System.Drawing.Size(600, 500),
					StartPosition = FormStartPosition.CenterScreen,
					BackColor = System.Drawing.Color.White
				};
				DataGridView dataGridView = new DataGridView
				{
					Dock = DockStyle.Fill,
					BackgroundColor = System.Drawing.Color.White,
					AllowUserToAddRows = false,
					ReadOnly = true,
					AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
					RowHeadersVisible = false
				};
				form.Controls.Add(dataGridView);
				dataGridView.DataSource = EnterpriseDataAccess.GetActivityLogs();
				form.ShowDialog();
			};
			button2.Click += delegate
			{
				Form f = new Form
				{
					Text = "\ud83d\udc64 Personel Kaydı",
					Size = new System.Drawing.Size(420, 520),
					StartPosition = FormStartPosition.CenterScreen,
					FormBorderStyle = FormBorderStyle.FixedDialog,
					BackColor = System.Drawing.Color.White
				};
				int ly = 25;
				AddInp("Ad", out var ctrl, new TextBox());
				TextBox txtF = (TextBox)ctrl;
				AddInp("Soyad", out var ctrl2, new TextBox());
				TextBox txtL = (TextBox)ctrl2;
				AddInp("Görev / Rol", out var ctrl3, new TextBox());
				TextBox txtR = (TextBox)ctrl3;
				AddInp("Maaş (₺)", out var ctrl4, new TextBox());
				TextBox txtS = (TextBox)ctrl4;
				AddInp("İşe Başlama", out var ctrl5, new DateTimePicker());
				DateTimePicker dtH = (DateTimePicker)ctrl5;
				Button button3 = new Button
				{
					Text = "KAYDET",
					Location = new Point(30, ly),
					Size = new System.Drawing.Size(340, 48),
					BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
					ForeColor = System.Drawing.Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold)
				};
				f.Controls.Add(button3);
				button3.Click += delegate
				{
					decimal.TryParse(txtS.Text, out var result);
					EnterpriseDataAccess.AddEmployee(txtF.Text, txtL.Text, txtR.Text, "000", result, dtH.Value);
					EnterpriseDataAccess.AddActivityLog("İK EKLENDİ", txtF.Text + " " + txtL.Text + " sisteme eklendi.");
					f.Close();
					RefreshGrid();
				};
				f.ShowDialog();
				void AddInp(string label, out Control reference, Control type)
				{
					f.Controls.Add(new System.Windows.Forms.Label
					{
						Text = label,
						Location = new Point(30, ly),
						AutoSize = true,
						Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
					});
					reference = type;
					reference.Location = new Point(30, ly + 22);
					reference.Width = 340;
					reference.Font = new Font("Segoe UI", 10f);
					f.Controls.Add(reference);
					ly += 65;
				}
			};
			void RefreshGrid()
			{
				dgv.DataSource = EnterpriseDataAccess.GetEmployees();
			}
		});
	}

	private void PageHousekeeping(Panel body)
	{
		DataTable dtRooms;
		try
		{
			dtRooms = EnterpriseDataAccess.GetHousekeepingTasks();
		}
		catch
		{
			dtRooms = new DataTable();
		}
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 80,
				BackColor = System.Drawing.Color.White,
				Padding = new Padding(25, 18, 25, 0)
			};
			body.Controls.Add(panel);
			panel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83e\uddf9 Temizlik & Kat Hizmetleri",
				Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				Location = new Point(20, 20),
				AutoSize = true
			});
			Button button = new Button
			{
				Text = "\ud83d\udd04 Yenile",
				Size = new System.Drawing.Size(110, 38),
				Location = new Point(panel.Width - 140, 20),
				BackColor = System.Drawing.Color.FromArgb(241, 245, 249),
				ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			button.FlatAppearance.BorderSize = 0;
			panel.Controls.Add(button);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (DataRow row in dtRooms.Rows)
			{
				string text = row["Durum"].ToString() ?? "";
				if (text.Contains("Kirli"))
				{
					num++;
				}
				else if (text.Contains("Temizleniyor"))
				{
					num2++;
				}
				else if (text.Contains("Bakımda"))
				{
					num3++;
				}
				else
				{
					num4++;
				}
			}
			Panel pnlStats = new Panel
			{
				Location = new Point(20, 90),
				Size = new System.Drawing.Size(body.Width - 40, 90),
				BackColor = System.Drawing.Color.Transparent,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			body.Controls.Add(pnlStats);
			AddHkCard("KİRLİ ODA", num.ToString(), System.Drawing.Color.FromArgb(239, 68, 68), 0);
			AddHkCard("TEMİZLENİYOR", num2.ToString(), System.Drawing.Color.FromArgb(245, 158, 11), 195);
			AddHkCard("BAKIMDA", num3.ToString(), System.Drawing.Color.FromArgb(100, 116, 139), 390);
			AddHkCard("TEMİZ / MÜSAİT", num4.ToString(), System.Drawing.Color.FromArgb(16, 185, 129), 585);
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(20, 195),
				Size = new System.Drawing.Size(body.Width - 40, 60),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 10,
				Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
			};
			body.Controls.Add(roundedPanel);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Atanacak Personel:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(15, 20),
				AutoSize = true
			});
			ComboBox cmbStaff = new ComboBox
			{
				Location = new Point(150, 16),
				Width = 200,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 10f)
			};
			cmbStaff.Items.Add("— Seçilmedi —");
			try
			{
				foreach (DataRow row2 in DataAccess.GetActiveEmployees().Rows)
				{
					cmbStaff.Items.Add($"{row2["FirstName"]} {row2["LastName"]}");
				}
			}
			catch
			{
			}
			cmbStaff.SelectedIndex = 0;
			roundedPanel.Controls.Add(cmbStaff);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "Yeni Durum:",
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Location = new Point(370, 20),
				AutoSize = true
			});
			ComboBox cmbStatus = new ComboBox
			{
				Location = new Point(455, 16),
				Width = 160,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 10f)
			};
			ComboBox.ObjectCollection items = cmbStatus.Items;
			object[] items2 = new string[4] { "Available", "Dirty", "Cleaning", "Maintenance" };
			items.AddRange(items2);
			cmbStatus.SelectedIndex = 0;
			roundedPanel.Controls.Add(cmbStatus);
			DataGridView dgv = new DataGridView
			{
				Location = new Point(20, 270),
				Size = new System.Drawing.Size(body.Width - 40, body.Height - 300),
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 48
				},
				Font = new Font("Segoe UI", 10f),
				EnableHeadersVisualStyles = false,
				Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
			};
			dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
			dgv.EnableHeadersVisualStyles = false;
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(238, 242, 255);
			dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
			dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
			body.Controls.Add(dgv);
			RefreshAll();
			dgv.CellFormatting += delegate(object? s, DataGridViewCellFormattingEventArgs e)
			{
				if (dgv.Columns[e.ColumnIndex].Name == "Durum" && e.Value != null)
				{
					string text2 = e.Value.ToString() ?? "";
					System.Drawing.Color foreColor = (text2.Contains("Kirli") ? System.Drawing.Color.FromArgb(239, 68, 68) : (text2.Contains("Temizleniyor") ? System.Drawing.Color.FromArgb(245, 158, 11) : (text2.Contains("Bakımda") ? System.Drawing.Color.FromArgb(100, 116, 139) : System.Drawing.Color.FromArgb(16, 185, 129))));
					if (e.CellStyle != null)
					{
						e.CellStyle.ForeColor = foreColor;
						e.CellStyle.Font = new Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
					}
				}
			};
			Button button2 = new Button
			{
				Text = "✅ Seçilen Odayı Güncelle",
				Size = new System.Drawing.Size(220, 35),
				Location = new Point(630, 18),
				BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			button2.FlatAppearance.BorderSize = 0;
			roundedPanel.Controls.Add(button2);
			button2.Click += delegate
			{
				if (dgv.SelectedRows.Count == 0)
				{
					MessageBox.Show("Lütfen bir oda satırı seçin.");
				}
				else
				{
					int roomId = Convert.ToInt32(dgv.SelectedRows[0].Cells["TaskID"].Value);
					string value = dgv.SelectedRows[0].Cells["Oda Numarası"].Value?.ToString() ?? "?";
					string text2 = cmbStatus.SelectedItem?.ToString() ?? "Available";
					string value2 = ((cmbStaff.SelectedIndex > 0) ? cmbStaff.SelectedItem.ToString() : "Belirtilmedi");
					EnterpriseDataAccess.UpdateRoomStatus(roomId, text2);
					EnterpriseDataAccess.LogAuditEvent("TEMİZLİK", "ROOMS", $"Oda {value} → {text2} | Personel: {value2}", AuthHelper.CurrentUser?.FullName ?? "Admin");
					RefreshAll();
				}
			};
			dgv.CellDoubleClick += delegate(object? s, DataGridViewCellEventArgs e)
			{
				if (e.RowIndex >= 0)
				{
					int roomId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["TaskID"].Value);
					string value = dgv.Rows[e.RowIndex].Cells["Oda Numarası"].Value?.ToString() ?? "?";
					string text2 = dgv.Rows[e.RowIndex].Cells["Durum"].Value?.ToString() ?? "";
					string text3 = ((text2.Contains("Kirli") || text2.Contains("Temizleniyor")) ? "Available" : "Dirty");
					DialogResult dialogResult = MessageBox.Show($"Oda {value} durumu '{text3}' olarak değiştirilsin mi?", "Hızlı Güncelle", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Yes)
					{
						EnterpriseDataAccess.UpdateRoomStatus(roomId, text3);
						EnterpriseDataAccess.LogAuditEvent("TEMİZLİK", "ROOMS", $"Oda {value} → {text3} (hızlı)", AuthHelper.CurrentUser?.FullName ?? "Admin");
						RefreshAll();
					}
				}
			};
			button.Click += delegate
			{
				RefreshAll();
			};
			void AddHkCard(string title, string val, System.Drawing.Color col, int x)
			{
				RoundedPanel c = new RoundedPanel
				{
					Location = new Point(x, 0),
					Size = new System.Drawing.Size(180, 80),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 12
				};
				c.Paint += delegate(object? s, PaintEventArgs e)
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					e.Graphics.FillRectangle(new SolidBrush(col), 0, 0, 5, c.Height);
				};
				c.Controls.Add(new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 22f, System.Drawing.FontStyle.Bold),
					ForeColor = col,
					Location = new Point(20, 12),
					AutoSize = true
				});
				c.Controls.Add(new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.Gray,
					Location = new Point(20, 50),
					AutoSize = true
				});
				pnlStats.Controls.Add(c);
			}
			void RefreshAll()
			{
				dtRooms = EnterpriseDataAccess.GetHousekeepingTasks();
				dgv.DataSource = null;
				dgv.DataSource = dtRooms;
				if (dgv.Columns.Contains("TaskID"))
				{
					dgv.Columns["TaskID"].Visible = false;
				}
			}
		});
	}

	private void PageEndOfDay(Panel body)
	{
		SafeInvoke(delegate
		{
			body.Controls.Clear();
			body.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 80,
				BackColor = System.Drawing.Color.White,
				Padding = new Padding(25, 15, 25, 0)
			};
			body.Controls.Add(panel);
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "\ud83c\udfe6 KASA VE GÜN SONU YÖNETİMİ",
				Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
				AutoSize = true,
				Location = new Point(25, 22)
			};
			panel.Controls.Add(value);
			Button button = new Button
			{
				Text = "\ud83d\udd12 GÜNÜ KAPAT (Z-RAPORU)",
				Size = new System.Drawing.Size(220, 45),
				Location = new Point(panel.Width - 245, 18),
				BackColor = System.Drawing.Color.FromArgb(220, 38, 38),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9.2f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			panel.Controls.Add(button);
			Button button2 = new Button
			{
				Text = "\ud83d\udcc4 İŞLEM DETAYI (PDF)",
				Size = new System.Drawing.Size(180, 45),
				Location = new Point(panel.Width - 435, 18),
				BackColor = System.Drawing.Color.FromArgb(51, 65, 85),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			panel.Controls.Add(button2);
			Button button3 = new Button
			{
				Text = "\ud83d\udcc4 KASA ARŞİV (PDF)",
				Size = new System.Drawing.Size(180, 45),
				Location = new Point(panel.Width - 625, 18),
				BackColor = System.Drawing.Color.FromArgb(71, 85, 105),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				Cursor = Cursors.Hand,
				Anchor = (AnchorStyles.Top | AnchorStyles.Right)
			};
			panel.Controls.Add(button3);
			Panel pnlMain = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(25)
			};
			body.Controls.Add(pnlMain);
			FlowLayoutPanel flowStats = new FlowLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = 130,
				Margin = new Padding(0)
			};
			pnlMain.Controls.Add(flowStats);
			(decimal, decimal, decimal, decimal) dailyFinancialTotals = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
			AddStat("NAKİT KASA", dailyFinancialTotals.Item1.ToString("N2") + " ₺", System.Drawing.Color.FromArgb(16, 185, 129));
			AddStat("KREDİ KARTI", dailyFinancialTotals.Item2.ToString("N2") + " ₺", System.Drawing.Color.FromArgb(59, 130, 246));
			AddStat("TOPLAM GİDER", dailyFinancialTotals.Item3.ToString("N2") + " ₺", System.Drawing.Color.FromArgb(244, 63, 94));
			AddStat("NET DURUM", (dailyFinancialTotals.Item1 + dailyFinancialTotals.Item2 - dailyFinancialTotals.Item3).ToString("N2") + " ₺", System.Drawing.Color.FromArgb(15, 23, 42));
			SplitContainer splitContainer = new SplitContainer();
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer.SplitterDistance = 250;
			splitContainer.Panel1.Padding = new Padding(0, 10, 0, 10);
			splitContainer.Panel2.Padding = new Padding(0, 10, 0, 10);
			SplitContainer splitContainer2 = splitContainer;
			pnlMain.Controls.Add(splitContainer2);
			splitContainer2.BringToFront();
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(15)
			};
			splitContainer2.Panel1.Controls.Add(roundedPanel);
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcdd Bugünkü Hareketler",
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Dock = DockStyle.Top,
				Height = 35
			});
			DataGridView dataGridView = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 38
				},
				Font = new Font("Segoe UI", 9f),
				EnableHeadersVisualStyles = false
			};
			dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			roundedPanel.Controls.Add(dataGridView);
			dataGridView.BringToFront();
			dataGridView.DataSource = EnterpriseDataAccess.GetDailyTransactions(DateTime.Today);
			RoundedPanel roundedPanel2 = new RoundedPanel
			{
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.White,
				BorderRadius = 15,
				Padding = new Padding(15)
			};
			splitContainer2.Panel2.Controls.Add(roundedPanel2);
			roundedPanel2.Controls.Add(new System.Windows.Forms.Label
			{
				Text = "\ud83d\udcda Geçmiş Gün Sonu Raporları",
				Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
				Dock = DockStyle.Top,
				Height = 35
			});
			DataGridView dataGridView2 = new DataGridView
			{
				Dock = DockStyle.Fill,
				BackgroundColor = System.Drawing.Color.White,
				BorderStyle = BorderStyle.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				RowHeadersVisible = false,
				AllowUserToAddRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				RowTemplate = 
				{
					Height = 38
				},
				Font = new Font("Segoe UI", 9f),
				EnableHeadersVisualStyles = false
			};
			dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
			dataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
			roundedPanel2.Controls.Add(dataGridView2);
			dataGridView2.BringToFront();
			dataGridView2.DataSource = EnterpriseDataAccess.GetEndOfDayReports();
			button2.Click += delegate
			{
				DataTable dailyTransactions = EnterpriseDataAccess.GetDailyTransactions(DateTime.Today);
				(decimal, decimal, decimal, decimal) dailyFinancialTotals2 = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
				ReportHelper.GenerateDailyTransactionsPdf(dailyTransactions, dailyFinancialTotals2.Item1, dailyFinancialTotals2.Item2, dailyFinancialTotals2.Item3);
			};
			button3.Click += delegate
			{
				DataTable endOfDayReports = EnterpriseDataAccess.GetEndOfDayReports();
				ReportHelper.GenerateEndOfDayPdf(endOfDayReports);
			};
			button.Click += delegate
			{
				(decimal, decimal, decimal, decimal) dailyFinancialTotals2 = EnterpriseDataAccess.GetDailyFinancialTotals(DateTime.Today);
				DialogResult dialogResult = MessageBox.Show($"\ud83d\udcb0 GÜN SONU ÖZETİ ({DateTime.Today:dd.MM.yyyy})\n\nNakit Giriş: {dailyFinancialTotals2.Item1:N2} ₺\nKart Giriş: {dailyFinancialTotals2.Item2:N2} ₺\nGiderler: {dailyFinancialTotals2.Item3:N2} ₺\nNet Ciro: {dailyFinancialTotals2.Item4:N2} ₺\n\n" + "Kasayı kapatmak ve yedek almak istiyor musunuz?", "Gün Sonu Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (dialogResult == DialogResult.Yes)
				{
					if (EnterpriseDataAccess.CreateEndOfDayReport(DateTime.Today, dailyFinancialTotals2.Item1, dailyFinancialTotals2.Item2, dailyFinancialTotals2.Item3, dailyFinancialTotals2.Item4, AuthHelper.CurrentUser?.FullName ?? "Admin"))
					{
						try
						{
							DatabaseBackupHelper.BackupDatabase();
						}
						catch
						{
						}
						MessageBox.Show("Gün başarıyla kapatıldı ve veriler mühürlendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						PageEndOfDay(body);
					}
					else
					{
						MessageBox.Show("Bu tarih için zaten gün sonu alınmış.");
					}
				}
			};
			void AddStat(string title, string val, System.Drawing.Color theme)
			{
				RoundedPanel roundedPanel3 = new RoundedPanel
				{
					Size = new System.Drawing.Size((pnlMain.Width - 100) / 4, 110),
					BackColor = System.Drawing.Color.White,
					BorderRadius = 15,
					Margin = new Padding(0, 0, 20, 0)
				};
				System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
				{
					Text = title,
					Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
					ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
					Location = new Point(20, 20),
					AutoSize = true
				};
				System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
				{
					Text = val,
					Font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold),
					ForeColor = theme,
					Location = new Point(18, 45),
					AutoSize = true
				};
				roundedPanel3.Controls.Add(value2);
				roundedPanel3.Controls.Add(value3);
				flowStats.Controls.Add(roundedPanel3);
			}
		});
	}

	private void ExportPoliceXML()
	{
		try
		{
			DataTable dailyPoliceReport = DataAccess.GetDailyPoliceReport(DateTime.Today);
			if (dailyPoliceReport.Rows.Count == 0)
			{
				MessageBox.Show("Bugün konaklayan misafir bulunamadı.", "Bilgi");
				return;
			}
			using SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "XML Files|*.xml",
				FileName = $"KBYS_Rapor_{DateTime.Today:yyyyMMdd}.xml"
			};
			if (saveFileDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			using StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, append: false, Encoding.UTF8);
			streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			streamWriter.WriteLine("<KbysRapor>");
			streamWriter.WriteLine("  <TesisKodu>TESIS001</TesisKodu>");
			streamWriter.WriteLine($"  <RaporTarihi>{DateTime.Today:yyyy-MM-dd}</RaporTarihi>");
			streamWriter.WriteLine("  <Misafirler>");
			foreach (DataRow row in dailyPoliceReport.Rows)
			{
				streamWriter.WriteLine("    <Misafir>");
				streamWriter.WriteLine($"      <TCKimlikNo>{row["IdentityNumber"]}</TCKimlikNo>");
				streamWriter.WriteLine($"      <Ad>{row["FirstName"]}</Ad>");
				streamWriter.WriteLine($"      <Soyad>{row["LastName"]}</Soyad>");
				streamWriter.WriteLine($"      <BabaAdi>{row["FatherName"]}</BabaAdi>");
				streamWriter.WriteLine($"      <AnneAdi>{row["MotherName"]}</AnneAdi>");
				streamWriter.WriteLine($"      <DogumYeri>{row["BirthPlace"]}</DogumYeri>");
				streamWriter.WriteLine("      <DogumTarihi>" + ((row["BirthDate"] != DBNull.Value) ? Convert.ToDateTime(row["BirthDate"]).ToString("yyyy-MM-dd") : "") + "</DogumTarihi>");
				streamWriter.WriteLine($"      <Cinsiyet>{row["Gender"]}</Cinsiyet>");
				streamWriter.WriteLine($"      <Uyruk>{row["Nationality"]}</Uyruk>");
				streamWriter.WriteLine($"      <OdaNo>{row["RoomNumber"]}</OdaNo>");
				streamWriter.WriteLine($"      <GirisTarihi>{Convert.ToDateTime(row["CheckInDate"]):yyyy-MM-dd}</GirisTarihi>");
				streamWriter.WriteLine($"      <CikisTarihi>{Convert.ToDateTime(row["CheckOutDate"]):yyyy-MM-dd}</CikisTarihi>");
				streamWriter.WriteLine("    </Misafir>");
			}
			streamWriter.WriteLine("  </Misafirler>");
			streamWriter.WriteLine("</KbysRapor>");
			MessageBox.Show("XML Raporu başarıyla oluşturuldu.", "Başarılı");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Hata: " + ex.Message);
		}
	}

	private void ShowCustomerCard(int customerId)
	{
		DataRow profile = EnterpriseDataAccess.GetGuestProfile(customerId);
		DataTable customerHistory = DataAccess.GetCustomerHistory(customerId);
		if (profile == null)
		{
			MessageBox.Show("Müşteri bulunamadı.");
			return;
		}
		string fullName = $"{profile["FirstName"]} {profile["LastName"]}";
		int num = ((profile["TotalStays"] != DBNull.Value) ? Convert.ToInt32(profile["TotalStays"]) : 0);
		decimal num2 = ((profile["TotalSpent"] != DBNull.Value) ? Convert.ToDecimal(profile["TotalSpent"]) : 0m);
		string text = profile["Notes"]?.ToString() ?? "";
		string text2 = "";
		string selectedItem = "Normal";
		string text3 = "";
		try
		{
			text2 = profile["Preferences"]?.ToString() ?? "";
		}
		catch
		{
		}
		try
		{
			selectedItem = profile["VipStatus"]?.ToString() ?? "Normal";
		}
		catch
		{
		}
		try
		{
			text3 = profile["Allergies"]?.ToString() ?? "";
		}
		catch
		{
		}
		string tier = ((num >= 10) ? "\ud83e\udd47 PLATİN ÜYE" : ((num >= 5) ? "\ud83e\udd48 ALTIN ÜYE" : ((num >= 2) ? "\ud83e\udd49 GÜMÜŞ ÜYE" : "\ud83c\udf96\ufe0f YENİ ÜYE")));
		System.Drawing.Color c = ((num >= 10) ? System.Drawing.Color.FromArgb(99, 102, 241) : ((num >= 5) ? System.Drawing.Color.FromArgb(245, 158, 11) : ((num >= 2) ? System.Drawing.Color.FromArgb(100, 116, 139) : System.Drawing.Color.FromArgb(16, 185, 129))));
		Form form = new Form
		{
			Text = "\ud83d\udc64 Misafir CRM Kartı — " + fullName,
			Size = new System.Drawing.Size(600, 780),
			StartPosition = FormStartPosition.CenterParent,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false
		};
		Panel pnlHead = new Panel
		{
			Dock = DockStyle.Top,
			Height = 100,
			BackColor = System.Drawing.Color.White
		};
		pnlHead.Paint += delegate(object? s, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using LinearGradientBrush brush = new LinearGradientBrush(pnlHead.ClientRectangle, System.Drawing.Color.FromArgb(99, 102, 241), System.Drawing.Color.FromArgb(59, 130, 246), 45f);
			e.Graphics.FillRectangle(brush, pnlHead.ClientRectangle);
			e.Graphics.FillEllipse(Brushes.White, 20, 20, 60, 60);
			string s2 = (profile["FirstName"]?.ToString() ?? "?").Substring(0, 1) + (profile["LastName"]?.ToString() ?? "?").Substring(0, 1);
			e.Graphics.DrawString(s2, new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold), new SolidBrush(System.Drawing.Color.FromArgb(99, 102, 241)), 30f, 35f);
			e.Graphics.DrawString(fullName, new Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold), Brushes.White, 95f, 25f);
			e.Graphics.DrawString(tier, new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold), Brushes.White, 97f, 52f);
			e.Graphics.DrawString($"TC: {profile["IdentityNumber"]}", new Font("Segoe UI", 8f), new SolidBrush(System.Drawing.Color.FromArgb(200, 255, 255, 255)), 97f, 72f);
		};
		form.Controls.Add(pnlHead);
		Panel pnlStats = new Panel
		{
			Dock = DockStyle.Top,
			Height = 75,
			BackColor = System.Drawing.Color.White,
			Padding = new Padding(15, 10, 15, 0)
		};
		form.Controls.Add(pnlStats);
		AddStatBadge("KONAKLAMA", num.ToString(), c, 0);
		AddStatBadge("TOPLAM HARCAMA", num2.ToString("N0") + "₺", System.Drawing.Color.FromArgb(16, 185, 129), 135);
		string val = ((profile["LastStay"] != DBNull.Value) ? Convert.ToDateTime(profile["LastStay"]).ToString("dd.MM.yy") : "—");
		AddStatBadge("SON KONAKLAMA", val, System.Drawing.Color.FromArgb(59, 130, 246), 270);
		Panel scroll = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
			Padding = new Padding(15, 10, 15, 10)
		};
		form.Controls.Add(scroll);
		int sy = 10;
		Panel panel = MakeSection("\ud83c\udf1f VIP DURUMU");
		ComboBox cmbVip = new ComboBox
		{
			Location = new Point(15, 38),
			Size = new System.Drawing.Size(200, 30),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Font = new Font("Segoe UI", 10f)
		};
		ComboBox.ObjectCollection items = cmbVip.Items;
		object[] items2 = new string[4] { "Normal", "VIP", "Kara Liste", "Düzenli Müşteri" };
		items.AddRange(items2);
		cmbVip.SelectedItem = selectedItem;
		if (cmbVip.SelectedIndex < 0)
		{
			cmbVip.SelectedIndex = 0;
		}
		panel.Controls.Add(cmbVip);
		panel.Height = 80;
		sy += 90;
		Panel panel2 = MakeSection("\ud83d\udecf\ufe0f ÖZEL TERCİHLER");
		TextBox txtPrefs = new TextBox
		{
			Location = new Point(15, 38),
			Size = new System.Drawing.Size(510, 60),
			Multiline = true,
			Font = new Font("Segoe UI", 10f),
			BorderStyle = BorderStyle.FixedSingle,
			PlaceholderText = "Ör: Yüksek kat, çift kişilik yatak, sessiz oda...",
			Text = text2
		};
		panel2.Controls.Add(txtPrefs);
		panel2.Height = 115;
		sy += 125;
		Panel panel3 = MakeSection("⚠\ufe0f ALERJİ / ÖZEL DURUM");
		TextBox txtAllerg = new TextBox
		{
			Location = new Point(15, 38),
			Size = new System.Drawing.Size(510, 45),
			Multiline = true,
			Font = new Font("Segoe UI", 10f),
			BorderStyle = BorderStyle.FixedSingle,
			PlaceholderText = "Ör: Fıstık alerjisi, laktoz intoleransı...",
			Text = text3,
			ForeColor = System.Drawing.Color.FromArgb(220, 38, 38)
		};
		panel3.Controls.Add(txtAllerg);
		panel3.Height = 95;
		sy += 105;
		Panel panel4 = MakeSection("\ud83d\udcdd ÖZEL NOTLAR");
		TextBox txtNotes = new TextBox
		{
			Location = new Point(15, 38),
			Size = new System.Drawing.Size(510, 70),
			Multiline = true,
			Font = new Font("Segoe UI", 10f),
			BorderStyle = BorderStyle.FixedSingle,
			PlaceholderText = "Serbest not...",
			Text = text
		};
		panel4.Controls.Add(txtNotes);
		panel4.Height = 120;
		sy += 130;
		Panel panel5 = MakeSection("\ud83d\udd52 KONAKLAMA GEÇMİŞİ");
		DataGridView dataGridView = new DataGridView
		{
			Location = new Point(15, 38),
			Size = new System.Drawing.Size(510, 160),
			BackgroundColor = System.Drawing.Color.White,
			BorderStyle = BorderStyle.None,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			RowHeadersVisible = false,
			AllowUserToAddRows = false,
			ReadOnly = true,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			RowTemplate = 
			{
				Height = 32
			},
			EnableHeadersVisualStyles = false,
			DataSource = customerHistory
		};
		dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
		dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		panel5.Controls.Add(dataGridView);
		panel5.Height = 210;
		sy += 220;
		Button button = new Button
		{
			Text = "\ud83d\udcbe PROFİLİ KAYDET",
			Dock = DockStyle.Bottom,
			Height = 50,
			BackColor = System.Drawing.Color.FromArgb(99, 102, 241),
			ForeColor = System.Drawing.Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderSize = 0;
		button.Click += delegate
		{
			string performedBy = AuthHelper.CurrentUser?.FullName ?? "Admin";
			EnterpriseDataAccess.UpdateGuestCrmProfile(customerId, txtNotes.Text, txtPrefs.Text, cmbVip.SelectedItem?.ToString() ?? "Normal", txtAllerg.Text);
			EnterpriseDataAccess.LogAuditEvent("GÜNCELLEME", "CUSTOMERS", $"{fullName} CRM profili güncellendi. VIP: {cmbVip.SelectedItem}", performedBy);
			MessageBox.Show("✅ Profil başarıyla güncellendi.", "Kaydedildi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		};
		form.Controls.Add(button);
		form.ShowDialog();
		void AddStatBadge(string lbl, string text4, System.Drawing.Color foreColor, int x)
		{
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(x, 8),
				Size = new System.Drawing.Size(120, 52),
				BackColor = System.Drawing.Color.FromArgb(248, 250, 252),
				BorderRadius = 8
			};
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = text4,
				Font = new Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
				ForeColor = foreColor,
				Location = new Point(8, 6),
				AutoSize = true
			});
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = lbl,
				Font = new Font("Segoe UI", 7f),
				ForeColor = System.Drawing.Color.Gray,
				Location = new Point(8, 30),
				AutoSize = true
			});
			pnlStats.Controls.Add(roundedPanel);
		}
		Panel MakeSection(string title)
		{
			RoundedPanel roundedPanel = new RoundedPanel
			{
				Location = new Point(0, sy),
				Size = new System.Drawing.Size(540, 0),
				BackColor = System.Drawing.Color.White,
				BorderRadius = 10,
				Padding = new Padding(15)
			};
			roundedPanel.Controls.Add(new System.Windows.Forms.Label
			{
				Text = title,
				Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
				ForeColor = System.Drawing.Color.FromArgb(99, 102, 241),
				Location = new Point(15, 12),
				AutoSize = true
			});
			scroll.Controls.Add(roundedPanel);
			return roundedPanel;
		}
	}

	private void AddRecRow(FlowLayoutPanel flow, string lbl, string val, bool bold = false, bool line = false, System.Drawing.Color? color = null)
	{
		if (line)
		{
			Panel value = new Panel
			{
				Size = new System.Drawing.Size(flow.Width - 40, 1),
				BackColor = System.Drawing.Color.FromArgb(226, 232, 240),
				Margin = new Padding(0, 10, 0, 10)
			};
			flow.Controls.Add(value);
			return;
		}
		Panel p = new Panel
		{
			Size = new System.Drawing.Size(flow.Width - 35, 28),
			Margin = new Padding(0, 2, 0, 2)
		};
		System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
		{
			Text = lbl,
			Font = new Font("Segoe UI", 9.5f, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
			ForeColor = (color ?? System.Drawing.Color.FromArgb(71, 85, 105)),
			Dock = DockStyle.Left,
			AutoSize = true,
			TextAlign = ContentAlignment.MiddleLeft
		};
		System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
		{
			Text = val,
			Font = new Font("Segoe UI", 9.5f, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
			ForeColor = (color ?? System.Drawing.Color.FromArgb(15, 23, 42)),
			Dock = DockStyle.Right,
			AutoSize = true,
			TextAlign = ContentAlignment.MiddleRight
		};
		p.Controls.Add(value3);
		p.Controls.Add(value2);
		flow.Controls.Add(p);
		flow.SizeChanged += delegate
		{
			p.Width = flow.Width - 35;
		};
	}

	private DataRow GetRow(int resId)
	{
		DataTable reservations = DataAccess.GetReservations();
		DataRow[] array = reservations.Select("ReservationID = " + resId);
		return (array.Length != 0) ? array[0] : null;
	}

	private GraphicsPath CustGetPath(System.Drawing.Rectangle r, int d)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(r.X, r.Y, d, d, 180f, 90f);
		graphicsPath.AddArc(r.X + r.Width - d, r.Y, d, d, 270f, 90f);
		graphicsPath.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Y + r.Height - d, d, d, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	private void DrawPieChart(Graphics g, System.Drawing.Rectangle rect, (int occupied, int available, int total, int dirty, int maintenance) occ)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (occ.total == 0)
		{
			return;
		}
		float num = (float)occ.occupied / (float)occ.total * 360f;
		float num2 = (float)occ.available / (float)occ.total * 360f;
		float num3 = (float)occ.dirty / (float)occ.total * 360f;
		float sweepAngle = (float)occ.maintenance / (float)occ.total * 360f;
		int num4 = Math.Min(rect.Width, rect.Height) - 100;
		System.Drawing.Rectangle rect2 = new System.Drawing.Rectangle(rect.X + (rect.Width - num4) / 2, rect.Y + (rect.Height - num4) / 2 + 20, num4, num4);
		float num5 = 0f;
		g.FillPie(new SolidBrush(System.Drawing.Color.FromArgb(59, 130, 246)), rect2, num5, num);
		num5 += num;
		g.FillPie(new SolidBrush(System.Drawing.Color.FromArgb(34, 197, 94)), rect2, num5, num2);
		num5 += num2;
		g.FillPie(new SolidBrush(System.Drawing.Color.FromArgb(249, 115, 22)), rect2, num5, num3);
		num5 += num3;
		g.FillPie(new SolidBrush(System.Drawing.Color.FromArgb(148, 163, 184)), rect2, num5, sweepAngle);
		int num6 = (int)((double)num4 * 0.6);
		System.Drawing.Rectangle rect3 = new System.Drawing.Rectangle(rect2.X + (num4 - num6) / 2, rect2.Y + (num4 - num6) / 2, num6, num6);
		g.FillEllipse(Brushes.White, rect3);
		string text = $"%{((occ.total > 0) ? (occ.occupied * 100 / occ.total) : 0)}";
		using Font font = new Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
		SizeF sizeF = g.MeasureString(text, font);
		g.DrawString(text, font, Brushes.Black, (float)rect3.X + ((float)num6 - sizeF.Width) / 2f, (float)rect3.Y + ((float)num6 - sizeF.Height) / 2f);
	}
}
