
using Microsoft.EntityFrameworkCore;
using WebApiGET.Data;
using WebApiGET.Models;

namespace WebApiGET.Repository;

public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext context) 
        : base(context) 
    { }


    public IEnumerable<Owner> GetAllOwners() => GetAll()
                                                    .OrderBy(ow => ow.Name)
                                                    .ToList();

    public new Owner? GetById(int id) => base.GetById(id);
    public new void Add(Owner entity) => base.Add(entity);
    public new void Update(Owner entity) => base.Update(entity);
    public new void Delete(Owner entity) => base.Delete(entity);

    public int SaveChanges() => _context.SaveChanges();
    
}