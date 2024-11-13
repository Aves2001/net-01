using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementSystem_Task2.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required] // Назва книги є обов'язковим полем
        public string Title { get; set; }

        [Required] // Автор книги є обов'язковим полем
        public string Author { get; set; }

        [Required] // Кількість доступних примірників є обов'язковим полем
        public int Available { get; set; }

        [JsonIgnore]  // Ігноруємо BorrowedBooks під час серіалізації, щоб уникнути циклів
        // Поле BorrowedBooks не є обов'язковим
        public ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
    }
}
