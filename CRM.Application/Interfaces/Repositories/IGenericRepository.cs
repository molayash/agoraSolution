using System.Linq.Expressions;

namespace CRM.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}
