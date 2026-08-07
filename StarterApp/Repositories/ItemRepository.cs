using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    // helper for getting categories for items.
    private static void GetCategoryNames(IEnumerable<Item> items)
    {
        foreach(var item in items)
            item.CategoryName = item.Category?.Name;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        var items = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Owner)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        GetCategoryNames(items);

        return items;
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item != null)
        {
            item.CategoryName = item.Category?.Name;
        }

        return item;
    }

    public async Task<Item> AddAsync(Item entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Items.Add(entity);
        await _context.SaveChangesAsync();

        if (entity.CategoryId.HasValue)
        {
            await _context.Entry(entity).Reference(i => i.Category).LoadAsync();
            entity.CategoryName = entity.Category?.Name;
        }

        return entity;
    }

    public async Task<Item?> UpdateAsync(Item entity)
    {
        var existing = await _context.Items.FindAsync(entity.Id);
        if (existing == null) return null;

        existing.Title = entity.Title;
        existing.Description = entity.Description;
        existing.CategoryId = entity.CategoryId;
        existing.Latitude = entity.Latitude;
        existing.Longitude = entity.Longitude;
        existing.DailyRate = entity.DailyRate;
        existing.IsAvailable = entity.IsAvailable;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _context.Entry(existing).Reference(i => i.Category).LoadAsync();
        existing.CategoryName = existing.Category?.Name;

        return existing;
    }

    public async Task<List<Item>> SearchAsync(string query)
    {
        var lowerCaseQuery = query.ToLower();

        var items = await _context.Items
            .Include(i => i.Category)
            .Where(i => EF.Functions.ILike(i.Title, $"%{query}%"))
            .ToListAsync();

        GetCategoryNames(items);

        return items;
    }

    public async Task<List<Item>> GetByCategoryAsync(int categoryId)
    {
        var items = await _context.Items
            .Include(i => i.Category)
            .Where(i => i.CategoryId == categoryId)
            .ToListAsync();

        GetCategoryNames(items);

        return items;
    }
}