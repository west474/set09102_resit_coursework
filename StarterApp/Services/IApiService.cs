using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Services;

public interface IApiService
{
    // Authentication
    Task<User> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<AuthToken> LoginAsync(string email, string password);
    Task<User> GetCurrentUserAsync();
    Task<User> GetUserProfileAsync(int userId);

    // Items
    // Task<List<Item>> GetItemsAsync(string category = null, string search = null, int page = 1);
    // Task<List<Item>> GetNearbyItemsAsync(double lat, double lon, double radius = 5.0, string category = null);
    // Task<Item> GetItemAsync(int id);
    // Task<Item> CreateItemAsync(CreateItemRequest request);
    // Task<Item> UpdateItemAsync(int id, UpdateItemRequest request);

    // Categories
    // Task<List<Category>> GetCategoriesAsync();

    // Rentals
    // Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    // Task<List<Rental>> GetIncomingRentalsAsync(string status = null);
    // Task<List<Rental>> GetOutgoingRentalsAsync(string status = null);
    // Task<Rental> GetRentalAsync(int id);
    // Task UpdateRentalStatusAsync(int rentalId, string status);

    // Reviews
    // Task<Review> CreateReviewAsync(int rentalId, int rating, string comment);
    // Task<List<Review>> GetItemReviewsAsync(int itemId, int page = 1);
    // Task<List<Review>> GetUserReviewsAsync(int userId, int page = 1);
}