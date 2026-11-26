using WorkerManagement.BusinessLogic.Models;

namespace WorkerManagement.BusinessLogic.Services;

public interface IWorkerService
{
    Task<IReadOnlyCollection<Worker>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Worker> GetByIdAsync(string workerId, CancellationToken cancellationToken = default);

    Task<Worker> CreateAsync(WorkerInput input, CancellationToken cancellationToken = default);

    Task<Worker> UpdateAsync(string workerId, WorkerUpdateInput input, CancellationToken cancellationToken = default);

    Task DeleteAsync(string workerId, CancellationToken cancellationToken = default);
}
