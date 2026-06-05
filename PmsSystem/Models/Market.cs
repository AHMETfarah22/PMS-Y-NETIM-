namespace PmsSystem.Models
{
    public class Market
    {
        public int MarketID { get; set; }
        public int ProductID { get; set; }    // Ürün ID
        public int StorageID { get; set; }    // Geldiği depo stok ID
        public string Barcode { get; set; }   // Barkod
        public string ItemName { get; set; }  // Ürün isim
        public string ManufacturerName { get; set; } // Üretici adı
        public int Quantity { get; set; }     // Kaç tane var
        public decimal Price { get; set; }    // Satış Fiyatı
    }
}
