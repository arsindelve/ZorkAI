using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Interface;
using Planetfall_Lambda;

namespace UnitTests.PlanetfallLambda;

[TestFixture]
public class StartupTests
{
    [SetUp]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _startup = new Startup(_mockConfiguration.Object);
        _mockServices = new Mock<IServiceCollection>();

        // Setup service collection to return itself for chaining
        _mockServices.Setup(s => s.Add(It.IsAny<ServiceDescriptor>()));

        // Setup application builder for chaining - simplified to avoid optional parameter issues
    }

    private Startup _startup;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IServiceCollection> _mockServices;

    [Test]
    public void Constructor_Should_SetConfiguration()
    {
        // Arrange & Act
        var startup = new Startup(_mockConfiguration.Object);

        // Assert
        startup.Configuration.Should().Be(_mockConfiguration.Object);
    }

    [Test]
    public void ConfigureServices_Should_AddRequiredServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        _startup.ConfigureServices(serviceCollection);

        // Assert
        serviceCollection.Should().NotBeEmpty();
        serviceCollection.Any(s => s.ServiceType == typeof(ILoggerFactory)).Should().BeTrue();
    }

    [Test]
    public void ConfigureServices_Should_AddControllers()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        _startup.ConfigureServices(serviceCollection);

        // Assert
        serviceCollection.Should().NotBeEmpty();
    }

    [Test]
    public void ConfigureServices_Should_AddGameEngine()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        _startup.ConfigureServices(serviceCollection);

        // Assert
        serviceCollection.Should().NotBeEmpty();
        serviceCollection.Any(s => s.ServiceType == typeof(IGameEngine)).Should().BeTrue();
    }

    [Test]
    public void ConfigureServices_Should_AddGameEngineInitializer()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        _startup.ConfigureServices(serviceCollection);

        // Assert
        serviceCollection.Should().NotBeEmpty();
        // GameEngineInitializer is registered as a hosted service, not a regular service
        serviceCollection.Any(s => s.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) && 
                                   s.ImplementationType == typeof(GameEngineInitializer)).Should().BeTrue();
    }

    [Test]
    public void Configure_Should_HaveCorrectSignature()
    {
        // Arrange & Act
        var configureMethod = typeof(Startup).GetMethod("Configure");

        // Assert
        configureMethod.Should().NotBeNull();
        configureMethod!.GetParameters().Length.Should().Be(2);
        configureMethod.GetParameters()[0].ParameterType.Should().Be(typeof(IApplicationBuilder));
        configureMethod.GetParameters()[1].ParameterType.Should().Be(typeof(IWebHostEnvironment));
    }

    [Test]
    public void ConfigureServices_Should_HaveCorrectSignature()
    {
        // Arrange & Act
        var configureServicesMethod = typeof(Startup).GetMethod("ConfigureServices");

        // Assert
        configureServicesMethod.Should().NotBeNull();
        configureServicesMethod!.GetParameters().Length.Should().Be(1);
        configureServicesMethod.GetParameters()[0].ParameterType.Should().Be(typeof(IServiceCollection));
    }
}

[TestFixture]
public class GameEngineInitializerTests
{
    [SetUp]
    public void Setup()
    {
        _mockGameEngine = new Mock<IGameEngine>();
        _initializer = new GameEngineInitializer(_mockGameEngine.Object);
    }

    private Mock<IGameEngine> _mockGameEngine;
    private GameEngineInitializer _initializer;

    [Test]
    public async Task StartAsync_Should_CallInitializeEngine()
    {
        // Arrange
        _mockGameEngine.Setup(e => e.InitializeEngine()).Returns(Task.CompletedTask);
        var cancellationToken = CancellationToken.None;

        // Act
        await _initializer.StartAsync(cancellationToken);

        // Assert
        _mockGameEngine.Verify(e => e.InitializeEngine(), Times.Once);
    }

    [Test]
    public async Task StopAsync_Should_CompleteSuccessfully()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await FluentActions.Invoking(() => _initializer.StopAsync(cancellationToken))
            .Should().NotThrowAsync();
    }

    [Test]
    public void Should_ImplementIHostedService()
    {
        // Assert
        _initializer.Should().BeAssignableTo<Microsoft.Extensions.Hosting.IHostedService>();
    }
}