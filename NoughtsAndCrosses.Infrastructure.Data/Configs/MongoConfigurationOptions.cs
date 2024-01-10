namespace NoughtsAndCrosses.Infrastructure.Data.Configs;

public class MongoConfigurationOptions
{
    public MongoConfigurationOptions(
        string host,
        string user,
        string password,
        string database,
        int? port = null)
    {
        Host = host;
        Port = port;
        User = user;
        Password = password;
        Database = database;
    }

    public string Host { get; set; }
    public int? Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }

    public string ConnectionStringDev => Port.HasValue
        ? $"mongodb://{User}:{Password}@{Host}:{Port}" 
        : $"mongodb://{User}:{Password}@{Host}";
    
    public string ConnectionStringProd => $"mongodb+srv://{User}:{Password}@{Host}";
}