
using WebApiGET.Models;

namespace WebApiGET.Repository;

public interface IOwnerRepository 
{
    IEnumerable<Owner> GetAllOwners();
    Owner? GetById(int id);
    void Add(Owner entity);
    void Update(Owner entity);
    void Delete(Owner entity);

    int SaveChanges();      // commit to DB
}