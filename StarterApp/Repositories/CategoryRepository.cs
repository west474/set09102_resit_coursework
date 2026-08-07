using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public class CategoryRepository : IRepository<Category>
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _context.Categories.ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await _context.Categories.FindAsync(id);

    public async Task<Category> AddAsync(Category entity)
    {
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Category?> UpdateAsync(Category entity)
    {
        var existing = await _context.Categories.FindAsync(entity.Id);
        if (existing == null) return null;

        existing.Name = entity.Name;
        existing.Slug = entity.Slug;
        await _context.SaveChangesAsync();
        return existing;
    }
}