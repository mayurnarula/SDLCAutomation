using WorkerManagement.DataAccess.Models;

namespace WorkerManagement.DataAccess.Repositories;

public interface IWorkerRepository
{
    Task<IReadOnlyCollection<WorkerRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WorkerRecord?> GetByIdAsync(string workerId, CancellationToken cancellationToken = default);

    Task<bool> AddAsync(WorkerRecord worker, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(WorkerRecord worker, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string workerId, CancellationToken cancellationToken = default);
}
