using System.Linq.Expressions;

namespace StarterApp.Repositories;

// reference: https://bit.ly/4wEoB0l 
// This is a generic CRUD interface.
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}