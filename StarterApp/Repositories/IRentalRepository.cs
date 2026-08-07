using StarterApp.Database.Models;

namespace StarterApp.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    // incoming rentals
    Task<List<Rental>> GetIncomingAsync(string? status = null);
    
    // outgoing rentals
    Task<List<Rental>> GetOutgoingAsync(string? status = null); 
    
    Task<bool> UpdateStatusAsync(int rentalId, string status);
}