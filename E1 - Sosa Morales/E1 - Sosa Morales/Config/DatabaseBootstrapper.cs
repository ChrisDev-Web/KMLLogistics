using Microsoft.Data.SqlClient;

namespace E1___Sosa_Morales.Config;

public static class DatabaseBootstrapper
{
    public static async Task EnsureDatabaseExistsAsync(
        string? connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var safeDatabaseName = databaseName.Replace("]", "]]");
        var escapedDatabaseName = databaseName.Replace("'", "''");
        await using var command = connection.CreateCommand();
        command.CommandText = $"IF DB_ID(N'{escapedDatabaseName}') IS NULL CREATE DATABASE [{safeDatabaseName}];";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
