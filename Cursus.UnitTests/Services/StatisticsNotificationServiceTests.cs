using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Cursus.Service;
using Cursus.API.Hubs;
using Cursus.Service.Services;
namespace Cursus.UnitTests.Services;
[TestFixture]
public class StatisticsNotificationServiceTests
{
    private Mock<IHubContext<StatisticsHub>> _hubContextMock;
    private Mock<IHubClients> _hubClientsMock;
    private Mock<IClientProxy> _clientProxyMock;
    private StatisticsNotificationService _service;

    [SetUp]
    public void Setup()
    {
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubClientsMock.Setup(clients => clients.All).Returns(_clientProxyMock.Object);

        _hubContextMock = new Mock<IHubContext<StatisticsHub>>();
        _hubContextMock.Setup(hub => hub.Clients).Returns(_hubClientsMock.Object);

        _service = new StatisticsNotificationService(_hubContextMock.Object);
    }

    [Test]
    public async Task NotifySalesAndRevenueUpdate_SendsMessageToAllClients()
    {
        // Act
        await _service.NotifySalesAndRevenueUpdate();

        // Assert
        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveSalesAndRevenueUpdate",
                It.IsAny<object[]>(),
                default
            ),
            Times.Once
        );
        Assert.That(() => _clientProxyMock.Invocations.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task NotifyOrderStatisticsUpdate_SendsMessageToAllClients()
    {
        // Act
        await _service.NotifyOrderStatisticsUpdate();

        // Assert
        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderStatisticsUpdate",
                It.IsAny<object[]>(),
                default
            ),
            Times.Once
        );
        Assert.That(() => _clientProxyMock.Invocations.Count, Is.EqualTo(1));
    }
}
