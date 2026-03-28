using System.Text.Json;
using System.Text.Json.Serialization;
using TrackerApp.Core.Interfaces;
using TrackerApp.Core.Logging;
using TrackerApp.Core.Models;

namespace TrackerApp.Core.Data
{
    /// <summary>
    /// Concrete JSON-backed Repository implementation.
    /// Implements the Repository pattern to abstract all file I/O from the rest of the system.
    /// Uses System.Text.Json to serialise/deserialise task data.
    /// </summary>
    public class JsonTaskRepository : IRepository<BaseTask>
    {
        private readonly string _filePath;
        private readonly AppLogger _logger;
        private List<BaseTask> _tasks = new();

        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new TaskConverter() }
        };

        public JsonTaskRepository(string dataDirectory = "data", AppLogger? logger = null)
        {
            Directory.CreateDirectory(dataDirectory);
            _filePath = Path.Combine(dataDirectory, "tasks.json");
            _logger = logger ?? AppLogger.GetInstance(dataDirectory);
            Load();
        }

        /// <inheritdoc/>
        public IEnumerable<BaseTask> GetAll() => _tasks;

        /// <inheritdoc/>
        public BaseTask? GetById(int id) => _tasks.FirstOrDefault(t => t.Id == id);

        /// <inheritdoc/>
        public void Add(BaseTask task)
        {
            _tasks.Add(task);
            _logger.Info($"Task added: ID={task.Id}, Title='{task.Title}', Type={task.Type}");
        }

        /// <inheritdoc/>
        public void Update(BaseTask updated)
        {
            int index = _tasks.FindIndex(t => t.Id == updated.Id);
            if (index < 0)
                throw new KeyNotFoundException($"No task with ID {updated.Id} found.");
            _tasks[index] = updated;
            _logger.Info($"Task updated: ID={updated.Id}, Status={updated.Status}");
        }

        /// <inheritdoc/>
        public void Delete(int id)
        {
            int removed = _tasks.RemoveAll(t => t.Id == id);
            if (removed == 0)
                throw new KeyNotFoundException($"No task with ID {id} found for deletion.");
            _logger.Info($"Task deleted: ID={id}");
        }

        /// <inheritdoc/>
        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_tasks, _options);
                File.WriteAllText(_filePath, json);
                _logger.Info($"Data persisted — {_tasks.Count} task(s) saved to '{_filePath}'.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save data: {ex.Message}");
                throw;
            }
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _logger.Info("No existing data file found. Starting with empty task list.");
                return;
            }
            try
            {
                string json = File.ReadAllText(_filePath);
                _tasks = JsonSerializer.Deserialize<List<BaseTask>>(json, _options) ?? new List<BaseTask>();
                _logger.Info($"Loaded {_tasks.Count} task(s) from '{_filePath}'.");
            }
            catch (JsonException ex)
            {
                _logger.Error($"Malformed JSON in data file: {ex.Message}. Starting fresh.");
                _tasks = new List<BaseTask>();
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read data file: {ex.Message}. Starting fresh.");
                _tasks = new List<BaseTask>();
            }
        }
    }

    /// <summary>
    /// Custom JSON converter to handle serialisation of abstract BaseTask subclasses.
    /// Reads the TaskTypeName discriminator field to reconstruct the correct concrete type.
    /// </summary>
    public class TaskConverter : JsonConverter<BaseTask>
    {
        public override BaseTask? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            string typeStr = root.TryGetProperty("TaskTypeName", out var typeProp)
                ? typeProp.GetString() ?? "Feature"
                : "Feature";

            BaseTask task = typeStr switch
            {
                "Bug"      => JsonSerializer.Deserialize<BugTask>(root.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!,
                "Chore"    => JsonSerializer.Deserialize<ChoreTask>(root.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!,
                "Research" => JsonSerializer.Deserialize<ResearchTask>(root.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!,
                _          => JsonSerializer.Deserialize<FeatureTask>(root.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!,
            };
            return task;
        }

        public override void Write(Utf8JsonWriter writer, BaseTask value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
