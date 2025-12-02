using System.Reflection;
using Amazon.Lambda.AspNetCoreServer;
using Lambda;
using Microsoft.AspNetCore.Hosting;

namespace UnitTests.Lambda;

[TestFixture]
public class LambdaEntryPointTests
{
    [SetUp]
    public void Setup()
    {
        _lambdaEntryPoint = new LambdaEntryPoint();
    }

    private LambdaEntryPoint _lambdaEntryPoint;

    [Test]
    public void Should_InheritFromCorrectBaseClass()
    {
        // Assert - verify the class structure and inheritance is correct
        _lambdaEntryPoint.Should().BeAssignableTo<APIGatewayProxyFunction>();

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
}