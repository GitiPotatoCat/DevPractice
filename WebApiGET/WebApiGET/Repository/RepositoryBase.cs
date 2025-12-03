
using Microsoft.EntityFrameworkCore;
using WebApiGET.Data;

namespace WebApiGET.Repository;

public abstract class RepositoryBase<T> where T : class 
{
    protected readonly ApplicationDbContext _context;

    protected RepositoryBase(ApplicationDbContext context) 
    {
        _context = context;
    }

    public IEnumerable<T> GetAll() 
    {
        return _context.Set<T>().ToList();
    }

    public T GetById(int id) 
    {
        return _context.Set<T>().Find(id);
    }

    public void Add(T entity) 
    {
        _context.Set<T>().Add(entity);
    }

    public void Update(T entity) 
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity) 
    {
        _context.Set<T>().Remove(entity);
    }
}