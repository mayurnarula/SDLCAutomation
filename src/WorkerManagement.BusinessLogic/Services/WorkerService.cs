using System.Net.Mail;
using WorkerManagement.BusinessLogic.Exceptions;
using WorkerManagement.BusinessLogic.Models;
using WorkerManagement.DataAccess.Models;
using WorkerManagement.DataAccess.Repositories;

namespace WorkerManagement.BusinessLogic.Services;

public sealed class WorkerService(IWorkerRepository repository) : IWorkerService
{
    public async Task<IReadOnlyCollection<Worker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await repository.GetAllAsync(cancellationToken);
        return records.Select(Map).ToArray();
    }

    public async Task<Worker> GetByIdAsync(string workerId, CancellationToken cancellationToken = default)
    {
        ValidateWorkerId(workerId);

        var record = await repository.GetByIdAsync(workerId, cancellationToken);
        return record is null
            ? throw new WorkerNotFoundException(workerId)
            : Map(record);
    }

    public async Task<Worker> CreateAsync(WorkerInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateCreateInput(input);

        var record = new WorkerRecord(
            input.WorkerId!,
            input.Name!,
            input.Email!,
            input.Department!,
            input.Designation!);

        if (!await repository.AddAsync(record, cancellationToken))
        {
            throw new DuplicateWorkerException(record.WorkerId);
        }

        return Map(record);
    }

    public async Task<Worker> UpdateAsync(
        string workerId,
        WorkerUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateWorkerId(workerId);
        ValidateUpdateInput(input);

        var existing = await repository.GetByIdAsync(workerId, cancellationToken);
        if (existing is null)
        {
            throw new WorkerNotFoundException(workerId);
        }

        var replacement = new WorkerRecord(
            existing.WorkerId,
            input.Name!,
            input.Email!,
            input.Department!,
            input.Designation!);

        if (!await repository.UpdateAsync(replacement, cancellationToken))
        {
            throw new WorkerNotFoundException(workerId);
        }

        return Map(replacement);
    }

    public async Task DeleteAsync(string workerId, CancellationToken cancellationToken = default)
    {
        ValidateWorkerId(workerId);

        if (!await repository.DeleteAsync(workerId, cancellationToken))
        {
            throw new WorkerNotFoundException(workerId);
        }
    }

    private static Worker Map(WorkerRecord record) => new(
        record.WorkerId,
        record.Name,
        record.Email,
        record.Department,
        record.Designation);

    private static void ValidateCreateInput(WorkerInput input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        AddRequiredError(errors, nameof(input.WorkerId), input.WorkerId);
        AddRequiredError(errors, nameof(input.Name), input.Name);
        AddRequiredError(errors, nameof(input.Email), input.Email);
        AddRequiredError(errors, nameof(input.Department), input.Department);
        AddRequiredError(errors, nameof(input.Designation), input.Designation);
        AddEmailError(errors, nameof(input.Email), input.Email);
        ThrowIfInvalid(errors);
    }

    private static void ValidateUpdateInput(WorkerUpdateInput input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        AddRequiredError(errors, nameof(input.Name), input.Name);
        AddRequiredError(errors, nameof(input.Email), input.Email);
        AddRequiredError(errors, nameof(input.Department), input.Department);
        AddRequiredError(errors, nameof(input.Designation), input.Designation);
        AddEmailError(errors, nameof(input.Email), input.Email);
        ThrowIfInvalid(errors);
    }

    private static void ValidateWorkerId(string workerId)
    {
        if (!string.IsNullOrWhiteSpace(workerId))
        {
            return;
        }

        throw new WorkerValidationException(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["workerId"] = ["WorkerId is required."]
        });
    }

    private static void AddRequiredError(IDictionary<string, string[]> errors, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
        }
    }

    private static void AddEmailError(IDictionary<string, string[]> errors, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || errors.ContainsKey(field))
        {
            return;
        }

        if (!MailAddress.TryCreate(value, out var address) ||
            !string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase))
        {
            errors[field] = ["Email must be a valid email address."];
        }
    }

    private static void ThrowIfInvalid(IReadOnlyDictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new WorkerValidationException(errors);
        }
    }
}
