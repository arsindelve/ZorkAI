using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests;

public class UniformTests : EngineTestsBase
{
    [Test]
    public async Task PatrolUniform_Examine()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("insects");
    }
    
    [Test]
    public async Task PatrolUniform_OpenPocket()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("open pocket");
        response.Should().Contain("There's no way to open or close the pocket of the Patrol uniform");
    }
    
    [Test]
    public async Task LabUniform_OpenPocket()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        var response = await target.GetResponse("open pocket");
        response.Should().Contain("You discover a teleportation access card and a piece of paper in the pocket of the uniform");
    }
    
    [Test]
    public async Task LabUniform_OpenPocket_ThenClose()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("open pocket");
        var response = await target.GetResponse("close pocket");
        response.Should().Contain("Closed");
    }
    
    [Test]
    public async Task LabUniform_OpenPocket_Examine()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("open pocket");
        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("open");
        response.Should().Contain("The lab uniform contains:");
        response.Should().Contain("A teleportation access card");
        response.Should().Contain("A piece of paper");
    }
    
    [Test]
    public async Task LabUniform_ClosedPocket_Examine()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("closed");
        response.Should().NotContain("The lab uniform contains:");
        response.Should().NotContain("A teleportation access card");
        response.Should().NotContain("A piece of paper");
    }
    
    [Test]
    public async Task LabUniform_OnTheGround_OpenPocket_Look()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("take uniform");
        await target.GetResponse("open pocket");
        await target.GetResponse("drop uniform");
        var response = await target.GetResponse("look");
        response.Should().Contain("The lab uniform contains:");
        response.Should().Contain("A teleportation access card");
        response.Should().Contain("A piece of paper");
        response.Should().Contain("There is a lab uniform here.");
        response.Should().NotContain("Hanging on a rack is");
    }
    
    [Test]
    public async Task LabUniform_OnTheGround_ClosedPocket_Look()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("take uniform");
        await target.GetResponse("drop uniform");
        var response = await target.GetResponse("look");
        response.Should().NotContain("The lab uniform contains:");
        response.Should().NotContain("A teleportation access card");
        response.Should().NotContain("A piece of paper");
        response.Should().Contain("There is a lab uniform here.");
        response.Should().NotContain("Hanging on a rack is");
    }
    
    [Test]
    public async Task LabUniform_ClosedPocket_Look()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        var response = await target.GetResponse("look");
        response.Should().NotContain("The lab uniform contains:");
        response.Should().NotContain("A teleportation access card");
        response.Should().NotContain("A piece of paper");
    }
    
    [Test]
    public async Task LabUniform_OpenPocket_Look()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("open pocket");
        var response = await target.GetResponse("look");
        response.Should().Contain("The lab uniform contains:");
        response.Should().Contain("A teleportation access card");
        response.Should().Contain("A piece of paper");
    }
    
    [Test]
    public async Task LabUniform_OpenPocket_Take()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        
        await target.GetResponse("open pocket");
        var response = await target.GetResponse("take paper");
        response.Should().Contain("Taken");
    }
    
    [Test]
    public async Task LabUniform_ClosedPocket_Take()
    {
        var target = GetTarget();
        StartHere<LabStorage>();

        var response = await target.GetResponse("take paper");
        response.Should().NotContain("Taken");
    }

    [Test]
    public async Task LabUniform_TakeCardFromPocket_ThenPutItBack()
    {
        // Issue #478: the pocket ships with two size-1 items (teleportation access card and
        // piece of paper) but capacity must fit both. A container that can't hold its own
        // starting contents is an invariant violation - after taking the card out, it must
        // be possible to put it back.
        var target = GetTarget();
        StartHere<LabStorage>();

        await target.GetResponse("open pocket");
        (await target.GetResponse("take card")).Should().Contain("Taken");

        var response = await target.GetResponse("put card in pocket");
        response.Should().NotContain("There's no room");
        response.Should().Contain("Done");

        // Both seeded items coexist in the pocket once the card is returned.
        var pocket = Repository.GetItem<LabUniformPocket>();
        pocket.Items.Should().Contain(Repository.GetItem<TeleportationAccessCard>());
        pocket.Items.Should().Contain(Repository.GetItem<PieceOfPaper>());
    }

    [Test]
    public void LabUniform_PocketHasRoomForItsSeededContents()
    {
        // Guard: the pocket's declared capacity must accommodate both items it starts with.
        GetTarget();
        StartHere<LabStorage>();

        var pocket = Repository.GetItem<LabUniformPocket>();
        var card = Repository.GetItem<TeleportationAccessCard>();

        pocket.Items.Should().HaveCount(2);

        // Remove the card, then confirm the pocket still reports room to take it back.
        pocket.RemoveItem(card);
        pocket.HaveRoomForItem(card).Should().BeTrue();
    }
    
    [Test]
    public async Task PatrolUniform_ClosePocket()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("close pocket");
        response.Should().Contain("There's no way to open or close the pocket of the Patrol uniform");
    }
    
    [Test]
    public async Task Uniform_Disambiguation()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        Take<PatrolUniform>();

        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("Do you mean the patrol uniform or the lab uniform");
    }

    [Test]
    public async Task PatrolUniform_TakeIdCard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>(); // Add uniform to inventory

        // ID card should be inside the uniform pocket
        var uniform = Repository.GetItem<PatrolUniform>();
        var pocket = Repository.GetItem<PatrolUniformPocket>();
        var idCard = Repository.GetItem<IdCard>();

        uniform.Items.Should().Contain(pocket);
        pocket.Items.Should().Contain(idCard);

        var response = await target.GetResponse("take id card");
        response.Should().Contain("Taken");

        // ID card should now be in direct inventory
        target.Context.Items.Should().Contain(idCard);
    }

    [Test]
    public async Task PatrolUniform_TakeCard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>(); // Add uniform to inventory

        var response = await target.GetResponse("take card");
        response.Should().Contain("Taken");

        var idCard = Repository.GetItem<IdCard>();
        target.Context.Items.Should().Contain(idCard);
    }

    [Test]
    public void PatrolUniform_IdCardIsInPocket()
    {
        GetTarget();
        StartHere<DeckNine>();

        var uniform = Repository.GetItem<PatrolUniform>();
        var pocket = Repository.GetItem<PatrolUniformPocket>();
        var idCard = Repository.GetItem<IdCard>();

        // Verify structure
        uniform.Items.Should().Contain(pocket);
        pocket.Items.Should().Contain(idCard);
        idCard.CurrentLocation.Should().Be(pocket);
        pocket.CurrentLocation.Should().Be(uniform);
    }

    [Test]
    public void PatrolUniform_HasMatchingNoun_FindsIdCard()
    {
        GetTarget();
        StartHere<DeckNine>();

        var uniform = Repository.GetItem<PatrolUniform>();

        var result = uniform.HasMatchingNoun("card", lookInsideContainers: true);
        result.HasItem.Should().BeTrue();
        result.TheItem.Should().BeOfType<IdCard>();
    }

    [Test]
    public void Context_HasMatchingNoun_FindsIdCard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        var uniform = Take<PatrolUniform>(); // Add uniform to inventory

        // Verify uniform is in inventory
        target.Context.Items.Should().Contain(uniform);

        var result = target.Context.HasMatchingNoun("card", lookInsideContainers: true);
        result.HasItem.Should().BeTrue();
        result.TheItem.Should().BeOfType<IdCard>();
    }

    [Test]
    public void GetItemInScope_FindsIdCard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>(); // Add uniform to inventory

        var item = Repository.GetItemInScope("card", target.Context);
        item.Should().NotBeNull();
        item.Should().BeOfType<IdCard>();
    }

    [Test]
    public async Task PatrolUniform_DropCard_StillInPocket_DoesNotDrop()
    {
        // Issue #362 regression guard: GetItemsToDrop's single-item branch resolves the nested ID
        // card via Repository.GetItemInInventory (which, like GetItemInScope, walks into containers),
        // but DropIt's own possession check is deliberately kept flat (context.Items.Contains) rather
        // than the canonical Repository.IsItemPossessedBy used by GIVE/SHOW - proven by
        // WalkthroughTestTwo's "put leaflet in sack" then "drop leaflet" step, which expects "You
        // don't have that!" even though the sack is open. DROP requires taking a nested item out
        // first; only GIVE/SHOW reach into a worn/held container via the ZIL (HAVE) flag.
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>(); // Add uniform to inventory; the ID card stays in its pocket

        var response = await target.GetResponse("drop card");

        response.Should().Contain("You don't have that!");
        var pocket = Repository.GetItem<PatrolUniformPocket>();
        var idCard = Repository.GetItem<IdCard>();
        pocket.Items.Should().Contain(idCard);
    }
}