using System.Diagnostics;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Lantern : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IAmALightSourceThatTurnsOnAndOff,
    ITurnBasedActor
{
    public int TurnsWhileOn { get; set; }

    public bool BurnedOut { get; set; }

    public override string[] NounsForMatching => ["lantern", "lamp", "light"];

    public override string GenericDescription(ILocation? currentLocation) => $"A brass lantern {(IsOn ? "(providing light)" : string.Empty)}";

    public override int Size => 3;

    public bool IsOn { get; set; }

    string IAmALightSourceThatTurnsOnAndOff.NowOnText => "The brass lantern is now on.";

    string IAmALightSourceThatTurnsOnAndOff.NowOffText => "The brass lantern is now off.";

    string IAmALightSourceThatTurnsOnAndOff.AlreadyOffText => "It is already off.";

    string IAmALightSourceThatTurnsOnAndOff.AlreadyOnText => "It is already on.";

    public string CannotBeTurnedOnText => BurnedOut ? "A burned-out lamp won't light. " : string.Empty;

    public string OnBeingTurnedOn(IContext context)
    {
        context.RegisterActor(this);
        return string.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
        context.RemoveActor(this);
    }

    string ICanBeExamined.ExaminationDescription => IsOn ? "The lamp is on." : "The lamp is turned off.";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a brass lantern (battery-powered) here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A battery-powered brass lantern is on the trophy case.";
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        Debug.WriteLine($"Lantern counter: {TurnsWhileOn}");
        TurnsWhileOn++;

        switch (TurnsWhileOn)
        {
            case 200:
                return Task.FromResult("The lamp appears a bit dimmer. ");

            case 300:
                return Task.FromResult("The lamp is definitely dimmer now. ");

            case 370:
                return Task.FromResult("The lamp is nearly out. ");

            case 375:
                BurnedOut = true;
                var result = new TurnLightOnOrOffProcessor().Process(
                    new SimpleIntent { Noun = NounsForMatching.First(), Verb = "turn off" }, context, this,
                    client);
                return Task.FromResult("You'd better have more light than from the lantern. " +
                                       result!.InteractionMessage);

            default:
                return Task.FromResult(string.Empty);
        }
    }
}