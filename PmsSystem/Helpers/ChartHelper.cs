using System.Data;
using ScottPlot;
using ScottPlot.WinForms;
using PmsSystem.Database;
using System.Drawing;

namespace PmsSystem.Helpers
{
    public static class ChartHelper
    {
        public static FormsPlot GetMonthlyRevenueChart()
        {
            var dt = EnterpriseDataAccess.GetMonthlyRevenue(DateTime.Now.Year);
            double[] values = new double[12];
            string[] labels = { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

            for (int i = 0; i < dt.Rows.Count && i < 12; i++)
            {
                values[i] = Convert.ToDouble(dt.Rows[i]["Revenue"]);
            }

            var formsPlot = new FormsPlot();
            var plt = formsPlot.Plot;
            
            var bars = plt.Add.Bars(values);
            foreach (var bar in bars.Bars) bar.FillColor = ScottPlot.Color.FromHex("#4F46E5");

            plt.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                Enumerable.Range(0, 12).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray()
            );

            plt.Title("Aylık Gelir Analizi (₺)");
            plt.Axes.Margins(bottom: 0);
            
            return formsPlot;
        }

        public static FormsPlot GetOccupancyPieChart()
        {
            var dt = EnterpriseDataAccess.GetOccupancyStats();
            var slices = new List<ScottPlot.PieSlice>();

            foreach (DataRow row in dt.Rows)
            {
                string status = row["Status"].ToString()!;
                double count = Convert.ToDouble(row["Count"]);

                ScottPlot.Color color = status switch
                {
                    "Available" => ScottPlot.Color.FromHex("#22C55E"),
                    "Occupied" => ScottPlot.Color.FromHex("#EF4444"),
                    "Dirty" => ScottPlot.Color.FromHex("#F59E0B"),
                    "Maintenance" => ScottPlot.Color.FromHex("#64748B"),
                    _ => ScottPlot.Color.FromHex("#6366F1")
                };

                string label = status switch
                {
                    "Available" => "Müsait",
                    "Occupied" => "Dolu",
                    "Dirty" => "Kirli",
                    "Maintenance" => "Bakımda",
                    _ => status
                };

                slices.Add(new ScottPlot.PieSlice { Value = count, FillColor = color, Label = $"{label} ({count})" });
            }

            var formsPlot = new FormsPlot();
            var plt = formsPlot.Plot;
            
            var pie = plt.Add.Pie(slices);
            pie.ExplodeFraction = 0.1;
            
            plt.Title("Oda Doluluk Oranı");
            plt.HideGrid();
            plt.Axes.Frameless();

            return formsPlot;
        }
    }
}
