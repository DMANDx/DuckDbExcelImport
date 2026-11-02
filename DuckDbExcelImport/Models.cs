using System;
using System.Collections.Generic;

namespace DuckDbExcelImport
{
    public class Transaction
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }

    public class ProductReportDto
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public decimal CumulativeRevenue { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = [];
        public Transaction? ValidTransaction { get; set; }
    }
}
