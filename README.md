# CET2007 – Task & Project Tracker

> A professional-grade console application built for the CET2007 module (SEM1 25/26).  
> Demonstrates advanced OOP, design patterns, algorithms, file I/O, and unit testing in **C# .NET 8**.

---

## System Purpose

This **Task & Project Tracker** allows small teams to manage tasks across their projects from the command line. Users can **create**, **update**, **delete**, **search**, and **sort** tasks, with all data automatically persisted to JSON and all actions logged to a `.txt` audit file.

---

## How to Run

### Prerequisites
- [.NET 8 SDK](https://dot.net)
- A terminal (macOS / Windows / Linux)

### Steps
```bash
# 1. Clone the repository
git clone https://github.com/Saugaatchh/CET2007-TaskTracker.git
cd CET2007-TaskTracker

# 2. Build the solution
dotnet build

# 3. Run the Console App
dotnet run --project src/TrackerApp.Console

# 4. Run Unit Tests
dotnet test --verbosity normal
```

Data files are stored in:
```
src/TrackerApp.Console/bin/Debug/net8.0/data/
├── tasks.json      ← (all task data persisted as JSON)
├── app.log         ← (audit log written as TXT)
└── report_*.txt    ← (exported reports)
```

---

## Architecture & Design Choices

### Project Structure
```
CET2007-TaskTracker/
├── src/
│   ├── TrackerApp.Core/          ← Domain logic, interfaces, patterns
│   │   ├── Models/               ← BaseTask, FeatureTask, BugTask, ChoreTask, ResearchTask, Enums
│   │   ├── Interfaces/           ← ITask, IRepository<T>, ISortStrategy<T>
│   │   ├── Factories/            ← TaskFactory (Factory Method Pattern)
│   │   ├── Logging/              ← AppLogger (Singleton Pattern)
│   │   ├── Data/                 ← JsonTaskRepository (Repository Pattern)
│   │   ├── Algorithms/           ← QuickSort, BubbleSort (Strategy Pattern), SearchEngine
│   │   └── Services/             ← TaskManager (orchestration service)
│   ├── TrackerApp.Console/       ← Console UI (minimal Program.cs + ConsoleUI)
│   └── TrackerApp.Tests/         ← MSTest unit tests (13 tests)
└── CET2007-TaskTracker.sln
```

### Design Patterns Applied

| Pattern | Where Used | Justification |
|---|---|---|
| **Singleton** | `AppLogger` | Ensures one consistent audit trail across all subsystems |
| **Factory Method** | `TaskFactory` | Decouples task creation; easy to add new task types without modifying callers |
| **Repository** | `JsonTaskRepository` implements `IRepository<T>` | Abstracts all file I/O from business logic; easily swappable (e.g., DB) |
| **Strategy** | `QuickSortStrategy<T>` / `BubbleSortStrategy<T>` implement `ISortStrategy<T>` | Allows runtime sorting algorithm swapping; demonstrates OCP |

### OOP Principles
- **Inheritance:** `FeatureTask`, `BugTask`, `ChoreTask`, `ResearchTask` all extend `BaseTask`
- **Interfaces:** `ITask`, `IRepository<T>`, `ISortStrategy<T>` enforce contracts
- **Polymorphism:** `TaskManager` references `ITask`/`IRepository` — never concrete types
- **Encapsulation:** `AppLogger` uses a private constructor; `TaskManager` exposes only safe public APIs

### Algorithms
- **QuickSort** (`O(n log n)` average): Sorts tasks by due date or priority
- **BubbleSort** (`O(n²)`): Included as a second strategy for comparison, swappable at runtime
- **Binary Search** (`O(log n)`): Finds tasks by ID on a sorted list
- **Linear Search** (`O(n)`): Full-text keyword search across title and description

---

## Unit Tests

13 tests in `TrackerApp.Tests/TaskTrackerTests.cs` covering:
- Factory type creation and ID sequencing
- CRUD operations (Add, Update, Delete)
- Edge cases (empty title, missing ID)
- Binary and Linear search correctness
- QuickSort ordering verification
- `IsOverdue()` logic

---

## Author
CET2007 Student — University of Sunderland, SEM1 2025/26
