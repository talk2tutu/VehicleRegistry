using System.Net;
using System.Net.Http.Json;
using VehicleRegistry.Api.Contracts;

namespace VehicleRegistry.IntegrationTests;

[Collection(ApiCollection.Name)]
public class VehiclesApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_AtExactBoundary_Returns201WithUpperCategory()
    {
        await factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/vehicles",
            new CreateVehicleRequest("Aroha Ngata", 3, 2021, 500.00m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Equal("Medium", vehicle!.CategoryName);   // 500.00 belongs to Medium, not Light
        Assert.Equal("Honda", vehicle.ManufacturerName);
        Assert.Equal(500.00m, vehicle.WeightKg);
    }

    [Fact]
    public async Task Create_InvalidVehicle_Returns400WithEveryProblemAndSavesNothing()
    {
        await factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/vehicles",
            new CreateVehicleRequest("", 99, 1700, -5m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await response.ReadValidationErrorsAsync();
        Assert.Equal(4, errors.Length);   // owner, manufacturer, year, weight

        var vehicles = await _client.GetFromJsonAsync<List<VehicleResponse>>("/api/vehicles");
        Assert.Empty(vehicles!);
    }

    [Fact]
    public async Task GetAll_SortedByWeightDescending_ReturnsHeaviestFirst()
    {
        await factory.ResetDatabaseAsync();
        await _client.AddVehicleAsync("Light One", 450.50m);
        await _client.AddVehicleAsync("Heavy One", 3000m);
        await _client.AddVehicleAsync("Medium One", 2200m);

        var vehicles = await _client.GetFromJsonAsync<List<VehicleResponse>>(
            "/api/vehicles?sortBy=weight&sortDir=desc");

        Assert.Equal(
            new[] { "Heavy One", "Medium One", "Light One" },
            vehicles!.Select(v => v.OwnerName));
    }

    [Fact]
    public async Task GetAll_UnknownSortField_Returns400()
    {
        var response = await _client.GetAsync("/api/vehicles?sortBy=colour");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}