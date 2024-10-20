using FluentAssertions;
using FluentAssertions.Equivalency.Steps;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests;

public class EggAndCanaryTests : EngineTestsBase
{
    [Test]
    public async Task UpTree_InNest()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().Contain(
            "In the bird's nest is a large egg encrusted with precious jewels, apparently scavenged by a childless songbird. The egg is covered with fine gold inlay, and ornamented in lapis lazuli and mother-of-pearl. Unlike most eggs, this one is hinged and closed with a delicate looking clasp. The egg appears extremely fragile");
    }

    [Test]
    public async Task UpTree_NotInNest()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().NotContain(
            "In the bird's nest is a large egg encrusted with precious jewels, apparently scavenged by a childless songbird. The egg is covered with fine gold inlay, and ornamented in lapis lazuli and mother-of-pearl. Unlike most eggs, this one is hinged and closed with a delicate looking clasp. The egg appears extremely fragile");
    }

    [Test]
    public async Task UpTree_DropTheEgg()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        var response = await target.GetResponse("drop egg");
        Console.WriteLine(response);

        response.Should().Contain(
            "The egg falls to the ground and springs open, seriously damaged. There is a golden clockwork canary nestled in the egg. It seems to have recently had a bad experience. The mountings for its jewel-like eyes are empty, and its silver beak is crumpled. Through a cracked crystal window below its left wing you can see the remains of intricate machinery. It is not clear what result winding it would have, as the mainspring seems sprung");

        Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
        Repository.GetItem<Egg>().CurrentLocation.Should().BeOfType<ForestPath>();
        Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
    }

    [Test]
    public async Task UpTree_DropTheEgg_SeeEggOnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        var response = await target.GetResponse("Down");
        Console.WriteLine(response);

        response.Should().Contain("There is a somewhat ruined egg here");
    }

    [Test]
    public async Task UpTree_DropTheEgg_SeeCanaryOnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        var response = await target.GetResponse("Down");
        Console.WriteLine(response);

        response.Should().Contain(
            "There is a golden clockwork canary nestled in the egg. It seems to have recently had a bad experience. The mountings for its jewel-like eyes are empty, and its silver beak is crumpled. Through a cracked crystal window below its left wing you can see the remains of intricate machinery. It is not clear what result winding it would have, as the mainspring seems sprung.");
    }

    [Test]
    public async Task UpTree_DropTheEgg_CloseTheEgg_DontSeeCanaryOnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        await target.GetResponse("Down");
        var response = await target.GetResponse("close egg");
        Console.WriteLine(response);

        response.Should().NotContain(
            "There is a golden clockwork canary nestled in the egg. It seems to have recently had a bad experience. The mountings for its jewel-like eyes are empty, and its silver beak is crumpled. Through a cracked crystal window below its left wing you can see the remains of intricate machinery. It is not clear what result winding it would have, as the mainspring seems sprung.");
    }

    [Test]
    public async Task BrokenCanary_InInventory()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        await target.GetResponse("Down");
        await target.GetResponse("take canary");
        var response = await target.GetResponse("i");
        Console.WriteLine(response);

        response.Should().Contain("A broken clockwork canary");
    }

    [Test]
    public async Task BrokenCanary_OnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        await target.GetResponse("Down");
        await target.GetResponse("take canary");
        await target.GetResponse("drop canary");
        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().Contain("There is a somewhat ruined egg here");
        response.Should().Contain("There is a broken clockwork canary here");
        response.Should().NotContain(
            "There is a golden clockwork canary nestled in the egg. It seems to have recently had a bad experience. The mountings for its jewel-like eyes are empty, and its silver beak is crumpled. Through a cracked crystal window below its left wing you can see the remains of intricate machinery. It is not clear what result winding it would have, as the mainspring seems sprung.");
    }

    [Test]
    public async Task GoodClosedEgg_InInventory()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        var response = await target.GetResponse("i");
        Console.WriteLine(response);

        response.Should().Contain("A jewel-encrusted egg");
    }

    [Test]
    public async Task OpenTheEgg_YouCannot()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        var response = await target.GetResponse("open egg");
        Console.WriteLine(response);

        response.Should().Contain("You have neither the tools nor the expertise");
    }

    [Test]
    public async Task BrokenEgg_InInventory()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take egg");
        await target.GetResponse("drop egg");
        await target.GetResponse("Down");
        await target.GetResponse("take egg");
        var response = await target.GetResponse("i");
        Console.WriteLine(response);

        response.Should().Contain("A broken jewel-encrusted egg");
    }
    
    [Test]
    public async Task GoodEgg_OnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<TreasureRoom>();
        Repository.GetLocation<TreasureRoom>().ItemPlacedHere(Repository.GetItem<Egg>());
        Repository.GetItem<Egg>().IsOpen = true;
        Repository.GetItem<Egg>().HasEverBeenPickedUp = true;
        
        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().Contain("There is a jewel-encrusted egg here");
        response.Should().Contain("There is a golden clockwork canary nestled in the egg. It has ruby eyes and a silver beak. Through a crystal window below its left wing you can see intricate machinery inside. It appears to have wound down");
    }
    
    [Test]
    public async Task OpenTheBrokenEgg_BrokenCanaryInside()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        Repository.GetItem<Egg>().IsDestroyed = true;
        Repository.GetItem<Canary>().IsDestroyed = true;
        
        var response = await target.GetResponse("open egg");
        Console.WriteLine(response);

        response.Should().Contain("Opening the broken jewel-encrusted egg reveals a broken clockwork canary");
    }
    
    [Test]
    public async Task OpenTheBrokenEgg_Empty()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        Repository.GetItem<Egg>().IsDestroyed = true;
        Repository.GetLocation<Cellar>().ItemPlacedHere(Repository.GetItem<Canary>());
        
        var response = await target.GetResponse("open egg");
        Console.WriteLine(response);

        response.Should().Contain("Opened");
    }

    //open egg with sword. 

    //>examine egg
    //The broken jewel-encrusted egg contains:
    //A broken clockwork canary
    
    // drop the nest
    // The nest falls to the ground, and the egg spills out of it, seriously damaged

}