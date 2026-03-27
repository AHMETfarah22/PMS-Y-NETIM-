namespace PmsSystem.Models
{
    public class Storage
    {
        public int StorageID { get; set; }
        public string ItemName { get; set; }       // Ürün adı
        public string Location { get; set; }       // Nerede kaldı (raf/konum)
        public DateTime ArrivalDate { get; set; }  // Ne zaman geldi
        public int PackageCount { get; set; }      // Kaç paket geldi
        public string Notes { get; set; }          // Notlar
    }
}
