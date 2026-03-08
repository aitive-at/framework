using System.Reflection;
using Aitive.Framework.Patterns.Disposal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Aitive.Framework.Orleans.Npgsql;

/// <summary>
/// Applies the embedded Orleans PostgreSQL schema scripts to a database.
/// The operation is idempotent — if the <c>OrleansQuery</c> table already exists,
/// no scripts are executed.
/// </summary>
public sealed partial class NpgsqlDatabaseSetup
{
    private static readonly string[] ScriptOrder =
    [
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-Main.sql",
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-Clustering.sql",
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-Persistence.sql",
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-Reminders.sql",
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-GrainDirectory.sql",
        "Aitive.Framework.Orleans.Npgsql.Scripts.PostgreSQL-Streaming.sql",
    ];

    private readonly string _connectionString;
    private readonly ILogger _logger;

    public NpgsqlDatabaseSetup(string connectionString, ILogger<NpgsqlDatabaseSetup>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
        _logger = logger ?? NullLogger<NpgsqlDatabaseSetup>.Instance;
    }

    /// <summary>
    /// Checks whether the Orleans schema exists and, if not, executes all
    /// embedded SQL scripts inside a single transaction.
    /// </summary>
    public async Task ApplySchemaAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (await SchemaExistsAsync(connection, cancellationToken))
        {
            LogSchemaAlreadyExists();
            return;
        }

        LogCreatingSchema();

        var assembly = typeof(NpgsqlDatabaseSetup).Assembly;

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var resourceName in ScriptOrder)
            {
                var sql = await ReadEmbeddedScriptAsync(assembly, resourceName, cancellationToken);

                await using var command = new NpgsqlCommand(sql, connection, transaction);
                await command.ExecuteNonQueryAsync(cancellationToken);

                LogScriptExecuted(resourceName);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        LogSchemaCreated();
    }

    private static async Task<bool> SchemaExistsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken
    )
    {
        const string sql =
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'orleansquery');";

        await using var command = new NpgsqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    private static async Task<string> ReadEmbeddedScriptAsync(
        Assembly assembly,
        string resourceName,
        CancellationToken cancellationToken
    )
    {
        await using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Orleans PostgreSQL schema already exists, skipping creation"
    )]
    private partial void LogSchemaAlreadyExists();

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Orleans PostgreSQL schema")]
    private partial void LogCreatingSchema();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Executed Orleans schema script: {ResourceName}"
    )]
    private partial void LogScriptExecuted(string resourceName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Orleans PostgreSQL schema created successfully"
    )]
    private partial void LogSchemaCreated();
}
