using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathCache.Api.Data;
using PathCache.Api.Dtos;

namespace PathCache.Api.Endpoints;

public static class PathEndpoints
{
    public static void MapPathEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/paths");

        group.MapPost("/", CreatePath);
        group.MapGet("/stats", GetStats);
        group.MapGet("/{id:int}", GetById);
        group.MapGet("/", GetBySourceAndTarget);
        group.MapDelete("/{id:int}", DeletePath);
    }

    private static async Task<IResult> CreatePath(CreatePathRequest request, PathCacheDbContext db)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Source))
            errors.Add(nameof(request.Source), ["Source is required."]);

        if (string.IsNullOrWhiteSpace(request.Target))
            errors.Add(nameof(request.Target), ["Target is required."]);

        if (request.Hops <= 0)
            errors.Add(nameof(request.Hops), ["Hops must be greater than 0."]);

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var record = new PathRecord
        {
            Source = request.Source,
            Target = request.Target,
            Hops = request.Hops,
            PathJson = request.PathJson,
            ComputedAt = DateTime.UtcNow,
        };

        db.Paths.Add(record);
        await db.SaveChangesAsync();

        var response = ToResponse(record);
        return Results.Created($"/api/paths/{record.Id}", response);
    }

    private static async Task<IResult> GetById(int id, PathCacheDbContext db)
    {
        var record = await db.Paths.FindAsync(id);
        return record is null ? Results.NotFound() : Results.Ok(ToResponse(record));
    }

    private static async Task<IResult> GetBySourceAndTarget(
        [FromQuery] string source,
        [FromQuery] string target,
        PathCacheDbContext db)
    {
        var record = await db.Paths.FirstOrDefaultAsync(p => p.Source == source && p.Target == target);
        return record is null ? Results.NotFound() : Results.Ok(ToResponse(record));
    }

    private static async Task<IResult> GetStats(PathCacheDbContext db)
    {
        var totalCount = await db.Paths.CountAsync();

        if (totalCount == 0)
        {
            return Results.Ok(new PathStatsResponse
            {
                TotalCount = 0,
                AverageHops = 0,
                Longest = null,
            });
        }

        var averageHops = await db.Paths.AverageAsync(p => p.Hops);
        var longest = await db.Paths.OrderByDescending(p => p.Hops).FirstAsync();

        return Results.Ok(new PathStatsResponse
        {
            TotalCount = totalCount,
            AverageHops = averageHops,
            Longest = ToResponse(longest),
        });
    }

    private static async Task<IResult> DeletePath(int id, PathCacheDbContext db)
    {
        var record = await db.Paths.FindAsync(id);
        if (record is null)
            return Results.NotFound();

        db.Paths.Remove(record);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static PathResponse ToResponse(PathRecord record) => new()
    {
        Id = record.Id,
        Source = record.Source,
        Target = record.Target,
        Hops = record.Hops,
        PathJson = record.PathJson,
        ComputedAt = record.ComputedAt,
    };
}
