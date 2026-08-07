using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetForItemAsync(int itemId);
}