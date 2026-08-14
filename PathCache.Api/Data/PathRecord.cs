namespace PathCache.Api.Data;

public class PathRecord
{
    public int Id { get; set; }
    public required string Source { get; set; }
    public required string Target { get; set; }
    public int Hops { get; set; }
    public string PathJson { get; set; } = string.Empty;
    public DateTime ComputedAt { get; set; }
}
