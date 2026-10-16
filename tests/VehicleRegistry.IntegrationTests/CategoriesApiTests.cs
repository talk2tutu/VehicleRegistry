using System.Net;
using System.Net.Http.Json;
using VehicleRegistry.Api.Contracts;

namespace VehicleRegistry.IntegrationTests;

[Collection(ApiCollection.Name)]
public class CategoriesApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ChangingHeavyStart_ImmediatelyRecategorisesExistingVehicle()
    {
        // The exact example from section 6 of the brief
        await factory.ResetDatabaseAsync();
        var vehicle = await _client.AddVehicleAsync("Jane Turei", 2200m);
        Assert.Equal("Medium", vehicle.CategoryName);

        var heavy = await _client.GetCategoryAsync("Heavy");
        var response = await _client.PutAsJsonAsync($"/api/categories/{heavy.Id}",
            new CategoryRequest("Heavy", 2000m, "truck"));
        response.EnsureSuccessStatusCode();

        var reloaded = await _client.GetFromJsonAsync<VehicleResponse>($"/api/vehicles/{vehicle.Id}");
        Assert.Equal("Heavy", reloaded!.CategoryName);
    }

    [Fact]
    public async Task Update_ThatWouldOverlap_Returns400AndSavesNothing()
    {
        await factory.ResetDatabaseAsync();
        var heavy = await _client.GetCategoryAsync("Heavy");

        var response = await _client.PutAsJsonAsync($"/api/categories/{heavy.Id}",
            new CategoryRequest("Heavy", 500m, "truck"));   // same start as Medium

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(await response.ReadValidationErrorsAsync(), e => e.Contains("overlap"));

        var unchanged = await _client.GetCategoryAsync("Heavy");
        Assert.Equal(2500m, unchanged.MinWeightKg);
    }

    [Fact]
    public async Task Delete_CategoryStartingAtZero_Returns400()
    {
        await factory.ResetDatabaseAsync();
        var light = await _client.GetCategoryAsync("Light");

        var response = await _client.DeleteAsync($"/api/categories/{light.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(await response.ReadValidationErrorsAsync(), e => e.Contains("0 kg"));
    }

    [Fact]
    public async Task Delete_MiddleCategory_LowerCategoryTakesOverItsRange()
    {
        await factory.ResetDatabaseAsync();
        var vehicle = await _client.AddVehicleAsync("Jane Turei", 2200m);
        var medium = await _client.GetCategoryAsync("Medium");

        var response = await _client.DeleteAsync($"/api/categories/{medium.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var light = await _client.GetCategoryAsync("Light");
        Assert.Equal(2500m, light.MaxWeightKg);   // Light now runs up to Heavy

        var reloaded = await _client.GetFromJsonAsync<VehicleResponse>($"/api/vehicles/{vehicle.Id}");
        Assert.Equal("Light", reloaded!.CategoryName);
    }

    [Fact]
    public async Task Create_WithUnknownIcon_Returns400()
    {
        await factory.ResetDatabaseAsync();

        var response = await _client.PostAsJsonAsync("/api/categories",
            new CategoryRequest("Super", 8000m, "rocket"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(await response.ReadValidationErrorsAsync(), e => e.Contains("unknown icon"));
    }
}