using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class PackageService
    {
        private readonly PackageRepository _repo;

        public PackageService(PackageRepository repo) => _repo = repo;
        public PackageService() : this(new PackageRepository()) { }

        public DataTable List() => _repo.List();

        public (bool Success, string Message) Add(PackageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Package name is required");
            if (req.Months <= 0)
                return (false, "Months must be greater than 0");
            if (req.Price < 0)
                return (false, "Price is invalid");

            _repo.Add(req);
            return (true, "Package added successfully");
        }

        public (bool Success, string Message, int RegistrationId, decimal DiscountAmount) Register(PackageRegistrationRequest req)
        {
            if (req.StartDate.Date < DateTime.Today)
                return (false, "Start date cannot be in the past", 0, 0);

            try
            {
                var (registrationId, discountAmount) = _repo.Register(req);
                return (true, "Package registered successfully", registrationId, discountAmount);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0, 0);
            }
        }

        public void Pause(PausePackageRequest req) => _repo.Pause(req);

        public void Resume(int pauseId) => _repo.Resume(pauseId);

        public DataTable ExpiringSoon(int days) => _repo.ExpiringSoon(days);
    }
}
