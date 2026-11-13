using System.Collections.Concurrent;
using Tally.Application;
using Tally.Domain;

namespace Tally.Infrastructure;

public sealed class InMemoryFileTracker : IFileTracker
{
    private readonly ConcurrentQueue<ProcessedFile> _files = new();

    public void Add(ProcessedFile file) => _files.Enqueue(file);

    public IReadOnlyList<ProcessedFile> GetAll() => _files.ToArray();
}
