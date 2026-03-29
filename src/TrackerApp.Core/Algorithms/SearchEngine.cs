using TrackerApp.Core.Models;

namespace TrackerApp.Core.Algorithms
{
    /// <summary>
    /// Provides custom search algorithms for task collections.
    /// Demonstrates both Linear and Binary search techniques on real system data.
    /// </summary>
    public static class SearchEngine
    {
        /// <summary>
        /// Linear Search: O(n) — scans every task to find all matches on title or description.
        /// Used when the list is unsorted or when searching by text content.
        /// </summary>
        /// <param name="tasks">The full task list to search.</param>
        /// <param name="keyword">Case-insensitive search keyword.</param>
        /// <returns>All matching tasks.</returns>
        public static IEnumerable<BaseTask> LinearSearchByKeyword(IEnumerable<BaseTask> tasks, string keyword)
        {
            string lower = keyword.ToLowerInvariant();
            foreach (var task in tasks)
            {
                if (task.Title.ToLowerInvariant().Contains(lower) ||
                    task.Description.ToLowerInvariant().Contains(lower))
                {
                    yield return task;
                }
            }
        }

        /// <summary>
        /// Binary Search: O(log n) — efficiently locates a task by its integer ID.
        /// Requires the input list to be sorted ascending by ID.
        /// </summary>
        /// <param name="sortedTasks">A list sorted ascending by ID.</param>
        /// <param name="id">The exact task ID to find.</param>
        /// <returns>The matching task, or null if not found.</returns>
        public static BaseTask? BinarySearchById(List<BaseTask> sortedTasks, int id)
        {
            int left = 0;
            int right = sortedTasks.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                int midId = sortedTasks[mid].Id;

                if (midId == id)   return sortedTasks[mid];
                if (midId < id)    left = mid + 1;
                else               right = mid - 1;
            }
            return null;
        }

        /// <summary>
        /// Filters tasks by a specific status value (Linear scan).
        /// </summary>
        public static IEnumerable<BaseTask> FilterByStatus(IEnumerable<BaseTask> tasks, Models.TaskStatus status)
            => tasks.Where(t => t.Status == status);

        /// <summary>
        /// Returns all overdue tasks (deadline passed, not yet Done).
        /// </summary>
        public static IEnumerable<BaseTask> GetOverdueTasks(IEnumerable<BaseTask> tasks)
            => tasks.Where(t => t.IsOverdue());
    }
}
