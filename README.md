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

## Secure DB Config

Never commit database passwords or any secret values. Treat any committed password as compromised and rotate it.

Development with user-secrets:

```bash
dotnet user-secrets init --project src/Kiyaslasana.PL/Kiyaslasana.PL.csproj
dotnet user-secrets set "ConnectionStrings:Default" "<real connection string>" --project src/Kiyaslasana.PL/Kiyaslasana.PL.csproj
```

Environment variable (all environments):

```bash
ConnectionStrings__Default="<real connection string>"
```

## Key Routes

- `/` home
- `/telefonlar?page=1` paged phone list (page size: 48)
- `/telefonlar/marka/{brandSlug}?page=1` brand landing list
- `/telefon/{slug}` phone detail
- `/karsilastir` compare builder entry
- `/karsilastir/{slug1}-vs-{slug2}` SEO indexable compare page
- `/karsilastir/{slug1}-vs-{slug2}-vs-{slug3}(-vs-{slug4})` functional compare page (`noindex,follow`)
- `/blog` public blog listing
- `/blog/{slug}` public blog detail
- `/admin/blog`, `/admin/blog/create`, `/admin/blog/edit/{id}` admin blog management (`Admin` role)
- `/admin/telefon-inceleme`, `/admin/telefon-inceleme/yeni`, `/admin/telefon-inceleme/duzenle/{slug}` phone review management (`Admin` role)
- `/giris`, `/kayit-ol`, `/cikis` auth flow (Identity MVC)
- `/sitemap.xml` sitemap index
- `/sitemaps/static.xml` static url sitemap page
- `/sitemaps/blog.xml` blog post sitemap page
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
- Guest compare limit: 2 phones, `Member`/`Admin` compare limit: 4 phones (server-side enforced).
- OutputCache is enabled for anonymous GET pages with route-based variation.
- Identity seed:
  - Roles `Admin` and `Member` are seeded idempotently on startup.
  - Optional first admin seed uses `Seed:Enabled`, `Seed:AdminEmail`, `Seed:AdminPassword`.
  - Startup migration behavior is controlled by `Database:ApplyMigrationsOnStartup`
    (`true` by default in Development, `false` in production appsettings).
- EF Core migrations are in `src/Kiyaslasana.DAL/Migrations`.
- Optional phone review content is stored in `telefon_reviews` and rendered on `/telefon/{slug}` when available.
- Import tool is available at `tools/Kiyaslasana.Import` (see `docs/IMPORT.md`).

See `docs/SETUP_DB.md` for database initialization.
