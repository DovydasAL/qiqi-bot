using QiQiBot.Services.Notifications;

namespace QiQiBot.UnitTests.Services.Notifications;

public class MessageBatcherTests
{
    [Fact]
    public void BatchLines_WhenInputSpansMultipleBatches_ReturnsExpectedBatches()
    {
        var sut = new MessageBatcher();

        var result = sut.BatchLines(["line-1", "line-2", "line-3"], 2).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal($"line-1{Environment.NewLine}line-2", result[0]);
        Assert.Equal("line-3", result[1]);
    }

    [Fact]
    public void BatchLines_WhenInputIsEmpty_ReturnsNoBatches()
    {
        var sut = new MessageBatcher();

        var result = sut.BatchLines([], 10).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void BatchLines_WhenMaxLinesIsNotPositive_Throws()
    {
        var sut = new MessageBatcher();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.BatchLines(["a"], 0).ToList());
    }
}
