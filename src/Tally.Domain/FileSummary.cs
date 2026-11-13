namespace Tally.Domain;

public sealed record FileSummary(
    int TotalFiles,
    int Succeeded,
    int Failed,
    double AverageProcessingTimeMs);
