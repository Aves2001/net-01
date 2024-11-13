using MongoDB.Driver;
using LibraryManagementSystem_Task3.Models;

namespace LibraryManagementSystem_Task3.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // Адреса сервера MongoDB
            _database = client.GetDatabase("LibraryDb"); // Назва бази даних
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Book> Books => _database.GetCollection<Book>("Books");
        public IMongoCollection<BorrowedBook> BorrowedBooks => _database.GetCollection<BorrowedBook>("BorrowedBooks");
    }
}
