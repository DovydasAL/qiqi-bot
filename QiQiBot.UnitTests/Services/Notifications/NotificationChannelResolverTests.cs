using Microsoft.Extensions.Logging;
using Moq;
using QiQiBot.Services;
using QiQiBot.Services.Notifications;
using System.Runtime.CompilerServices;

namespace QiQiBot.UnitTests.Services.Notifications;

public class NotificationChannelResolverTests
{
    [Fact]
    public void ResolveTextChannel_WhenNotificationTypeIsEmpty_Throws()
    {
        var discordClient = new Mock<IDiscordSocketClientWrapper>();
        var logger = new Mock<ILogger<NotificationChannelResolver>>();
        var sut = new NotificationChannelResolver(discordClient.Object, logger.Object);

        Assert.Throws<ArgumentException>(() => sut.ResolveTextChannel(1, 2, string.Empty));
    }

    [Fact]
    public void ResolveTextChannel_WhenGuildDoesNotExist_ReturnsNull()
    {
        var discordClient = new Mock<IDiscordSocketClientWrapper>();
        discordClient
            .Setup(x => x.GetGuild(123UL))
            .Returns((Discord.WebSocket.SocketGuild?)null);

        var logger = new Mock<ILogger<NotificationChannelResolver>>();
        var sut = new NotificationChannelResolver(discordClient.Object, logger.Object);

        var result = sut.ResolveTextChannel(123UL, 456UL, "achievement");

        Assert.Null(result);
    }

    [Fact]
    public void ResolveTextChannel_WhenGuildExistsButChannelDoesNotExist_ReturnsNull()
    {
        var guild = (Discord.WebSocket.SocketGuild)RuntimeHelpers.GetUninitializedObject(typeof(Discord.WebSocket.SocketGuild));

        var discordClient = new Mock<IDiscordSocketClientWrapper>();
        discordClient
            .Setup(x => x.GetGuild(123UL))
            .Returns(guild);
        discordClient
            .Setup(x => x.GetTextChannel(123UL, 456UL))
            .Returns((Discord.WebSocket.SocketTextChannel?)null);

        var logger = new Mock<ILogger<NotificationChannelResolver>>();
        var sut = new NotificationChannelResolver(discordClient.Object, logger.Object);

        var result = sut.ResolveTextChannel(123UL, 456UL, "achievement");

        Assert.Null(result);
    }
}
