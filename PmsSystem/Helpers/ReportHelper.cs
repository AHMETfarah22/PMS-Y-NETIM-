using System;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace PmsSystem.Helpers
{
    public static class ReportHelper
    {
        static ReportHelper()
        {
            // QuestPDF License setup
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static void GenerateEndOfDayPdf(DataTable dtHistory)
        {
            try
            {
                var save = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"Gun_Sonu_Arsiv_{DateTime.Now:yyyyMMdd}.pdf" };
                if (save.ShowDialog() != DialogResult.OK) return;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("GÜN SONU ARŞİV RAPORU").FontSize(20).SemiBold().FontColor(Colors.Indigo.Medium);
                                col.Item().Text($"{DateTime.Now:dd.MM.yyyy HH:mm} tarihinde oluşturuldu").FontSize(10).Italic();
                            });
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Tarih");
                                header.Cell().Element(CellStyle).Text("Nakit");
                                header.Cell().Element(CellStyle).Text("Kart");
                                header.Cell().Element(CellStyle).Text("Gider");
                                header.Cell().Element(CellStyle).Text("Ciro");

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            foreach (DataRow r in dtHistory.Rows)
                            {
                                table.Cell().Element(RowStyle).Text(Convert.ToDateTime(r["ReportDate"]).ToString("dd.MM.yyyy"));
                                table.Cell().Element(RowStyle).Text($"{r["TotalCash"]:N2} ₺");
                                table.Cell().Element(RowStyle).Text($"{r["TotalCreditCard"]:N2} ₺");
                                table.Cell().Element(RowStyle).Text($"{r["TotalExpenses"]:N2} ₺");
                                table.Cell().Element(RowStyle).Text($"{r["TotalRevenue"]:N2} ₺");

                                static IContainer RowStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Sayfa ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(save.FileName);

                Process.Start(new ProcessStartInfo(save.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("PDF Hatası: " + ex.Message); }
        }

        public static void GenerateDailyTransactionsPdf(DataTable dtTrans, decimal cash, decimal cc, decimal exp)
        {
            try
            {
                var save = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"Gunluk_Islemler_{DateTime.Now:yyyyMMdd}.pdf" };
                if (save.ShowDialog() != DialogResult.OK) return;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("GÜNLÜK FİNANSAL İŞLEM DETAYI").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Tarih: {DateTime.Today:dd.MM.yyyy}").FontSize(11);
                            });
                        });

                        page.Content().PaddingVertical(10).Column(col => 
                        {
                            // Summary Section
                            col.Item().PaddingBottom(10).Row(row => {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c => {
                                    c.Item().Text("KASA ÖZETİ").SemiBold();
                                    c.Item().Text($"Nakit: {cash:N2} ₺");
                                    c.Item().Text($"Kart: {cc:N2} ₺");
                                    c.Item().Text($"Gider: {exp:N2} ₺");
                                    c.Item().Text($"Net: {(cash + cc - exp):N2} ₺").Bold().FontSize(12);
                                });
                                row.RelativeItem(); // Spacer
                            });

                            // Transactions Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(60);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Saat");
                                    header.Cell().Element(CellStyle).Text("Tip");
                                    header.Cell().Element(CellStyle).Text("Açıklama");
                                    header.Cell().Element(CellStyle).Text("Tutar");
                                    header.Cell().Element(CellStyle).Text("Yöntem");

                                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                });

                                foreach (DataRow r in dtTrans.Rows)
                                {
                                    table.Cell().Element(RowStyle).Text(r["Saat"].ToString());
                                    table.Cell().Element(RowStyle).Text(r["Tip"].ToString());
                                    table.Cell().Element(RowStyle).Text(r["Açıklama"].ToString());
                                    table.Cell().Element(RowStyle).Text($"{r["Tutar"]:N2} ₺");
                                    table.Cell().Element(RowStyle).Text(r["Yöntem"].ToString());

                                    static IContainer RowStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(3);
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); });
                    });
                }).GeneratePdf(save.FileName);

                Process.Start(new ProcessStartInfo(save.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("PDF Hatası: " + ex.Message); }
        }
    }
}
