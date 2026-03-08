namespace Aitive.Framework.Orleans.Npgsql;

/// <summary>
/// Options for configuring Orleans with PostgreSQL via the <c>UseNpgsql</c> extension method.
/// </summary>
public sealed class NpgsqlSiloOptions
{
    /// <summary>
    /// When <c>true</c>, the embedded SQL scripts are executed against the database
    /// to create the Orleans schema if it does not already exist.
    /// Set to <c>false</c> when schema is managed externally (e.g. via migrations).
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoCreateSchema { get; set; } = true;

    /// <summary>
    /// The name used to register the default ADO.NET grain storage provider.
    /// Defaults to <c>"Default"</c>.
    /// </summary>
    public string DefaultStoreName { get; set; } = "Default";

    /// <summary>
    /// The name used to register the ADO.NET stream provider.
    /// Defaults to <c>"Default"</c>.
    /// </summary>
    public string StreamProviderName { get; set; } = "Default";

    /// <summary>
    /// Whether to configure ADO.NET clustering (membership tables).
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ConfigureClustering { get; set; } = true;

    /// <summary>
    /// Whether to configure ADO.NET grain state persistence.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ConfigurePersistence { get; set; } = true;

    /// <summary>
    /// Whether to configure ADO.NET reminder service.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ConfigureReminders { get; set; } = true;

    /// <summary>
    /// Whether to configure ADO.NET streaming.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ConfigureStreaming { get; set; } = true;
}