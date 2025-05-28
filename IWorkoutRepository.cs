// IWorkoutRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gym
{
    public interface IWorkoutRepository
    {
        // Match the existing WorkoutDB method signatures exactly
        Task InsertWorkoutAsync(Workout workout);
        void InsertWorkout(Workout workout);
        Task<List<Workout>> GetAllWorkoutsAsync();
        List<Workout> GetAllWorkouts();
        Task<List<Workout>> GetWorkoutsByExerciseAsync(string exercise);
        Task<List<Workout>> GetWorkoutsByCategoryAsync(string category);
        Task<List<Workout>> GetWorkoutsByDateRangeAsync(DateTime startDate, DateTime endDate);
        void AddTestWorkout();
        bool IsConnected();
    }
}
