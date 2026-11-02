using System;
using FluentAssertions;
using Xunit;
using DuckDbExcelImport;

namespace DuckTest
{
    public class DataValidatorTests
    {
        [Fact]
        public void ValidateRow_ShouldReturnValidResult_WhenDataIsCorrect()
        {
            object rawOrderId = "ORD-2026-123456";
            object rawProduct = "Смартфон";
            object rawQuantity = "3";
            object rawPrice = "450.50";
            object rawDate = "2026-11-02 10:00:00";

            var result = DataValidator.ValidateRow(rawOrderId, rawProduct, rawQuantity, rawPrice, rawDate);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.ValidTransaction.Should().NotBeNull();
            result.ValidTransaction!.OrderId.Should().Be("ORD-2026-123456");
        }

        [Theory]
        [InlineData(null, "Смартфон", "1", "100", "2026-11-02", "Пустой OrderId")]
        [InlineData("ORD-123", "", "1", "100", "2026-11-02", "Пустой Product")]
        public void ValidateRow_ShouldReturnError_WhenRequiredFieldsAreMissing(
            object? orderId, object? product, object? qty, object? price, object? date, string expectedError)
        {
            var result = DataValidator.ValidateRow(orderId, product, qty, price, date);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(expectedError);
        }
    }
}
