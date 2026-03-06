using Aitive.Framework.Configuration;
using YesSql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;

namespace Aitive.Framework.YesSql;

public enum DatabaseType
{
    Sqlite,
    Postgres,
}

[ConfigurationOptions(Name = "Database")]
public sealed class YesSqlOptions
{
    public required DatabaseType DatabaseType { get; init; } = DatabaseType.Sqlite;

    public required string ConnectionString { get; init; }

    internal void Apply(global::YesSql.Configuration configuration)
    {
        switch (DatabaseType)
        {
            case DatabaseType.Postgres:
                configuration.UsePostgreSql(ConnectionString);
                break;
            case DatabaseType.Sqlite:
                configuration.UseSqLite(ConnectionString);
                break;
        }
    }
}
