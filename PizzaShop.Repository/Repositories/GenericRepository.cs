using System.Linq.Expressions;
using PizzaShop.Entity.Models;
using PizzaShop.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PizzaShop.Repository.Repositories;

public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    private readonly PizzaShopContext _context;
    private readonly DbSet<T> _dbSet;
    
    public GenericRepository(PizzaShopContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

#region C : Create
/*------------------------------adds a new entity (record) to the database----------------------------------------
-------------------------------------------------------------------------------------------------------*/
    public async Task<bool> AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
        
    }

#endregion C : Create

#region R : Read
/*----------------------------To Get the all the values from table----------------------------------------
-------------------------------------------------------------------------------------------------------*/
    public IEnumerable<T> GetAll() => _dbSet;

    public IEnumerable<T> GetByCondition(Expression<Func<T, bool>> predicate)
    {
        return  _dbSet.Where(predicate);
    }


 /*------------------------------To Get sorted and paginated records with search functionality---------------
-------------------------------------------------------------------------------------------------------*/ 
    public async Task<(IEnumerable<T> items, int totalCount)> GetPagedRecordsAsync(
        int pageSize,
        int pageNumber,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null)
    {

        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query =  query.Include(include);
            }
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, totalCount);
        
    }

    
/*----------------------retrieves a single record from the database by its primary key (id)----------------------------------------
-------------------------------------------------------------------------------------------------------*/

    public async Task<T?> GetByIdAsync(long id) => await _dbSet.FindAsync(id);


/*----------------------fetches a single record from the database based on a given condition----------------------------------------
-------------------------------------------------------------------------------------------------------*/
    public async Task<T?> GetByStringAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

#endregion R : Read

#region U : Update
/*------------------------------updates an existing entity in the database----------------------------------------
-------------------------------------------------------------------------------------------------------*/
    public async Task<bool> UpdateAsync(T entity)
    {
        try{
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
        
    }

#endregion U : Update

#region D : Delete
/*------------------Deletes an entity from the database based on its id------------------------------------
-------------------------------------------------------------------------------------------------------*/
    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }


/*-------------------------Deletes multiple records based on a given condition (predicate)---------------
-------------------------------------------------------------------------------------------------------*/
    public async Task DeleteAssociatedEntitiesAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = _dbSet.Where(predicate);
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

#endregion D : Delete

#region Common
/*---------------Counts the number of records in a table, with an optional filter (predicate)-------------
-------------------------------------------------------------------------------------------------------*/
    public async Task<int> GetCount(Expression<Func<T, bool>>? predicate)
    {
        if (predicate is not null)
        {
            return await _dbSet.CountAsync(predicate);
        }
        return await _dbSet.CountAsync();
    }

    #endregion Common
}

