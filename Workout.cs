using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gym
{
    public class Workout
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Category { get; set; }
        public string Exercise { get; set; }
        public int Reps { get; set; }
        public int Sets { get; set; }
        public double Weight { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }

        public Workout()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
    }
}
