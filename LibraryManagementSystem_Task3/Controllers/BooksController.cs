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
    public class BooksController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public BooksController()
        {
            _context = new MongoDbContext();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _context.Books.Find(book => true).ToListAsync();
            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<Book>> PostBook([FromBody] Book book)
        {
            // Переконуємось, що Id порожнє, щоб MongoDB згенерувала його автоматично
            book.Id = null;

            await _context.Books.InsertOneAsync(book);
            return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
        }
    }
}
