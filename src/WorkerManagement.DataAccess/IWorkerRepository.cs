namespace WorkerManagement.DataAccess;

public interface IWorkerRepository
{
    Task<IReadOnlyList<WorkerRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WorkerRecord?> GetByIdAsync(string workerId, CancellationToken cancellationToken = default);

    Task<bool> AddAsync(WorkerRecord worker, CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkerRecord worker, CancellationToken cancellationToken = default);

    Task DeleteAsync(string workerId, CancellationToken cancellationToken = default);
}
