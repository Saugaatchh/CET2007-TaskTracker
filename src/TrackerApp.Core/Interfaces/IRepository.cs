namespace TrackerApp.Core.Interfaces
{
    /// <summary>
    /// Generic Repository pattern interface for CRUD operations.
    /// Decouples data access logic from business logic.
    /// </summary>
    /// <typeparam name="T">The entity type to manage.</typeparam>
    public interface IRepository<T>
    {
        /// <summary>Returns all entities.</summary>
        IEnumerable<T> GetAll();

        /// <summary>Finds an entity by its unique integer ID.</summary>
        T? GetById(int id);

        /// <summary>Adds a new entity to the store.</summary>
        void Add(T entity);

        /// <summary>Updates an existing entity.</summary>
        void Update(T entity);

        /// <summary>Removes an entity by its ID.</summary>
        void Delete(int id);

        /// <summary>Persists all current state to durable storage.</summary>
        void Save();
    }
}
