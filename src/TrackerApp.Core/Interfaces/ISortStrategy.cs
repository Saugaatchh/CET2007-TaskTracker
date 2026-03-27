namespace TrackerApp.Core.Interfaces
{
    /// <summary>
    /// Strategy pattern interface for interchangeable sorting algorithms.
    /// Allows different sort strategies (QuickSort, BubbleSort) to be swapped at runtime.
    /// </summary>
    /// <typeparam name="T">The type of elements to sort.</typeparam>
    public interface ISortStrategy<T>
    {
        /// <summary>Sorts the provided list using this strategy's algorithm.</summary>
        List<T> Sort(List<T> items, Comparison<T> comparison);
    }
}
