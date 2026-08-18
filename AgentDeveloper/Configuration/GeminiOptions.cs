namespace AgentDeveloper.Configuration;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gemini-embedding-001";
    public int Dimensions { get; set; } = 768;
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
}
