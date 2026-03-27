namespace PmsSystem.Models
{
    public class Bed
    {
        public int BedID { get; set; }
        public int RoomTypeID { get; set; }
        public string BedType { get; set; } // Single, Double, etc.
        public int Capacity { get; set; }
    }
}
