using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PmsSystem.Database;

namespace PmsSystem.Helpers
{
    public static class ReportService
    {
        static ReportService()
        {
            // QuestPDF Community License is required for version 2022.12 and later
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static void GenerateWeeklyReport(DateTime startDate, DateTime endDate, string filePath)
        {
            // Fetch Data
            decimal accommodationIncome = DataAccess.GetWeeklyAccommodationIncome(startDate, endDate);
            decimal restaurantIncome    = DataAccess.GetWeeklyRestaurantIncome(startDate, endDate);
            DataTable dtCheckIns        = DataAccess.GetWeeklyCheckIns(startDate, endDate);
            DataTable dtCheckOuts       = DataAccess.GetWeeklyCheckOuts(startDate, endDate);
            DataTable dtTopSales        = DataAccess.GetWeeklyTopSales(startDate, endDate);
            DataTable dtPayments        = DataAccess.GetWeeklyPayments(startDate, endDate);

            decimal totalIncome = accommodationIncome + restaurantIncome;
            string reportGenerated = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            // Generate PDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));

                    // ── HEADER ──────────────────────────────────────────────────
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("✨ SOM-PMS").FontSize(20).SemiBold().FontColor("#4F46E5");
                            col.Item().Text("Pansiyon Yönetim Sistemi").FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("HAFTALIK İŞLETME RAPORU").FontSize(14).SemiBold().FontColor("#0F172A");
                            col.Item().AlignRight().Text($"{startDate:dd MMMM} – {endDate:dd MMMM yyyy}").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().AlignRight().Text($"Oluşturulma: {reportGenerated}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // ── CONTENT ──────────────────────────────────────────────────
                    page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
                    {
                        // ── 1. FINANSAL ÖZET KARTLARI ─────────────────────────
                        col.Item().PaddingBottom(6).Text("💰 FİNANSAL ÖZET").FontSize(12).SemiBold().FontColor("#1E293B");
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().PaddingRight(5).Element(c => FinancialCard(c, "KONAKLAMA/ÖDEME GELİRİ", accommodationIncome, "#22C55E"));
                            row.RelativeItem().PaddingHorizontal(5).Element(c => FinancialCard(c, "LOKANTA / CAFE GELİRİ", restaurantIncome, "#EF466F"));
                            row.RelativeItem().PaddingLeft(5).Element(c => FinancialCard(c, "HAFTALIK TOPLAM HASILAT", totalIncome, "#4F46E5"));
                        });

                        // ── 2. GİRİŞ YAPANLAR ────────────────────────────────
                        col.Item().PaddingTop(14).Text("🏨 KONAKLAMA HAREKETLERİ").FontSize(12).SemiBold().FontColor("#1E293B");

                        col.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("• Haftaya Giriş Yapanlar").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                            if (dtCheckIns.Rows.Count == 0)
                            {
                                c.Item().PaddingVertical(6).Text("Bu hafta giriş kaydı bulunmamaktadır.").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                c.Item().PaddingTop(4).Element(container => TableHeader(container, new string[] { "Oda", "Müşteri", "Giriş", "Çıkış", "Tutar" }));
                                foreach (DataRow dr in dtCheckIns.Rows)
                                {
                                    c.Item().Element(container => TableRow(container, new string[]
                                    {
                                        dr["Oda"].ToString(),
                                        dr["Müşteri"].ToString(),
                                        Convert.ToDateTime(dr["Giriş"]).ToString("dd.MM.yyyy"),
                                        Convert.ToDateTime(dr["Çıkış"]).ToString("dd.MM.yyyy"),
                                        Convert.ToDecimal(dr["Tutar"]).ToString("N2") + " ₺"
                                    }));
                                }
                            }
                        });

                        // ── 3. ÇIKIŞ YAPANLAR ────────────────────────────────
                        col.Item().PaddingTop(14).Column(c =>
                        {
                            c.Item().Text("• Haftaya Çıkış Yapanlar").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                            if (dtCheckOuts.Rows.Count == 0)
                            {
                                c.Item().PaddingVertical(6).Text("Bu hafta çıkış kaydı bulunmamaktadır.").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                c.Item().PaddingTop(4).Element(container => TableHeader(container, new string[] { "Oda", "Müşteri", "Giriş", "Çıkış", "Tutar" }));
                                foreach (DataRow dr in dtCheckOuts.Rows)
                                {
                                    c.Item().Element(container => TableRow(container, new string[]
                                    {
                                        dr["Oda"].ToString(),
                                        dr["Müşteri"].ToString(),
                                        Convert.ToDateTime(dr["Giriş"]).ToString("dd.MM.yyyy"),
                                        Convert.ToDateTime(dr["Çıkış"]).ToString("dd.MM.yyyy"),
                                        Convert.ToDecimal(dr["Tutar"]).ToString("N2") + " ₺"
                                    }));
                                }
                            }
                        });

                        // ── 4. HAFTALIK ÖDEME İŞLEMLERİ ─────────────────────
                        col.Item().PaddingTop(14).Text("💳 HAFTALIK ÖDEME İŞLEMLERİ").FontSize(12).SemiBold().FontColor("#1E293B");
                        col.Item().PaddingTop(6).Column(c =>
                        {
                            if (dtPayments.Rows.Count == 0)
                            {
                                c.Item().PaddingVertical(6).Text("Bu hafta ödeme kaydı bulunmamaktadır.").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                c.Item().PaddingTop(4).Element(container => TableHeader(container,
                                    new string[] { "Tarih", "Oda", "Müşteri", "Yöntem", "Tutar" },
                                    new float[] { 1.5f, 0.8f, 2f, 1.5f, 1f }));
                                foreach (DataRow dr in dtPayments.Rows)
                                {
                                    c.Item().Element(container => TableRow(container, new string[]
                                    {
                                        Convert.ToDateTime(dr["Tarih"]).ToString("dd.MM.yyyy HH:mm"),
                                        dr["Oda"].ToString(),
                                        dr["Müşteri"].ToString(),
                                        dr["Yöntem"].ToString(),
                                        Convert.ToDecimal(dr["Tutar"]).ToString("N2") + " ₺"
                                    }, new float[] { 1.5f, 0.8f, 2f, 1.5f, 1f }));
                                }

                                // Ödeme toplamı
                                decimal payTotal = dtPayments.AsEnumerable().Sum(r =>
                                {
                                    try { return Convert.ToDecimal(r["Tutar"]); } catch { return 0; }
                                });
                                c.Item()
                                    .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                                    .PaddingTop(4)
                                    .AlignRight()
                                    .Text($"Haftalık Tahsilat Toplamı: {payTotal:N2} ₺")
                                    .FontSize(9).SemiBold().FontColor("#4F46E5");
                            }
                        });

                        // ── 5. EN ÇOK SATAN ÜRÜNLER ──────────────────────────
                        col.Item().PaddingTop(14).Text("🍴 LOKANTA VE CAFE ANALİZİ").FontSize(12).SemiBold().FontColor("#1E293B");
                        col.Item().PaddingTop(6).Column(c =>
                        {
                            c.Item().Text("• En Çok Satan Ürünler (İlk 10)").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                            if (dtTopSales.Rows.Count == 0)
                            {
                                c.Item().PaddingVertical(6).Text("Bu hafta satış kaydı bulunmamaktadır.").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                c.Item().PaddingTop(4).Element(container => TableHeader(container,
                                    new string[] { "Ürün Adı", "Adet", "Toplam Tutar" },
                                    new float[] { 3, 1, 1 }));
                                foreach (DataRow dr in dtTopSales.Rows)
                                {
                                    c.Item().Element(container => TableRow(container, new string[]
                                    {
                                        dr["Ürün"].ToString(),
                                        dr["Adet"].ToString(),
                                        Convert.ToDecimal(dr["Toplam"]).ToString("N2") + " ₺"
                                    }, new float[] { 3, 1, 1 }));
                                }
                            }
                        });
                    });

                    // ── FOOTER ──────────────────────────────────────────────────
                    page.Footer().Row(footRow =>
                    {
                        footRow.RelativeItem().AlignLeft()
                            .Text($"SOM-PMS  |  Pansiyon Yönetim Sistemi")
                            .FontSize(8).FontColor(Colors.Grey.Medium);

                        footRow.RelativeItem().AlignCenter().Text(x =>
                        {
                            x.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        footRow.RelativeItem().AlignRight()
                            .Text($"Oluşturma: {reportGenerated}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static void FinancialCard(IContainer container, string title, decimal amount, string color)
        {
            container
                .Background("#F8FAFC")
                .Border(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(12)
                .Column(col =>
                {
                    col.Item().Text(title).FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).Text($"{amount:N2} ₺").FontSize(16).SemiBold().FontColor(color);
                });
        }

        private static void TableHeader(IContainer container, string[] headers, float[] weights = null)
        {
            container
                .Background("#F1F5F9")
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .Row(row =>
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var item = row.RelativeItem(weights != null ? weights[i] : 1);
                        item.Text(headers[i]).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3);
                    }
                });
        }

        private static void TableRow(IContainer container, string[] cells, float[] weights = null)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten4)
                .PaddingVertical(5)
                .PaddingHorizontal(4)
                .Row(row =>
                {
                    for (int i = 0; i < cells.Length; i++)
                    {
                        var item = row.RelativeItem(weights != null ? weights[i] : 1);
                        item.Text(cells[i]).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });
        }
        public static void GenerateRoomReportPdf(DateTime start, DateTime end, DataTable data, dynamic stats, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row => {
                        row.RelativeItem().Text("🏨 ODA KONAKLAMA RAPORU").FontSize(20).SemiBold().FontColor("#4F46E5");
                        row.RelativeItem().AlignRight().Text($"{start:dd.MM.yyyy} - {end:dd.MM.yyyy}").FontSize(10);
                    });

                    page.Content().PaddingVertical(10).Column(col => {
                        col.Item().Row(row => {
                            row.RelativeItem().Element(c => FinancialCard(c, "TOPLAM GELİR", (decimal)stats.TotalRevenue, "#22C55E"));
                            row.RelativeItem().PaddingHorizontal(5).Element(c => FinancialCard(c, "TOPLAM GECELEME", (decimal)stats.TotalNights, "#4F46E5"));
                            row.RelativeItem().Element(c => FinancialCard(c, "DOLULUK ORANI", (decimal)stats.OccupancyRate, "#EF466F"));
                        });

                        col.Item().PaddingTop(20).Element(container => TableHeader(container, new string[] { "Oda", "Müşteri", "Gün", "Günlük Ücret", "Kazanç" }));
                        foreach (DataRow dr in data.Rows) {
                            col.Item().Element(container => TableRow(container, new string[] {
                                dr["Oda"].ToString(),
                                dr["Müşteri"].ToString(),
                                dr["Gün"].ToString() + " Gün",
                                Convert.ToDecimal(dr["Günlük Ücret"]).ToString("N2") + " ₺",
                                Convert.ToDecimal(dr["Kazanç"]).ToString("N2") + " ₺"
                            }));
                        }
                    });

                    page.Footer().AlignRight().Text(x => {
                        x.Span("Sayfa "); x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);
        }

        public static void GenerateRestaurantReportPdf(DateTime start, DateTime end, DataTable data, string filePath, string productFilter = "Hepsi")
        {
            decimal total = 0;
            foreach (DataRow r in data.Rows) total += Convert.ToDecimal(r["Toplam"]);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row => {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("🍽️ RESTORAN SATIŞ ANALİZİ").FontSize(20).SemiBold().FontColor("#EF466F");
                            if (productFilter != "Hepsi") c.Item().Text($"Filtre: {productFilter}").FontSize(10).Italic();
                        });
                        row.RelativeItem().AlignRight().Text($"{start:dd.MM.yyyy} - {end:dd.MM.yyyy}").FontSize(10);
                    });

                    page.Content().PaddingVertical(10).Column(col => {
                        col.Item().PaddingBottom(10).Text($"Toplam Restoran Cirosu: {total:N2} ₺").FontSize(14).SemiBold().FontColor("#EF466F");

                        col.Item().Element(container => TableHeader(container, new string[] { "Ürün", "Adet", "Birim Fiyat", "Toplam" }));
                        foreach (DataRow dr in data.Rows) {
                            col.Item().Element(container => TableRow(container, new string[] {
                                dr["Ürün"].ToString(),
                                dr["Adet"].ToString(),
                                Convert.ToDecimal(dr["Birim Fiyat"]).ToString("N2") + " ₺",
                                Convert.ToDecimal(dr["Toplam"]).ToString("N2") + " ₺"
                            }));
                        }
                    });

                    page.Footer().AlignRight().Text(x => {
                        x.Span("Sayfa "); x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
