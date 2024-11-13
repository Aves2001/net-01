using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementSystem_Task2.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required] // поле обов'язкове
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
        [JsonIgnore]  // Ігноруємо BorrowedBooks під час серіалізації, щоб уникнути циклів

        public ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
    }
}
