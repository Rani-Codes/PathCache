using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PathCache.Api.Dtos;

namespace PathCache.Tests;

public class PathEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ValidRequest_CreatesRecord()
    {
        var request = new CreatePathRequest
        {
            Source = "Alpha",
            Target = "Bravo",
            Hops = 2,
            PathJson = "[\"Alpha\",\"Charlie\",\"Bravo\"]",
        };

        var response = await _client.PostAsJsonAsync("/api/paths", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PathResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(request.Source, created.Source);
        Assert.Equal(request.Target, created.Target);
        Assert.Equal(request.Hops, created.Hops);
        Assert.Equal(request.PathJson, created.PathJson);
        Assert.NotEqual(default, created.ComputedAt);

        Assert.Equal($"/api/paths/{created.Id}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("", "Target", 1, "Source")]
    [InlineData("   ", "Target", 1, "Source")]
    [InlineData("Source", "", 1, "Target")]
    [InlineData("Source", "Target", 0, "Hops")]
    public async Task Post_InvalidRequest_ReturnsValidationProblem(
        string source, string target, int hops, string expectedErrorKey)
    {
        var request = new CreatePathRequest
        {
            Source = source,
            Target = target,
            Hops = hops,
            PathJson = "[]",
        };

        var response = await _client.PostAsJsonAsync("/api/paths", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(expectedErrorKey, problem!.Errors.Keys);
    }

    [Fact]
    public async Task Get_ById_ReturnsRecord()
    {
        var createRequest = new CreatePathRequest
        {
            Source = "Delta",
            Target = "Echo",
            Hops = 2,
            PathJson = "[\"Delta\",\"Foxtrot\",\"Echo\"]",
        };

        var created = await CreateAsync(createRequest);

        var response = await _client.GetAsync($"/api/paths/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<PathResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(createRequest.Source, fetched.Source);
        Assert.Equal(createRequest.Target, fetched.Target);
        Assert.Equal(createRequest.Hops, fetched.Hops);
        Assert.Equal(createRequest.PathJson, fetched.PathJson);
    }

    [Fact]
    public async Task Get_ById_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/paths/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_BySourceAndTarget_Hit_ReturnsRecord()
    {
        var createRequest = new CreatePathRequest
        {
            Source = "Golf",
            Target = "Hotel",
            Hops = 3,
            PathJson = "[\"Golf\",\"India\",\"Juliett\",\"Hotel\"]",
        };

        var created = await CreateAsync(createRequest);

        var response = await _client.GetAsync(
            $"/api/paths?source={createRequest.Source}&target={createRequest.Target}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<PathResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(createRequest.Source, fetched.Source);
        Assert.Equal(createRequest.Target, fetched.Target);
    }

    [Fact]
    public async Task Get_BySourceAndTarget_Miss_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/paths?source=Kilo&target=Lima");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesRecord_ThenReportsNotFound()
    {
        var created = await CreateAsync(new CreatePathRequest
        {
            Source = "Mike",
            Target = "November",
            Hops = 1,
            PathJson = "[\"Mike\",\"November\"]",
        });

        var response = await _client.DeleteAsync($"/api/paths/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var followUp = await _client.GetAsync($"/api/paths/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);

        var secondDelete = await _client.DeleteAsync($"/api/paths/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }

    private async Task<PathResponse> CreateAsync(CreatePathRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/paths", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PathResponse>();
        Assert.NotNull(created);
        return created!;
    }
}
