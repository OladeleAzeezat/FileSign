namespace FileSign.Controllers
{
    using FileSign.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Cryptography;
    using System.Text;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] User user)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("User already exists.");

            // Hash password
            //user.PasswordHash = HashPassword(user.PasswordHash, out byte[] salt);
            //user.PasswordSalt = salt;

            // Save user
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginRequest)
        {
            // Check if user exists
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null)
                return BadRequest("Invalid email or password.");

            // Verify password
            //if (!VerifyPassword(loginRequest.PasswordHash, user.PasswordHash, user.PasswordSalt))
            //    return BadRequest("Invalid email or password.");

            return Ok("Login successful.");
        }

        //private string HashPassword(string password, out byte[] salt)
        //{
        //    using var hmac = new HMACSHA256();
        //    salt = hmac.Key; // Generate a random salt (key)
        //    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    return Convert.ToBase64String(hash);
        //}

        //private bool VerifyPassword(string password, string storedHash, byte[] salt)
        //{
        //    using var hmac = new HMACSHA256(salt); // Use the stored salt
        //    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    return Convert.ToBase64String(computedHash) == storedHash;
        //}


        //private string HashPassword(string password)
        //{
        //    using var hmac = new HMACSHA256();
        //    return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        //}

        //private bool VerifyPassword(string password, string storedHash)
        //{
        //    using var hmac = new HMACSHA256();
        //    var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        //    return computedHash == storedHash;
        //}
    }

}
