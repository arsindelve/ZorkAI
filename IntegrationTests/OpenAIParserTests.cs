using GameEngine;
using Model.Intent;
using Model.Interface;
using Model.Location;
using Moq;
using OpenAI;
using Planetfall.Location.Kalamontee;
using ZorkAI.OpenAI;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.CoalMineLocation;
using Kitchen = ZorkOne.Location.Kitchen;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class OpenAIParserTests
{
    [SetUp]
    public void Setup()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var resolvedPath = Path.Combine(homePath, "RiderProjects/ZorkAI/.env");
        Console.WriteLine($"Resolved path: {resolvedPath}");
        if (!File.Exists(resolvedPath))
        {
            Console.WriteLine($"File does not exist at path: {resolvedPath}");
        }
        else
        {
            Console.WriteLine($"File exists at path: {resolvedPath}");
            Env.Load(resolvedPath, new LoadOptions());
        }
    }

    private readonly object _lockObject = new();

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
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        Console.WriteLine(intent.Message);

        if (intent.Message is null)
            throw new Exception("Intent message is null");

        var containsString1 = intent.Message.Contains("<verb>sip");
        var containsString2 = intent.Message.Contains("<verb>drink");

        (containsString1 || containsString2).Should().BeTrue();
    }


    [Test]

    // Wear, put on
    [TestCase(typeof(WestOfHouse), "put on the hat",
        new[] { "<intent>act</intent>", "<verb>don</verb>", "<noun>hat</noun>" })]
    [TestCase(typeof(WestOfHouse), "wear the hat",
        new[] { "<intent>act</intent>", "<verb>don</verb>", "<noun>hat</noun>" })]
    [TestCase(typeof(WestOfHouse), "don the hat",
        new[] { "<intent>act</intent>", "<verb>don</verb>", "<noun>hat</noun>" })]
    [TestCase(typeof(WestOfHouse), "dress in the hat",
        new[] { "<intent>act</intent>", "<verb>don</verb>", "<noun>hat</noun>" })]

    // Remove, take off
    [TestCase(typeof(WestOfHouse), "take off the hat",
        new[] { "<intent>act</intent>", "<verb>doff</verb>", "<noun>hat</noun>" })]
    [TestCase(typeof(WestOfHouse), "remove the hat",
        new[] { "<intent>act</intent>", "<verb>doff</verb>", "<noun>hat</noun>" })]
    [TestCase(typeof(WestOfHouse), "doff the hat",
        new[] { "<intent>act</intent>", "<verb>doff</verb>", "<noun>hat</noun>" })]

    // Turn on 
    [TestCase(typeof(WestOfHouse), "light the lamp",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn on lamp",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn lamp on ",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn on the lamp",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn the lamp on ",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lamp</noun>" })]

    // Turn off
    [TestCase(typeof(WestOfHouse), "extinguish the lamp",
        new[] { "<intent>act</intent>", "<verb>deactivate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn off lamp",
        new[] { "<intent>act</intent>", "<verb>deactivate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn lamp off ",
        new[] { "<intent>act</intent>", "<verb>deactivate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn off the lamp",
        new[] { "<intent>act</intent>", "<verb>deactivate</verb>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "turn the lamp off ",
        new[] { "<intent>act</intent>", "<verb>deactivate</verb>", "<noun>lamp</noun>" })]

    // Direction
    [TestCase(typeof(WestOfHouse), "walk north", new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "let's head north",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
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
    [TestCase(typeof(BehindHouse), "enter the house through the window",
        new[] { "<intent>move</intent>", "<verb>enter</verb>" })]
    [TestCase(typeof(BehindHouse), "use the window to go into the house",
        new[] { "<intent>move</intent>", "<direction>in</direction>" })]
    // NB: the bare "enter the house", "go into the house", and "go through the window" phrasings are
    // deliberately NOT asserted at the parser level — the AI classifies these ambiguous "enter the
    // structure" commands inconsistently (goto vs move). BehindHouse handles them deterministically on the
    // raw input, so the behavior contract lives in KitchenWindowTests, not here.

    // Single noun
    [TestCase(typeof(WestOfHouse), "drop the card",
        new[] { "<verb>drop</verb>", "<noun>card</noun>", "<intent>drop</intent>" })]
    [TestCase(typeof(WestOfHouse), "drop the access card",
        new[] { "<verb>drop</verb>", "<noun>access card</noun>", "<intent>drop</intent>" })]
    [TestCase(typeof(WestOfHouse), "drop access card",
        new[] { "<verb>drop</verb>", "<noun>access card</noun>", "<intent>drop</intent>" })]
    [TestCase(typeof(WestOfHouse), "drop upper elevator access card",
        new[] { "<verb>drop</verb>", "<noun>upper elevator access card</noun>", "<intent>drop</intent>" })]
    [TestCase(typeof(WestOfHouse), "hey, what do you say we open the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "next let's try opening the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "open the mailbox",
        new[] { "<verb>open</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "grab the bottle",
        new[] { "<intent>take</intent>" })]
    [TestCase(typeof(WestOfHouse), "please kick the mailbox",
        new[] { "<verb>kick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "I want to hug the mailbox",
        new[] { "<verb>hug</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "take mailbox",
        new[] { "<verb>take</verb>", "<noun>mailbox</noun>", "<intent>take</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, eat it",
        new[] { "<verb>eat</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "the mailbox, please smoke it",
        new[] { "<verb>smoke</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    // "steal" sits on the act/take boundary (steal == take the mailbox); both are defensible and the
    // mailbox is un-takeable scenery either way, so assert the stably-identified object, not the bucket.
    [TestCase(typeof(WestOfHouse), "let's steal the mailbox",
        new[] { "<noun>mailbox</noun>" })]
    [TestCase(typeof(WestOfHouse), "it would be great if I can please lick the mailbox",
        new[] { "<verb>lick</verb>", "<noun>mailbox</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(WestOfHouse), "light lantern",
        new[] { "<verb>activate</verb>", "<noun>lantern</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(MachineRoom), "examine machine",
        new[] { "<verb>examine</verb>", "<noun>machine</noun>", "<intent>act</intent>" })]
    [TestCase(typeof(DamBase), "take plastic",
        new[] { "<verb>take</verb>", "<noun>plastic</noun>", "<intent>take</intent>" })]
    [TestCase(typeof(DamBase), "take pile",
        new[] { "<verb>take</verb>", "<noun>pile</noun>", "<intent>take</intent>" })]

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
            "<verb>inflate</verb>", "<noun>pile of plastic</noun>", "<noun>air pump</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    // "Use the wrench to turn the bolt." — indirect phrasing the parser must still resolve to
    // turn-the-bolt-WITH-the-wrench. The connecting preposition is "with" (the tool relationship), NOT the
    // sentence's literal "to" (an infinitive "in order to"). The Dam handler accepts with/to/on/using, so
    // either turns the bolt, but "with" is the correct reading and we assert it.
    [TestCase(typeof(Dam), "Use the wrench to turn the bolt.",
        new[]
        {
            "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>",
            "<preposition>with</preposition>"
        })]
    [TestCase(typeof(Dam), "With the wrench, turn the bolt",
        new[]
        {
            "<verb>turn</verb>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<intent>act</intent>"
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

    // Sub-locations
    [TestCase(typeof(DamBase), "get in the boat",
        new[]
        {
            "<intent>board</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "let's go for a ride in the magic boat",
        new[]
        {
            "<intent>board</intent>", "<noun>magic boat</noun>"
        })]
    [TestCase(typeof(DamBase), "get inside the boat",
        new[]
        {
            "<intent>board</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "enter the boat",
        new[]
        {
            "<intent>board</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "board the magic boat",
        new[]
        {
            "<intent>board</intent>", "<noun>magic boat</noun>"
        })]
    [TestCase(typeof(DamBase), "sit in the boat",
        new[]
        {
            "<intent>board</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "leave the boat",
        new[]
        {
            "<intent>disembark</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "get out of the boat",
        new[]
        {
            "<intent>disembark</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "exit the boat",
        new[]
        {
            "<intent>disembark</intent>", "<noun>boat</noun>"
        })]
    [TestCase(typeof(DamBase), "stand up and get out of the magic boat",
        new[]
        {
            "<intent>disembark</intent>", "<noun>magic boat</noun>"
        })]
    [TestCase(typeof(DamBase), "set dial to 4775",
        new[]
        {
            "<intent>act</intent>", "<noun>dial</noun>", "<noun>4775</noun>", "<verb>set</verb>"
        })]
    [TestCase(typeof(DamBase), "turn dial to -1",
        new[]
        {
            "<intent>act</intent>", "<noun>dial</noun>", "<noun>-1</noun>", "<verb>turn</verb>"
        })]
    public async Task OpenAIParserTestCases(Type location, string sentence, string[] asserts)
    {
        string locationObjectDescription;

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            Repository.GetItem<PileOfPlastic>().IsInflated = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    [TestCase("upper elevator access card")]
    [TestCase("lower elevator access card")]
    [TestCase("upper card")]
    [TestCase("lower card")]
    [TestCase("upper elevator card")]
    [TestCase("lower elevator card")]
    [TestCase("shuttle access card")]
    [TestCase("shuttle card")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task CardsInSlots(string card)
    {
        string locationObjectDescription;
        var sentence = $"slide the {card} through the slot";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain("<verb>slide</verb>");
        response.Should().Contain($"<noun>{card}");
    }

    [Test]
    [TestCase("up")]
    [TestCase("down")]
    public async Task PressTheUpAndDownButton(string direction)
    {
        string locationObjectDescription;
        var sentence = $"press the {direction} button";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain("<verb>press</verb>");
        response.Should().Contain($"<noun>{direction} button");
    }

    [Test]
    [TestCase("up")]
    [TestCase("down")]
    public async Task PressTheUpAndDownButton_V2(string direction)
    {
        string locationObjectDescription;
        var sentence = $"press {direction}";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(UpperElevator))!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain("<verb>press</verb>");
        response.Should().Contain($"<noun>{direction}");
    }

    [Test]
    [TestCase("yellow")]
    [TestCase("red")]
    [TestCase("green")]
    [TestCase("blue")]
    [TestCase("purple")]
    [TestCase("orange")]
    [TestCase("pink")]
    public async Task PressTheColoredButton(string color)
    {
        string locationObjectDescription;
        var sentence = $"press the {color} button";

        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain("<verb>press</verb>");
        response.Should().Contain($"<noun>{color} button");
    }

    [Test]
    [TestCase("1")]
    [TestCase("2")]
    [TestCase("three")]
    [TestCase("four")]
    [TestCase("7")]
    [TestCase("8")]
    [TestCase("9")]
    public async Task TypeNumbers(string number)
    {
        var sentence = $"type {number}";

        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain("<verb>type</verb>");
        response.Should().Contain($"<noun>{number}");
    }

    [Test]
    [TestCase("type", "type")]
    [TestCase("key in", "key")]
    [TestCase("key", "key")]
    [TestCase("punch in", "punch")]
    public async Task TypeSynonyms(string verb, string expected)
    {
        var sentence = $"{verb} 12";

        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, locationObjectDescription, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        response.Should().Contain($"<verb>{expected}</verb>");
        response.Should().Contain("<noun>12");
    }
    
    [Test]
    [TestCase("what is in my inventory")]
    [TestCase("what am I carrying?")]
    [TestCase("tell me everything I have on me")]
    [TestCase("inventory")]
    [TestCase("tell me all the items I am carrying")]
    public async Task Inventory(string input)
    {
        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(input, locationObjectDescription, string.Empty);

        intent.Should().BeOfType<InventoryIntent>();
    }
    
    [Test]
    [TestCase("where am I")]
    [TestCase("what is this place?")]
    [TestCase("look at my surroundings")]
    [TestCase("tell me about my location")]
    public async Task Look(string input)
    {
        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(input, locationObjectDescription, string.Empty);

        intent.Should().BeOfType<LookIntent>();
    }
    
    [Test]
    [TestCase("look under the rug")]
    [TestCase("look under rug")]
    [TestCase("peek under rug")]
    
    public async Task LookUnderTheRug(string input)
    {
        string locationObjectDescription;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(input, locationObjectDescription, string.Empty);

        intent.Should().BeOfType<SimpleIntent>();
        var simpleIntent = intent as SimpleIntent;
        simpleIntent!.Noun.Should().Be("rug");
    }

    // ===== Conversational / messy natural language =====
    // This is the AI parser's actual job: standard Zork grammar ("take sword", "put sword in case") is
    // handled deterministically upstream, so what reaches the AI is polite, verbose, filler-laden,
    // conversational phrasing. It must still resolve to the correct intent.

    [Test]
    [TestCase(typeof(LivingRoom), "would you please put the sword in the trophy case, ok?",
        new[]
        {
            "<intent>act</intent>", "<verb>put</verb>", "<noun>trophy case</noun>", "<noun>sword</noun>",
            "<preposition>in</preposition>"
        })]
    [TestCase(typeof(WestOfHouse), "hey buddy, could you grab the lamp for me?",
        new[] { "<intent>take</intent>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "I reckon I'd like to open that there mailbox",
        new[] { "<intent>act</intent>", "<verb>open</verb>", "<noun>mailbox</noun>" })]
    [TestCase(typeof(TrollRoom), "um, maybe kill the troll with the sword?",
        new[] { "<intent>act</intent>", "<verb>kill</verb>", "<noun>troll</noun>", "<noun>sword</noun>" })]
    [TestCase(typeof(WestOfHouse), "pretty please drop the sword",
        new[] { "<intent>drop</intent>", "<noun>sword</noun>" })]
    [TestCase(typeof(WestOfHouse), "let's go ahead and turn on the lantern, shall we?",
        new[] { "<intent>act</intent>", "<verb>activate</verb>", "<noun>lantern</noun>" })]
    [TestCase(typeof(WestOfHouse), "would you kindly examine the trophy case for me",
        new[] { "<intent>act</intent>", "<verb>examine</verb>", "<noun>trophy case</noun>" })]
    [TestCase(typeof(WestOfHouse), "ok so I think I'd like to read the leaflet now please",
        new[] { "<intent>act</intent>", "<verb>read</verb>", "<noun>leaflet</noun>" })]
    [TestCase(typeof(DamBase), "alright, let's hop in the boat and go for a ride",
        new[] { "<intent>board</intent>", "<noun>boat</noun>" })]
    public async Task ConversationalInput_StillParsesToTheRightIntent(Type location, string sentence,
        string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    [TestCase("can you tell me what's in my pockets right now?")]
    [TestCase("hey, remind me what I'm lugging around")]
    [TestCase("what all am I holding onto at the moment?")]
    public async Task ConversationalInventory_ResolvesToInventory(string sentence)
    {
        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var desc = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);

        intent.Should().BeOfType<InventoryIntent>();
    }

    [Test]
    [TestCase("so like, where the heck am I right now?")]
    [TestCase("hmm, could you describe where I'm standing?")]
    [TestCase("remind me what this place looks like again")]
    public async Task ConversationalLook_ResolvesToLook(string sentence)
    {
        var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
        var desc = locationObject.GetDescription(Mock.Of<IContext>());

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);

        intent.Should().BeOfType<LookIntent>();
    }

    [Test]
    // Movement and vehicles wrapped in natural language.
    [TestCase(typeof(WestOfHouse), "let's mosey on north",
        new[] { "<intent>move</intent>", "<direction>north</direction>" })]
    [TestCase(typeof(WestOfHouse), "southward ho, let's get out of here",
        new[] { "<intent>move</intent>", "<direction>south</direction>" })]
    [TestCase(typeof(Kitchen), "I want to head up the stairs",
        new[] { "<intent>move</intent>", "<direction>up</direction>" })]
    [TestCase(typeof(Attic), "ok let's climb back down",
        new[] { "<intent>move</intent>", "<direction>down</direction>" })]
    [TestCase(typeof(DamBase), "let's climb aboard the boat",
        new[] { "<intent>board</intent>", "<noun>boat</noun>" })]
    [TestCase(typeof(DamBase), "alright, time to get out of the boat",
        new[] { "<intent>disembark</intent>", "<noun>boat</noun>" })]
    [TestCase(typeof(WestOfHouse), "let's make our way over to the cellar",
        new[] { "<intent>goto</intent>", "<noun>cellar</noun>" })]
    public async Task ConversationalMovementAndVehicles(Type location, string sentence, string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    // Terse / slang / typo'd input the AI must still map to the right intent + object(s). NOTE: we assert
    // the intent and nouns, NOT a canonicalised verb — the parser faithfully keeps an obscure verb ("yeet",
    // "peruse", "crack") rather than mapping it to a synonym; turning synonyms into behaviour is the
    // engine's verb-family matching, not the parser's job. (The prompt-guaranteed maps like turn-on->activate
    // are asserted in the conversational test above.)
    [TestCase(typeof(WestOfHouse), "gimme the lamp", new[] { "<intent>take</intent>", "<noun>lamp</noun>" })]
    [TestCase(typeof(WestOfHouse), "crack open the mailbox",
        new[] { "<intent>act</intent>", "<noun>mailbox</noun>" })]
    [TestCase(typeof(TrollRoom), "yeet the sword at the troll",
        new[] { "<intent>act</intent>", "<noun>sword</noun>", "<noun>troll</noun>" })]
    [TestCase(typeof(WestOfHouse), "smash the mailbox to bits",
        new[] { "<intent>act</intent>", "<noun>mailbox</noun>" })]
    [TestCase(typeof(WestOfHouse), "peruse the leaflet real quick",
        new[] { "<intent>act</intent>", "<noun>leaflet</noun>" })]
    [TestCase(typeof(WestOfHouse), "give the mailbox a swift kick",
        new[] { "<verb>kick</verb>", "<noun>mailbox</noun>" })]
    public async Task TerseAndSlangInput_StillParsesToTheRightIntent(Type location, string sentence,
        string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    // Multi-noun "use TOOL to do X" phrasing, conversationally wrapped — the connecting preposition should
    // resolve to the tool relationship regardless of how the sentence is phrased.
    [TestCase(typeof(Dam), "go ahead and loosen the bolt using the wrench",
        new[] { "<intent>act</intent>", "<noun>bolt</noun>", "<noun>wrench</noun>", "<preposition>using</preposition>" })]
    [TestCase(typeof(TrollRoom), "I'd like to attack the troll using my sword",
        new[] { "<intent>act</intent>", "<noun>troll</noun>", "<noun>sword</noun>" })]
    [TestCase(typeof(LivingRoom), "could you stash the sword inside the trophy case for me",
        new[] { "<intent>act</intent>", "<noun>trophy case</noun>", "<noun>sword</noun>" })]
    public async Task ConversationalMultiNounToolCommands(Type location, string sentence, string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    // More single-noun action verbs across varied objects.
    [TestCase(typeof(Kitchen), "I'm famished, let me eat the lunch",
        new[] { "<intent>act</intent>", "<verb>eat</verb>", "<noun>lunch</noun>" })]
    [TestCase(typeof(Kitchen), "have a drink from the bottle",
        new[] { "<intent>act</intent>", "<noun>bottle</noun>" })]
    [TestCase(typeof(WestOfHouse), "read the leaflet out loud",
        new[] { "<intent>act</intent>", "<verb>read</verb>", "<noun>leaflet</noun>" })]
    [TestCase(typeof(LivingRoom), "give the trophy case a good once-over",
        new[] { "<intent>act</intent>", "<noun>trophy case</noun>" })]
    [TestCase(typeof(WestOfHouse), "shut the mailbox please",
        new[] { "<intent>act</intent>", "<noun>mailbox</noun>" })]
    [TestCase(typeof(WestOfHouse), "pry open the mailbox",
        new[] { "<intent>act</intent>", "<noun>mailbox</noun>" })]
    public async Task MoreSingleNounActions(Type location, string sentence, string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    // Multi-noun across a variety of connecting prepositions and objects (bare nouns, so no adjective drift).
    [TestCase(typeof(DomeRoom), "tie the rope to the railing",
        new[]
        {
            "<intent>act</intent>", "<verb>tie</verb>", "<noun>rope</noun>", "<noun>railing</noun>",
            "<preposition>to</preposition>"
        })]
    [TestCase(typeof(DamBase), "inflate the boat with the pump",
        new[] { "<intent>act</intent>", "<noun>boat</noun>", "<noun>pump</noun>", "<preposition>with</preposition>" })]
    [TestCase(typeof(TrollRoom), "kill the thief with the knife",
        new[]
        {
            "<intent>act</intent>", "<noun>thief</noun>", "<noun>knife</noun>", "<preposition>with</preposition>"
        })]
    [TestCase(typeof(Kitchen), "pour the water into the bottle",
        new[] { "<intent>act</intent>", "<noun>water</noun>", "<noun>bottle</noun>" })]
    [TestCase(typeof(WestOfHouse), "unlock the grating with the key",
        new[] { "<intent>act</intent>", "<verb>unlock</verb>", "<noun>key</noun>", "<preposition>with</preposition>" })]
    public async Task MoreMultiNounScenarios(Type location, string sentence, string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    [Test]
    // More colloquial place/vehicle navigation.
    // NB: no direction word in the sentence — "pop DOWN to the cellar" would reasonably parse as move/down.
    [TestCase(typeof(Kitchen), "how about we head to the cellar",
        new[] { "<intent>goto</intent>", "<noun>cellar</noun>" })]
    [TestCase(typeof(WestOfHouse), "let's head over to the forest",
        new[] { "<intent>goto</intent>", "<noun>forest</noun>" })]
    [TestCase(typeof(DamBase), "hop into the boat",
        new[] { "<intent>board</intent>", "<noun>boat</noun>" })]
    [TestCase(typeof(DamBase), "clamber out of the boat",
        new[] { "<intent>disembark</intent>", "<noun>boat</noun>" })]
    [TestCase(typeof(EastOfChasm), "just keep following the path",
        new[] { "<intent>move</intent>" })]
    public async Task MoreColloquialNavigation(Type location, string sentence, string[] asserts)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            Repository.GetItem<KitchenWindow>().IsOpen = true;
            var locationObject = (ILocation)Activator.CreateInstance(location)!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);
        var intent = await target.AskTheAIParser(sentence, desc, string.Empty);
        var response = intent.Message;
        Console.WriteLine(response);

        foreach (var assert in asserts) response.Should().Contain(assert);
    }

    // ===== Structured-Outputs reliability guarantees =====
    // These exercise the three constraints added to the AI parser (guaranteed-valid JSON structure, a valid
    // intent from the closed set, and run-to-run determinism). They call the live model, hence [Explicit]
    // like the rest of this fixture. They complement the deterministic StructuredIntentParsingTests, which
    // cover the JSON->intent mapping without the network.

    private static readonly Type[] KnownIntentTypes =
    {
        typeof(SimpleIntent), typeof(MultiNounIntent), typeof(TakeIntent), typeof(DropIntent),
        typeof(MoveIntent), typeof(GoToDestinationIntent), typeof(EnterSubLocationIntent),
        typeof(ExitSubLocationIntent), typeof(LookIntent), typeof(InventoryIntent),
        typeof(MultipleCommandsIntent), typeof(NullIntent)
    };

    [Test]
    // Well-formed commands, awkward phrasings, and outright gibberish must all come back as a valid intent
    // and never throw — the structured schema makes malformed/duplicate-tag output (the old NullIntent /
    // HTTP-500 failure modes) impossible.
    [TestCase("examine the mailbox")]
    [TestCase("put the sword in the trophy case")]
    [TestCase("wave the sceptre around wildly like a lunatic and then dance")]
    [TestCase("go north south then maybe up who knows")]
    [TestCase("asdf qwer zxcv")]
    public async Task StructuredOutput_AlwaysReturnsAValidIntent_AndNeverThrows(string sentence)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);

        var act = async () => await target.AskTheAIParser(sentence, desc, string.Empty);

        var intent = await act.Should().NotThrowAsync();
        intent.Subject.Should().NotBeNull();
        KnownIntentTypes.Should().Contain(intent.Subject!.GetType());
    }

    [Test]
    // The same command parsed repeatedly should yield the same parse (seed + temperature 0 + a constrained
    // schema). If this ever fails it is a direct, measurable signal of residual non-determinism.
    [TestCase("open the mailbox")]
    [TestCase("put the sword in the trophy case")]
    [TestCase("take the lamp")]
    public async Task StructuredOutput_IsDeterministic_AcrossRepeatedCalls(string sentence)
    {
        string desc;
        lock (_lockObject)
        {
            Repository.Reset();
            var locationObject = (ILocation)Activator.CreateInstance(typeof(WestOfHouse))!;
            desc = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAIParser(null);

        var first = await target.AskTheAIParser(sentence, desc, string.Empty);
        for (var i = 0; i < 4; i++)
        {
            var again = await target.AskTheAIParser(sentence, desc, string.Empty);
            again.GetType().Should().Be(first.GetType());
            again.Message.Should().Be(first.Message, "the structured parse should be reproducible");
        }
    }
}