using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

return await ImportProgram.RunAsync(args);

internal static class ImportProgram
{
    private const int BatchSize = 1000;

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var mode = args[0].Trim().ToLowerInvariant();
        var options = ParseOptions(args, 1);

        try
        {
            return mode switch
            {
                "import-json" => await RunImportJsonAsync(options),
                "import-mysql" => await RunImportMySqlAsync(options),
                _ => UnknownMode(mode)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Import failed: {ex.Message}");
            return 2;
        }
    }

    private static async Task<int> RunImportJsonAsync(IReadOnlyDictionary<string, string> options)
    {
        var sqlServerConnection = GetRequiredOption(options, "sqlserver");
        var jsonPath = GetRequiredOption(options, "json");
        var truncate = GetOptionalBool(options, "truncate", false);

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("JSON file not found.", jsonPath);
        }

        await using var dbContext = CreateSqlServerContext(sqlServerConnection);
        await dbContext.Database.MigrateAsync();

        if (truncate)
        {
            await TruncateTelefonlarAsync(dbContext);
        }

        var existingSlugs = await LoadExistingSlugsAsync(dbContext);
        var phones = await ReadJsonAsync(jsonPath, existingSlugs);

        Console.WriteLine($"Prepared {phones.Count} records from JSON.");
        await InsertInBatchesAsync(dbContext, phones);
        Console.WriteLine("JSON import completed.");
        return 0;
    }

    private static async Task<int> RunImportMySqlAsync(IReadOnlyDictionary<string, string> options)
    {
        var mysqlConnection = GetRequiredOption(options, "mysql");
        var sqlServerConnection = GetRequiredOption(options, "sqlserver");
        var truncate = GetOptionalBool(options, "truncate", false);

        await using var dbContext = CreateSqlServerContext(sqlServerConnection);
        await dbContext.Database.MigrateAsync();
        if (truncate)
        {
            await TruncateTelefonlarAsync(dbContext);
        }

        var existingSlugs = await LoadExistingSlugsAsync(dbContext);
        var propertyMap = BuildTelefonPropertyMap();
        var batch = new List<Telefon>(BatchSize);
        var inserted = 0;
        var duplicateSlugs = 0;

        await using var mysql = new MySqlConnection(mysqlConnection);
        await mysql.OpenAsync();

        await using var cmd = mysql.CreateCommand();
        cmd.CommandText = "SELECT * FROM telefonlar";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var phone = MapFromDataRecord(reader, propertyMap);
            phone.Marka = NormalizeBrand(phone.Marka);
            var slug = NormalizeSlug(phone.Slug);
            if (slug.Length > 0)
            {
                if (!existingSlugs.Add(slug))
                {
                    duplicateSlugs++;
                    continue;
                }

                phone.Slug = slug;
            }
            else
            {
                phone.Slug = null;
            }

            batch.Add(phone);
            if (batch.Count >= BatchSize)
            {
                inserted += await FlushBatchAsync(dbContext, batch);
            }
        }

        inserted += await FlushBatchAsync(dbContext, batch);
        Console.WriteLine($"MySQL import completed. Inserted={inserted}, DuplicatesSkipped={duplicateSlugs}");
        return 0;
    }

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"Unknown mode: {mode}");
        PrintUsage();
        return 1;
    }

    private static IReadOnlyDictionary<string, string> ParseOptions(string[] args, int startIndex)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = startIndex; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = arg[2..];
            var value = "true";
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[i + 1];
                i++;
            }

            map[key] = value;
        }

        return map;
    }

    private static string GetRequiredOption(IReadOnlyDictionary<string, string> options, string key)
    {
        if (!options.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing required option --{key}");
        }

        return value;
    }

    private static bool GetOptionalBool(IReadOnlyDictionary<string, string> options, string key, bool defaultValue)
    {
        if (!options.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static KiyaslasanaDbContext CreateSqlServerContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<KiyaslasanaDbContext>()
            .UseSqlServer(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new KiyaslasanaDbContext(options);
    }

    private static async Task TruncateTelefonlarAsync(KiyaslasanaDbContext dbContext)
    {
        await dbContext.Telefonlar.ExecuteDeleteAsync();
        Console.WriteLine("telefonlar table truncated.");
    }

    private static async Task<HashSet<string>> LoadExistingSlugsAsync(KiyaslasanaDbContext dbContext)
    {
        var slugs = await dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => x.Slug != null && x.Slug != string.Empty)
            .Select(x => x.Slug!)
            .ToListAsync();

        return slugs
            .Select(NormalizeSlug)
            .Where(x => x.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static async Task<List<Telefon>> ReadJsonAsync(string jsonPath, HashSet<string> existingSlugs)
    {
        using var stream = File.OpenRead(jsonPath);
        using var doc = await JsonDocument.ParseAsync(stream);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("JSON root must be an array.");
        }

        var propertyMap = BuildTelefonPropertyMap();
        var phones = new List<Telefon>();
        var duplicateSlugs = 0;

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var phone = MapFromJsonElement(element, propertyMap);
            phone.Marka = NormalizeBrand(phone.Marka);
            var slug = NormalizeSlug(phone.Slug);
            if (slug.Length > 0)
            {
                if (!existingSlugs.Add(slug))
                {
                    duplicateSlugs++;
                    continue;
                }

                phone.Slug = slug;
            }
            else
            {
                phone.Slug = null;
            }

            phones.Add(phone);
        }

        if (duplicateSlugs > 0)
        {
            Console.WriteLine($"Skipped {duplicateSlugs} duplicate slug records from JSON.");
        }

        return phones;
    }

    private static async Task InsertInBatchesAsync(KiyaslasanaDbContext dbContext, IReadOnlyList<Telefon> phones)
    {
        var buffer = new List<Telefon>(BatchSize);
        var inserted = 0;

        foreach (var phone in phones)
        {
            buffer.Add(phone);
            if (buffer.Count >= BatchSize)
            {
                inserted += await FlushBatchAsync(dbContext, buffer);
            }
        }

        inserted += await FlushBatchAsync(dbContext, buffer);
        Console.WriteLine($"Inserted {inserted} records.");
    }

    private static async Task<int> FlushBatchAsync(KiyaslasanaDbContext dbContext, List<Telefon> batch)
    {
        if (batch.Count == 0)
        {
            return 0;
        }

        var count = batch.Count;
        await dbContext.Telefonlar.AddRangeAsync(batch);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        batch.Clear();
        return count;
    }

    private static Dictionary<string, PropertyInfo> BuildTelefonPropertyMap()
    {
        return typeof(Telefon)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanWrite && x.Name != nameof(Telefon.Id))
            .ToDictionary(
                x => NormalizeKey(x.Name),
                x => x,
                StringComparer.Ordinal);
    }

    private static Telefon MapFromJsonElement(JsonElement element, IReadOnlyDictionary<string, PropertyInfo> propertyMap)
    {
        var phone = new Telefon();

        foreach (var prop in element.EnumerateObject())
        {
            if (!propertyMap.TryGetValue(NormalizeKey(prop.Name), out var targetProperty))
            {
                continue;
            }

            var converted = ConvertJsonValue(prop.Value, targetProperty.PropertyType);
            targetProperty.SetValue(phone, converted);
        }

        return phone;
    }

    private static Telefon MapFromDataRecord(MySqlDataReader reader, IReadOnlyDictionary<string, PropertyInfo> propertyMap)
    {
        var phone = new Telefon();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            if (!propertyMap.TryGetValue(NormalizeKey(columnName), out var targetProperty))
            {
                continue;
            }

            var rawValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
            var converted = ConvertObjectValue(rawValue, targetProperty.PropertyType);
            targetProperty.SetValue(phone, converted);
        }

        return phone;
    }

    private static object? ConvertJsonValue(JsonElement value, Type targetType)
    {
        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (targetType == typeof(string))
        {
            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.ToString();
        }

        if (targetType == typeof(DateTimeOffset?) || targetType == typeof(DateTimeOffset))
        {
            if (value.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            {
                return dto;
            }

            return null;
        }

        return value.ToString();
    }

    private static object? ConvertObjectValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        if (targetType == typeof(DateTimeOffset?) || targetType == typeof(DateTimeOffset))
        {
            return value switch
            {
                DateTimeOffset dto => dto,
                DateTime dt => new DateTimeOffset(dt),
                string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) => parsed,
                _ => null
            };
        }

        return value;
    }

    private static string NormalizeKey(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var written = 0;

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer[written++] = char.ToLowerInvariant(ch);
            }
        }

        return new string(buffer[..written]);
    }

    private static string NormalizeSlug(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length <= TelefonConstraints.SlugMaxLength)
        {
            return normalized;
        }

        return normalized[..TelefonConstraints.SlugMaxLength];
    }

    private static string? NormalizeBrand(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length <= TelefonConstraints.MarkaMaxLength)
        {
            return normalized;
        }

        return normalized[..TelefonConstraints.MarkaMaxLength];
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  import-json --sqlserver \"<conn>\" --json \"<path>\" [--truncate true|false]");
        Console.WriteLine("  import-mysql --mysql \"<conn>\" --sqlserver \"<conn>\" [--truncate true|false]");
    }
}
