# PathCache API

REST API for storing and serving precomputed shortest paths between graph nodes.
Built with ASP.NET Core and EF Core.

## Stack

.NET 10, ASP.NET Core minimal API, Entity Framework Core, Postgres, xUnit, Docker

## Getting Started

Start Postgres:

    docker run --name pathcache-db -e POSTGRES_PASSWORD=devpass -p 5432:5432 -d postgres:16

Apply migrations and run:

    dotnet ef database update --project PathCache.Api
    dotnet run --project PathCache.Api

Connection string is in `appsettings.Development.json` and points at the local container above.

## Status

Data layer and migrations in place. Endpoints in progress.