namespace WorkerManagement.BusinessLogic.Models;

public sealed record Worker(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);

public sealed record WorkerInput(
    string? WorkerId,
    string? Name,
    string? Email,
    string? Department,
    string? Designation);

public sealed record WorkerUpdateInput(
    string? Name,
    string? Email,
    string? Department,
    string? Designation);
