namespace FileSign.Controllers
{
    using FileSign.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Check if user exists
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email &&
                                                                      u.Password == password);
            if (user != null)
            {
                // Generate JWT token
                var token = GenerateToken(user.Username);
                return Ok(token);
            }
            return Unauthorized("Invalid credentials");

        }


        [Route("api/generateToken")]
        [HttpPost]
        public IActionResult GenerateToken(string Username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    return BadRequest(new { Message = "Username is required" });
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey:Secret"]));
                var tokenExpiryTime = Convert.ToInt64(_configuration["JWTKey:TokenExpiryTimeInSeconds"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _configuration["JWTKey:ValidIssuer"],
                    Audience = _configuration["JWTKey:ValidAudience"],
                    Expires = DateTime.UtcNow.AddSeconds(tokenExpiryTime),
                    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, Username)
                    }),
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new { Token = tokenString });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return BadRequest(new { Message = "Error generating token", Details = ex.Message });
            }
        }

        [Route("api/allUsers")]
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admins can access this endpoint
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    try
        //    {
        //        // Retrieve all users from the database
        //        var users = await _context.Users
        //            .Select(u => new
        //            {
        //                u.Id,
        //                u.Username,
        //                u.Email,
        //                u.Role
        //            })
        //            .ToListAsync();

        //        // Return users as JSON response
        //        return Ok(users);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "An error occurred while fetching the users: " + ex.Message);
        //    }
        //}

        //[Route("api/getAllUser")]
        //[HttpGet]
        //public async Task<IActionResult> GetUsers()
        //{
        //    if(_context.Users == null)
        //    {
        //        return NotFound();
        //    }
        //    return await _context.Users.ToListAsync();
        //}


    }

}
