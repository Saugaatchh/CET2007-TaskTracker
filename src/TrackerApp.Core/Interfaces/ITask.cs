using TrackerApp.Core.Models;

namespace TrackerApp.Core.Interfaces
{
    /// <summary>
    /// Defines the contract that every task in the system must satisfy.
    /// Supports polymorphism across different task types (Feature, Bug, Chore, Research).
    /// </summary>
    public interface ITask
    {
        int Id { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        Models.TaskStatus Status { get; set; }
        TaskPriority Priority { get; set; }
        TaskType Type { get; }
        DateTime DueDate { get; set; }
        DateTime CreatedAt { get; }
        Assignee? Assignee { get; set; }

        /// <summary>Returns a formatted summary string for the task.</summary>
        string GetSummary();

        /// <summary>
        /// Indicates whether this task is currently overdue.
        /// </summary>
        bool IsOverdue();
    }
}
