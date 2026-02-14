# Database Setup (SQL Server)

This project uses EF Core with SQL Server and Identity tables in the same database.

## 1) Configure connection string

Set `ConnectionStrings:Default` in:

- `src/Kiyaslasana.PL/appsettings.json`
- `src/Kiyaslasana.PL/appsettings.Development.json` (for local dev overrides)

## 2) Restore tools and packages

```bash
dotnet restore Kiyaslasana.sln
dotnet tool restore
```

## 3) Apply migrations

```bash
dotnet tool run dotnet-ef database update \
  --project src/Kiyaslasana.DAL/Kiyaslasana.DAL.csproj \
  --startup-project src/Kiyaslasana.PL/Kiyaslasana.PL.csproj
```

## 4) Add a new migration (when model changes)

```bash
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/Kiyaslasana.DAL/Kiyaslasana.DAL.csproj \
  --startup-project src/Kiyaslasana.PL/Kiyaslasana.PL.csproj \
  --output-dir Migrations
```

## 5) Development behavior

When `ASPNETCORE_ENVIRONMENT=Development`, app startup:

- Runs `Database.Migrate()` automatically.
- Seeds Identity roles: `Admin`, `Member`.

If you prefer manual migrations only, remove the startup migration block in `src/Kiyaslasana.PL/Program.cs`.

## Notes

- The latest migrations include listing-focused index `ix_telefonlar_marka_slug` to speed up brand paging queries.
