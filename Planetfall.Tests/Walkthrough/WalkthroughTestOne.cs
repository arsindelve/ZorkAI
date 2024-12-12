using System;

namespace Planetfall.Tests.Walkthrough;

[TestFixture]

public sealed class WalkthroughTestOne : WalkthroughTestBase
{
    [Test]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    [TestCase("port", null, "Escape Pod")]
    [TestCase("sit", null, "You are now safely cushioned within the web")]
    [TestCase("wait", null, "You feel the pod begin to slide down its ejection")]
    [TestCase("wait", null, "the escape pod tumbles away from th")]
    [TestCase("wait", null, "The auxiliary rockets fire ")]
    [TestCase("wait", null, "The main thrusters fire a long")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "The pod is buffeted")]
    [TestCase("wait", null, "You feel the temperature")]
    [TestCase("wait", null, "The viewport suddenly becomes transparent")]
    [TestCase("wait", null, "The pod is now approaching")]
    [TestCase("wait", null, "The pod lands with a thud")]
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    // [TestCase("out", null, "Underwater")]
    // [TestCase("up", null, "Crag")]
    // [TestCase("up", null, "Balcony")]
    // [TestCase("up", null, "Winding Stair")]
    // [TestCase("up", null, "Courtyard")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}
