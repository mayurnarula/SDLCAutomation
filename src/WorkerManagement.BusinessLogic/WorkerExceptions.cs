namespace WorkerManagement.BusinessLogic;

public sealed class WorkerNotFoundException(string workerId)
    : Exception($"Worker '{workerId}' was not found.")
{
    public string WorkerId { get; } = workerId;
}

public sealed class DuplicateWorkerException(string workerId)
    : Exception($"A worker with identifier '{workerId}' already exists.")
{
    public string WorkerId { get; } = workerId;
}

public sealed class WorkerValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more worker fields are invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
