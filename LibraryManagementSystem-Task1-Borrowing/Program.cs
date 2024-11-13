using System;
using System.Text; // Потрібно для кодування
using System.Data.SqlClient; // Необхідно для SqlConnection
using Dapper;

class Program
{
    static void Main(string[] args)
    {
        // Встановлюємо кодування UTF-8 для виведення в консоль
        Console.OutputEncoding = Encoding.UTF8;

        // Встановлюємо кодування UTF-8 для введення з консолі (за потреби)
        Console.InputEncoding = Encoding.UTF8;// Встановлюємо кодування UTF-8 для виведення в консоль

        string connectionString = "Server=localhost\\MSSQLSERVER01;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;";

        using SqlConnection connection = new SqlConnection(connectionString);
        try
        {
            connection.Open();
            Console.WriteLine("Підключення до бази даних встановлено.");

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Додати користувача");
                Console.WriteLine("2. Додати книгу");
                Console.WriteLine("3. Позичити книгу");
                Console.WriteLine("4. Повернути книгу");
                Console.WriteLine("5. Показати список користувачів");
                Console.WriteLine("6. Показати список позичених книг");
                Console.WriteLine("0. Вийти");
                Console.Write("Виберіть опцію: ");

                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        AddUser(connection);
                        break;
                    case "2":
                        AddBook(connection);
                        break;
                    case "3":
                        BorrowBook(connection);
                        break;
                    case "4":
                        ReturnBook(connection);
                        break;
                    case "5":
                        ShowUsers(connection);
                        break;
                    case "6":
                        ShowBorrowedBooks(connection);
                        break;
                    case "0":
                        exit = true;
                        Console.WriteLine("Завершення програми...");
                        break;
                    default:
                        Console.WriteLine("Невідома опція, спробуйте ще раз.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
        finally
        {
            connection.Close();
            Console.WriteLine("Підключення до бази даних закрито.");
        }
    }

    static void AddUser(SqlConnection connection)
    {
        Console.Write("Введіть ім'я користувача: ");
        string name = Console.ReadLine();
        Console.Write("Введіть email користувача: ");
        string email = Console.ReadLine();
        Console.Write("Введіть номер телефону користувача: ");
        string phone = Console.ReadLine();

        var affectedRows = connection.Execute("AddUser",
            new { Name = name, Email = email, Phone = phone },
            commandType: System.Data.CommandType.StoredProcedure);

        Console.WriteLine($"{affectedRows} користувача додано.");
    }

    static void AddBook(SqlConnection connection)
    {
        Console.Write("Введіть назву книги: ");
        string title = Console.ReadLine();
        Console.Write("Введіть автора книги: ");
        string author = Console.ReadLine();
        Console.Write("Введіть кількість доступних примірників: ");
        if (!int.TryParse(Console.ReadLine(), out int available) || available < 0)
        {
            Console.WriteLine("Будь ласка, введіть дійсне число для кількості доступних примірників.");
            return;
        }

        var affectedRows = connection.Execute("AddBook",
            new { Title = title, Author = author, Available = available },
            commandType: System.Data.CommandType.StoredProcedure);

        Console.WriteLine($"{affectedRows} книг(и) додано.");
    }

    static void BorrowBook(SqlConnection connection)
    {
        Console.Write("Введіть телефон або email користувача: ");
        string userInput = Console.ReadLine();

        var user = connection.QueryFirstOrDefault("SELECT * FROM Users WHERE Phone = @Input OR Email = @Input", new { Input = userInput });
        if (user == null)
        {
            Console.WriteLine("Користувача не знайдено.");
            return;
        }

        int bookId = SearchBook(connection);
        if (bookId == -1) return; // Якщо книга не вибрана, виходимо

        try
        {
            var affectedRows = connection.Execute("BorrowBook",
                new { UserId = user.Id, BookId = bookId },
                commandType: System.Data.CommandType.StoredProcedure);

            if (affectedRows > 0)
                Console.WriteLine("Книга успішно позичена.");
            else
                Console.WriteLine("Не вдалося позичити книгу.");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Помилка при позиченні книги: {ex.Message}");
        }
    }

    // Функція для повернення книги
    static void ReturnBook(SqlConnection connection)
    {
        Console.Write("Введіть телефон або email користувача: ");
        string userInput = Console.ReadLine();

        var user = connection.QueryFirstOrDefault("SELECT * FROM Users WHERE Phone = @Input OR Email = @Input", new { Input = userInput });
        if (user == null)
        {
            Console.WriteLine("Користувача не знайдено.");
            return;
        }

        // Отримуємо список позичених книг користувача
        var borrowedBooks = connection.Query("SELECT bb.Id, b.Title FROM BorrowedBooks bb JOIN Books b ON bb.BookId = b.Id WHERE bb.UserId = @UserId AND bb.ReturnedAt IS NULL", new { UserId = user.Id });

        if (!borrowedBooks.AsList().Any())
        {
            Console.WriteLine("Користувач не має жодної позиченої книги.");
            return;
        }

        // Виводимо список позичених книг
        Console.WriteLine("Список позичених книг:");
        foreach (var book in borrowedBooks)
        {
            Console.WriteLine($"ID: {book.Id}, Назва: {book.Title}");
        }

        // Запитуємо ID книги, яку потрібно повернути
        Console.Write("Введіть ID книги, яку потрібно повернути: ");
        if (!int.TryParse(Console.ReadLine(), out int bookId) || !borrowedBooks.Any(b => b.Id == bookId))
        {
            Console.WriteLine("Невірний ID книги. Спробуйте ще раз.");
            return;
        }

        var affectedRows = connection.Execute("ReturnBook",
            new { UserId = user.Id, BookId = bookId },
            commandType: System.Data.CommandType.StoredProcedure);

        if (affectedRows > 0)
            Console.WriteLine("Книга повернена.");
        else
            Console.WriteLine("Не вдалося повернути книгу.");
    }
    

    static int SearchBook(SqlConnection connection)
    {
        while (true)
        {
            Console.Write("Введіть назву книги (частково або повністю): ");
            string bookTitle = Console.ReadLine();

            var books = connection.Query("SELECT Id, Title, Author FROM Books WHERE Title LIKE '%' + @Title + '%'", new { Title = bookTitle });

            if (books.AsList().Count == 0)
            {
                Console.WriteLine("Книг за введеною назвою не знайдено.");
                continue; // Продовжуємо пошук
            }
            else if (books.AsList().Count == 1)
            {
                var singleBook = books.First();
                Console.WriteLine($"Знайдено книгу: {singleBook.Title} (Автор: {singleBook.Author}).");
                Console.Write("Чи це та книга? (так/ні): ");
                string confirmation = Console.ReadLine().ToLower();

                if (confirmation == "так")
                {
                    return singleBook.Id; // Повертаємо ID книги
                }
            }
            else
            {
                // Виводимо список знайдених книг
                Console.WriteLine("Знайдено кілька книг:");
                foreach (var book in books)
                {
                    Console.WriteLine($"ID: {book.Id}, Назва: {book.Title}, Автор: {book.Author}");
                }

                Console.Write("Виберіть ID книги: ");
                if (int.TryParse(Console.ReadLine(), out int selectedId))
                {
                    var selectedBook = books.FirstOrDefault(b => b.Id == selectedId);
                    if (selectedBook != null)
                    {
                        Console.WriteLine($"Ви вибрали книгу: {selectedBook.Title} (Автор: {selectedBook.Author}).");
                        Console.Write("Чи це та книга? (так/ні): ");
                        string confirmation = Console.ReadLine().ToLower();

                        if (confirmation == "так")
                        {
                            return selectedBook.Id; // Повертаємо ID книги
                        }
                    }
                    else
                    {
                        Console.WriteLine("Невірний ID книги. Спробуйте ще раз.");
                    }
                }
                else
                {
                    Console.WriteLine("Будь ласка, введіть дійсний ID книги.");
                }
            }
        }
    }

    static void ShowUsers(SqlConnection connection)
    {
        var users = connection.Query("GetUsers", commandType: System.Data.CommandType.StoredProcedure);
        if (users.AsList().Count > 0)
        {
            Console.WriteLine("Список користувачів:");
            foreach (var user in users)
            {
                Console.WriteLine($"- {user.Name} (Email: {user.Email}, Phone: {user.Phone})");
            }
        }
        else
        {
            Console.WriteLine("Користувачів не знайдено.");
        }
    }

    static void ShowBorrowedBooks(SqlConnection connection)
    {
        var borrowedBooks = connection.Query("GetBorrowedBooks", commandType: System.Data.CommandType.StoredProcedure);
        if (borrowedBooks.AsList().Count > 0)
        {
            Console.WriteLine("Список позичених книг:");
            foreach (var book in borrowedBooks)
            {
                Console.WriteLine($"- Книга: {book.Title}, Позичив: {book.Name}, Дата позичення: {book.BorrowedAt}");
            }
        }
        else
        {
            Console.WriteLine("Позичених книг не знайдено.");
        }
    }
}
