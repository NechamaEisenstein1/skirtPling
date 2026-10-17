using Npgsql;

namespace SkirtPling.DAL.Context;

public static class PostgresConnectionString
{
    /// <summary>
    /// Hosts like Neon hand out URLs ("postgresql://user:pass@host/db?sslmode=require"), while Npgsql expects
    /// "Host=...;Username=..." pairs. Converts the URL form and leaves key/value strings unchanged.
    /// </summary>
    public static string Normalize(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var uri = new Uri(connectionString);
        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port < 0 ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
            // Hosted databases need TLS; "?sslmode=disable" allows a local PostgreSQL without TLS.
            SslMode = SslModeFrom(uri.Query)
        };
        return builder.ConnectionString;
    }

    private static SslMode SslModeFrom(string query)
    {
        var sslMode = query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .FirstOrDefault(pair => pair[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase) && pair.Length == 2)?[1];
        return Enum.TryParse<SslMode>(sslMode, ignoreCase: true, out var mode) ? mode : SslMode.Require;
    }
}
