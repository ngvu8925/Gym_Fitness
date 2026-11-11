using System.Data;
using GymManagement.Api.DTOs;
using GymManagement.Api.Repositories;

namespace GymManagement.Api.Services
{
    public class MemberService
    {
        private readonly MemberRepository _repo;

        public MemberService(MemberRepository repo) => _repo = repo;

        public (bool Success, string Message, int MemberId) Add(MemberRequest member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                return (false, "Full name is required", 0);
            if (string.IsNullOrWhiteSpace(member.Phone))
                return (false, "Phone number is required", 0);

            try
            {
                int memberId = _repo.Add(member);
                return (true, "Member added successfully", memberId);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }

        public (bool Success, string Message) Update(int memberId, MemberRequest member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                return (false, "Full name is required");

            _repo.Update(memberId, member);
            return (true, "Updated successfully");
        }

        public void Delete(int memberId) => _repo.Delete(memberId);

        public DataTable Search(string? keyword) => _repo.Search(keyword);
    }
}
