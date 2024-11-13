using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem_Task3.Data;
using LibraryManagementSystem_Task3.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementSystem_Task3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController()
        {
            _context = new MongoDbContext();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.Find(user => true).ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving users: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }

            // Перевіряємо, чи передано Id; якщо так, обнуляємо його, щоб MongoDB автоматично згенерувала нове значення
            user.Id = null;

            try
            {
                Console.WriteLine($"Adding user: {user.Name}, Email: {user.Email}, Phone: {user.Phone}");
                await _context.Users.InsertOneAsync(user);
                return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
