using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Notification.Service.Services;

namespace Notification.Service.IntegrationTests;

public class DependencyInjectionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DependencyInjectionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void NotificationService_ShouldBeRegisteredAsSingleton()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        
        var service1 = scope1.ServiceProvider.GetRequiredService<INotificationService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<INotificationService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void SignalR_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var hubContext = scope.ServiceProvider.GetService<Microsoft.AspNetCore.SignalR.IHubContext<Notification.Service.Hubs.NotificationHub>>();

        // Assert
        hubContext.Should().NotBeNull();
    }
}

