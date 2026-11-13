using System.Net;
using System.Net.Http.Json;
using Tally.Domain;

namespace Tally.IntegrationTests;

public sealed class FilesApiTests(TallyApiFactory factory) : IClassFixture<TallyApiFactory>
{
    [Fact]
    public async Task Health_WithoutKey_Returns200()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Summary_WithoutKey_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/files/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Summary_WithWrongKey_Returns401()
    {
        var response = await Client("wrong-key").GetAsync("/api/files/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ValidCsv_ReturnsAverage()
    {
        using var form = CsvForm("product,quantity\nA,2\nB,4\n");

        var response = await Client().PostAsync("/api/files?column=quantity", form);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileResult>();
        Assert.NotNull(result);
        Assert.Equal(3m, result.Average);
    }

    [Fact]
    public async Task Upload_UnknownColumn_Returns400()
    {
        using var form = CsvForm("a,b\n1,2\n");

        var response = await Client().PostAsync("/api/files?column=price", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient Client(string apiKey = TallyApiFactory.ApiKey)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        return client;
    }

    private static MultipartFormDataContent CsvForm(string csv) =>
        new() { { new StringContent(csv), "file", "test.csv" } };
}
