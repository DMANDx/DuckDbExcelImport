using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DuckDB.NET.Data;
using ExcelDataReader;

namespace DuckDbExcelImport
{
    public static class ExcelImporter
    {
        public static (int Imported, int Errors) Import(string excelPath, string logPath)
        {
            int importedCount = 0;
            int errorCount = 0;
            int currentExcelRow = 1;

            // aut.inc
            int nextId = 1;

            using var excelStream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(excelStream);
            using var logWriter = new StreamWriter(logPath, false, Encoding.UTF8);

            // 1. бінарна з DuckDB
            using var connection = new DuckDBConnection("Data Source=analytics_data.db");
            connection.Open();

            if (!reader.Read()) return (0, 0); // шапка

            logWriter.WriteLine($"--- НАЧАЛО ЛОГА ИМПОРТА (потоковый Appender): {DateTime.Now} ---");
            
            using var appender = connection.CreateAppender("Transactions");

            while (reader.Read())
            {
                currentExcelRow++;

                var validation = DataValidator.ValidateRow(
                    reader.GetValue(0), reader.GetValue(1), reader.GetValue(2), reader.GetValue(3), reader.GetValue(4)
                );

                if (!validation.IsValid)
                {
                    errorCount++;
                    LogAnomaly(logWriter, currentExcelRow, validation.Errors, reader);
                    continue;
                }

                Transaction t = validation.ValidTransaction!;

                var row = appender.CreateRow();

                row.AppendValue(nextId)       
                   .AppendValue(t.OrderId)    
                   .AppendValue(t.Product)    
                   .AppendValue(t.Quantity)   
                   .AppendValue(t.Price)      
                   .AppendValue(t.Date);      

                row.EndRow();

                importedCount++;
                nextId++; // id

                if (importedCount % 1000 == 0)
                {
                    Console.Write($"\rОбработано строк в Excel: {currentExcelRow}... Залито в ядро базы: {importedCount}");
                }
            }
            
            appender.Close();

            return (importedCount, errorCount);
        }

        private static void LogAnomaly(StreamWriter writer, int rowNum, List<string> errors, IExcelDataReader reader)
        {
            writer.WriteLine($"Строка Excel №{rowNum} пропущена:");
            foreach (var err in errors) writer.WriteLine($"  - {err}");
            writer.WriteLine($"  Данные ячеек: [{reader.GetValue(0) ?? "NULL"}], [{reader.GetValue(1) ?? "NULL"}], [{reader.GetValue(2) ?? "NULL"}], [{reader.GetValue(3) ?? "NULL"}], [{reader.GetValue(4) ?? "NULL"}]");
            writer.WriteLine(new string('-', 60));
        }
    }
}
