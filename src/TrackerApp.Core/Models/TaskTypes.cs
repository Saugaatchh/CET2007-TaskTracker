namespace TrackerApp.Core.Models
{
    /// <summary>
    /// Concrete task type: a new Feature to be developed.
    /// Inherits common behaviour from BaseTask.
    /// </summary>
    public class FeatureTask : BaseTask
    {
        public override TaskType Type => TaskType.Feature;

        /// <summary>Optional acceptance criteria for the feature.</summary>
        public string AcceptanceCriteria { get; set; } = string.Empty;

        public override string GetSummary()
        {
            string base_ = base.GetSummary();
            return string.IsNullOrWhiteSpace(AcceptanceCriteria)
                ? base_
                : $"{base_} | AC: {AcceptanceCriteria}";
        }
    }

    /// <summary>
    /// Concrete task type: a Bug to be fixed.
    /// Inherits common behaviour from BaseTask.
    /// </summary>
    public class BugTask : BaseTask
    {
        public override TaskType Type => TaskType.Bug;

        /// <summary>Severity level of the bug (e.g., P1, P2).</summary>
        // Severity follows standard P1-P4 notation: P1 = Critical, P4 = Minor
        public string Severity { get; set; } = "P3";

        public override string GetSummary()
        {
            return $"{base.GetSummary()} | Severity: {Severity}";
        }
    }

    /// <summary>
    /// Concrete task type: a routine Chore (maintenance, dependency updates, etc.).
    /// Inherits common behaviour from BaseTask.
    /// </summary>
    public class ChoreTask : BaseTask
    {
        public override TaskType Type => TaskType.Chore;
    }

    /// <summary>
    /// Concrete task type: a Research investigation or spike.
    /// Inherits common behaviour from BaseTask.
    /// </summary>
    public class ResearchTask : BaseTask
    {
        public override TaskType Type => TaskType.Research;

        /// <summary>The main research question or hypothesis to investigate.</summary>
        public string ResearchQuestion { get; set; } = string.Empty;
    }
}
