
using Microsoft.AspNetCore.Mvc;
using WebApiGET.Models;
using WebApiGET.Repository;


namespace WebApiGET.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OwnerController : ControllerBase
{
    private readonly IOwnerRepository _repository;

    public OwnerController(IOwnerRepository repository) 
    {
        _repository = repository;
    }


    // GET: api/owner
    [HttpGet]
    public ActionResult<IEnumerable<Owner>> GetAll() 
    {
        var allOwners = _repository.GetAllOwners();
        return Ok(allOwners);
    }


    // GET: api/owner/{id}
    [HttpGet("{id:int}")]
    public ActionResult<Owner> GetById(int id) 
    {
        var specificOwner = _repository.GetById(id);
        if (specificOwner is null)
            return NotFound();
        return Ok(specificOwner);
    }


    // POST: api/owner
    [HttpPost]
    public ActionResult<Owner> Create([FromBody] Owner createOwner) 
    {
        if (createOwner is null)
            return BadRequest("Owner payload id null.");

        _repository.Add(createOwner);
        _repository.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = createOwner.OwnerId }, createOwner);
    }


    // PUT: api/owner/{id}
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Owner updateSpecificOwner) 
    {
        if (updateSpecificOwner is null)
            return BadRequest("Owner payload is null.");
        if (updateSpecificOwner.OwnerId != id)
            return BadRequest("Route id and body id mismatch.");

        var existing = _repository.GetById(id);
        if (existing is null)
            return NotFound();

        existing.Name = updateSpecificOwner.Name;
        existing.OrgName = updateSpecificOwner.OrgName;
        existing.ProdId = updateSpecificOwner.ProdId;
        existing.ProdName = updateSpecificOwner.ProdName;

        _repository.Update(existing);
        _repository.SaveChanges();

        return NoContent();
    }


    // DELETE: api/owner/{id}
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) 
    {
        var existing = _repository.GetById(id);
        if (existing is null)
            return NotFound();

        _repository.Delete(existing);
        _repository.SaveChanges();

        return NoContent();
    }
}