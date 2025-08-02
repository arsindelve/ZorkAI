using Azure;
using FluentAssertions;
using Model.AIGeneration.Requests;
using Model.Interface;
using Moq;
using Planetfall.Item;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class FloydTests : EngineTestsBase
{
    [Test]
    public async Task Search_FindCard()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = false;
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("find and take");
        target.Context.HasItem<LowerElevatorAccessCard>().Should().BeTrue();
    }
    
    [Test]
    public async Task Search_AlreadyGone()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = false;
        Take<LowerElevatorAccessCard>();
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("search discovers nothing");
        target.Context.HasItem<LowerElevatorAccessCard>().Should().BeTrue();
    }
    
    [Test]
    public async Task Search_Activated()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("giggles");
    }
    
    [Test]
    public async Task TurnOn_AlreadyOn()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("activate floyd");

        response.Should().Contain("already been");
    }
    
    [Test]
    public async Task PlayWithFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("play with floyd");

        response.Should().Contain("centichrons");
    }
    
    [Test]
    public async Task KissFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("kiss floyd");

        response.Should().Contain("shock");
    }
    
    [Test]
    public async Task KickFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("kick floyd");

        response.Should().Contain("wire");
    }
    
    [Test]
    public async Task KillFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("punch floyd");

        response.Should().Contain("Chase and Tag");
    }
    
    [Test]
    public async Task GiveSomethingToFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>(); 
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("give the diary to floyd");

        response.Should().Contain("Neat");
        GetItem<Diary>().CurrentLocation.Should().BeOfType<Floyd>();
        GetItem<Floyd>().Items.Should().NotBeNull();
    }
    
    [Test]
    public async Task GiveSomethingToFloyd_AlreadyHolding()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        Take<Key>();
        GetItem<Floyd>().IsOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("give the key to floyd");

        response.Should().Contain("shrugs");
        GetItem<Diary>().CurrentLocation.Should().BeOfType<Floyd>();
        GetItem<Key>().CurrentLocation.Should().BeOfType<RobotShop>();
        GetItem<Floyd>().Items.Should().NotBeNull();
    }
    
    [Test]
    public async Task GiveSomethingToFloyd_SeeHimHoldingIt()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("look");

        response.Should().Contain("multiple purpose robot is holding:");
        response.Should().Contain("A diary");
    }
    
    [Test]
    public async Task GiveSomethingToFloyd_TakeItBack()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("take diary");

        response.Should().Contain("Taken");
        GetItem<Floyd>().ItemBeingHeld.Should().BeNull();
        target.Context.HasItem<Diary>().Should().BeTrue();
    }
    
    [Test]
    public async Task ExamineFloyd_On()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        var response = await target.GetResponse("examine floyd");

        response.Should().Contain("From its design, the robot seems to be of the multi-purpose sort");
    }
    
    [Test]
    public async Task ExamineFloyd_Off()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        
        var response = await target.GetResponse("examine floyd");

        response.Should().Contain("The deactivated robot is leaning against the wall");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_AllSystemsGo()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("Floyd claps his hands with excitement");
        GetItem<Floyd>().ItemBeingHeld.Should().BeOfType<LowerElevatorAccessCard>();
    }
    
    [Test]
    public async Task DoesFloydOfferCard_NotHere()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<Library>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_DiceRollFail()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == false);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_HeIsOff()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_NoLongerHasIt()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().Items.Clear();
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task GenerateCompanionSpeech_ExcludesFloydFromRoomDescription()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        
        // Mock the generation client to capture the request that was sent
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says something")
            .Callback<CompanionRequest>(request =>
            {
                // Verify that Floyd's description is not included in the room description
                var floydDescription = floyd.GenericDescription(GetLocation<RobotShop>()).Trim();
                request.UserMessage!.Should().NotContain(floydDescription);
                
                // Specifically check that it doesn't contain "multiple purpose robot"
                request.UserMessage!.Should().NotContain("multiple purpose robot");
                
                // But verify that the room name is still included
                request.UserMessage!.Should().Contain("Robot Shop");
            });
        
        // Call GenerateCompanionSpeech directly through reflection to test the specific method
        var methodInfo = typeof(QuirkyCompanion).GetMethod("GenerateCompanionSpeech", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo!.Should().NotBeNull();
        
        await (Task<string>)methodInfo!.Invoke(floyd, new object[] { target.Context, target.GenerationClient, null! })!;
        
        // Verify the mock was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }
}