using Microsoft.EntityFrameworkCore;
using PathCache.Api.Data;
using PathCache.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PathCacheDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PathCacheDbContext>();
    Seeder.Run(db, 200);
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapPathEndpoints();

app.Run();