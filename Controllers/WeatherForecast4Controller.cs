using ChessSchoolAPI.Models;
using ChessSchoolAPI.Services;
using Microsoft.AspNetCore.Mvc;  // Добавляем эту директиву
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessSchoolAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChessStudentsController : ControllerBase
    {
        private readonly ChessStudentService _studentService;
        private readonly Random _random = new Random();

        public ChessStudentsController(ChessStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/chessstudents
        [HttpGet]
        public async Task<ActionResult<List<ChessStudent>>> GetAllAsync()
        {
            var students = await _studentService.GetAllAsync();
            return Ok(students);
        }

        // GET: api/chessstudents/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<ChessStudent>> GetByIdAsync(string id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        // POST: api/chessstudents
        [HttpPost]
        public ActionResult<ChessStudent> Create(ChessStudent newStudent)
        {
            _studentService.Create(newStudent);
            
            // Возвращаем только созданный объект без указания маршрута
            return Created("", newStudent);
        }

        // PUT: api/chessstudents/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, ChessStudent updatedStudent)
        {
            var student = _studentService.GetByIdAsync(id).Result;

            if (student == null)
            {
                return NotFound();
            }

            _studentService.Update(id, updatedStudent);

            return NoContent();
        }

        // DELETE: api/chessstudents/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var student = _studentService.GetByIdAsync(id).Result;

            if (student == null)
            {
                return NotFound();
            }

            _studentService.Delete(id);

            return NoContent();
        }

        // --- Тестовые методы ---
        // POST: api/chessstudents/test/add100
        [HttpPost("test/add100")]
        public ActionResult Add100Students()
        {
            var students = new List<ChessStudent>();

            for (int i = 0; i < 100; i++)
            {
                students.Add(new ChessStudent
                {
                    Name = $"TestStudent_{i}",
                    Rating = _random.Next(250, 3201), // Рейтинг от 250 до 3200
                    EnrollmentDate = new DateTime(_random.Next(1990, DateTime.Now.Year + 1), _random.Next(1, 13), _random.Next(1, 28)) // Случайная дата от 1990 года
                });
            }

            _studentService.InsertMany(students);
            return Ok("100 students added.");
        }

        // POST: api/chessstudents/test/add100000
        [HttpPost("test/add100000")]
        public ActionResult Add100000Students()
        {
            var students = new List<ChessStudent>();

            for (int i = 0; i < 100000; i++)
            {
                students.Add(new ChessStudent
                {
                    Name = $"TestStudent_{i}",
                    Rating = _random.Next(250, 3201), // Рейтинг от 250 до 3200
                    EnrollmentDate = new DateTime(_random.Next(1990, DateTime.Now.Year + 1), _random.Next(1, 13), _random.Next(1, 28)) // Случайная дата от 1990 года
                });
            }
            _studentService.InsertMany(students);

            return Ok("100,000 students added.");
        }

        // DELETE: api/chessstudents/test/clear
        [HttpDelete("test/clear")]
        public ActionResult ClearAllStudents()
        {
            _studentService.DeleteAll();
            return Ok("All students deleted.");
        }
    }
}
