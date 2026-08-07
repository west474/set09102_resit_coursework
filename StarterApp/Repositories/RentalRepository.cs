using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;
    private readonly IAuthenticationService _authService;

    public RentalRepository(AppDbContext context, IAuthenticationService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<List<Rental>> GetAllAsync() => await _context.Rentals.ToListAsync();

    public async Task<Rental?> GetByIdAsync(int id) => await _context.Rentals.FindAsync(id);

    public async Task<Rental> AddAsync(Rental entity)
    {
        entity.Status = "Requested";
        entity.CreatedAt = DateTime.UtcNow;

        _context.Rentals.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Rental?> UpdateAsync(Rental entity)
    {
        var existing = await _context.Rentals.FindAsync(entity.Id);
        if (existing == null) return null;

        existing.StartDate = entity.StartDate;
        existing.EndDate = entity.EndDate;
        existing.Status = entity.Status;
        existing.TotalPrice = entity.TotalPrice;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<List<Rental>> GetIncomingAsync(string? status = null)
    {
        var userId = _authService.CurrentUser?.Id ?? 0;
        var query = _context.Rentals.Where(r => r.OwnerId == userId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        return await query.ToListAsync();
    }

    public async Task<List<Rental>> GetOutgoingAsync(string? status = null)
    {
        var userId = _authService.CurrentUser?.Id ?? 0;
        var query = _context.Rentals.Where(r => r.BorrowerId == userId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        return await query.ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int rentalId, string status)
    {
        var rental = await _context.Rentals.FindAsync(rentalId);
        if (rental == null) return false;

        rental.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}