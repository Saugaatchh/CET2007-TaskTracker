using System.Text.Json.Serialization;
using TrackerApp.Core.Interfaces;

namespace TrackerApp.Core.Models
{
    /// <summary>
    /// Abstract base class implementing common ITask properties.
    /// Uses encapsulation to protect internal state and inheritance
    /// to share logic across concrete task types (FeatureTask, BugTask, etc.).
    /// </summary>
    public abstract class BaseTask : ITask
    {
        /// <inheritdoc/>
        public int Id { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Description { get; set; } = string.Empty;

        /// <inheritdoc/>
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        /// <inheritdoc/>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <inheritdoc/>
        [JsonIgnore]
        public abstract TaskType Type { get; }

        // Store the TaskType as a string for JSON serialization
        public string TaskTypeName => Type.ToString();

        /// <inheritdoc/>
        public DateTime DueDate { get; set; }

        /// <inheritdoc/>
        public DateTime CreatedAt { get; init; } = DateTime.Now;

        /// <inheritdoc/>
        public Assignee? Assignee { get; set; }

        /// <inheritdoc/>
        public bool IsOverdue() => Status != TaskStatus.Done && DateTime.Now > DueDate;

        /// <inheritdoc/>
        public virtual string GetSummary()
        {
            string overdueFlag = IsOverdue() ? " [OVERDUE]" : "";
            string assigneeName = Assignee != null ? $" | Assignee: {Assignee.Name}" : " | Unassigned";
            return $"[{Id}] [{Type}] {Title}{overdueFlag} | {Priority} | {Status} | Due: {DueDate:dd-MMM-yyyy}{assigneeName}";
        }

        public override string ToString() => GetSummary();
    }
}
