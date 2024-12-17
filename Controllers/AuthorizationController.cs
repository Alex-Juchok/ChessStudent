using ChessSchoolAPI.Models;
using ChessSchoolAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChessSchoolAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public AuthController(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginModel registerModel)
        {
            // Проверяем, существует ли пользователь
            var existingUser = _userService.GetByUsername(registerModel.Username);
            if (existingUser != null)
            {
                return Conflict("Пользователь с таким именем уже существует.");
            }

            // Создаем нового пользователя
            var newUser = new User
            {
                Username = registerModel.Username,
                PasswordHash = _userService.HashPassword(registerModel.Password)
            };

            _userService.Create(newUser);

            // Генерируем JWT-токен
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, newUser.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                message = "Пользователь успешно зарегистрирован.",
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            // Ищем пользователя по имени
            var user = _userService.GetByUsername(loginModel.Username);
            if (user == null || user.PasswordHash != _userService.HashPassword(loginModel.Password))
            {
                return Unauthorized("Неверный логин или пароль.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginModel.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
