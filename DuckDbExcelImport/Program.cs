using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DuckDbExcelImport
{
    internal class Program
    {
        private static void Main()
        {
            // провайдер кодировок для корректной работы ExcelDataReader
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string excelFilePath = "huge_data_anomalies.xlsx";
            string logFilePath = "import_errors.log";
            string parquetFilePath = "monthly_analytics.parquet";
            string perfLogPath = "performance_metrics.txt";
            
            using (var writer = new StreamWriter(perfLogPath, false, Encoding.UTF8))
            {
                writer.WriteLine($"=== ЛОГ ПРОИЗВОДИТЕЛЬНОСТИ ОТ {DateTime.Now} ===");
                writer.WriteLine(new string('-', 50));
            }

            var totalSw = Stopwatch.StartNew();
            
            Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ===");
            var stageSw = Stopwatch.StartNew();

            using (var context = new AppDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            stageSw.Stop();
            LogTime(perfLogPath, "Инициализация БД DuckDB", stageSw.Elapsed);

            
            Console.WriteLine("\n=== ЗАПУСК ИМПОРТА ===");
            if (!File.Exists(excelFilePath))
            {
                Console.WriteLine($"Положите файл '{excelFilePath}' рядом с .exe!");
                return;
            }

            stageSw.Restart();
            var (importedCount, errorCount) = ExcelImporter.Import(excelFilePath, logFilePath);
            stageSw.Stop();

            LogTime(perfLogPath, $"Импорт данных из Excel (Успешно: {importedCount}, Ошибок: {errorCount})", stageSw.Elapsed);
            Console.WriteLine($"\nИмпорт завершен.");


            // --- ЭТАП 3: АНАЛИЗ ДАННЫХ (LINQ) ---
            Console.WriteLine("\n=== АНАЛИЗ ДАННЫХ (LINQ) ===");
            stageSw.Restart();
            AnalyticsService.ExecuteLinqAnalysis();
            stageSw.Stop();
            LogTime(perfLogPath, "Выполнение аналитического LINQ-запроса (Вариант 1)", stageSw.Elapsed);


            // --- ЭТАП 4: АНАЛИЗ ДАННЫХ (ОКОННАЯ ФУНКЦИЯ SQL) ---
            Console.WriteLine("\n=== АНАЛИЗ ДАННЫХ (ОКОННАЯ ФУНКЦИЯ SQL) ===");
            stageSw.Restart();
            AnalyticsService.ExecuteWindowFunctionSql();
            stageSw.Stop();
            LogTime(perfLogPath, "Выполнение оконной функции через SqlQuery (Вариант 2)", stageSw.Elapsed);


            // --- ЭТАП 5: ЭКСПОРТ В PARQUET ---
            Console.WriteLine("\n=== ЭКСПОРТ В PARQUET ===");
            stageSw.Restart();
            AnalyticsService.ExecuteDirectExportToParquet(parquetFilePath);
            stageSw.Stop();
            LogTime(perfLogPath, "Прямой экспорт отчета в формат Parquet (Вариант 3)", stageSw.Elapsed);


            // --- ФИНАЛ ПАЙПЛАЙНА ---
            totalSw.Stop();
            LogTime(perfLogPath, "ОБЩЕЕ ВРЕМЯ РАБОТЫ", totalSw.Elapsed);

            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"\nВсе этапы успешно замерены! Результаты сохранены в: {Path.GetFullPath(perfLogPath)}");

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Вспомогательный статический метод для записи метрик времени в файл.
        /// </summary>
        private static void LogTime(string logPath, string stageName, TimeSpan elapsed)
        {
            using var writer = new StreamWriter(logPath, true, Encoding.UTF8);

            string timeString = $"{elapsed.TotalSeconds:F3} сек ({elapsed.TotalMilliseconds:F0} мс)";

            writer.WriteLine($"{DateTime.Now:HH:mm:ss} | {stageName,-70} | {timeString}");
        }
    }
}