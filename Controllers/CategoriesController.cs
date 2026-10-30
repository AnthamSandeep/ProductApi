namespace ProductApi.Controllers;

[ApiController, Route("api/[controller]")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll() =>
        await db.Categories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description, ProductCount = c.Products.Count })
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var c = await db.Categories.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CategoryDto { Id = x.Id, Name = x.Name, Description = x.Description, ProductCount = x.Products.Count })
            .FirstOrDefaultAsync();
        return c == null ? NotFound() : c;
    }

    [Authorize, HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryInputDto dto)
    {
        if (await db.Categories.AnyAsync(c => c.Name == dto.Name))
            return Conflict(new { message = "Category already exists." });
        var c = new Category { Name = dto.Name, Description = dto.Description };
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id },
            new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description });
    }

    [Authorize, HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryInputDto dto)
    {
        var c = await db.Categories.FindAsync(id);
        if (c == null) return NotFound();
        if (await db.Categories.AnyAsync(x => x.Name == dto.Name && x.Id != id))
            return Conflict(new { message = "Category name already in use." });
        c.Name = dto.Name;
        c.Description = dto.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Admin only. Fails if the category still has products.</summary>
    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Categories.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Products.Any()) return Conflict(new { message = "Category has products and cannot be deleted." });
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
