using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

public class BioLabEscapeTests : EngineTestsBase
{
    [Test]
    public async Task LabDesk_OpenDesk_ShowsContents()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("open desk");

        response.Should().Contain("Opening the desk reveals a gas mask");
    }

    [Test]
    public async Task LabDesk_OpenDesk_WhenEmpty_ShowsOpened()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("open desk");
        await target.GetResponse("take mask");
        await target.GetResponse("close desk");
        var response = await target.GetResponse("open desk");

        // When desk is empty, should just say "Opened."
        response.Should().Contain("Opened");
        response.Should().NotContain("reveals");
    }

    [Test]
    public async Task LabOffice_FullEscapeSequence()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        // Setup: Floyd has already died, his body is in BioLockEast
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        var bioLockEast = GetLocation<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);

        // Step 1: Press red button - activates fungicide (TurnsRemaining = 3)
        var response = await target.GetResponse("press red button");
        Console.WriteLine($"Step 1: {response}");
        response.Should().Contain("hissing from beyond the door");
        // End of turn: TurnsRemaining decrements to 2

        // Step 2: Open office door
        response = await target.GetResponse("open door");
        Console.WriteLine($"Step 2: {response}");
        response.Should().Contain("office door is now open");
        response.Should().Contain("filled with a light mist");
        // Should only appear once (no duplicate)
        var mistCount = response.Split("filled with a light mist").Length - 1;
        mistCount.Should().Be(1);
        // End of turn: TurnsRemaining decrements to 1

        // Step 3: Go west to Bio Lab
        response = await target.GetResponse("w");
        Console.WriteLine($"Step 3: {response}");
        response.Should().Contain("Bio Lab");
        response.Should().Contain("mist");
        response.Should().Contain("mutants");
        // End of turn: TurnsRemaining decrements to 0, fungicide wears off, chase starts

        // Step 4: Open lab door (uses free turn in BioLab)
        // The chase has started, but player gets one free action to open the door
        response = await target.GetResponse("open lab door");
        Console.WriteLine($"Step 4: [{response}]");
        // Door should open
        response.Should().Contain("door opens");
        // The free turn message appears from ChaseSceneManager
        response.Should().Contain("mutants are almost upon you");
        // Should NOT contain death or Floyd rushing messages
        response.Should().NotContain("devour");
        response.Should().NotContain("Floyd");

        // Step 5: Go west to Bio Lock East - fleeing from mutants
        response = await target.GetResponse("w");
        Console.WriteLine($"Step 5: {response}");
        response.Should().Contain("Bio Lock");
        // Should see Floyd's body
        response.Should().Contain("Floyd");
        // Chase continues with random chase message
        response.Should().Contain("mutants burst into the room");
    }
}