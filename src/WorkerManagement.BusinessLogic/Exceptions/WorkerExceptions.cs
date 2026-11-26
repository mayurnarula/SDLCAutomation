namespace WorkerManagement.BusinessLogic.Exceptions;

public sealed class WorkerValidationException : Exception
{
    public WorkerValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more worker fields are invalid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

public sealed class WorkerNotFoundException : Exception
{
    public WorkerNotFoundException(string workerId)
        : base($"Worker '{workerId}' was not found.")
    {
        WorkerId = workerId;
    }

    public string WorkerId { get; }
}

public sealed class DuplicateWorkerException : Exception
{
    public DuplicateWorkerException(string workerId)
        : base($"A worker with identifier '{workerId}' already exists.")
    {
        WorkerId = workerId;
    }

    public string WorkerId { get; }
}
