using UserService.Models;
using UserService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace userService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserService.Services.UserService _userService;

        public UserController(IConfiguration configuration, UserService.Services.UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllAsync()
        {
            
            var user = await _userService.GetAllAsync();
            return Ok(user);
        }

        // GET: api/Users/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> GetByIdAsync(string id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }


        [HttpPost("addUser")]
        public IActionResult Register([FromBody] LoginModel registerModel)
        {
            // Проверяем, существует ли пользователь
            var existingUser = _userService.GetByUsername(registerModel.Username);
            if (existingUser != null)
            {
                return Conflict("Пользователь c таким именем уже существует.");
            }

            // Создаем нового пользователя
            var newUser = new User
            {
                Username = registerModel.Username,
                PasswordHash = _userService.HashPassword(registerModel.Password)
            };

            _userService.Create(newUser);


            return Ok(new
            {
                message = "Пользователь успешно зарегистрирован.",
            });
        }

        

        // PUT: api/user/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, [FromBody] User updatedUser)
        {
            var existingUser = _userService.GetByIdAsync(id).Result;
            if (existingUser == null)
            {
                return NotFound();
            }

            // Если оба поля пустые, можно вернуть ошибку
            if (string.IsNullOrEmpty(updatedUser.Username) && string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                return BadRequest("Нужно указать хотя бы одно поле для обновления.");
            }

            _userService.Update(id, updatedUser);

            return NoContent();
        }


        // DELETE: api/user/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.GetByIdAsync(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            _userService.Delete(id);

            return NoContent();
        }
    }

}
