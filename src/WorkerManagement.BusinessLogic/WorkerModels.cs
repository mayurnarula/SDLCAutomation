namespace WorkerManagement.BusinessLogic;

public sealed record WorkerResponse(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);

public sealed record CreateWorkerRequest(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);

public sealed record UpdateWorkerRequest(
    string Name,
    string Email,
    string Department,
    string Designation);
