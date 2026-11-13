namespace Tally.Domain;

public sealed record ProcessedFile(
    string FileName,
    string Column,
    bool Succeeded,
    string? Error,
    DateTime ProcessedAtUtc,
    long ProcessingTimeMs);
