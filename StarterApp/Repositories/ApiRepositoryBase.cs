using System.Text.Json;

namespace StarterApp.Repositories;

// reference: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-casing
// makes property name matching with System.Text.Json case-insensitive.
public abstract class ApiRepositoryBase
{
    protected readonly HttpClient HttpClient;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiRepositoryBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
}