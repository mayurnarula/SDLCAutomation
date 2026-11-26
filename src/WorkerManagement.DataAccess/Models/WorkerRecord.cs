namespace WorkerManagement.DataAccess.Models;

public sealed record WorkerRecord(
    string WorkerId,
    string Name,
    string Email,
    string Department,
    string Designation);
