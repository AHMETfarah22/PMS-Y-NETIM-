namespace PmsSystem.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public int FloorID { get; set; }
        public int RoomTypeID { get; set; }
        public int Capacity { get; set; } = 2; // Total beds
        public int OccupiedBeds { get; set; } = 0; // Currently occupied beds
        public string Status { get; set; } = "Available"; // Available, Occupied, Reserved, Maintenance


        public string Description { get; set; }
    }
}
