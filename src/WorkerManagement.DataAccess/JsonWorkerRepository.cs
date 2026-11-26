using System.Text.Json;

namespace WorkerManagement.DataAccess;

public sealed class JsonWorkerRepository : IWorkerRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonWorkerRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A worker data file path is required.", nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);
    }

    public async Task<IReadOnlyList<WorkerRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var workers = await ReadWorkersAsync(cancellationToken);
            return workers.ToArray();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<WorkerRecord?> GetByIdAsync(string workerId, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var workers = await ReadWorkersAsync(cancellationToken);
            return workers.FirstOrDefault(worker =>
                string.Equals(worker.WorkerId, workerId, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<bool> AddAsync(WorkerRecord worker, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var workers = await ReadWorkersAsync(cancellationToken);
            if (workers.Any(existing =>
                    string.Equals(existing.WorkerId, worker.WorkerId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            workers.Add(worker);
            await WriteWorkersAsync(workers, cancellationToken);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(WorkerRecord worker, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var workers = await ReadWorkersAsync(cancellationToken);
            var index = workers.FindIndex(existing =>
                string.Equals(existing.WorkerId, worker.WorkerId, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                throw new KeyNotFoundException($"Worker '{worker.WorkerId}' was not found.");
            }

            workers[index] = worker;
            await WriteWorkersAsync(workers, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task DeleteAsync(string workerId, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var workers = await ReadWorkersAsync(cancellationToken);
            var removed = workers.RemoveAll(worker =>
                string.Equals(worker.WorkerId, workerId, StringComparison.OrdinalIgnoreCase));

            if (removed == 0)
            {
                throw new KeyNotFoundException($"Worker '{workerId}' was not found.");
            }

            await WriteWorkersAsync(workers, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<WorkerRecord>> ReadWorkersAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        return await JsonSerializer.DeserializeAsync<List<WorkerRecord>>(
                   stream,
                   SerializerOptions,
                   cancellationToken)
               ?? [];
    }

    private async Task WriteWorkersAsync(
        IReadOnlyCollection<WorkerRecord> workers,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await JsonSerializer.SerializeAsync(stream, workers, SerializerOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

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
