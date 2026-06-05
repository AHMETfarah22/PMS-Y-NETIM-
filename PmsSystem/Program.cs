using PmsSystem.Forms;
using PmsSystem.Helpers;
using PmsSystem.Database;

namespace PmsSystem;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try { Database.DatabaseHelper.InitializeDatabase(); } catch(Exception ex) { MessageBox.Show("Veritabanı başlatılamadı: " + ex.Message); }
        
        // Auto-migrate CRM columns (Preferences, VipStatus, Allergies) if missing
        try { DataAccess.EnsureCrmColumns(); } catch { }
        
        // Process expired reservations (No-Show check)
        try { DataAccess.ProcessNoShowReservations(); } catch { }

        Application.Run(new LoginForm());
    }
}