using Model.AIGeneration.Requests;
using Model.AIParsing;
using Model.Intent;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

internal record UnitTestIntent : IntentBase;

public class EngineTests : EngineTestsBase
{
    [Test]
    public void DefaultConstructor()
    {
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", "XYZ");
        var target = new GameEngine<ZorkI, ZorkIContext>(null!);

        target.Should().NotBeNull();
    }

    [Test]
    public async Task EmptyCommand_Blank()
    {
        var target = GetTarget();

        Client.Setup(s => s.CompleteChat(It.IsAny<EmptyRequest>()))
            .ReturnsAsync("BOB");

        // Act
        var result = await target.GetResponse("");

        // Assert
        result.Should().Contain("BOB");
    }

    [Test]
    public async Task EmptyCommand_Null()
    {
        var target = GetTarget();

        Client.Setup(s => s.CompleteChat(It.IsAny<EmptyRequest>()))
            .ReturnsAsync("BOB");

        // Act
        var result = await target.GetResponse(null);

        // Assert
        result.Should().Contain("BOB");
    }

    [Test]
    public async Task Move_Simple_Success()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("S");

        // Assert
        result.Should().Contain("South of House");
    }

    [Test]
    public async Task Move_Simple_CantGoThatWay()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("NW");

        // Assert
        result.Should().Contain("You cannot go that way.");
    }

    [Test]
    public async Task Move_Simple_CantGoThatWay_CustomMessage()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("E");

        // Assert
        result.Should().Contain("The door is boarded and you can't remove the boards.");
    }

    [Test]
    public async Task Null_Intent()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("BOB", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new NullIntent());

        Client.Setup(
                s => s.CompleteChat(It.Is<CommandHasNoEffectOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("BOB"))))
            .ReturnsAsync("FRED");

        // Act
        var result = await target.GetResponse("BOB");

        // Assert
        result.Should().Contain("FRED");
    }

    [Test]
    public async Task Prompt_Intent()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("PROMPT", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PromptIntent { Message = "Please enter a value:" });

        // Act
        var result = await target.GetResponse("PROMPT");

        // Assert
        result.Should().Contain("Please enter a value:");
    }


    [Test]
    public async Task SimpleIntent_NoVerbMatch()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("push the mailbox", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "push", Noun = "mailbox", OriginalInput = "push the mailbox" });

        Client.Setup(
                s => s.CompleteChat(It.Is<VerbHasNoEffectOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("mailbox"))))
            .ReturnsAsync("no");

        // Act
        var result = await target.GetResponse("push the mailbox");

        // Assert
        result.Should().Contain("no");
    }

    [Test]
    public async Task SimpleIntent_NoVerbMatch_ItemInInventory()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("push the leaflet", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "push", Noun = "leaflet", OriginalInput = "push the leaflet" });

        Client.Setup(
                s => s.CompleteChat(It.Is<VerbHasNoEffectOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("leaflet"))))
            .ReturnsAsync("no");

        // Act
        await target.GetResponse("open mailbox");
        await target.GetResponse("take leaflet");
        var result = await target.GetResponse("push the leaflet");

        // Assert
        result.Should().Contain("no");
    }

    [Test]
    public async Task SimpleIntent_NounNotPresent()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("push the leaflet", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "push", Noun = "leaflet", OriginalInput = "push the leaflet" });

        Client.Setup(
                s => s.CompleteChat(It.Is<NounNotPresentOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("leaflet"))))
            .ReturnsAsync("no");

        // Act
        var result = await target.GetResponse("push the leaflet");

        // Assert
        // The leaflet is inside the mailbox, inaccessible, and therefore is not "here" to be interacted with. 
        result.Should().Contain("no");
    }

    [Test]
    public async Task SimpleIntent_MadeUpNoun()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("push the unicorn", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "push", Noun = "unicorn", OriginalInput = "push the unicorn" });

        Client.Setup(
                s => s.CompleteChat(It.Is<CommandHasNoEffectOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("unicorn"))))
            .ReturnsAsync("no");

        // Act
        var result = await target.GetResponse("push the unicorn");

        // Assert
        result.Should().Contain("no");
    }

    [Test]
    public async Task ParsedMoveIntent()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("go east", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MoveIntent { Direction = Direction.E });

        // Act
        var result = await target.GetResponse("go east");

        // Assert
        result.Should().Contain("The door is boarded and you can't remove the boards");
    }

    [Test]
    public async Task NoMatchingIntent()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("go east", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UnitTestIntent { Message = "bob" });

        Client.Setup(
                s => s.CompleteChat(It.Is<CommandHasNoEffectOperationRequest>(m =>
                    m.UserMessage != null && m.UserMessage.Contains("go east"))))
            .ReturnsAsync("no");

        // Act
        var result = await target.GetResponse("go east");

        // Assert
        result.Should().Contain("no");
    }

    [Test]
    public async Task ConditionalMoveToLocation_Failure()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
        Mock.Get(Parser).Setup(s => s.DetermineIntentType("go west", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MoveIntent { Direction = Direction.W });

        // Act
        var result = await target.GetResponse("go west");

        // Assert
        result.Should().Contain("The kitchen window is closed.");
    }

    [Test]
    public async Task ConditionalMoveToLocation_Success()
    {
        var target = GetTarget();

        Repository.GetItem<KitchenWindow>().IsOpen = true;
        target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
        Mock.Get(Parser).Setup(s => s.DetermineIntentType("go west", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MoveIntent { Direction = Direction.W });

        // Act
        var result = await target.GetResponse("go west");

        // Assert
        result.Should().Contain("You are in the kitchen of the white house");
    }

    [Test]
    public async Task MoveCounter()
    {
        var target = GetTarget();

        target.Context.Moves.Should().Be(0);
        await target.GetResponse("look");

        target.Context.Moves.Should().Be(1);
        await target.GetResponse("open mailbox");

        target.Context.Moves.Should().Be(2);
    }

    [Test]
    public void IHaveALightSource_NoInventory()
    {
        var target = GetTarget();
        target.Context.HasLightSource.Should().BeFalse();
    }

    [Test]
    public async Task IHaveALightSource_IHaveALeaflet()
    {
        var target = GetTarget();

        await target.GetResponse("open mailbox");
        await target.GetResponse("take leaflet");

        target.Context.HasLightSource.Should().BeFalse();
    }

    [Test]
    public async Task IHaveALightSource_IHaveALantern_ButItIsOff()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");

        target.Context.HasLightSource.Should().BeFalse();
    }

    [Test]
    public async Task IHaveALightSource_IHaveALantern_ItIsOn()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        await target.GetResponse("turn on lantern");

        target.Context.HasLightSource.Should().BeTrue();
    }

    [Test]
    public async Task TwoItemsInInventory_InteractWithSecondItem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        await target.GetResponse("take sword");
        var result = await target.GetResponse("drop sword");

        target.Context.HasItem<Sword>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }


    [Test]
    public async Task It_SuccessPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop it");

        target.Context.HasItem<Lantern>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task It_NoLastNoun()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take it");
        var result = await target.GetResponse("lantern");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task LightSource_ChangesDescription_TurnOn()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var result = await target.GetResponse("look");
        result.Should().Contain("eaten by a grue");

        result = await target.GetResponse("turn on lantern");
        result.Should().Contain("Attic");
    }

    [Test]
    public async Task LightSource_ChangesDescription_TurnOff()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        var result = await target.GetResponse("look");
        result.Should().Contain("attic");

        result = await target.GetResponse("turn off lantern");
        result.Should().Contain("grue");
    }

    [Test]
    public async Task MultiNoun_NeitherExistsInTheGame()
    {
        var target = GetTarget();

        Mock.Get(Parser).Setup(s => s.DetermineIntentType("dig hole with shovel", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MultiNounIntent
            {
                NounOne = "hole",
                NounTwo = "shovel",
                Preposition = "with",
                Verb = "dig",
                OriginalInput = "dig hole with shovel"
            });

        Client.Setup(s => s.CompleteChat(It.IsAny<CommandHasNoEffectOperationRequest>()))
            .ReturnsAsync("bob");

        var result = await target.GetResponse("dig hole with shovel");

        result.Should().Contain("bob");
    }

    [Test]
    public async Task SendInputToAIParser()
    {
        // Arrange 
        var aiParser = new Mock<IAIParser>();

        aiParser.Setup(s => s.AskTheAIParser("walk east", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MoveIntent { Direction = Direction.E });

        var parser = new IntentParser(aiParser.Object, new ZorkOneGlobalCommandFactory(), null);
        var target = GetTarget(parser);

        // Act
        var result = await target.GetResponse("walk east");

        result.Should().Contain("boarded");
    }

    [Test]
    public async Task Parser_DirectionMatch()
    {
        var aiParser = new Mock<IAIParser>();
        var parser = new IntentParser(aiParser.Object, new ZorkOneGlobalCommandFactory(), null);
        var target = GetTarget(parser);

        // Act
        var result = await target.GetResponse("east");

        result.Should().Contain("boarded");
    }

    [Test]
    public void StartText_ShouldNotBeNullOrEmpty()
    {
        // Act
        var startText = new ZorkI().StartText;

        // Assert
        startText.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void SerializeItems()
    {
        var target = GetTarget();

        Repository.Reset();
        Repository.GetItem<Rope>().TiedToRailing = true;
        Repository.GetItem<Lantern>().IsOn = true;

        var guts = target.SaveGame();

        guts.Should().Contain("\"TiedToRailing\":true,");
        guts.Should().Contain("\"IsOn\":true,");
    }
}