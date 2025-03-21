
using System.Linq.Expressions;

namespace PizzaShop.Repository.Interfaces;

public interface IGenericRepository<T>
    where T : class
{
    Task<bool> AddAsync(T entity);
    Task<long> AddAsyncReturnId(T entity);

    IEnumerable<T> GetAll();

    IEnumerable<T> GetByCondition(Expression<Func<T, bool>> predicate);

    Task<IEnumerable<T>> GetByConditionInclude(
        Expression<Func<T, bool>> predicate,
        List<Expression<Func<T, object>>>? includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>>? thenIncludes = null
    );

    Task<(IEnumerable<T> items, int totalCount)> GetPagedRecordsAsync
    (
        int pageSize,
        int pageNumber,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>>? thenIncludes = null
    );

    Task<T?> GetByIdAsync(long id);

    Task<T?> GetByStringAsync(Expression<Func<T, bool>> predicate);

    Task<bool> UpdateAsync(T entity);

    Task DeleteAsync(int id);

    Task DeleteAssociatedEntitiesAsync(Expression<Func<T, bool>> predicate);

    Task<int> GetCount(Expression<Func<T, bool>>? predicate = null);
    
}
