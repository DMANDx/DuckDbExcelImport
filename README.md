# !DB
A fast columnar database for analytics and ETL processes; a brief example comparing LINQ and pure SQL.
--------------------------------------------------
13:25:58 | Data import from Excel (Successful: 49625, Errors: 375)                | 2.154 sec (2154 ms)
13:25:58 | Execution of analytical LINQ query (Option 1)                          | 0.510 sec (510 ms)
13:25:58 | Execution of window function via SqlQuery (Option 2)                   | 0.261 sec (261 ms)
13:25:59 | Direct export of report to Parquet format (Option 3)                   | 0.069 sec (69 ms)
13:25:59 | TOTAL EXECUTION TIME FOR THE ENTIRE PIPELINE                           | 5.223 sec (5223 ms)
