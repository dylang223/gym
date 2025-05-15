using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace gym
{
    public class WorkoutDB
    {
        private readonly string _dbPath = "workout.db";
        
        public void InsertWorkout(Workout workout)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<Workout>("workouts");
            col.Insert(workout);
        }

        public List<Workout> GetAllWorkouts()
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<Workout>("workouts");
            return col.FindAll().ToList();
        }
    }
}
