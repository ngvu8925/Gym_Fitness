using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class TrainerService
    {
        private readonly TrainerRepository _repo;

        public TrainerService(TrainerRepository repo) => _repo = repo;
        public TrainerService() : this(new TrainerRepository()) { }

        public DataTable List() => _repo.List();

        public (bool Success, string Message, int TrainerId) Add(TrainerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                return (false, "Full name is required", 0);

            int trainerId = _repo.Add(req);
            return (true, "Trainer added successfully", trainerId);
        }
    }
}
