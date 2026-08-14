# PathCache API

REST API for storing and serving precomputed shortest paths between graph nodes.

## Stack

.NET 10, ASP.NET Core minimal API, Entity Framework Core (EF Core), Postgres, xUnit, Docker

## Getting Started

Start Postgres:

    docker run --name pathcache-db -e POSTGRES_PASSWORD=devpass -p 5432:5432 -d postgres:16

Create the database:

    docker exec -it pathcache-db psql -U postgres -c "CREATE DATABASE pathcache;"

Apply migrations and run:

    dotnet ef database update --project PathCache.Api
    dotnet run --project PathCache.Api

Swagger UI is at [http://localhost:5236/swagger](http://localhost:5236/swagger).

Connection string is in `appsettings.Development.json` and points at the local container above. In development, startup seeds 200 sample path records if the table is empty.

## Endpoints

| Method | Route | Example request | Example response |
| --- | --- | --- | --- |
| POST | `/api/paths` | `{"source":"Alpha","target":"Bravo","hops":2,"pathJson":"[\"Alpha\",\"Charlie\",\"Bravo\"]"}` | `201 Created`, `Location: /api/paths/1`<br>`{"id":1,"source":"Alpha","target":"Bravo","hops":2,"pathJson":"[\"Alpha\",\"Charlie\",\"Bravo\"]","computedAt":"2026-08-13T12:00:00Z"}` |
| GET | `/api/paths/{id}` | `GET /api/paths/1` | `200 OK`<br>`{"id":1,"source":"Alpha","target":"Bravo","hops":2,"pathJson":"[\"Alpha\",\"Charlie\",\"Bravo\"]","computedAt":"2026-08-13T12:00:00Z"}`<br>or `404 Not Found` |
| GET | `/api/paths?source=X&target=Y` | `GET /api/paths?source=Alpha&target=Bravo` | `200 OK`<br>`{"id":1,"source":"Alpha","target":"Bravo","hops":2,"pathJson":"[\"Alpha\",\"Charlie\",\"Bravo\"]","computedAt":"2026-08-13T12:00:00Z"}`<br>or `404 Not Found` |
| GET | `/api/paths/stats` | `GET /api/paths/stats` | `200 OK`<br>`{"totalCount":200,"averageHops":4.02,"longest":{"id":57,"source":"Kronos","target":"Nyx","hops":6,"pathJson":"[...]","computedAt":"2026-08-13T12:00:00Z"}}` |
| DELETE | `/api/paths/{id}` | `DELETE /api/paths/1` | `204 No Content`<br>or `404 Not Found` |
| GET | `/health` | `GET /health` | `200 OK`<br>`{"status":"healthy"}` |

`POST /api/paths` returns `400 Bad Request` with a `ValidationProblem` body when `source`/`target` are empty or `hops <= 0`, e.g.:

    {"errors":{"Source":["Source is required."]}}

## Performance

Cached `?source=X&target=Y` lookups return in ~2-3ms locally (10 runs via `curl -w "%{time_total}"`, first request excluded as a cold-start outlier). Measured on localhost against a local Postgres container, not a production network path.

## Running Tests

    dotnet test

`PathCache.Tests` spins up the API in-process via `WebApplicationFactory`, swapping the Postgres provider for EF Core's in-memory provider, so no database is required.

## Docker

    docker build -t pathcache-api .
    docker run -p 8080:8080 pathcache-api

Multi-stage build (`sdk:10.0` to publish, `aspnet:10.0` to run) on a non-root user, ~95 MB.

## Scaffolding

Commands used to originally scaffold the solution, for reference:

    dotnet new sln -n PathCache --format slnx
    dotnet new webapi -n PathCache.Api -o PathCache.Api
    dotnet new xunit -n PathCache.Tests -o PathCache.Tests
    dotnet sln add PathCache.Api PathCache.Tests
    dotnet add PathCache.Tests reference PathCache.Api

    dotnet add PathCache.Api package Npgsql.EntityFrameworkCore.PostgreSQL
    dotnet add PathCache.Api package Microsoft.EntityFrameworkCore.Design
    dotnet add PathCache.Api package Microsoft.EntityFrameworkCore.Relational
    dotnet add PathCache.Api package Swashbuckle.AspNetCore

    dotnet add PathCache.Tests package Microsoft.AspNetCore.Mvc.Testing
    dotnet add PathCache.Tests package Microsoft.EntityFrameworkCore.InMemory

    dotnet new tool-manifest
    dotnet tool install dotnet-ef
