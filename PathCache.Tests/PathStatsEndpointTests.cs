using System.Net.Http.Json;
using PathCache.Api.Dtos;

namespace PathCache.Tests;

/// <summary>
/// /api/paths/stats aggregates the whole table, so each test builds its own
/// factory (and therefore its own empty database) instead of sharing a fixture.
/// </summary>
public class PathStatsEndpointTests
{
    [Fact]
    public async Task Get_Stats_WithNoRecords_ReturnsEmptyAggregates()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var stats = await client.GetFromJsonAsync<PathStatsResponse>("/api/paths/stats");

        Assert.NotNull(stats);
        Assert.Equal(0, stats!.TotalCount);
        Assert.Equal(0, stats.AverageHops);
        Assert.Null(stats.Longest);
    }

    [Fact]
    public async Task Get_Stats_ReturnsCountAverageAndLongest()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var records = new[]
        {
            new CreatePathRequest { Source = "November", Target = "Oscar", Hops = 2, PathJson = "[\"November\",\"X\",\"Oscar\"]" },
            new CreatePathRequest { Source = "Papa", Target = "Quebec", Hops = 4, PathJson = "[\"Papa\",\"X\",\"Y\",\"Z\",\"Quebec\"]" },
            new CreatePathRequest { Source = "Romeo", Target = "Sierra", Hops = 6, PathJson = "[\"Romeo\",\"X\",\"Y\",\"Z\",\"W\",\"V\",\"Sierra\"]" },
        };

        foreach (var record in records)
        {
            var createResponse = await client.PostAsJsonAsync("/api/paths", record);
            createResponse.EnsureSuccessStatusCode();
        }

        var stats = await client.GetFromJsonAsync<PathStatsResponse>("/api/paths/stats");

        Assert.NotNull(stats);
        Assert.Equal(3, stats!.TotalCount);
        Assert.Equal(4, stats.AverageHops, precision: 5);
        Assert.NotNull(stats.Longest);
        Assert.Equal(6, stats.Longest!.Hops);
        Assert.Equal("Romeo", stats.Longest.Source);
        Assert.Equal("Sierra", stats.Longest.Target);
    }
}
