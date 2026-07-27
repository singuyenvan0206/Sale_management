using Xunit;
using ShopManager.Core.DTOs;
using System.Collections.Generic;

namespace ShopManager.Tests
{
    public class ApiResponseTests
    {
        [Fact]
        public void ApiResponse_Ok_ReturnsSuccessResult()
        {
            var data = new { Id = 1, Name = "Sản phẩm A" };
            var response = ApiResponse<object>.Ok(data, "Thành công");

            Assert.True(response.Success);
            Assert.Equal("Thành công", response.Message);
            Assert.Equal(data, response.Data);
            Assert.Null(response.Errors);
        }

        [Fact]
        public void ApiResponse_Fail_ReturnsErrorResult()
        {
            var errors = new List<string> { "Lỗi 1", "Lỗi 2" };
            var response = ApiResponse<object>.Fail("Thất bại", errors);

            Assert.False(response.Success);
            Assert.Equal("Thất bại", response.Message);
            Assert.Null(response.Data);
            Assert.Equal(errors, response.Errors);
        }
    }
}
