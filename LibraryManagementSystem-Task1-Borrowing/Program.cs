using System;
using System.Text; // ������� ��� ���������
using System.Data.SqlClient; // ��������� ��� SqlConnection
using Dapper;

class Program
{
    static void Main(string[] args)
    {
        // ������������ ��������� UTF-8 ��� ��������� � �������
        Console.OutputEncoding = Encoding.UTF8;

        // ������������ ��������� UTF-8 ��� �������� � ������ (�� �������)
        Console.InputEncoding = Encoding.UTF8;// ������������ ��������� UTF-8 ��� ��������� � �������

        string connectionString = "Server=localhost\\MSSQLSERVER01;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;";

        using SqlConnection connection = new SqlConnection(connectionString);
        try
        {
            connection.Open();
            Console.WriteLine("ϳ��������� �� ���� ����� �����������.");

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n����:");
                Console.WriteLine("1. ������ �����������");
                Console.WriteLine("2. ������ �����");
                Console.WriteLine("3. �������� �����");
                Console.WriteLine("4. ��������� �����");
                Console.WriteLine("5. �������� ������ ������������");
                Console.WriteLine("6. �������� ������ ��������� ����");
                Console.WriteLine("0. �����");
                Console.Write("������� �����: ");

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
                        Console.WriteLine("���������� ��������...");
                        break;
                    default:
                        Console.WriteLine("������� �����, ��������� �� ���.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�������: {ex.Message}");
        }
        finally
        {
            connection.Close();
            Console.WriteLine("ϳ��������� �� ���� ����� �������.");
        }
    }

    static void AddUser(SqlConnection connection)
    {
        Console.Write("������ ��'� �����������: ");
        string name = Console.ReadLine();
        Console.Write("������ email �����������: ");
        string email = Console.ReadLine();
        Console.Write("������ ����� �������� �����������: ");
        string phone = Console.ReadLine();

        var affectedRows = connection.Execute("AddUser",
            new { Name = name, Email = email, Phone = phone },
            commandType: System.Data.CommandType.StoredProcedure);

        Console.WriteLine($"{affectedRows} ����������� ������.");
    }

    static void AddBook(SqlConnection connection)
    {
        Console.Write("������ ����� �����: ");
        string title = Console.ReadLine();
        Console.Write("������ ������ �����: ");
        string author = Console.ReadLine();
        Console.Write("������ ������� ��������� ���������: ");
        if (!int.TryParse(Console.ReadLine(), out int available) || available < 0)
        {
            Console.WriteLine("���� �����, ������ ����� ����� ��� ������� ��������� ���������.");
            return;
        }

        var affectedRows = connection.Execute("AddBook",
            new { Title = title, Author = author, Available = available },
            commandType: System.Data.CommandType.StoredProcedure);

        Console.WriteLine($"{affectedRows} ����(�) ������.");
    }

    static void BorrowBook(SqlConnection connection)
    {
        Console.Write("������ ������� ��� email �����������: ");
        string userInput = Console.ReadLine();

        var user = connection.QueryFirstOrDefault("SELECT * FROM Users WHERE Phone = @Input OR Email = @Input", new { Input = userInput });
        if (user == null)
        {
            Console.WriteLine("����������� �� ��������.");
            return;
        }

        int bookId = SearchBook(connection);
        if (bookId == -1) return; // ���� ����� �� �������, ��������

        try
        {
            var affectedRows = connection.Execute("BorrowBook",
                new { UserId = user.Id, BookId = bookId },
                commandType: System.Data.CommandType.StoredProcedure);

            if (affectedRows > 0)
                Console.WriteLine("����� ������ ��������.");
            else
                Console.WriteLine("�� ������� �������� �����.");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"������� ��� �������� �����: {ex.Message}");
        }
    }

    // ������� ��� ���������� �����
    static void ReturnBook(SqlConnection connection)
    {
        Console.Write("������ ������� ��� email �����������: ");
        string userInput = Console.ReadLine();

        var user = connection.QueryFirstOrDefault("SELECT * FROM Users WHERE Phone = @Input OR Email = @Input", new { Input = userInput });
        if (user == null)
        {
            Console.WriteLine("����������� �� ��������.");
            return;
        }

        // �������� ������ ��������� ���� �����������
        var borrowedBooks = connection.Query("SELECT bb.Id, b.Title FROM BorrowedBooks bb JOIN Books b ON bb.BookId = b.Id WHERE bb.UserId = @UserId AND bb.ReturnedAt IS NULL", new { UserId = user.Id });

        if (!borrowedBooks.AsList().Any())
        {
            Console.WriteLine("���������� �� �� ����� �������� �����.");
            return;
        }

        // �������� ������ ��������� ����
        Console.WriteLine("������ ��������� ����:");
        foreach (var book in borrowedBooks)
        {
            Console.WriteLine($"ID: {book.Id}, �����: {book.Title}");
        }

        // �������� ID �����, ��� ������� ���������
        Console.Write("������ ID �����, ��� ������� ���������: ");
        if (!int.TryParse(Console.ReadLine(), out int bookId) || !borrowedBooks.Any(b => b.Id == bookId))
        {
            Console.WriteLine("������� ID �����. ��������� �� ���.");
            return;
        }

        var affectedRows = connection.Execute("ReturnBook",
            new { UserId = user.Id, BookId = bookId },
            commandType: System.Data.CommandType.StoredProcedure);

        if (affectedRows > 0)
            Console.WriteLine("����� ���������.");
        else
            Console.WriteLine("�� ������� ��������� �����.");
    }
    

    static int SearchBook(SqlConnection connection)
    {
        while (true)
        {
            Console.Write("������ ����� ����� (�������� ��� �������): ");
            string bookTitle = Console.ReadLine();

            var books = connection.Query("SELECT Id, Title, Author FROM Books WHERE Title LIKE '%' + @Title + '%'", new { Title = bookTitle });

            if (books.AsList().Count == 0)
            {
                Console.WriteLine("���� �� �������� ������ �� ��������.");
                continue; // ���������� �����
            }
            else if (books.AsList().Count == 1)
            {
                var singleBook = books.First();
                Console.WriteLine($"�������� �����: {singleBook.Title} (�����: {singleBook.Author}).");
                Console.Write("�� �� �� �����? (���/�): ");
                string confirmation = Console.ReadLine().ToLower();

                if (confirmation == "���")
                {
                    return singleBook.Id; // ��������� ID �����
                }
            }
            else
            {
                // �������� ������ ��������� ����
                Console.WriteLine("�������� ����� ����:");
                foreach (var book in books)
                {
                    Console.WriteLine($"ID: {book.Id}, �����: {book.Title}, �����: {book.Author}");
                }

                Console.Write("������� ID �����: ");
                if (int.TryParse(Console.ReadLine(), out int selectedId))
                {
                    var selectedBook = books.FirstOrDefault(b => b.Id == selectedId);
                    if (selectedBook != null)
                    {
                        Console.WriteLine($"�� ������� �����: {selectedBook.Title} (�����: {selectedBook.Author}).");
                        Console.Write("�� �� �� �����? (���/�): ");
                        string confirmation = Console.ReadLine().ToLower();

                        if (confirmation == "���")
                        {
                            return selectedBook.Id; // ��������� ID �����
                        }
                    }
                    else
                    {
                        Console.WriteLine("������� ID �����. ��������� �� ���.");
                    }
                }
                else
                {
                    Console.WriteLine("���� �����, ������ ������ ID �����.");
                }
            }
        }
    }

    static void ShowUsers(SqlConnection connection)
    {
        var users = connection.Query("GetUsers", commandType: System.Data.CommandType.StoredProcedure);
        if (users.AsList().Count > 0)
        {
            Console.WriteLine("������ ������������:");
            foreach (var user in users)
            {
                Console.WriteLine($"- {user.Name} (Email: {user.Email}, Phone: {user.Phone})");
            }
        }
        else
        {
            Console.WriteLine("������������ �� ��������.");
        }
    }

    static void ShowBorrowedBooks(SqlConnection connection)
    {
        var borrowedBooks = connection.Query("GetBorrowedBooks", commandType: System.Data.CommandType.StoredProcedure);
        if (borrowedBooks.AsList().Count > 0)
        {
            Console.WriteLine("������ ��������� ����:");
            foreach (var book in borrowedBooks)
            {
                Console.WriteLine($"- �����: {book.Title}, �������: {book.Name}, ���� ���������: {book.BorrowedAt}");
            }
        }
        else
        {
            Console.WriteLine("��������� ���� �� ��������.");
        }
    }
}
