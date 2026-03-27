namespace TrackerApp.Core.Models
{
    /// <summary>Represents the current status of a task.</summary>
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }

    /// <summary>Represents the priority level of a task.</summary>
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>Represents the type/category of a task.</summary>
    public enum TaskType
    {
        Feature,
        Bug,
        Chore,
        Research
    }
}
