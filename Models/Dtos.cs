namespace ProductApi.Models;

public class RegisterDto
{
    [Required, StringLength(50, MinimumLength = 3)] public string Username { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public record AuthResponse(string Token, DateTime ExpiresAt, string Username, string Role);

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryInputDto
{
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [StringLength(300)] public string? Description { get; set; }
}

public class ProductInputDto
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [StringLength(1000)] public string? Description { get; set; }
    [Range(0.01, 1000000)] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required")] public int CategoryId { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ProductQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    /// <summary>name | price | newest (default)</summary>
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
