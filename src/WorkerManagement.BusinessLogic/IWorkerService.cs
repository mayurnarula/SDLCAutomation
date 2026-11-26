namespace WorkerManagement.BusinessLogic;

public interface IWorkerService
{
    Task<IReadOnlyList<WorkerResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WorkerResponse> GetByIdAsync(string workerId, CancellationToken cancellationToken = default);

    Task<WorkerResponse> CreateAsync(
        CreateWorkerRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkerResponse> UpdateAsync(
        string workerId,
        UpdateWorkerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string workerId, CancellationToken cancellationToken = default);
}
