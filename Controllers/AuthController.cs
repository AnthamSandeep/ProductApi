using Microsoft.AspNetCore.Identity;
namespace ProductApi.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher, ITokenService tokens) : ControllerBase
{
    /// <summary>Create a new user account (role: User).</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
            return Conflict(new { message = "Username or email already exists." });

        var user = new User { Username = dto.Username, Email = dto.Email, Role = "User" };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Ok(tokens.CreateToken(user));
    }

    /// <summary>Login and receive a JWT. Demo admin: admin / Admin@123</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(tokens.CreateToken(user));
    }
}
