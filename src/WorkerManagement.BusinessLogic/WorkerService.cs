using System.Net.Mail;
using WorkerManagement.DataAccess;

namespace WorkerManagement.BusinessLogic;

public sealed class WorkerService(IWorkerRepository repository) : IWorkerService
{
    public async Task<IReadOnlyList<WorkerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var workers = await repository.GetAllAsync(cancellationToken);
        return workers.Select(Map).ToArray();
    }

    public async Task<WorkerResponse> GetByIdAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(workerId);
        var worker = await repository.GetByIdAsync(workerId, cancellationToken)
            ?? throw new WorkerNotFoundException(workerId);

        return Map(worker);
    }

    public async Task<WorkerResponse> CreateAsync(
        CreateWorkerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(
            request.WorkerId,
            request.Name,
            request.Email,
            request.Department,
            request.Designation);

        var worker = new WorkerRecord(
            request.WorkerId.Trim(),
            request.Name.Trim(),
            request.Email.Trim(),
            request.Department.Trim(),
            request.Designation.Trim());

        if (!await repository.AddAsync(worker, cancellationToken))
        {
            throw new DuplicateWorkerException(worker.WorkerId);
        }

        return Map(worker);
    }

    public async Task<WorkerResponse> UpdateAsync(
        string workerId,
        UpdateWorkerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateIdentifier(workerId);
        Validate(
            workerId,
            request.Name,
            request.Email,
            request.Department,
            request.Designation,
            includeIdentifier: false);

        var existing = await repository.GetByIdAsync(workerId, cancellationToken)
            ?? throw new WorkerNotFoundException(workerId);

        var updated = existing with
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim()
        };

        await repository.UpdateAsync(updated, cancellationToken);
        return Map(updated);
    }

    public async Task DeleteAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(workerId);
        if (await repository.GetByIdAsync(workerId, cancellationToken) is null)
        {
            throw new WorkerNotFoundException(workerId);
        }

        await repository.DeleteAsync(workerId, cancellationToken);
    }

    private static WorkerResponse Map(WorkerRecord worker) => new(
        worker.WorkerId,
        worker.Name,
        worker.Email,
        worker.Department,
        worker.Designation);

    private static void Validate(
        string? workerId,
        string? name,
        string? email,
        string? department,
        string? designation,
        bool includeIdentifier = true)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (includeIdentifier && string.IsNullOrWhiteSpace(workerId))
        {
            errors[nameof(CreateWorkerRequest.WorkerId)] = ["WorkerId is required."];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors[nameof(CreateWorkerRequest.Name)] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors[nameof(CreateWorkerRequest.Email)] = ["Email is required."];
        }
        else if (!IsValidEmail(email))
        {
            errors[nameof(CreateWorkerRequest.Email)] = ["Email must be a valid email address."];
        }

        if (string.IsNullOrWhiteSpace(department))
        {
            errors[nameof(CreateWorkerRequest.Department)] = ["Department is required."];
        }

        if (string.IsNullOrWhiteSpace(designation))
        {
            errors[nameof(CreateWorkerRequest.Designation)] = ["Designation is required."];
        }

        if (errors.Count > 0)
        {
            throw new WorkerValidationException(errors);
        }
    }

    private static void ValidateIdentifier(string? workerId)
    {
        if (string.IsNullOrWhiteSpace(workerId))
        {
            throw new WorkerValidationException(new Dictionary<string, string[]>
            {
                [nameof(CreateWorkerRequest.WorkerId)] = ["WorkerId is required."]
            });
        }
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value.Trim());
            return string.Equals(address.Address, value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
