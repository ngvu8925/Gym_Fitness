using GymManagement.Api.DTOs.Common;

namespace GymManagement.Api.Tests
{
    /// <summary>
    /// Task 04: kiểm soát đầu vào phân trang (clamp) + tính toán PageResult.
    /// </summary>
    public class PaginationTests
    {
        [Fact]
        public void Page_NhoHon1_ClampVe1()
        {
            var q = new UserQueryParameters { Page = 0, PageSize = 10 };
            Assert.Equal(1, q.Page);
        }

        [Fact]
        public void PageSize_NhoHon1_ClampVeMacDinh10()
        {
            var q = new UserQueryParameters { Page = 1, PageSize = -5 };
            Assert.Equal(10, q.PageSize);
        }

        [Fact]
        public void PageSize_LonHon100_ClampVe100()
        {
            var q = new UserQueryParameters { Page = 1, PageSize = 9999 };
            Assert.Equal(100, q.PageSize);
        }

        [Fact]
        public void PageSizeHopLe_GiuNguyen()
        {
            var q = new UserQueryParameters { Page = 3, PageSize = 50 };
            Assert.Equal(3, q.Page);
            Assert.Equal(50, q.PageSize);
            Assert.Equal(100, q.Skip); // (3-1)*50
        }

        [Fact]
        public void Keyword_TrangHoacRong_TraVeNull()
        {
            var q1 = new UserQueryParameters { Keyword = "   " };
            var q2 = new UserQueryParameters { Keyword = "  nguyen  " };
            Assert.Null(q1.NormalizedKeyword);
            Assert.Equal("nguyen", q2.NormalizedKeyword);
        }

        [Fact]
        public void PageResult_TinhToanTotalPages_HasNext_HasPrevious()
        {
            var q = new UserQueryParameters { Page = 2, PageSize = 10 };
            var result = PageResult<object>.Of(new List<object>(), 45, q);

            Assert.Equal(45, result.TotalCount);
            Assert.Equal(5, result.TotalPages);   // ceil(45/10)
            Assert.True(result.HasNext);          // 2 < 5
            Assert.True(result.HasPrevious);      // 2 > 1
        }

        [Fact]
        public void PageResult_TrangCuoi_HasNextFalse()
        {
            var q = new UserQueryParameters { Page = 5, PageSize = 10 };
            var result = PageResult<object>.Of(new List<object>(), 45, q);

            Assert.Equal(5, result.TotalPages);
            Assert.False(result.HasNext);
            Assert.True(result.HasPrevious);
        }

        [Fact]
        public void PageResult_KhongDuLieu_TotalPages0_HaiNutDisable()
        {
            var q = new UserQueryParameters { Page = 1, PageSize = 10 };
            var result = PageResult<object>.Of(new List<object>(), 0, q);

            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
            Assert.False(result.HasNext);
            Assert.False(result.HasPrevious);
        }
    }
}
