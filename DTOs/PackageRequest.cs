namespace GymManagement.Api.DTOs
{
    public class PackageRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "ThuongGoi";
        public int Months { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}
