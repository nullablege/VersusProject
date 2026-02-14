# Import Guide

This repository includes a data import console tool:

- `tools/Kiyaslasana.Import`

Supported modes:

1. `import-json`
2. `import-mysql`

## Build once

```bash
dotnet restore Kiyaslasana.sln
dotnet build Kiyaslasana.sln
```

## JSON import

Use a JSON file containing an array of objects. Property names can be `PascalCase` or `snake_case`.

```bash
dotnet run --project tools/Kiyaslasana.Import -- import-json \
  --sqlserver "Server=(localdb)\\MSSQLLocalDB;Database=KiyaslasanaDb;Trusted_Connection=True;TrustServerCertificate=True" \
  --json "C:\\data\\telefonlar.json" \
  --truncate true
```

Arguments:

- `--sqlserver`: SQL Server connection string
- `--json`: JSON file path
- `--truncate`: optional (`true`/`false`), default `false`

## MySQL -> SQL Server import

```bash
dotnet run --project tools/Kiyaslasana.Import -- import-mysql \
  --mysql "Server=127.0.0.1;Port=3306;Database=source_db;User ID=root;Password=your_password;Allow User Variables=True" \
  --sqlserver "Server=(localdb)\\MSSQLLocalDB;Database=KiyaslasanaDb;Trusted_Connection=True;TrustServerCertificate=True" \
  --truncate true
```

Arguments:

- `--mysql`: MySQL connection string (reads from `telefonlar` table)
- `--sqlserver`: SQL Server connection string
- `--truncate`: optional (`true`/`false`), default `false`

## Optional local MySQL with Docker

```bash
docker run --name kiyaslasana-mysql -e MYSQL_ROOT_PASSWORD=pass -e MYSQL_DATABASE=source_db -p 3306:3306 -d mysql:8
```

Then import your dump into `source_db` and run `import-mysql`.

## Notes

- Tool only touches `telefonlar` rows; Identity tables are not modified.
- Duplicate slugs are skipped.
- Inserts are batched (1000 rows per save).
