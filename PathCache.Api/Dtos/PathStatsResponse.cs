namespace PathCache.Api.Dtos;

public class PathStatsResponse
{
    public int TotalCount { get; set; }
    public double AverageHops { get; set; }
    public PathResponse? Longest { get; set; }
}
