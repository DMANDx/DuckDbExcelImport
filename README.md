### Pipeline Performance Results

```text
=== PERFORMANCE LOG FROM 02.11.2026 10:56:26 ===
--------------------------------------------------------------------------------
10:56:28 | Initialization (recreation) of DuckDB database             | 2.167 sec (2167 ms)
10:56:30 | Data import from Excel (Success: 49625, Errors: 375)        | 2.243 sec (2243 ms)
10:56:31 | Execution of analytical LINQ query (Option 1)              | 0.507 sec (507 ms)
10:56:31 | Execution of window function via SqlQuery (Option 2)       | 0.286 sec (286 ms)
10:56:31 | Direct report export to Parquet format (Option 3)          | 0.067 sec (67 ms)
10:56:31 | TOTAL PIPELINE EXECUTION TIME                              | 5.288 sec (5288 ms)
```

