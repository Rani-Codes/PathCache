FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PathCache.Api/PathCache.Api.csproj PathCache.Api/
RUN dotnet restore PathCache.Api/PathCache.Api.csproj

COPY PathCache.Api/ PathCache.Api/
RUN dotnet publish PathCache.Api/PathCache.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app .
RUN chown -R app:app /app
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "PathCache.Api.dll"]
