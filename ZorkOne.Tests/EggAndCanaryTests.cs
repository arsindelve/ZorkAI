using FluentAssertions;
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
    public async Task UpTree_DropNest()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        await target.GetResponse("take nest");
        var response = await target.GetResponse("drop nest");
        Console.WriteLine(response);

        response.Should().Contain("The nest falls to the ground, and the egg spills out of it, seriously damaged");
        Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
        Repository.GetItem<Egg>().CurrentLocation.Should().BeOfType<ForestPath>();
        Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
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
            "There is a golden clockwork canary nestled in the broken egg. It seems to have recently had a bad experience. The mountings for its jewel-like eyes are empty, and its silver beak is crumpled. Through a cracked crystal window below its left wing you can see the remains of intricate machinery. It is not clear what result winding it would have, as the mainspring seems sprung.");
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
        response.Should()
            .Contain(
                "There is a golden clockwork canary nestled in the egg. It has ruby eyes and a silver beak. Through a crystal window below its left wing you can see intricate machinery inside. It appears to have wound down");
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

    [Test]
    public async Task WindBrokenCanary()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());
        Repository.GetItem<Canary>().IsDestroyed = true;

        var response = await target.GetResponse("wind canary");
        Console.WriteLine(response);

        response.Should().Contain("There is an unpleasant grinding noise from inside the canary");
    }

    [Test]
    public async Task WindGoodCanary_RandomPlace()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        var response = await target.GetResponse("wind canary");
        Console.WriteLine(response);

        response.Should().Contain("The canary chirps blithely, if somewhat tinnily, for a short time");
    }

    [Test]
    public async Task WindGoodCanary_UpATree()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        var response = await target.GetResponse("wind canary");
        Console.WriteLine(response);

        response.Should().Contain("The canary chirps, slightly off-key, an aria from a forgotten opera. From out of");
    }

    [Test]
    public async Task WindGoodCanary_ForestPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        var response = await target.GetResponse("wind canary");
        Console.WriteLine(response);

        response.Should().Contain("The canary chirps, slightly off-key, an aria from a forgotten opera. From out of");
    }

    [Test]
    public async Task WindGoodCanary_ForestPath_SecondTime()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        await target.GetResponse("wind canary");
        var response = await target.GetResponse("wind canary");
        Console.WriteLine(response);

        response.Should().Contain("The canary chirps blithely, if somewhat tinnily, for a short time");
    }

    [Test]
    public async Task WindGoodCanary_UpATree_BaubleBelowMe()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        await target.GetResponse("wind canary");
        var response = await target.GetResponse("Down");
        Console.WriteLine(response);

        response.Should().Contain("There is a beautiful brass bauble here");
    }

    [Test]
    public async Task WindGoodCanary_ForestPath_BaubleIsHere()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        await target.GetResponse("wind canary");
        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().Contain("There is a beautiful brass bauble here.");
    }

    [Test]
    public async Task TakeTheNest_InInventoryDescription()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        await target.GetResponse("take nest");
        var response = await target.GetResponse("i");
        Console.WriteLine(response);

        response.Should().Contain("A bird's nest");
        response.Should().Contain("The bird's nest contains:");
        response.Should().Contain("A jewel-encrusted egg");
    }

    [Test]
    public async Task DropTheNestFromTheTree()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();
        target.Context.ItemPlacedHere(Repository.GetItem<Canary>());

        await target.GetResponse("take nest");
        var response = await target.GetResponse("drop nest");
        Console.WriteLine(response);

        response.Should().Contain("The nest falls to the ground, and the egg spills out of it, seriously damaged");
        Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
        Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
        Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        Repository.GetItem<Egg>().CurrentLocation.Should().BeOfType<ForestPath>();
        Repository.GetItem<Nest>().CurrentLocation.Should().BeOfType<ForestPath>();
    }

        [Test]
        public async Task OpenEggWithSword()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
            target.Context.ItemPlacedHere(Repository.GetItem<Egg>());
            target.Context.ItemPlacedHere(Repository.GetItem<Sword>());
            
            var response = await target.GetResponse("open the egg with the sword");
            Console.WriteLine(response);
    
            response.Should().Contain("The egg is now open, but the clumsiness of your attempt has seriously compromised");
            Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        }
        
        [Test]
        public async Task OpenEggWithKnife()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
            target.Context.ItemPlacedHere(Repository.GetItem<Egg>());
            target.Context.ItemPlacedHere(Repository.GetItem<NastyKnife>());
            
            var response = await target.GetResponse("open the egg with the knife");
            Console.WriteLine(response);
    
            response.Should().Contain("The egg is now open, but the clumsiness of your attempt has seriously compromised");
            Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        }
        
        [Test]
        public async Task OpenEggWithScrewdriver()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
            target.Context.ItemPlacedHere(Repository.GetItem<Egg>());
            target.Context.ItemPlacedHere(Repository.GetItem<Screwdriver>());
            
            var response = await target.GetResponse("open the egg with the screwdriver");
            Console.WriteLine(response);
    
            response.Should().Contain("The egg is now open, but the clumsiness of your attempt has seriously compromised");
            Repository.GetItem<Canary>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsDestroyed.Should().BeTrue();
            Repository.GetItem<Egg>().IsOpen.Should().BeTrue();
        }
        
        [Test]
        public async Task ExamineEggAfterOpenEggWithScrewdriver()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();
            target.Context.ItemPlacedHere(Repository.GetItem<Egg>());
            target.Context.ItemPlacedHere(Repository.GetItem<Screwdriver>());
            
            await target.GetResponse("open the egg with the screwdriver");
            var response = await target.GetResponse("examine egg");
            Console.WriteLine(response);
    
            response.Should().Contain("It seems to have recently had a bad");
        }

    //>examine egg
    //The broken jewel-encrusted egg contains:
    //A broken clockwork canary
}