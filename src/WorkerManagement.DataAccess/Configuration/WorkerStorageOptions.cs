namespace WorkerManagement.DataAccess.Configuration;

public sealed class WorkerStorageOptions
{
    public const string SectionName = "WorkerStorage";

    public string FilePath { get; set; } = "data/workers.json";
}
