using Cutly.Models;
using Cutly.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Cutly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            var existing = await _userService.GetByEmailAsync(user.Email);
            if (existing != null)
                return BadRequest("Email already registered.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userService.CreateAsync(user);
            return Ok("Signup successful!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existing = await _userService.GetByEmailAsync(user.Email);
            if (existing == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, existing.PasswordHash))
                return Unauthorized("Invalid credentials.");

            return Ok("Login successful!");
        }

        [HttpGet("testconnection")]
        public IActionResult TestConnection()
        {
            if (_userService.TestConnection())
                return Ok("MongoDB connection successful!");
            else
                return StatusCode(500, "MongoDB connection failed.");
        }
    }
} 