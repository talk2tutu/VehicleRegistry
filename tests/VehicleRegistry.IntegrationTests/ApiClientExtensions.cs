using System.Net.Http.Json;
using VehicleRegistry.Api.Contracts;

namespace VehicleRegistry.IntegrationTests;

public static class ApiClientExtensions
{
    public static async Task<VehicleResponse> AddVehicleAsync(
        this HttpClient client, string ownerName, decimal weightKg)
    {
        var response = await client.PostAsJsonAsync("/api/vehicles",
            new CreateVehicleRequest(ownerName, 1, 2020, weightKg));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    public static async Task<CategoryResponse> GetCategoryAsync(this HttpClient client, string name)
    {
        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        return categories!.Single(c => c.Name == name);
    }

    public static async Task<string[]> ReadValidationErrorsAsync(this HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemBody>();
        return problem!.Errors["validation"];
    }

    private record ProblemBody(Dictionary<string, string[]> Errors);
}