namespace PmsSystem.Models
{
    public class RoomType
    {
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public int MaxOccupancy { get; set; }
        public decimal BasePrice { get; set; }
    }
}
