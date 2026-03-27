namespace PmsSystem.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string IdentityNumber { get; set; }
        public int? UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
