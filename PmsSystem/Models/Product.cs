namespace PmsSystem.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string ManufacturerName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal SuggestedSalePrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
