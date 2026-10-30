namespace ProductApi.Controllers;

[ApiController, Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    /// <summary>Search, filter, sort (name | price | newest) and paginate products.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductQuery q)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => p.Name.Contains(q.Search) || (p.Description != null && p.Description.Contains(q.Search)));
        if (q.CategoryId.HasValue) query = query.Where(p => p.CategoryId == q.CategoryId);
        if (q.MinPrice.HasValue) query = query.Where(p => p.Price >= q.MinPrice);
        if (q.MaxPrice.HasValue) query = query.Where(p => p.Price <= q.MaxPrice);

        query = (q.SortBy?.ToLowerInvariant(), q.Desc) switch
        {
            ("price", true) => query.OrderByDescending(p => p.Price),
            ("price", _) => query.OrderBy(p => p.Price),
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("name", _) => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.Id) // newest first by default
        };

        var total = await query.CountAsync();
        var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, Stock = p.Stock,
                CategoryId = p.CategoryId, CategoryName = p.Category!.Name, CreatedAt = p.CreatedAt
            }).ToListAsync();

        return new PagedResult<ProductDto> { Items = items, Page = q.Page, PageSize = q.PageSize, TotalCount = total };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var p = await db.Products.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new ProductDto
            {
                Id = x.Id, Name = x.Name, Description = x.Description, Price = x.Price, Stock = x.Stock,
                CategoryId = x.CategoryId, CategoryName = x.Category!.Name, CreatedAt = x.CreatedAt
            }).FirstOrDefaultAsync();
        return p == null ? NotFound() : p;
    }

    [Authorize, HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductInputDto dto)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest(new { message = "Category does not exist." });

        var p = new Product
        {
            Name = dto.Name, Description = dto.Description, Price = dto.Price,
            Stock = dto.Stock, CategoryId = dto.CategoryId
        };
        db.Products.Add(p);
        await db.SaveChangesAsync();
        var created = new ProductDto
        {
            Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, Stock = p.Stock,
            CategoryId = p.CategoryId, CreatedAt = p.CreatedAt,
            CategoryName = (await db.Categories.FindAsync(p.CategoryId))!.Name
        };
        return CreatedAtAction(nameof(Get), new { id = p.Id }, created);
    }

    [Authorize, HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductInputDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p == null) return NotFound();
        if (!await db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest(new { message = "Category does not exist." });

        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        p.CategoryId = dto.CategoryId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Admin only.</summary>
    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p == null) return NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
