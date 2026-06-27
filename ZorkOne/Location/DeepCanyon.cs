using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class DeepCanyon : DarkLocation
{
    public override string Name => "Deep Canyon";

    public override string[] NounsForMatching => ["ravine", "gorge"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.SW, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<LoudRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // The water-sound line varies with two pieces of world state, mirroring the original
        // (zork1/1actions.zil DEEP-CANYON-F, lines 1730-1745): whether the dam's sluice gates are
        // open (GATES-OPEN) and whether the reservoir has drained to low tide (LOW-TIDE).
        const string baseText =
            "You are on the south edge of a deep canyon. Passages lead off to the east, " +
            "northwest and southwest. A stairway leads down.";

        var gatesOpen = GetLocation<Dam>().SluiceGatesOpen;          // ZIL GATES-OPEN
        var lowTide = GetLocation<ReservoirSouth>().IsDrained;       // ZIL LOW-TIDE

        if (gatesOpen && !lowTide)
            return baseText + " You can hear a loud roaring sound, like that of rushing water, from below.";

        if (!gatesOpen && lowTide)
            return baseText; // reservoir drained and gates shut — no water sound

        return baseText + " You can hear the sound of flowing water from below.";
    }

    public override void Init()
    {
    }
}