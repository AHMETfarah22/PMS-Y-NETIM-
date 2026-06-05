namespace PmsApi.Models
{
    public class RoomResponse
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalCapacity { get; set; }
        public int AvailableBedsCount { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
    }

    public class BookingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReservationCode { get; set; }
    }
}
