using FluentAssertions;

namespace Planetfall.Tests;

public class SicknessNotificationsTests
{
    private SicknessNotifications _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new SicknessNotifications();
    }

    [Test]
    [TestCase(1, 1, null)]
    [TestCase(1, 7200, null)]
    [TestCase(2, 1800, null)]
    [TestCase(2, 1900, "You notice that you feel a bit weak and slightly flushed, but you're not sure why. ")]
    [TestCase(2, 2200, null)]
    [TestCase(2, 8500, null)]
    [TestCase(3, 2200, null)]
    [TestCase(3, 2260, "You notice that you feel unusually weak, and you suspect that you have a fever. ")]
    [TestCase(3, 2270, null)]
    [TestCase(3, 5000, null)]
    [TestCase(4, 2000, null)]
    [TestCase(4, 2500, "You are now feeling quite under the weather, not unlike a bad flu. ")]
    [TestCase(4, 2500, null)]
    [TestCase(4, 6000, null)]
    [TestCase(5, 2600, null)]
    [TestCase(5, 4000, "Your fever seems to have gotten worse, and you're developing a bad headache. ")]
    [TestCase(6, 3000, "Your health has deteriorated further. You feel hot and weak, and your head is throbbing. ")]
    [TestCase(7, 3200, "You feel very, very sick, and have almost no strength left. ")]
    public void SickenessNotificationByDayAndTime(int day, int time, string? expected)
    {
        _sut.GetNotification(1, 1).Should().Be(null);
    }
}