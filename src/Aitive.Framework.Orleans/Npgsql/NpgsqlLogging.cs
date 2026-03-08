using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Orleans.Npgsql;

public static partial class NpgsqlLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Orleans PostgreSQL schema already exists, skipping creation"
    )]
    public static partial void LogSchemaAlreadyExists(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Orleans PostgreSQL schema")]
    public static partial void LogCreatingSchema(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Executed Orleans schema script: {ResourceName}"
    )]
    public static partial void LogScriptExecuted(this ILogger logger, string resourceName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Orleans PostgreSQL schema created successfully"
    )]
    public static partial void LogSchemaCreated(this ILogger logger);
}
