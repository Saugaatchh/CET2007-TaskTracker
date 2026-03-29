using TrackerApp.Core.Interfaces;

namespace TrackerApp.Core.Algorithms
{
    /// <summary>
    /// Strategy pattern: QuickSort implementation.
    /// Chosen for average-case O(n log n) performance — ideal for sorting tasks by due date or priority.
    /// This is a manual implementation demonstrating algorithmic knowledge (not using LINQ OrderBy).
    /// </summary>
    public class QuickSortStrategy<T> : ISortStrategy<T>
    {
        /// <inheritdoc/>
        public List<T> Sort(List<T> items, Comparison<T> comparison)
        {
            // QuickSort chosen for O(n log n) average case — more efficient than BubbleSort for larger task lists
            if (items.Count <= 1) return items;
            var copy = new List<T>(items);
            QuickSort(copy, 0, copy.Count - 1, comparison);
            return copy;
        }

        private void QuickSort(List<T> list, int low, int high, Comparison<T> comparison)
        {
            if (low < high)
            {
                int pi = Partition(list, low, high, comparison);
                QuickSort(list, low, pi - 1, comparison);
                QuickSort(list, pi + 1, high, comparison);
            }
        }

        private int Partition(List<T> list, int low, int high, Comparison<T> comparison)
        {
            T pivot = list[high];
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                if (comparison(list[j], pivot) <= 0)
                {
                    i++;
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
            (list[i + 1], list[high]) = (list[high], list[i + 1]);
            return i + 1;
        }
    }

    /// <summary>
    /// Strategy pattern: BubbleSort implementation.
    /// Included as a second sort strategy for comparison purposes — demonstrates the
    /// Strategy pattern by allowing runtime algorithm swapping.
    /// </summary>
    public class BubbleSortStrategy<T> : ISortStrategy<T>
    {
        /// <inheritdoc/>
        public List<T> Sort(List<T> items, Comparison<T> comparison)
        {
            var copy = new List<T>(items);
            int n = copy.Count;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (comparison(copy[j], copy[j + 1]) > 0)
                    {
                        (copy[j], copy[j + 1]) = (copy[j + 1], copy[j]);
                        swapped = true;
                    }
                }
                if (!swapped) break;
            }
            return copy;
        }
    }
}
