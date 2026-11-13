using Tally.Domain;

namespace Tally.Application;

public interface IFileTracker
{
    void Add(ProcessedFile file);

    IReadOnlyList<ProcessedFile> GetAll();
}
