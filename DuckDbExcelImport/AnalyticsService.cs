using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DuckDbExcelImport
{
    public static class AnalyticsService
    {
        public static void ExecuteLinqAnalysis()
        {            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n [АНАЛИЗ] ТОП-3 ТОВАРОВ ПО ВЫРУЧКЕ (LINQ)");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();

            using var context = new AppDbContext();

            var topProducts = context.Transactions
                .AsNoTracking()
                .GroupBy(t => t.Product)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(t => t.Quantity),
                    TotalRevenue = g.Sum(t => t.Quantity * t.Price)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(3)
                .ToList();

            foreach (var item in topProducts)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  > ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{item.ProductName,-15}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Продано: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{item.TotalSold,5} шт.");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  |  Выручка: ");
                Console.ForegroundColor = ConsoleColor.Green;
                
                // символ рубля
                Console.WriteLine($"{item.TotalRevenue:N2} руб.");

                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();
        }

        public static void ExecuteWindowFunctionSql()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n [АНАЛИЗ] НАРАСТАЮЩИЙ ИТОГ ПО ВРЕМЕННЫМ РЯДАМ (SQL)");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();

            using var context = new AppDbContext();
            int limitValue = 5;

            var reportLines = context.Database.SqlQuery<ProductReportDto>($@"
                SELECT Product, Quantity, Price, Date,
                       SUM(Quantity * Price) OVER (PARTITION BY Product ORDER BY Date) as CumulativeRevenue
                FROM Transactions
                ORDER BY Product, Date
                LIMIT {limitValue}")
                .ToList();

            foreach (var line in reportLines)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  > ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{line.Product,-15}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Дата: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{line.Date:yyyy-MM-dd}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  |  Кумулятивно: ");
                Console.ForegroundColor = ConsoleColor.Green;
                // Заменили символ рубля на безопасное текстовое "руб."
                Console.WriteLine($"{line.CumulativeRevenue:N2} руб.");

                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();
        }

        public static void ExecuteDirectExportToParquet(string outputFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n [ЭКСПОРТ] ГЕНЕРАЦИЯ ОТЧЕТА");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();

            using var context = new AppDbContext();

            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);

            context.Database.ExecuteSql($@"
                COPY (
                    SELECT strftime(Date, '%Y-%m') as Month, Product, SUM(Quantity * Price) as Revenue
                    FROM Transactions
                    GROUP BY Month, Product
                    ORDER BY Month, Revenue DESC
                ) TO {outputFilePath} (FORMAT 'PARQUET', COMPRESSION 'ZSTD');");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  [OK] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Аналитический отчет успешно сохранен: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(outputFilePath);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" ─────────────────────────────────────────────────────────────");
            Console.ResetColor();
        }
    }
}
