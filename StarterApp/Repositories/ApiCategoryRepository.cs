using System.Net.Http.Json;
using System.Text.Json;
using StarterApp.Database.Models;
using StarterApp.Repositories;

namespace StarterApp.Repositories;

public class ApiCategoryRepository : ApiRepositoryBase, IRepository<Category>
{
    private const string CategoriesEndpoint = "categories";

    public ApiCategoryRepository(HttpClient httpClient) : base(httpClient) {}

    public async Task<List<Category>> GetAllAsync()
    {
        var response = await HttpClient.GetAsync(CategoriesEndpoint);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CategoriesResponse>(JsonOptions);
        return result?.Categories ?? new List<Category>();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(c => c.Id == id);
    }

    public Task<Category> AddAsync(Category entity) =>
        throw new NotSupportedException("Categories are managed by the API and cannot be created by users.");

    public Task<Category?> UpdateAsync(Category entity) =>
        throw new NotSupportedException("Categories are managed by the API and cannot be edited by users.");

    private record CategoriesResponse(List<Category> Categories);
}