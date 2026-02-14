# Kiyaslasana

Kiyaslasana is an ASP.NET Core MVC (.NET 8) application organized as a 4-layer solution.

## Solution Layout

- `Kiyaslasana.sln`: primary solution entry point.
- `src/Kiyaslasana.PL`: Presentation layer (MVC controllers, Razor views, static assets).
- `src/Kiyaslasana.BL`: Business layer (services, app contracts, compare rules, caching logic).
- `src/Kiyaslasana.EL`: Entity layer (domain entities).
- `src/Kiyaslasana.DAL`: Data access layer (EF Core DbContext, repository implementations, migrations).
- `tests/Kiyaslasana.Tests`: unit tests.

## Prerequisites

- .NET SDK 8.x
- SQL Server (LocalDB or full SQL Server)

## Run Commands

From repository root:

```bash
dotnet restore Kiyaslasana.sln
dotnet build Kiyaslasana.sln
dotnet test Kiyaslasana.sln
dotnet run --project src/Kiyaslasana.PL/Kiyaslasana.PL.csproj
```

## Key Routes

- `/` home
- `/telefonlar` phone list
- `/telefon/{slug}` phone detail
- `/karsilastir` compare builder entry
- `/karsilastir/{slugs}` compare (`slug1-vs-slug2` up to 4 for authenticated users)
- `/sitemap.xml` sitemap index
- `/sitemaps/static.xml` static url sitemap page
- `/sitemaps/telefonlar-{page}.xml` paged phone sitemap pages
- `/robots.txt` robots with sitemap pointer
- `/system/info` lightweight info endpoint

## Notes

- Static template assets are served from `src/Kiyaslasana.PL/wwwroot/versus` (lowercase).
- Canonical URL is always emitted from layout.
- Compare requests are rendered in normalized order without redirect.
- Compare URL format is `slug1-vs-slug2[-vs-slug3-vs-slug4]`.
- OutputCache is enabled for anonymous GET pages with route-based variation.
- EF Core migrations are in `src/Kiyaslasana.DAL/Migrations`.
- Import tool is available at `tools/Kiyaslasana.Import` (see `docs/IMPORT.md`).

See `docs/SETUP_DB.md` for database initialization.
