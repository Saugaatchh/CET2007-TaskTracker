# CET2007 Task & Project Tracker

## Target Audience: Developers / Markers

This application is a comprehensive console-based task tracker built as the final assessment for the **CET2007** module. It demonstrates advanced Object-Oriented Programming (OOP) principles, clean multi-tier architecture, and professional implementations of GoF design patterns.

### Key Technical Features:
- **Design Patterns**: Singleton (`AppLogger`), Factory Method (`TaskFactory`), Repository (`JsonTaskRepository`), Strategy (`QuickSort` and `BubbleSort`).
- **Data Persistence**: robust JSON serialization handling abstract types via polymorphism with local TXT log auditing.
- **Algorithms**: Manual implementation of QuickSort (O(n log n)) and Binary Search (O(log n)).
- **Testing**: 14 robust MSTest unit tests targeting 100% core domain logic coverage.

---

## How to Build & Run

Ensure you have the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.

To run the interactive application:
```bash
dotnet execute src/TrackerApp.Console  # OR
dotnet run --project src/TrackerApp.Console
```

## How to Run Tests

To verify test coverage and assertions:
```bash
dotnet test src/TrackerApp.Tests
```
