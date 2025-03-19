using Data;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class, IEntity
{

    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var key = entity.GetType().GetProperty("Id")?.GetValue(entity, null);
        if (key == null)
        {
            throw new ArgumentException("Entity does not have an 'Id' property.");
        }

        // נוודא שהישות כבר קיימת

        var existingEntity = await _dbSet.FindAsync(key);
        _context.Entry(existingEntity).State = EntityState.Detached;
        if (existingEntity == null)
        {
            throw new ArgumentException("Entity not found.");
        }

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        return entity;
    }
}
