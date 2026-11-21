using DbUp;

namespace BE.Infrastructure.Database;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var result = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .LogToConsole()
            .Build()
            .PerformUpgrade();

        if (!result.Successful)
            throw result.Error;
    }
}
