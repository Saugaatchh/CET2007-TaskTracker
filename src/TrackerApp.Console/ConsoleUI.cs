using TrackerApp.Core.Algorithms;
using TrackerApp.Core.Data;
using TrackerApp.Core.Logging;
using TrackerApp.Core.Models;
using TrackerApp.Core.Services;
using AppTaskStatus = TrackerApp.Core.Models.TaskStatus;

namespace TrackerApp.Console
{
    /// <summary>
    /// Interactive console menu for the Task and Project Tracker system.
    /// Handles all user input/output with thorough validation and error handling.
    /// All business logic is delegated to TaskManager — keeping UI concerns separate.
    /// </summary>
    public class ConsoleUI
    {
        private readonly TaskManager _manager;
        private readonly AppLogger _logger;

        private static readonly string DataDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "data");

        public ConsoleUI()
        {
            _logger  = AppLogger.GetInstance(DataDir);
            _manager = new TaskManager(
                new JsonTaskRepository(DataDir, _logger),
                _logger,
                new QuickSortStrategy<BaseTask>());
        }

        /// <summary>Runs the main menu loop until the user exits.</summary>
        public void Run()
        {
            PrintHeader();
            bool running = true;
            while (running)
            {
                PrintMainMenu();
                string choice = ReadLine("Select an option").Trim();
                try
                {
                    running = HandleMainMenu(choice);
                }
                catch (KeyNotFoundException ex)
                {
                    PrintError(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    PrintError(ex.Message);
                }
                catch (Exception ex)
                {
                    PrintError($"Unexpected error: {ex.Message}");
                    _logger.Error(ex.ToString());
                }
            }
            System.Console.WriteLine("\n  Goodbye! All data has been saved.\n");
        }

        // ------------------------------------------------------------------ //
        //  Main Menu
        // ------------------------------------------------------------------ //

        private void PrintMainMenu()
        {
            System.Console.WriteLine();
            ColorLine("  ┌─────────────────────────────────────┐", ConsoleColor.DarkCyan);
            ColorLine("  │          MAIN MENU                  │", ConsoleColor.Cyan);
            ColorLine("  ├─────────────────────────────────────┤", ConsoleColor.DarkCyan);
            ColorLine("  │  1. View All Tasks                  │", ConsoleColor.White);
            ColorLine("  │  2. Add New Task                    │", ConsoleColor.White);
            ColorLine("  │  3. Update Task Status              │", ConsoleColor.White);
            ColorLine("  │  4. Edit Task Details               │", ConsoleColor.White);
            ColorLine("  │  5. Delete Task                     │", ConsoleColor.White);
            ColorLine("  │  6. Search Tasks                    │", ConsoleColor.White);
            ColorLine("  │  7. Sort Tasks                      │", ConsoleColor.White);
            ColorLine("  │  8. View Overdue Tasks              │", ConsoleColor.Yellow);
            ColorLine("  │  9. Export Report                   │", ConsoleColor.White);
            ColorLine("  │  0. Exit                            │", ConsoleColor.Red);
            ColorLine("  └─────────────────────────────────────┘", ConsoleColor.DarkCyan);
        }

        private bool HandleMainMenu(string choice)
        {
            switch (choice)
            {
                case "1": ShowAllTasks();        break;
                case "2": AddTask();             break;
                case "3": UpdateTaskStatus();    break;
                case "4": EditTaskDetails();     break;
                case "5": DeleteTask();          break;
                case "6": SearchMenu();          break;
                case "7": SortMenu();            break;
                case "8": ShowOverdueTasks();    break;
                case "9": ExportReport();        break;
                case "0": return false;
                default:  PrintError("Invalid option. Please enter 0-9."); break;
            }
            return true;
        }

        // ------------------------------------------------------------------ //
        //  Task Operations
        // ------------------------------------------------------------------ //

        private void ShowAllTasks()
        {
            var tasks = _manager.GetAllTasks();
            PrintSectionHeader($"ALL TASKS  ({tasks.Count} total)");
            if (tasks.Count == 0) { ColorLine("  No tasks yet.", ConsoleColor.Gray); return; }
            foreach (var t in tasks) PrintTask(t);
        }

        private void AddTask()
        {
            PrintSectionHeader("ADD NEW TASK");

            TaskType type = PickEnum<TaskType>("Task type");
            string title = ReadLine("Title (required)");
            if (string.IsNullOrWhiteSpace(title)) { PrintError("Title is required."); return; }
            string desc  = ReadLine("Description (optional)");
            TaskPriority priority = PickEnum<TaskPriority>("Priority");
            DateTime due = ReadDate("Due date (dd/MM/yyyy)");

            Assignee? assignee = null;
            if (ReadBool("Assign to someone?"))
            {
                int aId    = ReadInt("Assignee ID");
                string name  = ReadLine("Assignee name");
                string email = ReadLine("Assignee email");
                assignee = new Assignee(aId, name, email);
            }

            var task = _manager.AddTask(type, title, desc, priority, due, assignee);
            PrintSuccess($"Task #{task.Id} '{task.Title}' added successfully.");
        }

        private void UpdateTaskStatus()
        {
            PrintSectionHeader("UPDATE TASK STATUS");
            int id = ReadInt("Enter Task ID");
            var task = _manager.FindById(id);
            if (task == null) { PrintError($"Task #{id} not found."); return; }
            System.Console.WriteLine($"  Current status: {task.Status}");
            AppTaskStatus newStatus = PickEnum<AppTaskStatus>("New status");
            _manager.UpdateStatus(id, newStatus);
            PrintSuccess($"Task #{id} status updated to {newStatus}.");
        }

        private void EditTaskDetails()
        {
            PrintSectionHeader("EDIT TASK DETAILS");
            int id = ReadInt("Enter Task ID");
            var task = _manager.FindById(id);
            if (task == null) { PrintError($"Task #{id} not found."); return; }

            System.Console.WriteLine($"  Editing: {task.GetSummary()}");
            System.Console.WriteLine("  (Press Enter to keep existing value)");

            string title = ReadLine($"Title [{task.Title}]");
            if (string.IsNullOrWhiteSpace(title)) title = task.Title;

            string desc = ReadLine($"Description [{task.Description}]");
            if (string.IsNullOrWhiteSpace(desc)) desc = task.Description;

            System.Console.WriteLine($"  Current priority: {task.Priority}");
            TaskPriority priority = PickEnum<TaskPriority>("New priority");

            System.Console.WriteLine($"  Current due date: {task.DueDate:dd/MM/yyyy}");
            DateTime due = ReadDate("New due date (dd/MM/yyyy)");

            _manager.UpdateTask(id, title, desc, priority, due, task.Assignee);
            PrintSuccess($"Task #{id} updated successfully.");
        }

        private void DeleteTask()
        {
            PrintSectionHeader("DELETE TASK");
            int id = ReadInt("Enter Task ID to delete");
            var task = _manager.FindById(id);
            if (task == null) { PrintError($"Task #{id} not found."); return; }
            System.Console.WriteLine($"  About to delete: {task.GetSummary()}");
            if (!ReadBool("Are you sure?")) { System.Console.WriteLine("  Cancelled."); return; }
            _manager.DeleteTask(id);
            PrintSuccess($"Task #{id} deleted.");
        }

        // ------------------------------------------------------------------ //
        //  Search & Sort
        // ------------------------------------------------------------------ //

        private void SearchMenu()
        {
            PrintSectionHeader("SEARCH TASKS");
            System.Console.WriteLine("  1. Search by keyword (title/description)");
            System.Console.WriteLine("  2. Find by ID (Binary Search)");
            System.Console.WriteLine("  3. Filter by status");
            string choice = ReadLine("Select").Trim();

            switch (choice)
            {
                case "1":
                    string kw = ReadLine("Enter keyword");
                    var res = _manager.SearchByKeyword(kw).ToList();
                    PrintSectionHeader($"RESULTS FOR '{kw}'  ({res.Count} found)");
                    if (res.Count == 0) { ColorLine("  No matches.", ConsoleColor.Gray); return; }
                    res.ForEach(PrintTask);
                    break;
                case "2":
                    int id = ReadInt("Enter Task ID");
                    var found = _manager.FindById(id);
                    if (found != null) PrintTask(found);
                    else PrintError($"No task with ID {id}.");
                    break;
                case "3":
                    AppTaskStatus status = PickEnum<AppTaskStatus>("Status to filter");
                    var filtered = _manager.FilterByStatus(status).ToList();
                    PrintSectionHeader($"TASKS WITH STATUS: {status}  ({filtered.Count} found)");
                    if (filtered.Count == 0) { ColorLine("  None.", ConsoleColor.Gray); return; }
                    filtered.ForEach(PrintTask);
                    break;
                default:
                    PrintError("Invalid option.");
                    break;
            }
        }

        private void SortMenu()
        {
            PrintSectionHeader("SORT TASKS");
            System.Console.WriteLine("  Sort by:");
            System.Console.WriteLine("    1. Due Date (ascending)  — QuickSort");
            System.Console.WriteLine("    2. Priority (descending) — QuickSort");
            System.Console.WriteLine("    3. Due Date (ascending)  — BubbleSort (comparison)");
            string choice = ReadLine("Select").Trim();

            switch (choice)
            {
                case "1":
                    _manager.SetSortStrategy(new QuickSortStrategy<BaseTask>());
                    var byDate = _manager.GetSortedByDueDate();
                    PrintSectionHeader("SORTED BY DUE DATE (QuickSort)");
                    byDate.ForEach(PrintTask);
                    break;
                case "2":
                    _manager.SetSortStrategy(new QuickSortStrategy<BaseTask>());
                    var byPriority = _manager.GetSortedByPriority();
                    PrintSectionHeader("SORTED BY PRIORITY (QuickSort)");
                    byPriority.ForEach(PrintTask);
                    break;
                case "3":
                    _manager.SetSortStrategy(new BubbleSortStrategy<BaseTask>());
                    var bubbleSorted = _manager.GetSortedByDueDate();
                    PrintSectionHeader("SORTED BY DUE DATE (BubbleSort)");
                    bubbleSorted.ForEach(PrintTask);
                    break;
                default:
                    PrintError("Invalid option.");
                    break;
            }
        }

        private void ShowOverdueTasks()
        {
            var overdue = _manager.GetOverdueTasks().ToList();
            PrintSectionHeader($"OVERDUE TASKS ({overdue.Count})");
            if (overdue.Count == 0) { ColorLine("  No overdue tasks!", ConsoleColor.Green); return; }
            overdue.ForEach(PrintTask);
        }

        private void ExportReport()
        {
            string path = _manager.ExportReport(DataDir);
            PrintSuccess($"Report exported to:\n    {path}");
        }

        // ------------------------------------------------------------------ //
        //  UI Helpers & Input Validation
        // ------------------------------------------------------------------ //

        private static void PrintHeader()
        {
            System.Console.Clear();
            ColorLine("\n  ╔══════════════════════════════════════════╗", ConsoleColor.Cyan);
            ColorLine("  ║    TASK & PROJECT TRACKER  v1.1          ║", ConsoleColor.Cyan);
            ColorLine("  ║    CET2007 Assignment — SEM1 25/26       ║", ConsoleColor.DarkCyan);
            ColorLine("  ╚══════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        private static void PrintSectionHeader(string title)
        {
            System.Console.WriteLine();
            ColorLine($"  ── {title.ToUpper()} ──", ConsoleColor.Cyan);
        }

        private static void PrintTask(BaseTask task)
        {
            ConsoleColor color = task.IsOverdue() ? ConsoleColor.Red :
                                 task.Status == AppTaskStatus.Done ? ConsoleColor.DarkGray :
                                 task.Priority == TaskPriority.Critical ? ConsoleColor.Yellow :
                                 ConsoleColor.White;
            ColorLine("  " + task.GetSummary(), color);
        }

        private static void PrintSuccess(string msg) => ColorLine($"\n  ✓ {msg}", ConsoleColor.Green);
        private static void PrintError(string msg)   => ColorLine($"\n  ✗ {msg}", ConsoleColor.Red);

        private static void ColorLine(string text, ConsoleColor color)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(text);
            System.Console.ResetColor();
        }

        private static string ReadLine(string prompt)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.Write($"\n  {prompt}: ");
            System.Console.ResetColor();
            return System.Console.ReadLine() ?? string.Empty;
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                string raw = ReadLine(prompt);
                if (int.TryParse(raw, out int val)) return val;
                PrintError("Please enter a valid number.");
            }
        }

        private static bool ReadBool(string prompt)
        {
            string raw = ReadLine($"{prompt} (y/n)").Trim().ToLower();
            return raw is "y" or "yes";
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                string raw = ReadLine(prompt).Trim();
                if (DateTime.TryParseExact(raw, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dt))
                    return dt;
                PrintError("Invalid date format. Please use dd/MM/yyyy.");
            }
        }

        private static T PickEnum<T>(string prompt) where T : struct, Enum
        {
            var values = Enum.GetValues<T>();
            System.Console.WriteLine();
            for (int i = 0; i < values.Length; i++)
                System.Console.WriteLine($"  {i + 1}. {values[i]}");

            while (true)
            {
                string raw = ReadLine(prompt);
                if (int.TryParse(raw, out int idx) && idx >= 1 && idx <= values.Length)
                    return values[idx - 1];
                PrintError($"Please enter a number between 1 and {values.Length}.");
            }
        }
    }
}
