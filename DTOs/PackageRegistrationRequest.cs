namespace GymManagement.Api.DTOs
{
    public class PackageRegistrationRequest
    {
        public int MemberId { get; set; }
        public int PackageId { get; set; }
        public int BranchId { get; set; }
        public DateTime StartDate { get; set; }
        public int AccountId { get; set; }
        public string? PromoCode { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
    }
}
