using LibraryManagementSystem_Task2.Data;
using LibraryManagementSystem_Task2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem_Task2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowedBooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BorrowedBooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/borrowedbooks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowedBook>>> GetBorrowedBooks()
        {
            var borrowedBooks = await _context.BorrowedBooks
                .Include(bb => bb.User)
                .Include(bb => bb.Book)
                .ToListAsync();

            return Ok(borrowedBooks);
        }

        // POST: api/borrowedbooks
        [HttpPost]
        public async Task<ActionResult<BorrowedBook>> BorrowBook(int userId, int bookId)
        {
            // Перевірка, чи існує користувач
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Користувача не знайдено.");
            }

            // Перевірка, чи існує книга
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return BadRequest("Книга не знайдена.");
            }

            // Перевірка, чи є книга доступною
            if (book.Available <= 0)
            {
                return BadRequest("Книга недоступна для видачі.");
            }

            // Створення запису про позичену книгу
            var borrowedBook = new BorrowedBook
            {
                UserId = userId,
                BookId = bookId,
                BorrowedAt = DateTime.Now
            };

            _context.BorrowedBooks.Add(borrowedBook);

            // Зменшення кількості доступних примірників книги
            book.Available--;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBorrowedBooks), new { id = borrowedBook.Id }, borrowedBook);
        }

        // POST: api/borrowedbooks/return
        [HttpPost("return")]
        public async Task<ActionResult> ReturnBook(int userId, int bookId)
        {
            var borrowedBook = await _context.BorrowedBooks
                .FirstOrDefaultAsync(bb => bb.UserId == userId && bb.BookId == bookId && bb.ReturnedAt == null);

            if (borrowedBook == null)
            {
                return BadRequest("Для цього користувача та комбінації книг не знайдено виданих книг.");
            }

            // Встановлюємо дату повернення
            borrowedBook.ReturnedAt = DateTime.Now;

            // Збільшуємо кількість доступних примірників книги
            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.Available++;
            }

            await _context.SaveChangesAsync();

            return Ok("Book returned successfully.");
        }
    }
}
