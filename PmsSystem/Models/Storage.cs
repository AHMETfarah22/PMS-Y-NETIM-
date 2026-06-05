namespace PmsSystem.Models
{
    public class Storage
    {
        public int StorageID { get; set; }
        public int ProductID { get; set; }         // Ürün ID
        public string Barcode { get; set; }        // Barkod
        public string ItemName { get; set; }       // Ürün adı
        public string ManufacturerName { get; set; } // Üretici adı
        public string Location { get; set; }       // Nerede kaldı (raf/konum)
        public DateTime ArrivalDate { get; set; }  // Ne zaman geldi
        public int Quantity { get; set; }          // Kaç adet var
        public string Notes { get; set; }          // Notlar
    }
}
