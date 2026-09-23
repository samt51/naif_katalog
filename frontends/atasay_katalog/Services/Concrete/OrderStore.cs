using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;

namespace atasay_katalog.Services.Concrete;

public sealed class OrderStore : IOrderStore, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string databasePath;
    private readonly string pdfDirectory;
    private readonly string webRootPath;
    private readonly string connectionString;
    private readonly ConcurrentDictionary<string, string?> productImageByCode = new(StringComparer.OrdinalIgnoreCase);

    public OrderStore(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        pdfDirectory = Path.Combine(dataDirectory, "orders");
        Directory.CreateDirectory(pdfDirectory);
        databasePath = Path.Combine(dataDirectory, "orders.db");
        webRootPath = environment.WebRootPath;
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
        Initialize();
    }

    public async Task<(List<OrderRecord> Items, int Total)> ListAsync(int page, int pageSize, OrderListFilter? filter, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = Open();
            await using var count = connection.CreateCommand();
            await using var list = connection.CreateCommand();
            var where = BuildWhere(filter, count, list);
            count.CommandText = "SELECT COUNT(*) FROM Orders " + where;
            var total = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            list.CommandText = $"""
                SELECT {Columns} FROM Orders {where}
                ORDER BY CreatedUtc DESC
                LIMIT $take OFFSET $skip
                """;
            list.Parameters.AddWithValue("$take", pageSize);
            list.Parameters.AddWithValue("$skip", (page - 1) * pageSize);
            var items = new List<OrderRecord>();
            await using var reader = await list.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                items.Add(Read(reader, includeItems: false));
            return (items, total);
        }
        finally { gate.Release(); }
    }

    private static string BuildWhere(OrderListFilter? filter, SqliteCommand count, SqliteCommand list)
    {
        if (filter == null || !filter.HasAny) return "";
        var clauses = new List<string>();
        void Like(string sql, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            clauses.Add(sql);
            var token = "%" + value.Trim() + "%";
            count.Parameters.AddWithValue(name, token);
            list.Parameters.AddWithValue(name, token);
        }
        void Equal(string sql, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            clauses.Add(sql);
            count.Parameters.AddWithValue(name, value.Trim());
            list.Parameters.AddWithValue(name, value.Trim());
        }

        Like("(OrderNumber LIKE $q OR AccountName LIKE $q OR AccountEmail LIKE $q OR AccountCompany LIKE $q OR CompanyName LIKE $q OR FirstName LIKE $q OR LastName LIKE $q OR PhoneNumber LIKE $q OR ItemsJson LIKE $q)", "$q", filter.Search);
        Like("OrderNumber LIKE $orderNumber", "$orderNumber", filter.OrderNumber);
        Like("(AccountName LIKE $account OR AccountEmail LIKE $account OR AccountCompany LIKE $account)", "$account", filter.Account);
        Like("(FirstName LIKE $customer OR LastName LIKE $customer OR CompanyName LIKE $customer OR (FirstName || ' ' || LastName) LIKE $customer)", "$customer", filter.Customer);
        Like("PhoneNumber LIKE $phone", "$phone", filter.Phone);
        Like("ItemsJson LIKE $productCode", "$productCode", filter.ProductCode);
        Equal("EmailStatus = $emailStatus", "$emailStatus", filter.EmailStatus);
        Equal("Status = $status", "$status", OrderStatuses.IsValid(filter.Status) ? filter.Status : null);
        if (filter.AccountId is > 0)
        {
            clauses.Add("AccountId = $accountId");
            count.Parameters.AddWithValue("$accountId", filter.AccountId.Value);
            list.Parameters.AddWithValue("$accountId", filter.AccountId.Value);
        }

        var fromUtc = ToUtcBoundary(filter.FromDate, endOfDay: false);
        if (fromUtc.HasValue)
        {
            clauses.Add("CreatedUtc >= $fromDate");
            var value = fromUtc.Value.ToString("O");
            count.Parameters.AddWithValue("$fromDate", value);
            list.Parameters.AddWithValue("$fromDate", value);
        }
        var toUtc = ToUtcBoundary(filter.ToDate, endOfDay: true);
        if (toUtc.HasValue)
        {
            clauses.Add("CreatedUtc <= $toDate");
            var value = toUtc.Value.ToString("O");
            count.Parameters.AddWithValue("$toDate", value);
            list.Parameters.AddWithValue("$toDate", value);
        }

        return clauses.Count == 0 ? "" : "WHERE " + string.Join(" AND ", clauses);
    }

    private static DateTime? ToUtcBoundary(DateTime? date, bool endOfDay)
    {
        if (!date.HasValue) return null;
        var local = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Unspecified);
        if (endOfDay) local = local.AddDays(1).AddTicks(-1);
        return TimeZoneInfo.ConvertTimeToUtc(local, IstanbulTimeZone);
    }

    private static readonly TimeZoneInfo IstanbulTimeZone = ResolveIstanbul();

    private static TimeZoneInfo ResolveIstanbul()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
    }

    public async Task<OrderRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = Open();
            return await FindById(connection, id, cancellationToken);
        }
        finally { gate.Release(); }
    }

    public async Task<byte[]?> GetPdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var path = PdfPath(id);
        if (!File.Exists(path)) return null;
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    public async Task<OrderRecord> CreateOrGetAsync(ConfirmOrderRequest request, OrderAccountSnapshot account, CancellationToken cancellationToken = default)
    {
        var pdf = DecodePdf(request.PdfBase64);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = Open();
            var existing = await FindByRequestId(connection, request.RequestId, cancellationToken);
            if (existing != null) return existing;

            var order = new OrderRecord
            {
                Id = Guid.NewGuid(),
                RequestId = request.RequestId,
                OrderNumber = await NextOrderNumber(connection, cancellationToken),
                CreatedUtc = DateTime.UtcNow,
                AccountId = account.Id,
                AccountName = account.Name,
                AccountEmail = account.Email,
                AccountCompany = account.Company,
                CompanyName = request.CompanyName.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                EmailStatus = "Pending",
                Status = OrderStatuses.Pending,
                StatusChangedUtc = DateTime.UtcNow,
                Items = request.Items,
                ItemCount = request.Items.Count,
                TotalQuantity = request.Items.Sum(i => i.Quantity > 0 ? i.Quantity : 1),
                HasPdf = true,
                PreviewImageUrl = request.Items.Select(i => i.ImageUrl).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
            };
            FillMissingImages(order.Items);
            order.PreviewImageUrl ??= order.Items.Select(i => i.ImageUrl).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));

            var temporary = PdfPath(order.Id) + ".tmp";
            await File.WriteAllBytesAsync(temporary, pdf, cancellationToken);
            try
            {
                await using var insert = connection.CreateCommand();
                insert.CommandText = """
                    INSERT INTO Orders (
                        Id, RequestId, OrderNumber, CreatedUtc, AccountId, AccountName, AccountEmail, AccountCompany,
                        CompanyName, FirstName, LastName, PhoneNumber, EmailStatus, EmailSentUtc, ItemsJson, Status, StatusChangedUtc)
                    VALUES (
                        $id, $requestId, $orderNumber, $createdUtc, $accountId, $accountName, $accountEmail, $accountCompany,
                        $companyName, $firstName, $lastName, $phoneNumber, $emailStatus, $emailSentUtc, $itemsJson, $status, $statusChangedUtc)
                    """;
                Bind(insert, order);
                await insert.ExecuteNonQueryAsync(cancellationToken);
                File.Move(temporary, PdfPath(order.Id), true);
                return order;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                return await FindByRequestId(connection, request.RequestId, cancellationToken)
                    ?? throw new InvalidOperationException("Sipariş kaydı tekrar oluşturulamadı.");
            }
            catch
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                throw;
            }
        }
        finally { gate.Release(); }
    }

    public async Task<OrderRecord?> UpdateEmailStatusAsync(Guid id, bool sent, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = Open();
            await using var update = connection.CreateCommand();
            update.CommandText = """
                UPDATE Orders
                SET EmailStatus = $status, EmailSentUtc = $sentAt
                WHERE Id = $id
                """;
            update.Parameters.AddWithValue("$status", sent ? "Sent" : "Failed");
            update.Parameters.AddWithValue("$sentAt", sent ? DateTime.UtcNow.ToString("O") : DBNull.Value);
            update.Parameters.AddWithValue("$id", id.ToString("D"));
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0) return null;
            return await FindById(connection, id, cancellationToken);
        }
        finally { gate.Release(); }
    }

    public async Task<OrderRecord?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        if (!OrderStatuses.IsValid(status)) throw new ArgumentException("Geçersiz sipariş durumu.");
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = Open();
            await using var update = connection.CreateCommand();
            update.CommandText = """
                UPDATE Orders
                SET Status = $status, StatusChangedUtc = $changedAt
                WHERE Id = $id
                """;
            update.Parameters.AddWithValue("$status", status);
            update.Parameters.AddWithValue("$changedAt", DateTime.UtcNow.ToString("O"));
            update.Parameters.AddWithValue("$id", id.ToString("D"));
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0) return null;
            return await FindById(connection, id, cancellationToken);
        }
        finally { gate.Release(); }
    }

    private void Initialize()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Orders (
                Id TEXT PRIMARY KEY,
                RequestId TEXT NOT NULL UNIQUE,
                OrderNumber TEXT NOT NULL UNIQUE,
                CreatedUtc TEXT NOT NULL,
                AccountId INTEGER NOT NULL,
                AccountName TEXT NOT NULL,
                AccountEmail TEXT NOT NULL,
                AccountCompany TEXT NOT NULL,
                CompanyName TEXT NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                PhoneNumber TEXT NOT NULL,
                EmailStatus TEXT NOT NULL,
                EmailSentUtc TEXT,
                ItemsJson TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Pending',
                StatusChangedUtc TEXT
            );
            CREATE INDEX IF NOT EXISTS IX_Orders_CreatedUtc ON Orders(CreatedUtc DESC);
            CREATE INDEX IF NOT EXISTS IX_Orders_AccountId ON Orders(AccountId);
            """;
        command.ExecuteNonQuery();
        EnsureColumn(connection, "Status", "TEXT NOT NULL DEFAULT 'Pending'");
        EnsureColumn(connection, "StatusChangedUtc", "TEXT");
    }

    private static void EnsureColumn(SqliteConnection connection, string name, string definition)
    {
        using var info = connection.CreateCommand();
        info.CommandText = "PRAGMA table_info(Orders)";
        using var reader = info.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), name, StringComparison.OrdinalIgnoreCase))
                return;
        }
        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE Orders ADD COLUMN {name} {definition}";
        alter.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    private static async Task<string> NextOrderNumber(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var prefix = "ATASAY-" + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-";
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT OrderNumber FROM Orders WHERE OrderNumber LIKE $prefix ORDER BY OrderNumber DESC LIMIT 1";
        command.Parameters.AddWithValue("$prefix", prefix + "%");
        var last = await command.ExecuteScalarAsync(cancellationToken) as string;
        var next = 1;
        if (!string.IsNullOrWhiteSpace(last) && last.Length > prefix.Length
            && int.TryParse(last[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            next = parsed + 1;
        return prefix + next.ToString("0000", CultureInfo.InvariantCulture);
    }

    private async Task<OrderRecord?> FindById(SqliteConnection connection, Guid id, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {Columns} FROM Orders WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Read(reader, includeItems: true) : null;
    }

    private async Task<OrderRecord?> FindByRequestId(SqliteConnection connection, Guid requestId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {Columns} FROM Orders WHERE RequestId = $requestId";
        command.Parameters.AddWithValue("$requestId", requestId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Read(reader, includeItems: true) : null;
    }

    private const string Columns = "Id, RequestId, OrderNumber, CreatedUtc, AccountId, AccountName, AccountEmail, AccountCompany, CompanyName, FirstName, LastName, PhoneNumber, EmailStatus, EmailSentUtc, ItemsJson, Status, StatusChangedUtc";

    private OrderRecord Read(SqliteDataReader reader, bool includeItems)
    {
        var id = Guid.Parse(reader.GetString(0));
        var items = JsonSerializer.Deserialize<List<ConfirmOrderItem>>(reader.GetString(14), JsonOptions) ?? [];
        return new OrderRecord
        {
            Id = id,
            RequestId = Guid.Parse(reader.GetString(1)),
            OrderNumber = reader.GetString(2),
            CreatedUtc = DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            AccountId = reader.GetInt32(4),
            AccountName = reader.GetString(5),
            AccountEmail = reader.GetString(6),
            AccountCompany = reader.GetString(7),
            CompanyName = reader.GetString(8),
            FirstName = reader.GetString(9),
            LastName = reader.GetString(10),
            PhoneNumber = reader.GetString(11),
            EmailStatus = reader.GetString(12),
            EmailSentUtc = reader.IsDBNull(13) ? null : DateTime.Parse(reader.GetString(13), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Items = includeItems ? items : [],
            ItemCount = items.Count,
            TotalQuantity = items.Sum(i => i.Quantity > 0 ? i.Quantity : 1),
            HasPdf = File.Exists(PdfPath(id)),
            PreviewImageUrl = FillMissingImages(items),
            Status = reader.FieldCount > 15 && !reader.IsDBNull(15) && !string.IsNullOrWhiteSpace(reader.GetString(15))
                ? reader.GetString(15) : OrderStatuses.Pending,
            StatusChangedUtc = reader.FieldCount > 16 && !reader.IsDBNull(16)
                ? DateTime.Parse(reader.GetString(16), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : null
        };
    }

    private static void Bind(SqliteCommand command, OrderRecord order)
    {
        command.Parameters.AddWithValue("$id", order.Id.ToString("D"));
        command.Parameters.AddWithValue("$requestId", order.RequestId.ToString("D"));
        command.Parameters.AddWithValue("$orderNumber", order.OrderNumber);
        command.Parameters.AddWithValue("$createdUtc", order.CreatedUtc.ToString("O"));
        command.Parameters.AddWithValue("$accountId", order.AccountId);
        command.Parameters.AddWithValue("$accountName", order.AccountName);
        command.Parameters.AddWithValue("$accountEmail", order.AccountEmail);
        command.Parameters.AddWithValue("$accountCompany", order.AccountCompany);
        command.Parameters.AddWithValue("$companyName", order.CompanyName);
        command.Parameters.AddWithValue("$firstName", order.FirstName);
        command.Parameters.AddWithValue("$lastName", order.LastName);
        command.Parameters.AddWithValue("$phoneNumber", order.PhoneNumber);
        command.Parameters.AddWithValue("$emailStatus", order.EmailStatus);
        command.Parameters.AddWithValue("$emailSentUtc", (object?)order.EmailSentUtc?.ToString("O") ?? DBNull.Value);
        command.Parameters.AddWithValue("$itemsJson", JsonSerializer.Serialize(order.Items, JsonOptions));
        command.Parameters.AddWithValue("$status", string.IsNullOrWhiteSpace(order.Status) ? OrderStatuses.Pending : order.Status);
        command.Parameters.AddWithValue("$statusChangedUtc", (object?)order.StatusChangedUtc?.ToString("O") ?? DBNull.Value);
    }

    private static byte[] DecodePdf(string pdfBase64)
    {
        byte[] bytes;
        try { bytes = Convert.FromBase64String(pdfBase64); }
        catch (FormatException) { throw new ArgumentException("PDF dosyası geçersiz."); }
        if (bytes.Length is < 8 or > 25 * 1024 * 1024)
            throw new ArgumentException("PDF boyutu geçersiz.");
        if (!(bytes[0] == (byte)'%' && bytes[1] == (byte)'P' && bytes[2] == (byte)'D' && bytes[3] == (byte)'F'))
            throw new ArgumentException("Yalnızca PDF dosyası kaydedilebilir.");
        return bytes;
    }

    private string PdfPath(Guid id) => Path.Combine(pdfDirectory, id.ToString("D") + ".pdf");

    private string? FillMissingImages(List<ConfirmOrderItem> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.ImageUrl))
                item.ImageUrl = FindProductImage(item.Code);
        }
        return items.Select(i => i.ImageUrl).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
    }

    private string? FindProductImage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return productImageByCode.GetOrAdd(code.Trim(), ResolveProductImage);
    }

    private string? ResolveProductImage(string code)
    {
        var catalogRoot = Path.Combine(webRootPath, "images", "katalog");
        if (!Directory.Exists(catalogRoot)) return null;
        string? best = null;
        foreach (var file in Directory.EnumerateFiles(catalogRoot, code + "*.*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!name.StartsWith(code, StringComparison.OrdinalIgnoreCase)) continue;
            var remainder = name[code.Length..];
            if (remainder.Length > 0 && remainder[0] is not ('_' or '-')) continue;
            if (best == null || name.Length < Path.GetFileNameWithoutExtension(best).Length)
                best = file;
        }
        if (best == null) return null;
        var relative = Path.GetRelativePath(webRootPath, best).Replace('\\', '/');
        return "/" + relative;
    }

    public void Dispose() => gate.Dispose();
}
