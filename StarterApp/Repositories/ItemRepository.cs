using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

// reference: https://bit.ly/4z2H2gQ
// implementation of IItemRepository.
public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    // --- CRUD functions --- //

    // get all items.
    public async Task<List<Item>> GetAllAsync()
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items: {ex.Message}");
            throw;
        }
    }

    // looks for a specific item.
    public async Task<Item?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading item {id}: {ex.Message}");
            throw;
        }
    }

    // creates an item.
    public async Task<Item> AddAsync(Item entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Items.Add(entity);
            await _context.SaveChangesAsync();

            if (entity.CategoryId.HasValue)
            {
                await _context.Entry(entity)
                    .Reference(i => i.Category)
                    .LoadAsync();
            }

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating item: {ex.Message}");
            throw;
        }
    }

    // updates an item.
    public async Task<Item?> UpdateAsync(Item entity)
    {
        try
        {
            var existing = await _context.Items.FindAsync(entity.Id);
            if (existing == null)
            {
                return null;
            }

            existing.Title = entity.Title;
            existing.Description = entity.Description;
            existing.CategoryId = entity.CategoryId;
            existing.Latitude = entity.Latitude;
            existing.Longitude = entity.Longitude;
            existing.DailyRate = entity.DailyRate;
            existing.IsAvailable = entity.IsAvailable;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _context.Entry(existing)
                .Reference(i => i.Category)
                .LoadAsync();

            return existing;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating item {entity.Id}: {ex.Message}");
            throw;
        }
    }

    // delete an item
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting item {id}: {ex.Message}");
            throw;
        }
    }

    // search for an item.
    public async Task<List<Item>> SearchAsync(string query)
    {
        try
        {
            var lowerCaseQuery = query.ToLower();

            return await _context.Items
                .Include(i => i.Category)
                .Where(i => i.Title.ToLower().Contains(lowerCaseQuery)
                    || (i.Description != null && i.Description.ToLower().Contains(lowerCaseQuery)))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for item: {ex.Message}");
            throw;
        }
    }

    // show items by category.
    public async Task<List<Item>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading category {categoryId}: {ex.Message}");
            throw;
        }
    }
}