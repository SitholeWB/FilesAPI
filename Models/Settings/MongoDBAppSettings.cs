namespace Models;

public class MongoDBAppSettings
{
    public MongoDBAppSettings()
    {
        ConnectionString = "mongodb://localhost:27017";
    }

    public string ConnectionString { get; set; }
}