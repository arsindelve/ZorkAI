using GameEngine.Location;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class InsideTheBarrow : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) => new();

    protected override string GetContextBasedDescription(IContext context) =>
        """
        As you enter the barrow, the door closes inexorably behind you. Around you it is dark, but ahead is an enormous
        cavern, brightly lit. Through its center runs a wide stream. Spanning the stream is a small wooden footbridge,
        and beyond a path leads into a dark tunnel. Above the bridge, floating in the air, is a large sign. It reads:
        All ye who stand before this bridge have completed a great and perilous adventure which has tested your wit and
        courage. You have mastered the first part of the ZORK trilogy. Those who pass over this bridge must be prepared
        to undertake an even greater adventure that will severely test your skill and bravery!

        """;

    public override string Name => "Inside the Barrow";

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context, IGenerationClient client)
    {
        var response = await client.GenerateNarration(new WonTheGameRequest());
        return new PositiveInteractionResult(response);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation, IGenerationClient generationClient)
    {
        return Task.FromResult( $"""

               The ZORK trilogy continues with "ZORK II: The Wizard of Frobozz" and is completed in "ZORK III: The Dungeon
               Master," available now at fine stores everywhere.
               Your score would be {context.Score} (total of 350 points), in {context.Moves} moves.
               This score gives you the rank of Master Adventurer.
               """);
    }
}