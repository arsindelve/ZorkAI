using GameEngine;
using Model.Interface;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests;

/// <summary>
///     Tests for <see cref="Context{T}.HasLightSource" /> covering light sources that live
///     inside a container in the room (the "shaft basket" puzzle scenario).
/// </summary>
public class HasLightSourceContainerTests : EngineTestsBase
{
    [Test]
    public void OffLampInTransparentContainerInRoom_IsNotALightSource()
    {
        var engine = GetTarget();

        // Dark room as the current location.
        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        // A transparent container in the room.
        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        // A toggleable lamp inside the container, turned OFF.
        var lantern = Repository.GetItem<Lantern>();
        lantern.IsOn = false;
        basket.ItemPlacedHere(lantern);

        // An OFF lamp provides no light, even inside an open/transparent container.
        engine.Context.HasLightSource.Should().BeFalse();
    }

    [Test]
    public void OnLampInTransparentContainerInRoom_IsALightSource()
    {
        var engine = GetTarget();

        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        var lantern = Repository.GetItem<Lantern>();
        lantern.IsOn = true;
        basket.ItemPlacedHere(lantern);

        engine.Context.HasLightSource.Should().BeTrue();
    }

    [Test]
    public void ConstantLightSourceInTransparentContainerInRoom_IsALightSource()
    {
        var engine = GetTarget();

        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        // The torch is a constant light source (ICannotBeTurnedOff) - the actual shaft puzzle item.
        var torch = Repository.GetItem<Torch>();
        basket.ItemPlacedHere(torch);

        engine.Context.HasLightSource.Should().BeTrue();
    }
}
