using System;

namespace DuckDbExcelImport
{
    public static class DataValidator
    {
        public static ValidationResult ValidateRow(object? rawOrderId, object? rawProduct, object? rawQuantity, object? rawPrice, object? rawDate)
        {
            var result = new ValidationResult();

            string? orderId = rawOrderId?.ToString()?.Trim();
            if (string.IsNullOrEmpty(orderId)) result.Errors.Add("Пустой OrderId");

            string? product = rawProduct?.ToString()?.Trim();
            if (string.IsNullOrEmpty(product)) result.Errors.Add("Пустой Product");

            if (!int.TryParse(rawQuantity?.ToString(), out int quantity))
                result.Errors.Add($"Кривое количество: '{rawQuantity}'");
            else if (quantity <= 0)
                result.Errors.Add($"Quantity <= 0 ({quantity})");

            string? priceStr = rawPrice?.ToString()?.Replace('.', ',');
            if (!decimal.TryParse(priceStr, out decimal price))
                result.Errors.Add($"Кривая цена: '{rawPrice}'");
            else if (price < 0)
                result.Errors.Add($"Цена отрицательная ({price})");

            if (!DateTime.TryParse(rawDate?.ToString(), out DateTime date))
                result.Errors.Add($"Неверный формат даты: '{rawDate}'");

            if (result.IsValid)
            {
                result.ValidTransaction = new Transaction
                {
                    OrderId = orderId!,
                    Product = product!,
                    Quantity = quantity,
                    Price = price,
                    Date = date
                };
            }

            return result;
        }
    }
}
