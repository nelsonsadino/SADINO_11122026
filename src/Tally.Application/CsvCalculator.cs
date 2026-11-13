using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Tally.Domain;

namespace Tally.Application;

public static class CsvCalculator
{
    private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture)
    {
        MissingFieldFound = null,
        TrimOptions = TrimOptions.Trim,
    };

    public static async Task<FileResult> CalculateAsync(Stream csvFile, string fileName, string column)
    {
        using var reader = new StreamReader(csvFile);
        using var csv = new CsvReader(reader, Config);

        try
        {
            var columnIndex = await FindColumnAsync(csv, column);
            var values = new List<decimal>();
            var rowCount = 0;

            while (await csv.ReadAsync())
            {
                rowCount++;
                if (decimal.TryParse(csv.GetField(columnIndex), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                {
                    values.Add(value);
                }
            }

            if (values.Count == 0)
            {
                throw new InvalidCsvException($"Column '{column}' has no numeric values.");
            }

            return new FileResult(
                fileName,
                column,
                rowCount,
                SkippedRows: rowCount - values.Count,
                Average: Math.Round(values.Average(), 2),
                Min: values.Min(),
                Max: values.Max());
        }
        catch (CsvHelperException ex)
        {
            throw new InvalidCsvException("The file could not be read as CSV.", ex);
        }
    }

    private static async Task<int> FindColumnAsync(CsvReader csv, string column)
    {
        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord is null)
        {
            throw new InvalidCsvException("The file is empty.");
        }

        var index = Array.FindIndex(csv.HeaderRecord, header => header.Equals(column, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            throw new InvalidCsvException(
                $"Column '{column}' was not found. Columns in the file: {string.Join(", ", csv.HeaderRecord)}.");
        }

        return index;
    }
}
