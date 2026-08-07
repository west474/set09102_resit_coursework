using System.Net.Http.Json;
using System.Text.Json;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public class ApiRentalRepository : ApiRepositoryBase, IRentalRepository
{
    private const string RentalsEndpoint = "rentals";

    public ApiRentalRepository(HttpClient httpClient) : base(httpClient){}

    public async Task<List<Rental>> GetAllAsync() => await GetIncomingAsync();

    public async Task<Rental?> GetByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"{RentalsEndpoint}/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
    }

    public async Task<Rental> AddAsync(Rental entity)
    {
        var request = new
        {
            itemId = entity.ItemId,
            startDate = entity.StartDate.ToString("yyyy-MM-dd"),
            endDate = entity.EndDate.ToString("yyyy-MM-dd")
        };

        var response = await HttpClient.PostAsJsonAsync(RentalsEndpoint, request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
        return created!;
    }

    public Task<Rental?> UpdateAsync(Rental entity) =>
        throw new NotSupportedException("Use UpdateStatusAsync — the API only supports status transitions.");

    public async Task<List<Rental>> GetIncomingAsync(string? status = null)
    {
        var url = string.IsNullOrEmpty(status) ? $"{RentalsEndpoint}/incoming" : $"{RentalsEndpoint}/incoming?status={Uri.EscapeDataString(status)}";
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>(JsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

    public async Task<List<Rental>> GetOutgoingAsync(string? status = null)
    {
        var url = string.IsNullOrEmpty(status) ? $"{RentalsEndpoint}/outgoing" : $"{RentalsEndpoint}/outgoing?status={Uri.EscapeDataString(status)}";
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>(JsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

    public async Task<bool> UpdateStatusAsync(int rentalId, string status)
    {
        var response = await HttpClient.PatchAsJsonAsync($"{RentalsEndpoint}/{rentalId}/status", new { status });
        return response.IsSuccessStatusCode;
    }

    private record RentalsResponse(List<Rental> Rentals, int TotalRentals);
}