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
- `/telefonlar?page=1` paged phone list (page size: 48)
- `/telefonlar/marka/{brandSlug}?page=1` brand landing list
- `/telefon/{slug}` phone detail
- `/karsilastir` compare builder entry
- `/karsilastir/{slug1}-vs-{slug2}` SEO indexable compare page
- `/karsilastir/{slug1}-vs-{slug2}-vs-{slug3}(-vs-{slug4})` functional compare page (`noindex,follow`)
- `/sitemap.xml` sitemap index
- `/sitemaps/static.xml` static url sitemap page
- `/sitemaps/telefonlar-{page}.xml` paged phone sitemap pages
- `/robots.txt` robots with sitemap pointer
- `/system/info` lightweight info endpoint

## Notes

- Static template assets are served from `src/Kiyaslasana.PL/wwwroot/versus` (lowercase).
- Canonical URL is always emitted from layout.
- Listing pagination SEO:
  - `page=1` canonical stays queryless (`/telefonlar` or `/telefonlar/marka/{brandSlug}`)
  - `page>=2` canonical includes `?page=N` and emits `robots=noindex,follow`
  - `rel=prev` / `rel=next` link tags are emitted when applicable.
- Compare requests are rendered in normalized order without redirect.
- Compare URL format is `slug1-vs-slug2[-vs-slug3-vs-slug4]`.
- Guest compare limit: 2 phones, authenticated compare limit: 4 phones (server-side enforced).
- OutputCache is enabled for anonymous GET pages with route-based variation.
- EF Core migrations are in `src/Kiyaslasana.DAL/Migrations`.
- Import tool is available at `tools/Kiyaslasana.Import` (see `docs/IMPORT.md`).

See `docs/SETUP_DB.md` for database initialization.
