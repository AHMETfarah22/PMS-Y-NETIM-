namespace PmsApi.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string? IdentityNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties for Frontend
        public List<CustomerMessage> Messages { get; set; } = new();
        public List<CustomerReservation> Reservations { get; set; } = new();
    }

    public class CustomerMessage
    {
        public int MessageID { get; set; }
        public int CustomerID { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public string Direction { get; set; } = "Incoming";
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerReservation
    {
        public int ReservationID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
