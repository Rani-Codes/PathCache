namespace PathCache.Api.Dtos;

public class CreatePathRequest
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Hops { get; set; }
    public string PathJson { get; set; } = string.Empty;
}
