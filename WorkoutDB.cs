using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Windows;
using System.Linq;

namespace gym
{
    public class WorkoutDB
    {
        private IMongoCollection<Workout> _workouts;
        private bool _isConnected = false;

        // Changed to use localhost by default with Atlas as fallback
        public WorkoutDB(string connectionString = "mongodb+srv://DylanGlynn:Topgear2015@wp1.edblxxv.mongodb.net/", string databaseName = "gymDB")
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

                var client = new MongoClient(settings);
                var database = client.GetDatabase(databaseName);
                _workouts = database.GetCollection<Workout>("workouts");
                _isConnected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot connect to MongoDB: {ex.Message}\nCreating in-memory database instead.",
                    "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Create in-memory fallback DB
                UseInMemoryDatabase();
            }
        }

        // Fallback in-memory storage
        private List<Workout> _inMemoryWorkouts = new List<Workout>();

        private void UseInMemoryDatabase()
        {
            _isConnected = false;
        }

        public async Task InsertWorkoutAsync(Workout workout)
        {
            if (string.IsNullOrEmpty(workout.Id))
            {
                workout.Id = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                if (_isConnected)
                {
                    await _workouts.InsertOneAsync(workout);
                }
                else
                {
                    _inMemoryWorkouts.Add(workout);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving workout: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fall back to memory
                _inMemoryWorkouts.Add(workout);
                _isConnected = false;
            }
        }

        public void InsertWorkout(Workout workout)
        {
            var sampleWorkouts = new List<Workout>
{
            new Workout { Category = "Push", Exercise = "Bench Press", Reps = 8, Sets = 4, Weight = 50, Date = new DateTime(2025, 5, 1) },
            new Workout { Category = "Push", Exercise = "Bench Press", Reps = 9, Sets = 4, Weight = 52.5, Date = new DateTime(2025, 5, 8) },
            new Workout { Category = "Push", Exercise = "Bench Press", Reps = 10, Sets = 4, Weight = 55, Date = new DateTime(2025, 5, 15) },
            new Workout { Category = "Push", Exercise = "Overhead Shoulder Press", Reps = 8, Sets = 3, Weight = 30, Date = new DateTime(2025, 5, 1) },
            new Workout { Category = "Push", Exercise = "Overhead Shoulder Press", Reps = 9, Sets = 3, Weight = 32.5, Date = new DateTime(2025, 5, 8) },
            new Workout { Category = "Push", Exercise = "Overhead Shoulder Press", Reps = 10, Sets = 3, Weight = 35, Date = new DateTime(2025, 5, 15) },
            new Workout { Category = "Push", Exercise = "Triceps Pushdown", Reps = 12, Sets = 3, Weight = 20, Date = new DateTime(2025, 5, 1) },
            new Workout { Category = "Push", Exercise = "Triceps Pushdown", Reps = 13, Sets = 3, Weight = 22.5, Date = new DateTime(2025, 5, 8) },
            new Workout { Category = "Push", Exercise = "Triceps Pushdown", Reps = 14, Sets = 3, Weight = 25, Date = new DateTime(2025, 5, 15) }
        };

            if (string.IsNullOrEmpty(workout.Id))
            {
                workout.Id = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                if (_isConnected)
                {
                    _workouts.InsertOne(workout);
                }
                else
                {
                    _inMemoryWorkouts.Add(workout);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving workout: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fall back to memory
                _inMemoryWorkouts.Add(workout);
                _isConnected = false;
            }
        }

        public async Task<List<Workout>> GetAllWorkoutsAsync()
        {
            try
            {
                if (_isConnected)
                {
                    var filter = Builders<Workout>.Filter.Empty;
                    return await _workouts.Find(filter).ToListAsync();
                }
                else
                {
                    return _inMemoryWorkouts.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving workouts: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
                return _inMemoryWorkouts.ToList();
            }
        }

        public List<Workout> GetAllWorkouts()
        {
            try
            {
                if (_isConnected)
                {
                    var filter = Builders<Workout>.Filter.Empty;
                    return _workouts.Find(filter).ToList();
                }
                else
                {
                    return _inMemoryWorkouts.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving workouts: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
                return _inMemoryWorkouts.ToList();
            }
        }

        public async Task<List<Workout>> GetWorkoutsByExerciseAsync(string exercise)
        {
            try
            {
                if (_isConnected)
                {
                    var filter = Builders<Workout>.Filter.Eq(w => w.Exercise, exercise);
                    return await _workouts.Find(filter).ToListAsync();
                }
                else
                {
                    return _inMemoryWorkouts.Where(w => w.Exercise == exercise).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving workouts: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
                return _inMemoryWorkouts.Where(w => w.Exercise == exercise).ToList();
            }
        }

        public async Task<List<Workout>> GetWorkoutsByCategoryAsync(string category)
        {
            try
            {
                if (_isConnected)
                {
                    var filter = Builders<Workout>.Filter.Eq(w => w.Category, category);
                    return await _workouts.Find(filter).ToListAsync();
                }
                else
                {
                    return _inMemoryWorkouts.Where(w => w.Category == category).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving workouts: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
                return _inMemoryWorkouts.Where(w => w.Category == category).ToList();
            }
        }

        public async Task<List<Workout>> GetWorkoutsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (_isConnected)
                {
                    var filter = Builders<Workout>.Filter.And(
                        Builders<Workout>.Filter.Gte(w => w.Date, startDate),
                        Builders<Workout>.Filter.Lte(w => w.Date, endDate)
                    );
                    return await _workouts.Find(filter).ToListAsync();
                }
                else
                {
                    return _inMemoryWorkouts.Where(w => w.Date >= startDate && w.Date <= endDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving workouts: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
                return _inMemoryWorkouts.Where(w => w.Date >= startDate && w.Date <= endDate).ToList();
            }
        }

        // Test method made public for debugging
        public void AddTestWorkout()
        {
            var workout = new Workout
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Category = "Strength",
                Exercise = "Bench Press",
                Reps = 10,
                Sets = 3,
                Weight = 135,
                Date = DateTime.Now
            };

            InsertWorkout(workout);
            MessageBox.Show("Test workout added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Status method to check connection
        public bool IsConnected()
        {
            return _isConnected;
        }
    }
}
