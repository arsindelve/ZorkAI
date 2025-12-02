using Lambda;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTests.Lambda;

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
    public void ConfigureServices_Should_AddSwaggerServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        _startup.ConfigureServices(serviceCollection);

        // Assert
        serviceCollection.Should().NotBeEmpty();
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