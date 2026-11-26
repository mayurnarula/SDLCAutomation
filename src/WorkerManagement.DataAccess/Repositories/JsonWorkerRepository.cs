using System.Text.Json;
using WorkerManagement.DataAccess.Configuration;
using WorkerManagement.DataAccess.Models;

namespace WorkerManagement.DataAccess.Repositories;

public sealed class JsonWorkerRepository : IWorkerRepository
{
    private static readonly SemaphoreSlim FileGate = new(1, 1);
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonWorkerRepository(WorkerStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var configuredPath = string.IsNullOrWhiteSpace(options.FilePath)
            ? "data/workers.json"
            : options.FilePath;

        _filePath = Path.IsPathFullyQualified(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath);
    }

    public async Task<IReadOnlyCollection<WorkerRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await FileGate.WaitAsync(cancellationToken);
        try
        {
            return await ReadAllUnsafeAsync(cancellationToken);
        }
        finally
        {
            FileGate.Release();
        }
    }

    public async Task<WorkerRecord?> GetByIdAsync(string workerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        var workers = await GetAllAsync(cancellationToken);
        return workers.FirstOrDefault(worker =>
            string.Equals(worker.WorkerId, workerId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> AddAsync(WorkerRecord worker, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        await FileGate.WaitAsync(cancellationToken);
        try
        {
            var workers = (await ReadAllUnsafeAsync(cancellationToken)).ToList();
            if (workers.Any(existing => string.Equals(existing.WorkerId, worker.WorkerId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            workers.Add(worker);
            await WriteAllUnsafeAsync(workers, cancellationToken);
            return true;
        }
        finally
        {
            FileGate.Release();
        }
    }

    public async Task<bool> UpdateAsync(WorkerRecord worker, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        await FileGate.WaitAsync(cancellationToken);
        try
        {
            var workers = (await ReadAllUnsafeAsync(cancellationToken)).ToList();
            var index = workers.FindIndex(existing =>
                string.Equals(existing.WorkerId, worker.WorkerId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return false;
            }

            workers[index] = worker;
            await WriteAllUnsafeAsync(workers, cancellationToken);
            return true;
        }
        finally
        {
            FileGate.Release();
        }
    }

    public async Task<bool> DeleteAsync(string workerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        await FileGate.WaitAsync(cancellationToken);
        try
        {
            var workers = (await ReadAllUnsafeAsync(cancellationToken)).ToList();
            var removed = workers.RemoveAll(existing =>
                string.Equals(existing.WorkerId, workerId, StringComparison.OrdinalIgnoreCase)) > 0;
            if (!removed)
            {
                return false;
            }

            await WriteAllUnsafeAsync(workers, cancellationToken);
            return true;
        }
        finally
        {
            FileGate.Release();
        }
    }

    private async Task<IReadOnlyCollection<WorkerRecord>> ReadAllUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<WorkerRecord>();
        }

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<WorkerRecord>();
        }

        return JsonSerializer.Deserialize<List<WorkerRecord>>(json, SerializerOptions)
            ?? new List<WorkerRecord>();
    }

    private async Task WriteAllUnsafeAsync(IReadOnlyCollection<WorkerRecord> workers, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var json = JsonSerializer.Serialize(workers, SerializerOptions);
            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
