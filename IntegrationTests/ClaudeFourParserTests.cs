using Bedrock;
using GameEngine;
using Model.Location;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.CoalMineLocation;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Ignore("")]
// [Parallelizable(ParallelScope.Children)]
public class ClaudeFourParserTests
{
    [SetUp]
    public void Setup()
    {
        Env.Load("/Users/michael/RiderProjects/ZorkAI/.env",
            new LoadOptions());
    }

    private readonly object _lockObject = new();

    [Test]

    // Direction
    [TestCase(typeof(WestOfHouse), "walk north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "let's head north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "i am going to go north",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "let's saunter north",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "crawl northly", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(Cellar), "follow the crawlway", new[] { "<intent>move</intent>", "<direction>south</direction>" })]
    [TestCase(typeof(Cellar), "follow the passageway",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(Kitchen), "climb the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "go up the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "ascend the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "climb the stairs", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "climb down", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "descend down", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "descend the stairs", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "let's descend the staircase",
        new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(EastOfChasm), "follow the path", new[] { "<intent>move</intent>", "<direction>east</direction>" })]
    [TestCase(typeof(EastOfChasm), "follow the narrow passage",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(EastOfChasm), "choose the narrow passage",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(EastOfChasm), "continue down the path",
        new[] { "<intent>move</intent>", "<direction>east</direction>" })]
    [TestCase(typeof(BehindHouse), "enter the house",
        new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "enter the house through the window",
        new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "go through the window",
        new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "use the window to go into the house",
        new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "go into the house", new[] { "<intent>move</intent>", "<direction>in</direction>" })]

    // Single noun
    [TestCase(typeof(WestOfHouse), "hey, what do you say we open the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "next let's try opening the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "open the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "grab the bottle",
        new[] { "<verb>grab</verb>", "<noun>bottle</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "please kick the mailbox",
        new[] { "<verb>kick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "I want to hug the mailbox",
        new[] { "<verb>hug</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "take mailbox",
        new[] { "<verb>take</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, eat it",
        new[] { "<verb>eat</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, please smoke it",
        new[] { "<verb>smoke</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "let's abscond the mailbox",
        new[] { "<verb>take</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "it would be great if I can please lick the mailbox",
        new[] { "<verb>lick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "light lantern",
        new[] { "<verb>light</verb>", "<noun>lantern</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(MachineRoom), "examine machine",
        new[] { "<verb>examine</verb>", "<noun>machine</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(DamBase), "take plastic",
        new[] { "<verb>take</verb>", "<noun>plastic</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(DamBase), "take pile",
        new[] { "<verb>take</verb>", "<noun>pile</noun>", "<intent>act</intent>" })]

    // Multi noun
    [TestCase(typeof(DomeRoom), "tie the rope to the railing",
        new[]
        {
            "<verb>tie</verb>", "<noun>rope</noun>", "<noun>railing</noun>", "<intent>act</intent>",
            "<preposition>to</preposition>"
        })]
    [TestCase(typeof(DomeRoom), "inflate the pile of plastic with the air pump",
        new[]
        {
            "<verb>inflate</verb>", "<noun>pile</noun>", "<noun>pump</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(DomeRoom), "blow up the pile of plastic using the air pump",
        new[]
        {
            "<verb>inflate</verb>", "<noun>pile</noun>", "<noun>air pump</noun>", "<intent>act</intent>",
            "<preposition>using</preposition>"
        })]
    [TestCase(typeof(Dam), "Use the wrench to turn the bolt.",
        new[]
        {
            "<verb>use</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>",
            "<preposition>to</preposition>"
        })]
    [TestCase(typeof(Dam), "With the wrench, turn the bolt",
        new[]
        {
            "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>"
        })]
    [TestCase(typeof(Dam), "The wrench should be used to turn the bolt.",
        new[]
        {
            "<verb>use</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>",
            "<preposition>to</preposition>"
        })]
    [TestCase(typeof(Dam), "Turn the bolt using the wrench.",
        new[]
        {
            "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>",
            "<preposition>using</preposition>"
        })]
    [TestCase(typeof(TrollRoom), "kill the troll with the sword",
        new[]
        {
            "<verb>kill</verb>", "<noun>troll</noun>", "<noun>sword</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(TrollRoom), "kill troll with sword",
        new[]
        {
            "<verb>kill</verb>", "<noun>troll</noun>", "<noun>sword</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(Dam), "let's turn the bolt with the wrench",
        new[]
        {
            "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(LivingRoom), "I want to put the sword in the trophy case",
        new[]
        {
            "<verb>put</verb>", "<noun>trophy case</noun>", "<noun>sword</noun>", "<intent>act</intent>",
            "<preposition>in</preposition>"
        })]
    [TestCase(typeof(TrollRoom), "With the glowing sword, kill the ugly troll.",
        new[]
        {
            "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(TrollRoom), "The glowing sword should be used to kill the ugly troll.",
        new[]
        {
            "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>",
            "<preposition>to</preposition>"
        })]
    [TestCase(typeof(TrollRoom), "Use the sword that glows to kill the troll that's ugly.",
        new[]
        {
            "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>"
        })]
    
    
    // Sub-locations
    [TestCase(typeof(DamBase), "get in the boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "let's go for a ride in the magic boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "get inside the boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "enter the boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "board the magic boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "sit in the boat",
        new[]
        {
            "<intent>board</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "leave the boat",
        new[]
        {
            "<intent>disembark</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "get out of the boat",
        new[]
        {
            "<intent>disembark</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "exit the boat",
        new[]
        {
            "<intent>disembark</intent>",  "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "stand up and get out of the magic boat",
        new[]
        {
            "<intent>disembark</intent>",  "<noun>boat</noun>"
        })]
    
    public async Task ClaudeParserTests(Type location, string sentence, string[] asserts)
    {
        string locationObjectDescription;

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            Repository.GetItem<PileOfPlastic>().IsInflated = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    public async Task ToKillTheUglyTrollUseTheGlowingSword()
    {
        string locationObjectDescription;
        var sentence = "To kill the ugly troll, use the glowing sword";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        response.Should().Contain("<verb>kill</verb>");
        response.Should().Contain("<noun>sword</noun>");
        response.Should().Contain("<noun>troll</noun>");
    }

    [Test]
    public async Task KillTheTrollUsingTheSword()
    {
        string locationObjectDescription;
        var sentence = "kill the troll using the sword";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        var containsString1 = response.Contains("<verb>kill");
        var containsString2 = response.Contains("<verb>use");

        (containsString1 || containsString2).Should().BeTrue();
        response.Should().Contain("<noun>sword</noun>");
        response.Should().Contain("<noun>troll</noun>");
    }

    [Test]
    public async Task UseTheWrenchOnTheBolt()
    {
        string locationObjectDescription;
        var sentence = "use the wrench on the bolt";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        var containsString1 = response.Contains("<verb>use");
        var containsString2 = response.Contains("<verb>turn");

        (containsString1 || containsString2).Should().BeTrue();
        response.Should().Contain("<noun>bolt</noun>");
        response.Should().Contain("<noun>wrench</noun>");
    }

    [Test]
    public async Task ToTurnTheBoltUseTheWrench()
    {
        string locationObjectDescription;
        var sentence = "to turn the bolt, use the wrench";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        var containsString1 = response.Contains("<verb>use");
        var containsString2 = response.Contains("<verb>turn");

        (containsString1 || containsString2).Should().BeTrue();
        response.Should().Contain("<noun>bolt</noun>");
        response.Should().Contain("<noun>wrench</noun>");
    }

    [Test]
    public async Task UseTheGlowingSwordToKillUglyTroll()
    {
        string locationObjectDescription;
        var sentence = "use the glowing sword to kill the ugly troll";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        var containsString1 = response.Contains("<verb>kill");
        var containsString2 = response.Contains("<verb>use");

        (containsString1 || containsString2).Should().BeTrue();
        response.Should().Contain("<noun>sword</noun>");
        response.Should().Contain("<noun>troll</noun>");
    }

    [Test]
    public async Task SipTheWater()
    {
        string locationObjectDescription;
        var sentence = "sip the water";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        var containsString1 = response.Contains("<verb>sip");
        var containsString2 = response.Contains("<verb>drink");

        (containsString1 || containsString2).Should().BeTrue();
    }

    [Test]
    public async Task PressTheYellowButton()
    {
        string locationObjectDescription;
        var sentence = "press the yellow button";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.Description;
        }

        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        response.Should().Contain("<verb>press</verb>");
        response.Should().Contain("<noun>button");
        response.Should().Contain("<adjective>yellow");
    }
}