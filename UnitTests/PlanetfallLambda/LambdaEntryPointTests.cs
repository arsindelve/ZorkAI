using System.Reflection;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Planetfall_Lambda;

namespace UnitTests.PlanetfallLambda;

[TestFixture]
public class LambdaEntryPointTests
{
    [SetUp]
    public void Setup()
    {
        // Don't instantiate LambdaEntryPoint in setup since it starts the AWS Lambda host
        // We'll test it through reflection instead
    }

    [Test]
    public void Should_InheritFromCorrectBaseClass()
    {
        // Assert - verify the class structure and inheritance is correct through type checking
        typeof(LambdaEntryPoint).Should().BeAssignableTo<APIGatewayProxyFunction>();

        // Verify the class has the expected protected Init method
        var initMethod = typeof(LambdaEntryPoint).GetMethod("Init",
            BindingFlags.NonPublic | BindingFlags.Instance,
            [typeof(IWebHostBuilder)]);
        initMethod.Should().NotBeNull("LambdaEntryPoint should have a protected Init method");
    }

    [Test]
    public void Should_HaveCorrectClassStructure()
    {
        // Assert - verify that the class is public and can be instantiated
        var type = typeof(LambdaEntryPoint);
        type.Should().NotBeNull();
        type.IsPublic.Should().BeTrue();
        type.IsClass.Should().BeTrue();
    }

    [Test]
    public void Should_HaveParameterlessConstructor()
    {
        // Act & Assert
        var constructor = typeof(LambdaEntryPoint).GetConstructor(Type.EmptyTypes);
        constructor.Should().NotBeNull();
    }

    [Test]
    public void Should_BeInCorrectNamespace()
    {
        // Assert - verify the namespace matches expectations
        var type = typeof(LambdaEntryPoint);
        type.Namespace.Should().Be("Planetfall_Lambda");
    }
}