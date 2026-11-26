using System.ComponentModel.DataAnnotations;

namespace WorkerManagement.Api.Contracts;

public sealed class CreateWorkerModel
{
    [Required, StringLength(100)]
    public string WorkerId { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Department { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Designation { get; init; } = string.Empty;
}

public sealed class UpdateWorkerModel
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Department { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Designation { get; init; } = string.Empty;
}

public sealed record WorkerModel(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);
