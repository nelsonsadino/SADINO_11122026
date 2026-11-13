using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Tally.Application;
using Tally.Infrastructure;

namespace Tally.UnitTests;

public sealed class FileServiceTests
{
    private readonly InMemoryFileTracker _tracker = new();
    private readonly FileService _service;

    public FileServiceTests()
    {
        _service = new FileService(_tracker, NullLogger<FileService>.Instance);
    }

    [Fact]
    public async Task ProcessAsync_ValidFile_IsTrackedAsSucceeded()
    {
        await _service.ProcessAsync(Csv("amount\n5\n"), "good.csv", "amount");

        var tracked = Assert.Single(_tracker.GetAll());
        Assert.Equal("good.csv", tracked.FileName);
        Assert.True(tracked.Succeeded);
    }

    [Fact]
    public async Task ProcessAsync_InvalidFile_IsTrackedAsFailedAndThrows()
    {
        await Assert.ThrowsAsync<InvalidCsvException>(() => _service.ProcessAsync(Csv("a\n1\n"), "bad.csv", "price"));

        var tracked = Assert.Single(_tracker.GetAll());
        Assert.False(tracked.Succeeded);
        Assert.NotNull(tracked.Error);
    }

    [Fact]
    public async Task GetSummary_OneGoodAndOneBadFile_CountsBoth()
    {
        await _service.ProcessAsync(Csv("amount\n5\n"), "good.csv", "amount");
        await Assert.ThrowsAsync<InvalidCsvException>(() => _service.ProcessAsync(Csv("a\n1\n"), "bad.csv", "price"));

        var summary = _service.GetSummary();

        Assert.Equal(2, summary.TotalFiles);
        Assert.Equal(1, summary.Succeeded);
        Assert.Equal(1, summary.Failed);
    }

    private static MemoryStream Csv(string content) => new(Encoding.UTF8.GetBytes(content));
}
