using TrackerApp.Core.Algorithms;
using TrackerApp.Core.Data;
using TrackerApp.Core.Interfaces;
using TrackerApp.Core.Logging;
using TrackerApp.Core.Models;
using AppTaskFactory = TrackerApp.Core.Factories.TaskFactory;

namespace TrackerApp.Core.Services
{
    /// <summary>
    /// Central service coordinating all task operations.
    /// Delegates persistence to IRepository, sorting to ISortStrategy, and logs all actions via AppLogger.
    /// </summary>
    public class TaskManager
    {
        private readonly IRepository<BaseTask> _repository;
        private readonly AppLogger _logger;
        private ISortStrategy<BaseTask> _sortStrategy;

        public TaskManager(
            IRepository<BaseTask>? repository = null,
            AppLogger? logger = null,
            ISortStrategy<BaseTask>? sortStrategy = null)
        {
            _repository   = repository   ?? new JsonTaskRepository();
            _logger       = logger       ?? AppLogger.GetInstance();
            _sortStrategy = sortStrategy ?? new QuickSortStrategy<BaseTask>();

            // Seed the factory with the next unique ID based on existing data
            var all = _repository.GetAll().ToList();
            int maxId = all.Count > 0 ? all.Max(t => t.Id) : 0;
            AppTaskFactory.SetNextId(maxId + 1);
        }

        // ------------------------------------------------------------------ //
        //  CRUD Operations
        // ------------------------------------------------------------------ //

        /// <summary>Creates and persists a new task of the given type.</summary>
        public BaseTask AddTask(TaskType type, string title, string description,
                                TaskPriority priority, DateTime dueDate, Assignee? assignee = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty.", nameof(title));

            var task = AppTaskFactory.Create(type);
            task.Title       = title.Trim();
            task.Description = description.Trim();
            task.Priority    = priority;
            task.DueDate     = dueDate;
            task.Assignee    = assignee;

            _repository.Add(task);
            _repository.Save();
            return task!;
        }

        /// <summary>Updates the status of an existing task by ID.</summary>
        public void UpdateStatus(int id, Models.TaskStatus newStatus)
        {
            var task = GetTaskOrThrow(id);
            task.Status = newStatus;
            _repository.Update(task);
            _repository.Save();
        }

        /// <summary>Updates all editable fields of a task.</summary>
        public void UpdateTask(int id, string title, string description,
                               TaskPriority priority, DateTime dueDate, Assignee? assignee)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty.", nameof(title));

            var task = GetTaskOrThrow(id);
            task.Title       = title.Trim();
            task.Description = description.Trim();
            task.Priority    = priority;
            task.DueDate     = dueDate;
            task.Assignee    = assignee;

            _repository.Update(task);
            _repository.Save();
        }

        /// <summary>Permanently deletes a task by ID.</summary>
        public void DeleteTask(int id)
        {
            GetTaskOrThrow(id); // validate it exists first
            _repository.Delete(id);
            _repository.Save();
        }

        // ------------------------------------------------------------------ //
        //  Retrieval & Search
        // ------------------------------------------------------------------ //

        /// <summary>Returns all tasks in the system.</summary>
        public IReadOnlyList<BaseTask> GetAllTasks()
            => _repository.GetAll().ToList().AsReadOnly();

        /// <summary>Finds a task by ID using Binary Search (list sorted by ID).</summary>
        public BaseTask? FindById(int id)
        {
            var sorted = _repository.GetAll().OrderBy(t => t.Id).ToList();
            return SearchEngine.BinarySearchById(sorted, id);
        }

        /// <summary>Searches tasks by keyword in title or description (Linear Search).</summary>
        public IEnumerable<BaseTask> SearchByKeyword(string keyword)
            => SearchEngine.LinearSearchByKeyword(_repository.GetAll(), keyword);

        /// <summary>Returns all tasks currently overdue.</summary>
        public IEnumerable<BaseTask> GetOverdueTasks()
            => SearchEngine.GetOverdueTasks(_repository.GetAll());

        /// <summary>Filters tasks by status.</summary>
        public IEnumerable<BaseTask> FilterByStatus(Models.TaskStatus status)
            => SearchEngine.FilterByStatus(_repository.GetAll(), status);

        // ------------------------------------------------------------------ //
        //  Sorting
        // ------------------------------------------------------------------ //

        /// <summary>Switches the active sort strategy at runtime (Strategy pattern).</summary>
        public void SetSortStrategy(ISortStrategy<BaseTask> strategy) => _sortStrategy = strategy;

        /// <summary>Returns all tasks sorted by due date (ascending) using the active strategy.</summary>
        public List<BaseTask> GetSortedByDueDate()
            => _sortStrategy.Sort(_repository.GetAll().ToList(),
                                  (a, b) => a.DueDate.CompareTo(b.DueDate));

        /// <summary>Returns all tasks sorted by priority (descending) using the active strategy.</summary>
        public List<BaseTask> GetSortedByPriority()
            => _sortStrategy.Sort(_repository.GetAll().ToList(),
                                  (a, b) => b.Priority.CompareTo(a.Priority));

        // ------------------------------------------------------------------ //
        //  Reporting
        // ------------------------------------------------------------------ //

        /// <summary>Exports a plain-text summary report to the data directory.</summary>
        public string ExportReport(string dataDirectory = "data")
        {
            var all = _repository.GetAll().ToList();
            var lines = new List<string>
            {
                "===================================================",
                $"  TASK TRACKER — REPORT  ({DateTime.Now:dd MMM yyyy HH:mm})",
                "===================================================",
                $"Total Tasks : {all.Count}",
                $"To Do       : {all.Count(t => t.Status == Models.TaskStatus.ToDo)}",
                $"In Progress : {all.Count(t => t.Status == Models.TaskStatus.InProgress)}",
                $"Done        : {all.Count(t => t.Status == Models.TaskStatus.Done)}",
                $"Overdue     : {all.Count(t => t.IsOverdue())}",
                "---------------------------------------------------",
                "OVERDUE TASKS:"
            };
            foreach (var t in GetOverdueTasks())
                lines.Add("  " + t.GetSummary());

            lines.Add("---------------------------------------------------");
            lines.Add("UPCOMING (next 7 days):");
            var upcoming = all.Where(t => !t.IsOverdue() &&
                                         t.Status != Models.TaskStatus.Done &&
                                         t.DueDate <= DateTime.Now.AddDays(7));
            foreach (var t in upcoming)
                lines.Add("  " + t.GetSummary());

            lines.Add("===================================================");

            string reportPath = Path.Combine(dataDirectory, $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllLines(reportPath, lines);
            _logger.Info($"Report exported to '{reportPath}'.");
            return reportPath;
        }

        // ------------------------------------------------------------------ //
        //  Private Helpers
        // ------------------------------------------------------------------ //

        private BaseTask GetTaskOrThrow(int id)
        {
            return _repository.GetById(id)
                ?? throw new KeyNotFoundException($"Task with ID {id} does not exist.");
        }
    }
}
