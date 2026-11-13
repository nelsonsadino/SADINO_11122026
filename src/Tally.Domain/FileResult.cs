namespace Tally.Domain;

public sealed record FileResult(
       string FileName,
       string Column,
       int RowCount,
       int SkippedRows,
       decimal Average,
       decimal Min,
       decimal Max);

