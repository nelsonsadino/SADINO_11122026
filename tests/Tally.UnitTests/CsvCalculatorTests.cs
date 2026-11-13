using System.Text;
using Tally.Application;
using Tally.Domain;

namespace Tally.UnitTests;

public sealed class CsvCalculatorTests
{
    [Fact]
    public async Task CalculateAsync_NumericColumn_ReturnsAverageMinAndMax()
    {
        var result = await RunAsync("name,amount\nA,10\nB,20\nC,30\n", "amount");

        Assert.Equal(3, result.RowCount);
        Assert.Equal(20m, result.Average);
        Assert.Equal(10m, result.Min);
        Assert.Equal(30m, result.Max);
    }

    [Fact]
    public async Task CalculateAsync_BlankAndTextCells_AreSkipped()
    {
        var result = await RunAsync("id,amount\n1,10\n2,abc\n3,\n4,30\n", "amount");

        Assert.Equal(4, result.RowCount);
        Assert.Equal(2, result.SkippedRows);
        Assert.Equal(20m, result.Average);
    }

    [Fact]
    public async Task CalculateAsync_QuotedComma_StaysInOneField()
    {
        var result = await RunAsync("product,qty\n\"Widget, Large\",3\nGizmo,5\n", "qty");

        Assert.Equal(4m, result.Average);
    }

    [Fact]
    public async Task CalculateAsync_ColumnNameInDifferentCase_StillMatches()
    {
        var result = await RunAsync("Amount\n5\n", "amount");

        Assert.Equal(5m, result.Average);
    }

    [Fact]
    public async Task CalculateAsync_UnknownColumn_Throws()
    {
        var ex = await Assert.ThrowsAsync<InvalidCsvException>(() => RunAsync("a,b\n1,2\n", "price"));

        Assert.Contains("price", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CalculateAsync_EmptyFile_Throws()
    {
        await Assert.ThrowsAsync<InvalidCsvException>(() => RunAsync(string.Empty, "amount"));
    }

    [Fact]
    public async Task CalculateAsync_NoNumbersInColumn_Throws()
    {
        await Assert.ThrowsAsync<InvalidCsvException>(() => RunAsync("amount\nx\ny\n", "amount"));
    }

    private static Task<FileResult> RunAsync(string csv, string column) =>
        CsvCalculator.CalculateAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "test.csv", column);
}
