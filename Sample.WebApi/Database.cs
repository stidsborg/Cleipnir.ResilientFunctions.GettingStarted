using Cleipnir.ResilientFunctions.PostgreSQL;

namespace Sample.WebApi;

public static class Database
{
    public const string ConnectionString = "Server=localhost;Port=5432;Userid=postgres;Password=Pa55word!;Database=orderprocessorsample;";
    
    public static Task CreateIfNotExists()
        => DatabaseHelper.CreateDatabaseIfNotExists(ConnectionString);

    public static Task RecreateDatabase()
        => DatabaseHelper.RecreateDatabase(ConnectionString);
}