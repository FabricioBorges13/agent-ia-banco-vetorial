namespace AgentDeveloper.Configuration;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public bool UseHttps { get; set; } = false;
    public string? ApiKey { get; set; }
    public string Collection { get; set; } = "knowledge";
    public int Dimensions { get; set; } = 768;
}
