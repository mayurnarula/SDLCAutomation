namespace WorkerManagement.Api.Contracts;

public sealed class CreateWorkerRequest
{
    public string? WorkerId { get; init; }

    public string? Name { get; init; }

    public string? Email { get; init; }

    public string? Department { get; init; }

    public string? Designation { get; init; }
}

public sealed class UpdateWorkerRequest
{
    public string? Name { get; init; }

    public string? Email { get; init; }

    public string? Department { get; init; }

    public string? Designation { get; init; }
}

public sealed record WorkerResponse(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);
