using Aitive.Framework.Configuration;

namespace Aitive.Framework.Orleans.Npgsql;

public static class NpgsqlSiloBuilderExtensions
{
    private const string AdoNetInvariant = "Npgsql";

    extension(ISiloBuilder siloBuilder)
    {
        /// <summary>
        /// Configures the Orleans silo to use PostgreSQL for all enabled providers
        /// (clustering, persistence, reminders, streaming) and optionally creates the
        /// database schema from the embedded SQL scripts.
        /// </summary>
        /// <param name="connectionStringKey">The PostgreSQL connection string id in configuration.</param>
        /// <param name="configure">
        /// Optional callback to customise <see cref="NpgsqlSiloOptions"/>.
        /// Use this to disable auto-schema creation or individual providers.
        /// </param>
        /// <returns>The silo builder for chaining.</returns>
        public ISiloBuilder UseNpgsql(
            string connectionStringKey,
            Action<NpgsqlSiloOptions>? configure = null
        )
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringKey);

            var connectionString = siloBuilder.Configuration.GetRequiredConnectionString(
                connectionStringKey
            );

            var options = new NpgsqlSiloOptions();
            configure?.Invoke(options);

            if (options.AutoCreateSchema)
            {
                var setup = new NpgsqlDatabaseSetup(connectionString);
                setup.ApplySchemaAsync().GetAwaiter().GetResult();
            }

            if (options.ConfigureClustering)
            {
                siloBuilder.UseAdoNetClustering(clusteringOptions =>
                {
                    clusteringOptions.Invariant = AdoNetInvariant;
                    clusteringOptions.ConnectionString = connectionString;
                });
            }

            if (options.ConfigurePersistence)
            {
                siloBuilder.AddAdoNetGrainStorage(
                    options.DefaultStoreName,
                    storageOptions =>
                    {
                        storageOptions.Invariant = AdoNetInvariant;
                        storageOptions.ConnectionString = connectionString;
                    }
                );
            }

            if (options.ConfigureReminders)
            {
                siloBuilder.UseAdoNetReminderService(reminderOptions =>
                {
                    reminderOptions.Invariant = AdoNetInvariant;
                    reminderOptions.ConnectionString = connectionString;
                });
            }

            if (options.ConfigureStreaming)
            {
                siloBuilder.AddAdoNetStreams(
                    options.StreamProviderName,
                    streamOptions =>
                    {
                        streamOptions.Invariant = AdoNetInvariant;
                        streamOptions.ConnectionString = connectionString;
                    }
                );
            }

            return siloBuilder;
        }
    }
}
