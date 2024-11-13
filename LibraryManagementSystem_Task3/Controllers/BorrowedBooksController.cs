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
    public class BorrowedBooksController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public BorrowedBooksController()
        {
            _context = new MongoDbContext();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowedBook>>> GetBorrowedBooks()
        {
            var borrowedBooks = await _context.BorrowedBooks.Find(bb => true).ToListAsync();
            return Ok(borrowedBooks);
        }

        [HttpPost]
        public async Task<ActionResult<BorrowedBook>> BorrowBook(BorrowedBook borrowedBook)
        {
            await _context.BorrowedBooks.InsertOneAsync(borrowedBook);
            return CreatedAtAction(nameof(GetBorrowedBooks), new { id = borrowedBook.Id }, borrowedBook);
        }
    }
}
