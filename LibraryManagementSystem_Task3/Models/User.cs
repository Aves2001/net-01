﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManagementSystem_Task3.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
