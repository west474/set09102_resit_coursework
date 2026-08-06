using StarterApp.Database.Models;

namespace StarterApp.Repositories;

// extends IRepository.cs with item-specific query operations.
public interface IItemRepository : IRepository<Item>
{
    // search items by title or description.
    Task<List<Item>> SearchAsync(string query);

    // get items belonging to a certain category.
    Task<List<Item>> GetByCategoryAsync(int categoryId);
}