using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerApp.Core.Algorithms;
using TrackerApp.Core.Data;
using TrackerApp.Core.Logging;
using TrackerApp.Core.Models;
using TrackerApp.Core.Services;
using AppTaskFactory = TrackerApp.Core.Factories.TaskFactory;
using AppTaskStatus = TrackerApp.Core.Models.TaskStatus;

namespace TrackerApp.Tests
{
    /// <summary>
    /// MSTest unit tests for the Task Tracker core library.
    /// Tests cover factory creation, CRUD operations, search algorithms,
    /// sort strategies, and edge cases — targeting >80% coverage of core logic.
    /// </summary>
    [TestClass]
    public class TaskTrackerTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

        /// <summary>Creates an in-memory TaskManager backed by a temp directory.</summary>
        private static TaskManager MakeManager()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logger = AppLogger.GetInstance(tempDir);
            var repo   = new JsonTaskRepository(tempDir, logger);
            return new TaskManager(repo, logger, new QuickSortStrategy<BaseTask>());
        }

        // ------------------------------------------------------------------ //
        //  Test 1: Factory creates correct concrete types
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void TaskFactory_CreatesCorrectConcreteType()
        {
            var feature  = AppTaskFactory.Create(TaskType.Feature);
            var bug      = AppTaskFactory.Create(TaskType.Bug);
            var chore    = AppTaskFactory.Create(TaskType.Chore);
            var research = AppTaskFactory.Create(TaskType.Research);

            Assert.IsInstanceOfType(feature,  typeof(FeatureTask));
            Assert.IsInstanceOfType(bug,      typeof(BugTask));
            Assert.IsInstanceOfType(chore,    typeof(ChoreTask));
            Assert.IsInstanceOfType(research, typeof(ResearchTask));
        }

        // ------------------------------------------------------------------ //
        //  Test 2: Factory IDs are unique and auto-incremented
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void TaskFactory_AssignsUniqueSequentialIds()
        {
            AppTaskFactory.SetNextId(100);
            var t1 = AppTaskFactory.Create(TaskType.Feature);
            var t2 = AppTaskFactory.Create(TaskType.Bug);
            var t3 = AppTaskFactory.Create(TaskType.Chore);

            Assert.AreEqual(100, t1.Id);
            Assert.AreEqual(101, t2.Id);
            Assert.AreEqual(102, t3.Id);
        }

        // ------------------------------------------------------------------ //
        //  Test 3: AddTask stores a task and GetAllTasks returns it
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void AddTask_IncreasesTaskCount()
        {
            var mgr = MakeManager();
            int initial = mgr.GetAllTasks().Count;

            mgr.AddTask(TaskType.Feature, "Test Feature", "desc", TaskPriority.High,
                        DateTime.Now.AddDays(7));

            Assert.AreEqual(initial + 1, mgr.GetAllTasks().Count);
        }

        // ------------------------------------------------------------------ //
        //  Test 4: AddTask with empty title throws ArgumentException
        // ------------------------------------------------------------------ //

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddTask_EmptyTitle_ThrowsArgumentException()
        {
            var mgr = MakeManager();
            mgr.AddTask(TaskType.Bug, "   ", "desc", TaskPriority.Low, DateTime.Now.AddDays(1));
        }

        // ------------------------------------------------------------------ //
        //  Test 5: UpdateStatus correctly changes task status
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void UpdateStatus_ChangesTaskStatus()
        {
            var mgr = MakeManager();
            var task = mgr.AddTask(TaskType.Chore, "Do maintenance", "clean up",
                                   TaskPriority.Low, DateTime.Now.AddDays(3));

            mgr.UpdateStatus(task.Id, AppTaskStatus.InProgress);
            var updated = mgr.FindById(task.Id);

            Assert.IsNotNull(updated);
            Assert.AreEqual(AppTaskStatus.InProgress, updated.Status);
        }

        // ------------------------------------------------------------------ //
        //  Test 6: DeleteTask removes the task
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void DeleteTask_RemovesTaskFromList()
        {
            var mgr = MakeManager();
            var task = mgr.AddTask(TaskType.Bug, "Fix crash", "app crashes on start",
                                   TaskPriority.Critical, DateTime.Now.AddDays(1));
            int id = task.Id;

            mgr.DeleteTask(id);

            Assert.IsNull(mgr.FindById(id));
        }

        // ------------------------------------------------------------------ //
        //  Test 7: DeleteTask on non-existent ID throws KeyNotFoundException
        // ------------------------------------------------------------------ //

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteTask_NonExistentId_ThrowsKeyNotFoundException()
        {
            var mgr = MakeManager();
            mgr.DeleteTask(99999);
        }

        // ------------------------------------------------------------------ //
        //  Test 8: Binary Search finds task by ID
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void BinarySearch_FindsExistingTaskById()
        {
            var list = new List<BaseTask>
            {
                new FeatureTask { Id = 1, Title = "A" },
                new FeatureTask { Id = 3, Title = "B" },
                new FeatureTask { Id = 5, Title = "C" },
                new FeatureTask { Id = 7, Title = "D" },
            };

            var result = SearchEngine.BinarySearchById(list, 5);

            Assert.IsNotNull(result);
            Assert.AreEqual("C", result.Title);
        }

        // ------------------------------------------------------------------ //
        //  Test 9: Binary Search returns null for missing ID
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void BinarySearch_ReturnsNull_WhenIdNotFound()
        {
            var list = new List<BaseTask>
            {
                new BugTask { Id = 2 },
                new BugTask { Id = 4 },
                new BugTask { Id = 6 },
            };

            var result = SearchEngine.BinarySearchById(list, 99);
            Assert.IsNull(result);
        }

        // ------------------------------------------------------------------ //
        //  Test 10: Linear keyword search returns correct results
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void LinearSearch_FindsTasksByKeyword()
        {
            var tasks = new List<BaseTask>
            {
                new FeatureTask { Id = 1, Title = "Login page",            Description = "OAuth integration" },
                new BugTask     { Id = 2, Title = "Fix null reference",    Description = "crash on startup" },
                new ChoreTask   { Id = 3, Title = "Update dependencies",   Description = "Bump packages" },
            };

            var results = SearchEngine.LinearSearchByKeyword(tasks, "login").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].Id);
        }

        // ------------------------------------------------------------------ //
        //  Test 11: QuickSort sorts tasks by due date ascending
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void QuickSort_SortsByDueDateAscending()
        {
            var tasks = new List<BaseTask>
            {
                new FeatureTask { Id = 1, DueDate = DateTime.Now.AddDays(10) },
                new FeatureTask { Id = 2, DueDate = DateTime.Now.AddDays(2)  },
                new FeatureTask { Id = 3, DueDate = DateTime.Now.AddDays(5)  },
            };

            var strategy = new QuickSortStrategy<BaseTask>();
            var sorted = strategy.Sort(tasks, (a, b) => a.DueDate.CompareTo(b.DueDate));

            Assert.AreEqual(2, sorted[0].Id);
            Assert.AreEqual(3, sorted[1].Id);
            Assert.AreEqual(1, sorted[2].Id);
        }

        // ------------------------------------------------------------------ //
        //  Test 12: IsOverdue returns correct value
        // ------------------------------------------------------------------ //

        [TestMethod]
        public void IsOverdue_ReturnsTrue_WhenDeadlinePassed()
        {
            var task = new BugTask
            {
                DueDate = DateTime.Now.AddDays(-1),
                Status  = AppTaskStatus.ToDo
            };
            Assert.IsTrue(task.IsOverdue());
        }

        [TestMethod]
        public void IsOverdue_ReturnsFalse_WhenStatusIsDone()
        {
            var task = new BugTask
            {
                DueDate = DateTime.Now.AddDays(-1),
                Status  = AppTaskStatus.Done
            };
            Assert.IsFalse(task.IsOverdue());
        }
    }
}
