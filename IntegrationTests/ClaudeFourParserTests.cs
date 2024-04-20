using Bedrock;
using DotNetEnv;
using FluentAssertions;
using Game;
using Model.Location;
using ZorkOne.Item;
using ZorkOne.Location;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ClaudeFourParserTests
{
    private object lockObject = new object();
    
    [SetUp]
    public void Setup()
    {
        Env.Load("/Users/michael/RiderProjects/ZorkAI/.env",
            new LoadOptions());
    }

    [Test]
    
    // Direction
    [TestCase(typeof(WestOfHouse), "walk north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "i am going to go north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "let's saunter north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "wander away and head in a northerly direction", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "crawl northly", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(Cellar), "follow the crawlway", new[] { "<intent>move</intent>", "<direction>south</direction>" })]
    [TestCase(typeof(Cellar), "follow the passageway", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(Kitchen), "climb the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "go up the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "ascend the staircase", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "climb the stairs", new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Kitchen), "climb down", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "descend down", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "descend the stairs", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(Attic), "let's descend the staircase", new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(EastOfChasm), "follow the path", new[] { "<intent>move</intent>", "<direction>east</direction>" })]
    [TestCase(typeof(EastOfChasm), "follow the narrow passage", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(EastOfChasm), "choose the narrow passage", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(EastOfChasm), "continue down the path", new[] { "<intent>move</intent>", "<direction>east</direction>" })]
    [TestCase(typeof(BehindHouse), "enter the house", new[] { "<intent>move</intent>", "<direction>enter</direction>" })]
    [TestCase(typeof(BehindHouse), "enter the house through the window", new[] { "<intent>move</intent>", "<direction>enter</direction>" })]
    [TestCase(typeof(BehindHouse), "go through the window", new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "use the window to go into the house", new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "enter the house via the window", new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    [TestCase(typeof(BehindHouse), "go into the house", new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    
    // Single noun
    [TestCase(typeof(WestOfHouse), "open the mailbox", new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "please kick the mailbox", new[] { "<verb>kick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "I want to hug the mailbox", new[] { "<verb>hug</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "take mailbox", new[] { "<verb>take</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, eat it", new[] { "<verb>eat</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, please smoke it", new[] { "<verb>smoke</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "let's abscond the mailbox", new[] { "<verb>take</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "it would be great if I can please lick the mailbox", new[] { "<verb>lick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "light lantern", new[] { "<verb>light</verb>", "<noun>lantern</noun>", "<intent>act</intent>" })]
   
    // Multi noun
    [TestCase(typeof(WestOfHouse), "tie the rope to the railing", new[] { "<verb>tie</verb>", "<noun>rope</noun>", "<noun>railing</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]
    [TestCase(typeof(WestOfHouse), "Use the wrench to turn the bolt.", new[] { "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]
    [TestCase(typeof(WestOfHouse), "With the wrench, turn the bolt", new[] { "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "The wrench should be used to turn the bolt.", new[] { "<verb>use</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]
    [TestCase(typeof(WestOfHouse), "Turn the bolt using the wrench.", new[] { "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>using</preposition>" })]
    
    [TestCase(typeof(WestOfHouse), "kill the troll with the sword", new[] { "<verb>kill</verb>", "<noun>troll</noun>", "<noun>sword</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "kill troll with sword", new[] { "<verb>kill</verb>", "<noun>troll</noun>", "<noun>sword</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "let's turn the bolt with the wrench", new[] { "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "use the wrench on the bolt", new[] { "<verb>use</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>on</preposition>" })]
    [TestCase(typeof(WestOfHouse), "To turn the bolt, use the wrench.", new[] { "<verb>use</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]
    
    [TestCase(typeof(WestOfHouse), "I want to put the sword in the trophy case", new[] { "<verb>put</verb>", "<noun>trophy case</noun>", "<noun>sword</noun>", "<intent>act</intent>", "<preposition>in</preposition>" })]
    [TestCase(typeof(WestOfHouse), "kill the troll using the sword", new[] { "<verb>kill</verb>", "<noun>troll</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>using</preposition>" })]
    [TestCase(typeof(WestOfHouse), "use the glowing sword to kill the ugly troll", new[] { "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    
    [TestCase(typeof(WestOfHouse), "With the glowing sword, kill the ugly troll.", new[] { "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "The glowing sword should be used to kill the ugly troll.", new[] { "<verb>use</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]
    [TestCase(typeof(WestOfHouse), "Use the sword that glows to kill the troll that's ugly.", new[] { "<verb>kill</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>with</preposition>" })]
    [TestCase(typeof(WestOfHouse), "To kill the ugly troll, use the glowing sword.", new[] { "<verb>use</verb>", "<noun>sword</noun>", "<noun>troll</noun>", "<intent>act</intent>", "<preposition>to</preposition>" })]

    public async Task ClaudeParserTests(Type location, string sentence, string[] asserts)
    {
        string locationObjectDescription;
        
        lock (lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            locationObjectDescription = locationObject.Description;
        }
    
        var target = new ClaudeFourParserClient();
        var response = (await target.GetResponse(locationObjectDescription, sentence))!.ToLowerInvariant();
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }
}