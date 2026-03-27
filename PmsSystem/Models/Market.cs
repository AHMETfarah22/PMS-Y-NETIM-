namespace PmsSystem.Models
{
    public class Market
    {
        public int MarketID { get; set; }
        public int StorageID { get; set; }    // Depodaki ürünün ID'si (Bağlantı)
        public string Barcode { get; set; }   // Barkod
        public string ItemName { get; set; }  // Ürün isim
        public int Quantity { get; set; }     // Kaç tane var
        public decimal Price { get; set; }    // Fiyat
    }
}
