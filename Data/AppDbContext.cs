using Microsoft.AspNetCore.Identity;
namespace ProductApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db, IPasswordHasher<User> hasher)
    {
        if (!db.Users.Any())
        {
            var admin = new User { Username = "admin", Email = "admin@example.com", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            db.Users.Add(admin);
        }
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Phones, laptops and gadgets" },
                new Category { Name = "Books", Description = "Printed and digital books" },
                new Category { Name = "Clothing", Description = "Men and women apparel" },
                new Category { Name = "Home & Kitchen", Description = "Appliances and furniture" });
            db.SaveChanges();

            var c = db.Categories.ToDictionary(x => x.Name, x => x.Id);
            (string n, string cat, decimal p, int s)[] items =
            {
                ("Wireless Mouse", "Electronics", 19.99m, 120), ("Mechanical Keyboard", "Electronics", 59.00m, 60),
                ("27-inch Monitor", "Electronics", 229.50m, 25), ("USB-C Hub", "Electronics", 34.90m, 80),
                ("Clean Code", "Books", 32.00m, 40), ("The Pragmatic Programmer", "Books", 38.50m, 35),
                ("C# in Depth", "Books", 44.00m, 20), ("Cotton T-Shirt", "Clothing", 12.99m, 200),
                ("Denim Jacket", "Clothing", 59.99m, 45), ("Running Shoes", "Clothing", 79.00m, 70),
                ("Electric Kettle", "Home & Kitchen", 24.50m, 90), ("Air Fryer", "Home & Kitchen", 99.00m, 30)
            };
            foreach (var i in items)
                db.Products.Add(new Product { Name = i.n, Price = i.p, Stock = i.s, CategoryId = c[i.cat], Description = i.n + " - sample product" });
        }
        db.SaveChanges();
    }
}
