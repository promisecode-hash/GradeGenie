namespace GradeGenie.Infrastructure.AI;

public sealed class AiProviderOptions
{
    public const string SectionName = "AiProvider";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
