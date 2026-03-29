using TrackerApp.Core.Models;

namespace TrackerApp.Core.Factories
{
    /// <summary>
    /// Factory Method pattern: Centralises task object creation.
    /// Consumers request a task by its TaskType without knowing which
    /// concrete class will be instantiated. This promotes open/closed principle.
    /// </summary>
    public static class TaskFactory
    {
        private static int _nextId = 1;

        /// <summary>
        /// Seeds the factory with the next available ID (called on app load).
        /// </summary>
        public static void SetNextId(int nextId) => _nextId = nextId;

        /// <summary>
        /// Creates a new task of the given type with a unique, auto-incremented ID.
        /// </summary>
        /// <param name="type">The type of task to create.</param>
        /// <returns>A concrete ITask implementation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown task types.</exception>
        public static BaseTask Create(TaskType type)
        {
            BaseTask task = type switch
            {
                TaskType.Feature  => new FeatureTask(),
                TaskType.Bug      => new BugTask(),
                TaskType.Chore    => new ChoreTask(),
                TaskType.Research => new ResearchTask(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown task type: {type}")
            };

            task.Id = _nextId++;
            return task;
        }
    }
}
