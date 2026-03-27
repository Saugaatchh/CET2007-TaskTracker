namespace TrackerApp.Core.Models
{
    /// <summary>Represents a team member that can be assigned to a task.</summary>
    public class Assignee
    {
        /// <summary>Unique identifier for the assignee.</summary>
        public int Id { get; set; }

        /// <summary>Full name of the assignee.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Email address of the assignee.</summary>
        public string Email { get; set; } = string.Empty;

        public Assignee() { }

        public Assignee(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public override string ToString() => $"[{Id}] {Name} <{Email}>";
    }
}
