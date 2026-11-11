using System.Reflection;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Stationfall_Lambda;

namespace UnitTests.StationfallLambda;

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
        type.Namespace.Should().Be("Stationfall_Lambda");
    }

    [Test]
    public void Should_DeclareTheHandlerNamedByTheServerlessTemplate()
    {
        // The serverless.template's Handler string is a deploy-time contract: if the assembly name,
        // namespace or type ever drifts from it the deploy still succeeds and every request 500s at
        // runtime. Assert the three halves the template names actually exist.
        var type = typeof(LambdaEntryPoint);
        type.Assembly.GetName().Name.Should().Be("Stationfall-Lambda");
        type.FullName.Should().Be("Stationfall_Lambda.LambdaEntryPoint");
        type.GetMethod("FunctionHandlerAsync").Should().NotBeNull();
    }
}
