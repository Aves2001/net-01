using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LibraryManagementSystem_Task3.Models
{
    public class BorrowedBook
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; }
        public string BookId { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}
