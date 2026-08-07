using System.Net.Http.Json;
using System.Text.Json;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

// implements IItemRepository via REST API.
// reference: https://bit.ly/4fIjZAp .
public class ApiItemRepository : IItemRepository
{
    private readonly HttpClient _httpClient;
    private const string ItemsEndPoint = "items";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiItemRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;   
    }

    // --- CRUD functions --- //
    
    // get all items.
    public async Task<List<Item>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync(ItemsEndPoint);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ItemsResponse>(JsonOptions);
        return result?.Items ?? new List<Item>();
    }

    // get a specific item.
    public async Task<Item?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ItemsEndPoint}/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Item>(JsonOptions);
    }

    // create a new item.
    public async Task<Item> AddAsync(Item entity)
    {
        var response = await _httpClient.PostAsJsonAsync(ItemsEndPoint, entity);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<Item>(JsonOptions);
        return created;
    }

    // update an item.
    public async Task<Item?> UpdateAsync(Item entity)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ItemsEndPoint}/{entity.Id}", entity);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Item>(JsonOptions);
    }

    // delete an item.
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ItemsEndPoint}/{id}");
        
        // reference: https://tinyurl.com/4ftdxw3r
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    // search for an item.
    public async Task<List<Item>> SearchAsync(string query)
    {
        // reference: https://bit.ly/4hTviXO
        var url = $"{ItemsEndPoint}?search={Uri.EscapeDataString(query)}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ItemsResponse>(JsonOptions);
        return result?.Items ?? new List<Item>();
    }

    // filter by category.
    public async Task<List<Item>> GetByCategoryAsync(int categoryId)
    {
        var allItems = await GetAllAsync();
        return allItems.Where(i => i.CategoryId == categoryId).ToList();
    }

    private record ItemsResponse(List<Item> Items, int TotalItems, int Page, int PageSize, int TotalPages);
}