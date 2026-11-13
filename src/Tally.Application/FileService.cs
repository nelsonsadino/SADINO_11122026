using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tally.Domain;

namespace Tally.Application;

public sealed class FileService(IFileTracker tracker, ILogger<FileService> logger)
{
    public async Task<FileResult> ProcessAsync(Stream csvFile, string fileName, string column)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await CsvCalculator.CalculateAsync(csvFile, fileName, column);
            Track(fileName, column, stopwatch.ElapsedMilliseconds, error: null);
            logger.LogInformation("Processed {FileName} in {ElapsedMs} ms", fileName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (InvalidCsvException ex)
        {
            Track(fileName, column, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    public IReadOnlyList<ProcessedFile> GetHistory() =>
        tracker.GetAll().OrderByDescending(file => file.ProcessedAtUtc).ToList();

    public FileSummary GetSummary()
    {
        var files = tracker.GetAll();

        return new FileSummary(
            TotalFiles: files.Count,
            Succeeded: files.Count(file => file.Succeeded),
            Failed: files.Count(file => !file.Succeeded),
            AverageProcessingTimeMs: files.Count == 0 ? 0 : files.Average(file => file.ProcessingTimeMs));
    }

    private void Track(string fileName, string column, long elapsedMs, string? error) =>
        tracker.Add(new ProcessedFile(fileName, column, Succeeded: error is null, error, DateTime.UtcNow, elapsedMs));
}
