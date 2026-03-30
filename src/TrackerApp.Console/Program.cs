using TrackerApp.Core.Algorithms;
using TrackerApp.Core.Models;
using TrackerApp.Core.Services;

namespace TrackerApp.Console
{
    /// <summary>
    /// Entry point for the Task & Project Tracker Console Application.
    /// Minimal Program.cs — all logic is delegated to ConsoleUI.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var ui = new ConsoleUI();
            ui.Run();
        }
    }
}
